import { FullConfig } from '@playwright/test';
import {
  PostgreSqlContainer,
  StartedPostgreSqlContainer,
} from '@testcontainers/postgresql';
import { spawn, spawnSync } from 'node:child_process';
import {
  existsSync,
  mkdirSync,
  readFileSync,
  rmSync,
  writeFileSync,
} from 'node:fs';
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
const financialArtifactPath = resolve(
  currentDirectory,
  'e2e/seed/financial.export',
);

// API base URL must match the webServer port configured in playwright.config.ts.
const apiBaseUrl = 'http://127.0.0.1:5242';

// Canonical admin credentials matching baseline.sql seed data.
const adminCredentials = {
  username: 'e2e_admin',
  password: 'E2E_admin-password',
};

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

async function runFinancialImportAndRenewal(): Promise<void> {
  console.log('📦 Importing financial artifact and running renewal/accrual...');

  // Step 1: Login to get an access token for subsequent API calls.
  const loginResponse = await fetch(`${apiBaseUrl}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(adminCredentials),
  });

  if (!loginResponse.ok) {
    const body = await loginResponse.text();
    throw new Error(
      `Login failed during financial import setup: ${loginResponse.status} ${body}`,
    );
  }

  const loginBody = (await loginResponse.json()) as {
    status: string;
    accessToken?: string;
  };

  if (!loginBody.accessToken) {
    throw new Error(
      `Login did not return an access token. Status: ${loginBody.status}`,
    );
  }

  const accessToken = loginBody.accessToken;
  const authHeaders = { Authorization: `Bearer ${accessToken}` };

  // Step 2: Import the financial artifact via multipart form upload.
  console.log('   Importing financial artifact...');

  const fileBuffer = readFileSync(financialArtifactPath);
  const formData = new FormData();
  formData.append('File', new Blob([fileBuffer]), 'financial.export');

  const importResponse = await fetch(`${apiBaseUrl}/api/maintenance/import`, {
    method: 'POST',
    headers: authHeaders,
    body: formData,
  });

  if (!importResponse.ok) {
    const body = await importResponse.text();
    throw new Error(
      `Financial artifact import failed: ${importResponse.status} ${body}`,
    );
  }

  const importBody = (await importResponse.json()) as { imported: number };
  console.log(`   Imported ${importBody.imported} records`);

  // Compute today's local date once for all renewal operations (format: YYYY-MM-DD).
  const now = new Date();
  const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;

  // Step 3: Fetch all accounts and accrue expenses.
  const accountsResponse = await fetch(`${apiBaseUrl}/api/accounts`, {
    headers: authHeaders,
  });

  if (!accountsResponse.ok) {
    const body = await accountsResponse.text();
    throw new Error(
      `Failed to fetch accounts after import: ${accountsResponse.status} ${body}`,
    );
  }

  const accounts = (await accountsResponse.json()) as Array<{ rowId: string }>;
  const accountRowIds = accounts.map(account => account.rowId);

  console.log(`   Found ${accountRowIds.length} account(s)`);

  if (accountRowIds.length > 0) {
    const accrueResponse = await fetch(
      `${apiBaseUrl}/api/accruals/accrue-expenses`,
      {
        method: 'POST',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({ rowIds: accountRowIds }),
      },
    );

    if (!accrueResponse.ok) {
      const body = await accrueResponse.text();
      throw new Error(
        `Accrue expenses failed: ${accrueResponse.status} ${body}`,
      );
    }

    console.log('   Accrued expenses for all accounts');
  }

  // Step 4: Fetch all incomes and run renewal.
  const incomesResponse = await fetch(`${apiBaseUrl}/api/incomes`, {
    headers: authHeaders,
  });

  if (!incomesResponse.ok) {
    const body = await incomesResponse.text();
    throw new Error(
      `Failed to fetch incomes after import: ${incomesResponse.status} ${body}`,
    );
  }

  const incomes = (await incomesResponse.json()) as Array<{ rowId: string }>;
  const incomeRowIds = incomes.map(income => income.rowId);

  if (incomeRowIds.length > 0) {
    const renewIncomesResponse = await fetch(
      `${apiBaseUrl}/api/incomes/renew`,
      {
        method: 'POST',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          rowIds: incomeRowIds,
          asOfDate: today,
          mode: 'Overdue',
        }),
      },
    );

    if (!renewIncomesResponse.ok) {
      const body = await renewIncomesResponse.text();
      throw new Error(
        `Income renewal failed: ${renewIncomesResponse.status} ${body}`,
      );
    }

    console.log(`   Renewed ${incomeRowIds.length} income(s)`);
  }

  // Step 5: Fetch all expenses and run renewal.
  const expensesResponse = await fetch(`${apiBaseUrl}/api/expenses`, {
    headers: authHeaders,
  });

  if (!expensesResponse.ok) {
    const body = await expensesResponse.text();
    throw new Error(
      `Failed to fetch expenses after import: ${expensesResponse.status} ${body}`,
    );
  }

  const expenses = (await expensesResponse.json()) as Array<{ rowId: string }>;
  const expenseRowIds = expenses.map(expense => expense.rowId);

  if (expenseRowIds.length > 0) {
    const renewExpensesResponse = await fetch(
      `${apiBaseUrl}/api/expenses/renew`,
      {
        method: 'POST',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          rowIds: expenseRowIds,
          asOfDate: today,
          mode: 'Overdue',
        }),
      },
    );

    if (!renewExpensesResponse.ok) {
      const body = await renewExpensesResponse.text();
      throw new Error(
        `Expense renewal failed: ${renewExpensesResponse.status} ${body}`,
      );
    }

    console.log(`   Renewed ${expenseRowIds.length} expense(s)`);
  }

  console.log('✅ Financial import and renewal/accrual completed');
}

async function waitForDatabaseReady(): Promise<void> {
  console.log('⏳ Waiting for database to become ready...');

  while (true) {
    try {
      const response = await fetch(`${apiBaseUrl}/_health/ready`);

      if (response.ok) {
        console.log('✅ Database is ready');
        return;
      }

      console.log(
        `   /_health/ready returned ${response.status} — retrying in 2 s...`,
      );
    } catch (error) {
      // Server temporarily unreachable during Docker network disruption — keep polling.
      const message = error instanceof Error ? error.message : String(error);
      console.log(`   Server unreachable (${message}) — retrying in 2 s...`);
    }

    await new Promise(resolve => setTimeout(resolve, 2000));
  }
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

    // Remove any stale container left by an interrupted previous run.
    //
    // When a run is hard-killed (SIGKILL, crash, forced IDE stop), neither globalTeardown
    // nor the Testcontainers Ryuk reaper has a chance to clean up. The container keeps
    // running and holds port 55432, causing the next run to fail with "port already allocated".
    //
    // We handle this by checking the runtime state file written during the previous
    // globalSetup. If it still exists here, that run never cleaned up. We force-remove
    // the stale container now, before binding port 55432 for the new run.
    //
    // `docker rm -f` is used (not `docker stop` + `docker rm`) to avoid Docker Desktop
    // on Windows holding the port reservation in TIME_WAIT after a graceful stop.
    const staleContainerId = existsSync(runtimeStatePath)
      ? (
          JSON.parse(readFileSync(runtimeStatePath, 'utf-8')) as {
            databaseContainerId?: string;
          }
        ).databaseContainerId
      : undefined;

    if (staleContainerId) {
      console.log(
        `🧹 Stale container from previous run detected: ${staleContainerId}`,
      );

      const removeResult = spawnSync('docker', ['rm', '-f', staleContainerId], {
        encoding: 'utf-8',
      });

      if (removeResult.status === 0) {
        // Docker Desktop on Windows needs a moment after force-removal before the port
        // binding is fully released. Without this delay the next container start can
        // still see the port as allocated.
        console.log(
          '   Removed. Waiting 3 s for Docker Desktop to release port 55432...',
        );
        await new Promise(resolve => setTimeout(resolve, 3000));
        console.log('✅ Stale container removed');
      } else {
        console.warn(
          `⚠️  Could not remove stale container (may already be gone): ${removeResult.stderr || removeResult.stdout}`,
        );
      }

      rmSync(runtimeStatePath, { force: true });
    }

    configureDotNetEnvironment();

    // Create and start a PostgreSQL container bound to fixed host port 55432.
    // The webServer (dotnet) starts before globalSetup, so its DB connection args
    // are hardcoded in playwright.config.ts to match this fixed port.
    postgresContainer = await new PostgreSqlContainer('postgres:16')
      .withDatabase('pot_e2e')
      .withUsername('test_user')
      .withPassword('test_pass')
      .withExposedPorts({ container: 5432, host: 55432 })
      .withCopyContentToContainer([
        {
          // Install citext before the container first starts so that when the API server
          // opens its initial connection, Npgsql loads citext into its type cache.
          // Without this, Npgsql caches types from an empty DB (no citext), and login
          // fails later with "citext could not be found in the types loaded by Npgsql".
          content: 'CREATE EXTENSION IF NOT EXISTS citext;\n',
          target: '/docker-entrypoint-initdb.d/01-init-citext.sql',
        },
      ])
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
    await waitForDatabaseReady();

    if (existsSync(financialArtifactPath)) {
      await runFinancialImportAndRenewal();
    } else {
      throw new Error(
        'Financial artifact not found at e2e/seed/financial.export. ' +
          'Create an export from the application and place it at that path before running E2E tests.',
      );
    }
  } catch (error) {
    console.error('❌ Failed to start Postgres container:', error);
    throw error;
  }
};

/**
 * Global teardown runs after all tests complete.
 *
 * We do NOT call `docker stop` here. On Docker Desktop for Windows, stopping a container
 * explicitly during teardown leaves the port in TCP TIME_WAIT. Docker Desktop's proxy layer
 * then reports the port as "already allocated" on the very next test run.
 *
 * Instead we rely on the Testcontainers Ryuk reaper, which removes the container AFTER
 * the Playwright process exits. By the time the user starts another run, the port has been
 * cleanly released.
 *
 * For runs where Ryuk also cannot fire (hard kill / SIGKILL), globalSetup detects the
 * leftover container from the runtime state file at the start of the next run and removes
 * it with `docker rm -f` (see stale cleanup block in the default export above).
 *
 * This function's only job is to delete the runtime state file so a stale container ID
 * written by this run cannot confuse the next run's stale-cleanup logic.
 */
export async function globalTeardown() {
  rmSync(runtimeStatePath, { force: true });
  console.log('✅ Runtime state cleared');
}
