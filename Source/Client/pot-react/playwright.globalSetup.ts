import { FullConfig } from '@playwright/test';
import {
  PostgreSqlContainer,
  StartedPostgreSqlContainer,
} from '@testcontainers/postgresql';
import { spawn } from 'node:child_process';
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
const migrationProjectPath = resolve(
  serverDirectory,
  'Pot.Data.Migrations/Pot.Data.Migrations.csproj',
);

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
  process.env.DOTNET_ENVIRONMENT = 'Production';
  process.env.ASPNETCORE_ENVIRONMENT = 'Production';

  console.log('✅ .NET environment set for E2E');
  console.log('   DOTNET_ENVIRONMENT=Production');
  console.log('   ASPNETCORE_ENVIRONMENT=Production');
}

export default async (config: FullConfig) => {
  console.log('🐘 Starting Testcontainers Postgres...');

  try {
    configureDotNetEnvironment();

    // Create and start a PostgreSQL container
    postgresContainer = await new PostgreSqlContainer('postgres:16')
      .withDatabase('pot_e2e')
      .withUsername('test_user')
      .withPassword('test_pass')
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
    const sslMode = 'disable'; // Testcontainers usually disables SSL

    console.log('📝 Parsed connection details:');
    console.log(`   Host: ${host}`);
    console.log(`   Port: ${port}`);
    console.log(`   Database: ${database}`);
    console.log(`   Username: ${username}`);

    // Set environment variables in the format that ASP.NET Core expects
    // .NET Core configuration uses : as separator, so DATABASE_HOST becomes DATABASE:HOST
    process.env.DATABASE__HOST = host;
    process.env.DATABASE__PORT = port;
    process.env.DATABASE__NAME = database;
    process.env.DATABASE__USERNAME = username;
    process.env.DATABASE__PASSWORD = password;
    process.env.DATABASE__SSLMODE = sslMode;

    // Also store the full URI for reference (tests might use this)
    process.env.DATABASE_URL = connectionUri;

    console.log('✅ Environment variables set for backend');
    console.log(
      '   DATABASE__HOST, DATABASE__PORT, DATABASE__NAME, DATABASE__USERNAME, DATABASE__PASSWORD, DATABASE__SSLMODE',
    );

    await runDatabaseMigrations();
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
