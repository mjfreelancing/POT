/// <reference types="node" />

/**
 * Database seed helpers for isolated-sequence E2E tests.
 *
 * These helpers allow a test suite to reset the shared Testcontainers database
 * to a known state before a sequence of tests begins. They are only intended
 * for use in isolated-sequence tests (separate npm invocation, workers: 1).
 *
 * Parallel-safe and fixture-managed tests must not call these functions —
 * they share the database established by globalSetup and manage their own
 * state through afterEach cleanup.
 *
 * Available exports:
 *   applyDatabaseSeedMode  — reset the database to blank, seeded-users, or fully-seeded
 *   listExistingUsernames  — query which of a given set of usernames exist in the database
 *
 * Container ID resolution order (used by all docker exec calls):
 *   1. DATABASE_CONTAINER_ID environment variable (set by globalSetup at runtime)
 *   2. e2e/.runtime/testcontainers.json (written by globalSetup, used when env var is not present)
 *   3. Hard failure — DATABASE_CONTAINER_ID is required
 */

import { spawnSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

/**
 * The three database start states a test suite can request.
 *
 * blank        — migrations applied; no site, no users, no financial data.
 *                Used for tests that exercise first-run / registration flows.
 *
 * seeded-users — site and canonical E2E users exist; no financial data.
 *                Established by loading e2e/seed/baseline.sql into the container.
 *                Used when a test creates its own financial records.
 *
 * fully-seeded — seeded-users state plus financial artifact imported and
 *                renewal/accrual run to advance data to the current date.
 *                Used for projection, filter, and long-flow scenarios.
 */
type E2eDatabaseSeedMode = 'blank' | 'seeded-users' | 'fully-seeded';

// Async callback responsible for importing the financial artifact and running
// server-side renewal/accrual. Required when mode is 'fully-seeded'.
type FullySeededHook = () => Promise<void>;

type ApplyDatabaseSeedModeOptions = {
  mode: E2eDatabaseSeedMode;
  // Absolute path to e2e/seed/baseline.sql — the committed data-only pg_dump snapshot.
  baselineSqlPath: string;
  // Required when mode === 'fully-seeded'. Must import the financial artifact
  // and run server-side renewal/accrual before resolving.
  runFullySeededSetup?: FullySeededHook;
};

// Throws if the named environment variable is absent or empty.
function requireEnvironmentVariable(name: string): string {
  const value = process.env[name];

  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }

  return value;
}

// Reads the container ID written by globalSetup to e2e/.runtime/testcontainers.json.
// Returns null when the file does not exist (e.g. globalSetup has not run yet).
function getRuntimeStateContainerIdOrNull(): string | null {
  const currentFilePath = fileURLToPath(import.meta.url);
  const currentDirectory = dirname(currentFilePath);
  const runtimeStatePath = resolve(
    currentDirectory,
    '../.runtime/testcontainers.json',
  );

  if (!existsSync(runtimeStatePath)) {
    return null;
  }

  const runtimeState = JSON.parse(readFileSync(runtimeStatePath, 'utf-8')) as {
    databaseContainerId?: string;
  };

  return runtimeState.databaseContainerId ?? null;
}

// Returns the Docker container ID for the running Testcontainers Postgres instance.
// Prefers the environment variable set by globalSetup; falls back to the runtime
// state file written by globalSetup; fails hard if neither is available.
function getContainerIdFromEnvironment(): string {
  const containerIdFromEnvironment = process.env.DATABASE_CONTAINER_ID;

  if (containerIdFromEnvironment) {
    return containerIdFromEnvironment;
  }

  const containerIdFromRuntimeState = getRuntimeStateContainerIdOrNull();

  if (containerIdFromRuntimeState) {
    return containerIdFromRuntimeState;
  }

  return requireEnvironmentVariable('DATABASE_CONTAINER_ID');
}

