/// <reference types="node" />

/**
 * Database query helpers for parallel-safe E2E tests.
 *
 * These helpers run read-only queries against the shared Testcontainers
 * database established by globalSetup. They must never mutate state.
 *
 * Available exports:
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

export { listExistingUsernames };
