import { FullConfig } from '@playwright/test';
import {
  PostgreSqlContainer,
  StartedPostgreSqlContainer,
} from '@testcontainers/postgresql';
import { spawn, spawnSync } from 'node:child_process';
import { mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

/**
 * Global setup runs once per test run, before any tests execute.
 *
 * Purpose:
 * - Start a disposable Postgres container
 * - Make connection details available to all tests via environment variables
 * - Ensure container runs for the entire test session
 *
 * Lifecycle:
 * 1. This runs when you execute `npx playwright test`
 * 2. Container starts and stays alive
 * 3. All tests run (workers use the connection URI from process.env)
 * 4. globalTeardown runs automatically (Playwright handles cleanup)
 */

let postgresContainer: StartedPostgreSqlContainer;

const currentFilePath = fileURLToPath(import.meta.url);
const currentDirectory = dirname(currentFilePath);
const serverDirectory = resolve(currentDirectory, '../../Server');
const runtimeStatePath = resolve(
  currentDirectory,
  'e2e/.runtime/testcontainers.json',
);
const migrationProjectPath = resolve(
  serverDirectory,
  'Pot.Data.Migrations/Pot.Data.Migrations.csproj',
);
const baselineSqlPath = resolve(currentDirectory, 'e2e/seed/baseline.sql');

function writeRuntimeState(databaseContainerId: string): void {
  const runtimeDirectory = resolve(currentDirectory, 'e2e/.runtime');

  mkdirSync(runtimeDirectory, { recursive: true });

  writeFileSync(
    runtimeStatePath,
    JSON.stringify({ databaseContainerId }),
    'utf-8',
  );
}

function runBaselineSeed(
  containerId: string,
  username: string,
  password: string,
  database: string,
): void {
  console.log('🌱 Running baseline seed...');

  const sql = readFileSync(baselineSqlPath, 'utf-8');

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
    throw new Error(`Baseline seed failed. ${result.stderr || result.stdout}`);
  }

  console.log('✅ Baseline seed completed');
}

function runDatabaseMigrations(): Promise<void> {
  return new Promise((resolvePromise, rejectPromise) => {
    console.log('🛠️ Running database migrations...');

    const migrationProcess = spawn(
      'dotnet',
      ['run', '--no-launch-profile', '--project', migrationProjectPath],
      {
        cwd: serverDirectory,
        env: process.env,
        stdio: 'inherit',
      },
    );

    migrationProcess.on('error', error => {
      rejectPromise(error);
    });

    migrationProcess.on('exit', code => {
      if (code === 0) {
        console.log('✅ Database migrations completed');
        resolvePromise();
        return;
      }

      rejectPromise(
        new Error(
          `Database migrations failed with exit code ${code ?? 'null'}`,
        ),
      );
    });
  });
}

function configureDotNetEnvironment(): void {
  // Only DOTNET_ENVIRONMENT and ASPNETCORE_ENVIRONMENT are set here.
  // These are consumed by the migration runner child process spawned from globalSetup.
  // The API (webServer) starts before globalSetup runs, so it cannot receive config via
  // env vars — it gets all config (DB, JWT, CORS, rate-limiting) via CLI args in
  // playwright.config.ts.
  process.env.DOTNET_ENVIRONMENT = 'Production';
  process.env.ASPNETCORE_ENVIRONMENT = 'Production';

  console.log('✅ .NET environment set for migration runner');
  console.log('   DOTNET_ENVIRONMENT=Production');
  console.log('   ASPNETCORE_ENVIRONMENT=Production');
}

export default async (config: FullConfig) => {
  console.log('🐘 Starting Testcontainers Postgres...');

  try {
    // Clear auth state files from any previous run to prevent stale sessions.
    const authDirectory = resolve(currentDirectory, 'e2e/.auth');
    rmSync(authDirectory, { recursive: true, force: true });
    console.log('🗑️  Cleared stale auth state from previous run');

    configureDotNetEnvironment();

    // Create and start a PostgreSQL container bound to fixed host port 55432.
    // The webServer (dotnet) starts before globalSetup, so its DB connection args
    // are hardcoded in playwright.config.ts to match this fixed port.
    postgresContainer = await new PostgreSqlContainer('postgres:16')
      .withDatabase('pot_e2e')
      .withUsername('test_user')
      .withPassword('test_pass')
      .withExposedPorts({ container: 5432, host: 55432 })
      .start();

    // Get the connection URI (format: postgresql://user:pass@host:port/database)
    const connectionUri = postgresContainer.getConnectionUri();

    console.log('✅ Postgres container started');
    console.log(`   URI: ${connectionUri}`);

    // Parse the connection URI to extract components
    // Format: postgresql://username:password@host:port/database
    const url = new URL(connectionUri);
    const host = url.hostname;
    const port = url.port || '5432';
    const database = url.pathname.slice(1); // Remove leading /
    const username = url.username;
    const password = url.password;
    const sslMode = 'disable';

    console.log('📝 Parsed connection details:');
    console.log(`   Host: ${host}`);
    console.log(`   Port: ${port}`);
    console.log(`   Database: ${database}`);
    console.log(`   Username: ${username}`);

    // Set env vars for child processes spawned from globalSetup (e.g. migrations).
    // Note: the webServer (dotnet backend) starts BEFORE globalSetup, so it cannot
    // use these — its DB config is passed as command-line args in playwright.config.ts.
    process.env.DATABASE__HOST = host;
    process.env.DATABASE__PORT = port;
    process.env.DATABASE__NAME = database;
    process.env.DATABASE__USERNAME = username;
    process.env.DATABASE__PASSWORD = password;
    process.env.DATABASE__SSLMODE = sslMode;
    process.env.DATABASE_URL = connectionUri;
    process.env.DATABASE_CONTAINER_ID = postgresContainer.getId();

    writeRuntimeState(postgresContainer.getId());

    console.log('✅ Environment variables set for child processes');
    console.log(
      '   DATABASE__HOST, DATABASE__PORT, DATABASE__NAME, DATABASE__USERNAME, DATABASE__PASSWORD, DATABASE__SSLMODE',
    );

    await runDatabaseMigrations();
    runBaselineSeed(postgresContainer.getId(), username, password, database);
  } catch (error) {
    console.error('❌ Failed to start Postgres container:', error);
    throw error;
  }
};

/**
 * Global teardown runs after all tests complete.
 * Playwright automatically calls this, but we can define custom cleanup if needed.
 *
 * In this case, Testcontainers handles cleanup automatically when the container
 * goes out of scope, so we don't need to manually stop it.
 */
export async function globalTeardown() {
  console.log(
    '🛑 Test run complete. Container will be cleaned up by Testcontainers.',
  );
}