// Pipes a SQL file into the running Postgres container via `docker exec ... psql`.
// Uses ON_ERROR_STOP=1 so any SQL error immediately aborts and is surfaced as a
// thrown error rather than silently continuing.
async function importSqlIntoRunningPostgresContainer(
  baselineSqlPath: string,
): Promise<void> {
  const containerId = getContainerIdFromEnvironment();
  const databaseUrl = new URL(requireEnvironmentVariable('DATABASE_URL'));
  const sql = readFileSync(baselineSqlPath, 'utf-8');
  const username = decodeURIComponent(databaseUrl.username);
  const password = decodeURIComponent(databaseUrl.password);
  const database = decodeURIComponent(databaseUrl.pathname.replace(/^\//, ''));

  const result = spawnSync(
    'docker',
    [
      'exec',
      '-i',
      '-e',
      `PGPASSWORD=${password}`,
      containerId,
      'psql',
      '-v',
      'ON_ERROR_STOP=1',
      '-U',
      username,
      '--dbname',
      database,
    ],
    {
      encoding: 'utf-8',
      input: sql,
    },
  );

  if (result.status !== 0) {
    throw new Error(
      `Failed to import SQL into PostgreSQL container. ${result.stderr || result.stdout}`,
    );
  }
}

/**
 * Queries the database for which of the supplied usernames already exist.
 *
 * Used by smoke tests (e.g. baselineUsersImport.test.ts) to assert that the
 * baseline seed loaded the expected canonical users. Returns only the usernames
 * that were found; absent usernames are not included.
 *
 * Safe to call from parallel-safe tests — SELECT only, no mutations.
 */
async function listExistingUsernames(usernames: string[]): Promise<string[]> {
  if (usernames.length === 0) {
    return [];
  }

  const containerId = getContainerIdFromEnvironment();
  const databaseUrl = new URL(requireEnvironmentVariable('DATABASE_URL'));
  const username = decodeURIComponent(databaseUrl.username);
  const password = decodeURIComponent(databaseUrl.password);
  const database = decodeURIComponent(databaseUrl.pathname.replace(/^\//, ''));

  // Build a SQL IN-list with each username single-quoted and escaped.
  const escapedUsernames = usernames
    .map(currentUsername => `'${currentUsername.replace(/'/g, "''")}'`)
    .join(', ');

  const result = spawnSync(
    'docker',
    [
      'exec',
      '-i',
      '-e',
      `PGPASSWORD=${password}`,
      containerId,
      'psql',
      '-v',
      'ON_ERROR_STOP=1',
      '-U',
      username,
      '--dbname',
      database,
      '-t', // tuples only — suppress column headers and row counts
      '-A', // unaligned output — one username per line, no padding
      '-c',
      `SELECT "Username" FROM public."User" WHERE "Username" IN (${escapedUsernames}) ORDER BY "Username";`,
    ],
    {
      encoding: 'utf-8',
    },
  );

  if (result.status !== 0) {
    throw new Error(
      `Failed to query usernames from PostgreSQL container. ${result.stderr || result.stdout}`,
    );
  }

  return result.stdout
    .split(/\r?\n/)
    .map(entry => entry.trim())
    .filter(entry => entry.length > 0);
}

/**
 * Resets the shared Testcontainers database to the requested seed mode.
 *
 * Intended for use in isolated-sequence tests only — call from `beforeAll` to
 * establish a known database state before the test sequence begins.
 *
 * Mode behaviour:
 *   blank        — no-op; the container already has only migrations applied.
 *   seeded-users — loads baseline.sql (site + canonical users) via psql.
 *   fully-seeded — loads baseline.sql then calls runFullySeededSetup() to
 *                  import the financial artifact and run renewal/accrual.
 *
 * Throws if mode is 'fully-seeded' and runFullySeededSetup is not provided.
 */
async function applyDatabaseSeedMode(
  options: ApplyDatabaseSeedModeOptions,
): Promise<void> {
  switch (options.mode) {
    case 'blank':
      return;

    case 'seeded-users':
      await importSqlIntoRunningPostgresContainer(options.baselineSqlPath);
      return;

    case 'fully-seeded':
      if (!options.runFullySeededSetup) {
        throw new Error(
          'Fully-seeded mode requires runFullySeededSetup to complete artifact import and renewal/accrual.',
        );
      }

      await importSqlIntoRunningPostgresContainer(options.baselineSqlPath);
      await options.runFullySeededSetup();
      return;
  }
}

export { applyDatabaseSeedMode, listExistingUsernames };
export type { E2eDatabaseSeedMode };
