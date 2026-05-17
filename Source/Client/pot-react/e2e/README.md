# POT E2E Testing Infrastructure

**Status**: Phase A/B Planning - Infrastructure not yet complete  
**Last Updated**: 2026-05-16  
**Phase**: Phase A (Infrastructure Baseline) → Phase B (Seeded Execution Ready)
**Readiness**: Pre-Gate A (Pending: seed data strategy, auth fixtures, database modes)

This document explains the E2E testing setup, how it works, and how to troubleshoot issues.

---

## Quick Start

### Using npm Scripts (From `Source/Client/pot-react`)

```bash
npm run e2e                    # Alias of e2e:dev (headless default)
npm run e2e:all                # Alias of e2e:all:dev (full matrix default)
npm run e2e:dev                # Run all tests against dev-mode UI server
npm run e2e:all:dev            # Run full browser/device matrix in dev mode
npm run e2e:prodlike           # Run all tests against production-like UI build + preview
npm run e2e:all:prodlike       # Run full browser/device matrix in production-like mode
npm run e2e:headed             # Run tests with browser visible (debugging)
npm run e2e:smoke              # Run smoke tests only
npm run e2e:smoke:headed       # Run smoke tests with browser visible
npm run e2e:chromium           # Run tests using Chromium only
npm run e2e:edge               # Run tests using Microsoft Edge channel
npm run e2e:mobile             # Run mobile emulation (Chrome + Safari profiles)
npm run e2e:ui                 # Open Playwright UI mode
npm run e2e:debug              # Step-through debug mode
npm run e2e:report             # Open the last HTML test report
npm run e2e:install            # Install required Playwright browsers (chromium, webkit)
```

**Database Modes** (PENDING: npm script setup):

```bash
npm run e2e:blank             # Blank database (no site, no users, no financial data)
npm run e2e:seeded-users      # Site + canonical users, no financial data
npm run e2e:fully-seeded      # Site + users + financial data (imported + renewal run)
```

---

## Database Modes

E2E tests use one of three database setup modes, depending on what they're testing:

### Mode 1: Blank

**What it is**: Migrations run, database schema created, no data.

**Use for**:

- Signup flow tests (create users from scratch)
- Password reset / OTP tests (if implemented)
- Blank database error handling tests
- Unauthenticated feature tests

**Expected state**:

- `sites` table: empty (COUNT = 0)
- `users` table: empty (COUNT = 0)
- No auth required; tests interact with public pages

### Mode 2: Site-and-Users-Only

**What it is**: Migrations run, SQL seed file loaded (site + canonical users created).

**Use for**:

- Login validation tests
- Auth fixture tests
- Feature tests that create their own financial data
- Tests that need authenticated user context but don't require historical data

**Expected state**:

- `sites` table: 1 row (`pot-e2e-site`)
- `users` table: 2 rows (`e2e_admin`, `e2e_viewer`)
- `expenses` / `income` tables: empty (COUNT = 0)
- Auth fixture runs automatically; tests start authenticated

**Canonical Users**:

- **Admin**: username `e2e_admin`, role: Admin, has full mutation access
- **Viewer**: username `e2e_viewer`, role: Viewer, has read-only access

### Mode 3: Fully Seeded

**What it is**: Migrations run, SQL seed file loaded, financial export artifact imported via API, renewal/accrual run to advance data to local date.

**Use for**:

- Projection and forecasting tests
- Report and filter tests (requires existing financial data)
- Multi-user collaboration scenarios
- Dashboard and analytics tests
- Data reconciliation tests

**Expected state**:

- Site and users present (same as Mode 2)
- `expenses` / `income` tables: populated with test data
- Projected financial values calculated and available
- All renewal/accrual computations complete

**Setup Timeline**:

1. Migrations create schema
2. SQL seed loads site/users
3. Dev server connects to test database
4. Import API called to load financial artifact
5. Renewal/accrual API called to advance data to today
6. Tests execute with full financial state ready

---

## Test Isolation and Concurrency Model

Understanding this model is essential before writing any new test. A single E2E run uses
exactly **one API process** and **one Testcontainers database** — both shared across all
Playwright workers. Any test that writes to the database changes state that every other
concurrently-running test can observe.

### Infrastructure Constraints

| Resource      | Value                                 | Shared by                                          |
| ------------- | ------------------------------------- | -------------------------------------------------- |
| API process   | `http://127.0.0.1:5242`               | All workers                                        |
| Database      | Testcontainers Postgres, port `55432` | All workers                                        |
| Baseline seed | `e2e/seed/baseline.sql` (Mode 2)      | Loaded once in `globalSetup`, before any test runs |

There are **no per-test isolated containers**. Tests achieve isolation through serialisation
and fixture teardown, not separate infrastructure.

### Worker Configuration

| Setting         | Local              | CI     |
| --------------- | ------------------ | ------ |
| `fullyParallel` | `true`             | `true` |
| `workers`       | auto (½ CPU count) | `1`    |
| `retries`       | `0`                | `2`    |

With `fullyParallel: true` and multiple local workers, tests in **different files** and
**within the same file** can run simultaneously. CI forces `workers: 1`, serialising
everything — which hides parallelism bugs locally. Always classify tests for parallel
safety; never rely on CI serialisation to cover a classification mistake.

### Test Safety Classification

Every test must be one of three classes before it is written:

| Class                 | DB Writes?                                                                                                 | Requirement                                                                                                           |
| --------------------- | ---------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| **Parallel-safe**     | No (GET-only or UI-only)                                                                                   | Runs freely with all workers in parallel                                                                              |
| **Fixture-managed**   | Yes — creates / updates / deletes its own records only                                                     | `test.describe.serial()` block; `afterEach` teardown cleans up every run, including on failure                        |
| **Isolated sequence** | Yes — triggers import, accrual, renewal, or any operation that changes aggregate / calculated global state | Dedicated npm script or separate Playwright project; `workers: 1`; `applyDatabaseSeedMode()` resets DB in `beforeAll` |

### Parallel-Safe Tests

Parallel-safe tests only read data. They are safe to run concurrently against the shared
infrastructure and make up the majority of tests.

All of the following must be true:

- Only calls GET endpoints, or interacts with UI without triggering server-side state changes.
- Asserts against seed data or data created within the same test — never against data that
  a concurrent sibling test could also create or delete.
- Does not assert the absence of records that a sibling test may create.

Smoke tests and read-only view, filter, and navigation tests fall into this class.

### Fixture-Managed Tests (Mutation with Cleanup)

Tests that create, update, or delete their own records must:

1. Use `test.describe.serial()` so steps within the suite are never interleaved with other workers.
2. Clean up in `afterEach` — not `afterAll` — so cleanup runs even when a step fails.

```typescript
import { test, expect } from '../fixtures/auth';

test.describe.serial('expense CRUD', () => {
  let createdExpenseId: string;

  test.afterEach(async ({ request }) => {
    if (createdExpenseId) {
      await request.delete(`/api/expenses/${createdExpenseId}`);
      createdExpenseId = '';
    }
  });

  test('creates an expense', async ({ page, request }) => {
    // ... create the expense, capture the returned ID into createdExpenseId
  });

  test('edits the expense', async ({ page }) => {
    // ... uses createdExpenseId set by the previous step
  });
});
```

Parallel-safe tests running concurrently are unaffected because they do not query the
created records. The teardown guarantees the baseline is restored regardless of pass/fail.

### Isolated Sequence Tests (Long-Running / Global State)

Tests that trigger **import**, **accrual**, **renewal**, or any operation that modifies
aggregate financial state (projection totals, accrual history, renewal state) cannot share
infrastructure with any other test. These operations:

- Take seconds to minutes to complete.
- Produce side effects immediately visible to all workers.
- Require a known starting state that a concurrent test could invalidate.

"Their own api/db" means a **dedicated test run** — a separate Playwright invocation that
uses the shared infrastructure exclusively, with no other tests competing for it.

Strategy:

1. Place them in `tests/sequences/`.
2. Run via a dedicated npm script (e.g. `e2e:sequences`) with `--project=chromium` and a
   serialised worker model.
3. Call `applyDatabaseSeedMode()` (from `e2e/helpers/databaseSeed.ts`) in `beforeAll` to
   reset the database to the required mode before the sequence begins.
4. Never run sequence tests in the same Playwright invocation as parallel-safe tests.

```typescript
import { test } from '@playwright/test';
import { applyDatabaseSeedMode } from '../helpers/databaseSeed';

test.describe.serial('import and projection flow', () => {
  test.beforeAll(async () => {
    await applyDatabaseSeedMode('seeded-users');
  });

  test('imports financial data', async ({ page }) => {
    /* ... */
  });
  test('runs accrual', async ({ page }) => {
    /* ... */
  });
  test('verifies projection totals', async ({ page }) => {
    /* ... */
  });
});
```

### Decision Guide for New Tests

Ask in order:

1. **Does this test call any POST, PUT, PATCH, or DELETE endpoint, or trigger any action
   that writes to the database?**

   > **Exception**: Authentication calls (e.g. POST `/api/auth/login`) that create a
   > transient session with no effect on data other tests observe are treated as
   > parallel-safe provided the browser context is closed immediately after the assertion.

   - No (or login-only POST with immediate context teardown) → **Parallel-safe**
   - Yes → go to 2

2. **Does this test call import, accrual, renewal, or any operation that changes aggregate
   or calculated global data (projections, totals, accrual state)?**
   - Yes → **Isolated sequence** (dedicated npm script, `workers: 1`, `applyDatabaseSeedMode()` in `beforeAll`)
   - No → **Fixture-managed** (create own records, delete in `afterEach` teardown)

---

## Test Organization and Categories

### Smoke Tests (Level 1, 2, 3)

Located in `tests/smoke/`:

| File                          | Mode           | Purpose                                  | Status               |
| ----------------------------- | -------------- | ---------------------------------------- | -------------------- |
| `appSmoke.test.ts`            | Blank          | Verify app renders with fresh schema     | ✅ Complete          |
| `baselineUsersImport.test.ts` | Site-and-Users | Verify seeded users exist and can log in | ✅ Complete          |
| `seedData.test.ts`            | Fully-Seeded   | Verify financial data displays           | ⏳ PENDING (Phase B) |

**How to run**:

```bash
npm run e2e:smoke              # Run all smoke tests (default mode)
npm run e2e:smoke:headed       # Run with browser visible
```

### Scenario Tests (PRD 011 - Account Filter)

Located in `tests/scenarios/` (future):

- Account filter persistence tests
- Sidebar reactivity tests
- URL query string contracts
- Mobile/responsive tests

**Status**: Blocked until Gate A passes (see Phase 8 in TODO).

### Auth Tests

Located in `tests/auth/` (future):

- Login flow validation
- Token refresh and expiry
- OTP-based signup (when OTP sink integrated)
- Permission enforcement

### Integration Tests

Located in `tests/integration/` (future):

- Multi-step workflows combining features
- Cross-user collaboration scenarios
- API integration with UI correctness

---

## How to Add a New Test

**Quick Checklist**:

1. **Decide your test category**: Smoke? Scenario? Auth? Integration?

2. **Decide your database mode**:

   - Blank: You need empty database or you're testing signup/errors
   - Site-and-Users: You need auth and will create your own financial data
   - Fully-Seeded: You need realistic historical data

3. **Create your test file**:

   ```bash
   touch Source/Client/pot-react/e2e/tests/{category}/{testName}.test.ts
   ```

4. **Import from fixtures** (NOT from `@playwright/test`):

   ```typescript
   import { test, expect } from '../fixtures/auth';
   ```

5. **Write your test**:

   ```typescript
   test('my feature works', async ({ page }) => {
     // Auth fixture runs automatically; tests start authenticated if mode supports it
     await page.goto('/dashboard');
     await expect(page.locator('#siteName')).toContainText('POT E2E Site');
   });
   ```

6. **Validate API responses** (not hardcoded values):

   ```typescript
   // GOOD: Derive expected values from API
   const responsePromise = page.waitForResponse('**/api/projections');
   await page.goto('/projections');
   const response = await responsePromise;
   const projections = await response.json();
   await expect(page.locator('[data-total]')).toContainText(projections.total);

   // BAD: Hardcoded expected value
   // await expect(page.locator('[data-total]')).toContainText('$12,345.67');
   ```

7. **Test locally**:

   ```bash
   npm run e2e:headed  # Run with browser visible
   ```

8. **Commit and let CI verify**:
   ```bash
   git add tests/
   git commit -m "Add test for my feature"
   git push
   ```

### Common Patterns

**Pattern 1: Read-only test (assumes auth)**

```typescript
test('can view dashboard', async ({ page }) => {
  await page.goto('/dashboard');
  await expect(page.locator('h1')).toContainText('Dashboard');
});
```

**Pattern 2: Create data and verify**

```typescript
test('can create expense', async ({ page, request }) => {
  await page.goto('/expenses/new');
  await page.fill('[name="amount"]', '50.00');
  await page.fill('[name="description"]', 'Lunch');
  await page.click('button[type="submit"]');

  // Verify API returned the created expense
  const response = await request.get('/api/expenses?limit=1');
  const expenses = await response.json();
  await expect(expenses[0].description).toBe('Lunch');
});
```

**Pattern 3: Long-flow test**

```typescript
test('can import and view financial data', async ({ page, request }) => {
  // Fully-seeded mode has financial data ready
  await page.goto('/dashboard');

  // Navigate through flow
  await page.click('[href="/projections"]');
  await expect(page.locator('[data-projection]')).toBeVisible();

  // Verify calculations
  const response = await request.get('/api/projections');
  const data = await response.json();
  await expect(page.locator('[data-total]')).toContainText(data.total);
});
```

---

## Architecture Overview

### The Stack

```
Playwright (@1.59.1)
    ↓
playwright.globalSetup.ts
    ↓
Testcontainers (@testcontainers/postgresql)
    ↓
PostgreSQL 16 Container (disposable)
    ↓
Pot.Data.Migrations (schema creation)
   ↓
POT Dev Server (connects to test DB)
    ↓
E2E Tests (interact with app)
```

### Key Files

| File                               | Purpose                                          |
| ---------------------------------- | ------------------------------------------------ |
| `playwright.config.ts`             | Main Playwright config; references global setup  |
| `playwright.globalSetup.ts`        | Starts Postgres and runs migrations before tests |
| `tests/smoke/appSmoke.test.ts`     | Level 1 smoke test: verify app renders           |
| `../../Server/Pot.Data.Migrations` | Applies EF Core migrations to the test database  |
| `.gitignore` (to update)           | Excludes `.auth/` and `test-results/`            |

---

**Headless** (default):

- Runs in the background
- No browser window
- Fastest execution
- Better for CI and bulk testing

**Headed** (debugging):

- Opens a browser window
- See exactly what the test is doing
- Pause and inspect with Playwright Inspector
- Better for troubleshooting test failures

**Interactive UI Mode** (advanced debugging):

- Run from terminal: `npm run e2e:ui` (from `Source/Client/pot-react/`)
- Opens browser and separate Playwright Inspector UI
- Step through tests line-by-line
- Inspect DOM elements and Playwright selectors
- Time-travel backward/forward through test steps
- Use when: writing complex tests, understanding selector issues, deep debugging
- Not recommended for routine runs (requires interactive display)

---

## Execution Flow (What Happens When You Run Tests)

### Phase 1: Playwright Initialization

```
1. npm run e2e (alias of npm run e2e:dev)
2. Playwright reads playwright.config.ts (dev mode)
   or playwright.prod.config.ts (production-like mode)
3. Finds globalSetup: './playwright.globalSetup.ts'
4. Playwright knows to run globalSetup FIRST
```

### Phase 2: API + UI Startup

```
5. Playwright reads webServer config from the selected Playwright config
6. Starts dedicated API process on http://127.0.0.1:5242
7. API reads DB, JWT, CORS, and rate-limiting config from CLI args
   (hardcoded in playwright.config.ts to fixed port 55432)
8. Starts UI process on http://127.0.0.1:5175
9. In dev mode: npm run dev -- --host 127.0.0.1 --strictPort
10. In production-like mode: npm run build && npm run preview -- --host 127.0.0.1 --strictPort
```

### Phase 3: Global Setup (Container Startup)

```
11. playwright.globalSetup.ts executes
12. Creates PostgreSqlContainer("postgres:16")
13. Starts container with fixed host port 55432:
    - Database: pot_e2e
    - Username: test_user
    - Password: test_pass
14. Gets connection URI: postgresql://test_user:test_pass@localhost:55432/pot_e2e
15. Parses URI and sets environment variables for child processes (migration runner):
    - DATABASE__HOST, DATABASE__PORT, DATABASE__NAME
    - DATABASE__USERNAME, DATABASE__PASSWORD, DATABASE__SSLMODE
16. Forces .NET host environment to Production (for migration runner):
    - DOTNET_ENVIRONMENT=Production
    - ASPNETCORE_ENVIRONMENT=Production
17. Runs Pot.Data.Migrations with `dotnet run --no-launch-profile`
    against the Testcontainers database
18. Runs baseline seed from e2e/seed/baseline.sql
19. Returns control to Playwright
```

> **Note**: webServers start before globalSetup in Playwright's execution model. The API
> cannot use env vars set in globalSetup because it is already running by then. All API
> config (DB, JWT, CORS, rate-limiting) is passed as CLI args in `playwright.config.ts`.
> The env vars set in globalSetup are only consumed by child processes it spawns
> (migration runner).

### Phase 4: Tests Run

```
21. Playwright spawns browser
22. Tests execute (e.g., appSmoke.test.ts)
23. Tests navigate to http://127.0.0.1:5175 (hits UI server)
24. UI proxies /api requests to dedicated API at http://127.0.0.1:5242
25. App renders and responds to test interactions
```

### Phase 5: Cleanup

```
26. Tests complete
27. playwright.globalTeardown runs
28. Container stops (Testcontainers auto-cleanup)
29. Temporary database is discarded
```

---

## Configuration Details

### playwright.config.ts

```typescript
globalSetup: './playwright.globalSetup.ts',
```

- Tells Playwright to run the global setup file before tests
- This is where container startup happens
- Uses a string path (Playwright handles resolution in ESM context)

```typescript
webServer: [
  {
    command:
      'dotnet run --no-launch-profile --project Pot.AspNetCore/Pot.AspNetCore.csproj -- --urls http://127.0.0.1:5242 --Cors:AllowedOrigins=http://127.0.0.1:5175,http://localhost:5175 --Jwt:Issuer=pot-e2e --Jwt:Audience=pot-e2e-client --Jwt:SecretKey=F4B58D0C1Z2B3E4D5F6A7B8C9D0E1F2A3B4C536E7F8A194A0EF29B0C1D2E3F4A5B6C7D8E9F0A1B2CWD4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C --Database:Host=localhost --Database:Port=55432 --Database:Name=pot_e2e --Database:Username=test_user --Database:Password=test_pass --Database:SSLMode=disable --RateLimiting:Anonymous:PermitLimit=200 --RateLimiting:Anonymous:WindowSeconds=10 --RateLimiting:Authenticated:PermitLimit=500 --RateLimiting:Authenticated:WindowSeconds=30',
    cwd: '../../Server',
    port: 5242,
    reuseExistingServer: false,
    timeout: 120000,
    stdout: 'pipe',
    stderr: 'pipe',
  },
  {
    command: 'npm run dev -- --host 127.0.0.1 --strictPort',
    port: 5175,
    reuseExistingServer: false,
    timeout: 120000,
  },
];
```

- First entry starts dedicated API for tests
- Second entry starts UI server for tests
- `reuseExistingServer: false` ensures tests do not attach to unrelated running services
- `timeout`: Wait up to 2 minutes for each server startup

### playwright.prod.config.ts

```typescript
webServer: [
  {
    command:
      'dotnet run --no-launch-profile --project Pot.AspNetCore/Pot.AspNetCore.csproj -- --urls http://127.0.0.1:5242 --Cors:AllowedOrigins=http://127.0.0.1:5175,http://localhost:5175 --Jwt:Issuer=pot-e2e --Jwt:Audience=pot-e2e-client --Jwt:SecretKey=F4B58D0C1Z2B3E4D5F6A7B8C9D0E1F2A3B4C536E7F8A194A0EF29B0C1D2E3F4A5B6C7D8E9F0A1B2CWD4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C --Database:Host=localhost --Database:Port=55432 --Database:Name=pot_e2e --Database:Username=test_user --Database:Password=test_pass --Database:SSLMode=disable --RateLimiting:Anonymous:PermitLimit=200 --RateLimiting:Anonymous:WindowSeconds=10 --RateLimiting:Authenticated:PermitLimit=500 --RateLimiting:Authenticated:WindowSeconds=30',
    cwd: '../../Server',
    port: 5242,
    reuseExistingServer: false,
    timeout: 120000,
    stdout: 'pipe',
    stderr: 'pipe',
  },
  {
    command:
      'npm run build && npm run preview -- --host 127.0.0.1 --strictPort',
    port: 5175,
    reuseExistingServer: false,
    timeout: 180000,
  },
];
```

- Uses production-like UI artifact serving for higher confidence checks
- Recommended for CI/nightly runs alongside faster dev-mode runs

```typescript
use: {
  baseURL: 'http://127.0.0.1:5175',
  trace: 'on-first-retry',
  screenshot: 'only-on-failure',
  video: 'retain-on-failure',
}
```

- `baseURL`: Tests use this as the base URL for navigation
- `trace`: Save Playwright trace on first retry (helps debugging)
- `screenshot`: Save screenshot if test fails
- `video`: Save video recording if test fails

### playwright.globalSetup.ts

```typescript
const connectionUri = postgresContainer.getConnectionUri();
// Parse URI and set individual env vars
const url = new URL(connectionUri);
process.env.DATABASE__HOST = url.hostname;
process.env.DATABASE__PORT = url.port;
process.env.DATABASE__NAME = url.pathname.substring(1);
process.env.DATABASE__USERNAME = url.username;
process.env.DATABASE__PASSWORD = url.password;
process.env.DATABASE__SSLMODE = 'disable';

process.env.DOTNET_ENVIRONMENT = 'Production';
process.env.ASPNETCORE_ENVIRONMENT = 'Production';

await runDatabaseMigrations();
```

- The global setup **parses** the connection URI from Testcontainers
- **Sets individual environment variables** in .NET Core format
- **Forces both .NET host environments to Production** for E2E
- **Runs the existing `Pot.Data.Migrations` console app** before the server starts
- Uses `--no-launch-profile` so `launchSettings.json` does not affect E2E runs
- Backend reads these and builds its own connection string
- This happens **before** the dev server starts

---

## The Critical Gap: Dev Server Connection

### SOLVED! ✅ Connection Integration

**The Solution:**

1. Testcontainers provides a connection URI: `postgresql://user:pass@host:port/database`
2. `playwright.globalSetup.ts` **parses** this URI into components
3. Sets environment variables in .NET Core format:
   - `DATABASE__HOST` → `host`
   - `DATABASE__PORT` → `port`
   - `DATABASE__NAME` → `database`
   - `DATABASE__USERNAME` → `user`
   - `DATABASE__PASSWORD` → `password`
   - `DATABASE__SSLMODE` → `disable`

**How It Works:**

The API and migration runner each get DB config through separate channels, both targeting fixed port 55432:

```
playwright.config.ts (CLI args: --Database:Port=55432, --Jwt:Issuer=pot-e2e, etc.)
   ↓ (passed when webServer starts — BEFORE globalSetup runs)
Backend API (http://127.0.0.1:5242)
   ↓ (reads CLI args directly as ASP.NET Core configuration)
Connects to Testcontainers Postgres at localhost:55432

playwright.globalSetup.ts (parses container URI → port 55432)
   ↓ (sets: DATABASE__HOST, DATABASE__PORT=55432, DATABASE__NAME, etc.)
Migration runner (Pot.Data.Migrations child process)
   ↓ (reads DATABASE__* env vars; .NET Core maps them to Database:* keys)
Connects to Testcontainers Postgres at localhost:55432
```

### Why This Works

- ✅ No changes to backend code needed
- ✅ Fixed port 55432 ensures both the API and migration runner target the same container
- ✅ API reads config from CLI args (`--Database:Host`, `--Database:Port`, etc.) — no env-var mapping needed
- ✅ Migration runner reads `DATABASE__*` env vars; .NET Core maps them to `Database:*` keys automatically
- ✅ Connection string is built by the backend using the same logic as production

### Current State

✅ Testcontainers Postgres starts on fixed port 55432  
✅ API configured via CLI args in `playwright.config.ts` and connects  
✅ `DATABASE__*` environment variables set for migration runner  
✅ `Pot.Data.Migrations` runs against the test database  
✅ Baseline seed (`e2e/seed/baseline.sql`) loaded before tests run

---

## Seed Data Management

### Overview

E2E tests use seed data to create reproducible baseline state. The seed data file `seed/baseline.sql` contains the database state for the "Site-and-Users-Only" and "Fully-Seeded" modes.

**Current Seed State**:

- **File**: `e2e/seed/baseline.sql` (2,089 bytes)
- **Tables**: `Site`, `User`, `UserRole`
- **Data**: 1 test site (`pot-e2e-site`), 2 test users (`e2e_admin`, `e2e_viewer`)
- **Generated**: 2026-05-16

### When to Regenerate Seed Data

Regenerate `baseline.sql` when:

1. **Canonical identities change**: usernames, site key, role assignments
2. **Schema changes**: New non-nullable columns added to `Site`, `User`, or `UserRole` tables without defaults
3. **Role structure changes**: Admin/Viewer role definitions or permissions change

Do **NOT** regenerate just because:

- Financial data changes (financial artifacts are separate)
- Test data is stale (use fully-seeded mode with fresh artifact import)

### Regenerating Seed Data

**Prerequisites**:

- **PowerShell 7 or later (`pwsh`)**
  - This script uses PowerShell 7-only syntax features (try/catch with argument arrays, etc.)
  - If you have pwsh installed but your terminal is using the old Windows PowerShell, run `pwsh` first to switch shells

**Quick Path (Using Script)**:

Instead of running manual commands, use the provided PowerShell 7 script from `Source/Client/pot-react/e2e/scripts/`:

```bash
# If you're in Windows PowerShell, launch pwsh first
pwsh ./test-e2e-seed-export.ps1

# Or, if you're already in a pwsh shell
./test-e2e-seed-export.ps1
```

The script handles all steps below, including validation and error checking.

---

**Manual Path**:

**Prerequisites**:

- Docker Desktop running with POT dev stack (run `docker-start-client-server` task)
- PostgreSQL client tools available (or use `docker exec`)

**Steps**:

1. **Ensure the E2E Docker database has the canonical test users**:

   - Start the E2E Docker postgres container: `.\Start-E2EPot.ps1` (from `Data/E2E Docker/`)
   - Start your local client/server applications — they will automatically connect to the E2E postgres on `localhost:5432`
   - Create or verify these users exist:
     - User: `e2e_admin` (email: `e2e_admin@local.test`, role: Admin)
     - User: `e2e_viewer` (email: `e2e_viewer@local.test`, role: Viewer)

2. **Export the seed data**:

   ```bash
   # From workspace root
   mkdir -p Source/Client/pot-react/e2e/seed

   # Export only the canonical data (no financial artifacts)
   docker exec -e PGPASSWORD=password pot-e2e-postgres pg_dump \
     --data-only \
     -t '"Site"' \
     -t '"User"' \
     -t '"UserRole"' \
     -U postgres \
     -d Pot \
     > Source/Client/pot-react/e2e/seed/baseline.sql
   ```

3. **Verify the export**:

   ```bash
   # Check file size and contents
   wc -l Source/Client/pot-react/e2e/seed/baseline.sql
   head -20 Source/Client/pot-react/e2e/seed/baseline.sql  # Should show SQL
   ```

4. **Test the export** (optional but recommended):

   ```bash
   # Create a temporary test database and load the seed to verify it works
   # This validates that the seed file is syntactically correct
   ```

5. **Commit the updated seed file**:

   ```bash
   git add Source/Client/pot-react/e2e/seed/baseline.sql
   git commit -m "Update E2E seed data (baseline.sql)"
   git push
   ```

### Seed Data File Format

The `baseline.sql` file is a PostgreSQL `pg_dump` data-only export. It contains:

- `COPY` statements for each table
- INSERT statements with sequence values
- No schema definitions (schema is created by migrations)

**Example structure**:

```sql
COPY public."Site" (...) FROM stdin;
...
\.

COPY public."User" (...) FROM stdin;
...
\.

COPY public."UserRole" (...) FROM stdin;
...
\.
```

### Key Points

- **Migrations create the schema first**, then the seed data is loaded on top
- **Seed data is deterministic**: Same seed file produces same users/site every test run
- **No plaintext passwords in seed**: User records contain only bcrypt hashes; plaintext test passwords are hardcoded in test files
- **Role assignments are preserved**: Admin/Viewer role links are exported and reloaded

---

## Previous Content (Archived)

---

## Directory Structure

```
e2e/
├── README.md                          # This file
├── playwright.config.ts               # Playwright configuration
├── playwright.globalSetup.ts          # Container startup orchestrator
├── tests/                             # Test files
│   ├── smoke/
│   │   ├── appSmoke.test.ts           # Level 1: app renders
│   │   └── baselineUsersImport.test.ts # Level 2: seeded users can log in
│   ├── core/                         # (future)
│   ├── auth/                         # (future)
│   └── concurrency/                  # (future)
├── scripts/                          # Utility scripts
│   └── test-e2e-seed-export.ps1     # Export seed data from local Docker DB
├── seed/                             # Seed data files
│   └── baseline.sql                  # Canonical test data (Site, User, UserRole)
├── fixtures/                         # Test fixtures (future)
│   ├── auth.ts                       # Auth fixture
│   ├── db.ts                         # Database fixture
│   └── pom/                          # Page Object Models
├── .auth/                            # Storage states (gitignored)
├── test-results/                     # Test output (gitignored)
└── playwright-report/                # HTML report (gitignored)
```

---

## Common Issues & Troubleshooting

### Issue: "Testcontainers failed to start"

**Cause**: Docker is not running  
**Fix**: Open Docker Desktop and verify containers can start

```bash
docker ps  # Should list containers without error
```

### Issue: "Backend failed to connect to database"

**Cause**: Environment variables not set correctly, or backend didn't pick them up  
**Fix**:

1. Verify global setup ran and logged the environment variables
2. Check that variables are in the format: `DATABASE__HOST` (double underscore)
3. Look for errors in the dev server output mentioning missing Database configuration

### Issue: "Dev server starts but app shows database error"

**Cause**: Backend started but couldn't initialize database schema  
**Fix**:

1. Check if the test database needs migrations
2. This might be expected for Level 1 (blank database has no schema)
3. The app might show an error, but that's OK for testing infrastructure startup

### Debugging Environment Variables

To verify the environment variables are set, add logging to `playwright.globalSetup.ts`:

```typescript
// After setting env vars, add:
console.log('Environment Variables Set:');
console.log('DATABASE__HOST:', process.env.DATABASE__HOST);
console.log('DATABASE__PORT:', process.env.DATABASE__PORT);
console.log('DATABASE__NAME:', process.env.DATABASE__NAME);
console.log('DATABASE__USERNAME:', process.env.DATABASE__USERNAME);
console.log('DATABASE__PASSWORD:', process.env.DATABASE__PASSWORD);
console.log('DATABASE__SSLMODE:', process.env.DATABASE__SSLMODE);
```

Also check the backend startup logs for:

```
Database configured: Host=localhost, Port=XXXXX, Database=pot_e2e
```

### Issue: "Backend failed to connect to database"

**Cause**: The API reads all DB config from CLI args in `playwright.config.ts` (not env vars). The
Testcontainers container must be running on port 55432 before the API starts (it is, because the container
starts in `globalSetup` and the webServer `--Database:Port=55432` arg is hardcoded to match).  
**Fix**:

1. Verify Testcontainers started (look for `✅ Postgres container started` in globalSetup output)
2. Verify migration runner received the DATABASE vars (look for `✅ Environment variables set for child processes`)
3. Check `playwright.config.ts` webServer command includes `--Database:Port=55432`
4. Look for errors in the API process output (piped to test runner logs via `stdout: 'pipe'`)

### Issue: "Test passes locally but fails in CI"

**Cause**: CI environment doesn't have Docker or proper DATABASE\_\_\* setup  
**Fix**: We'll address this in Phase C (CI integration is out of scope for now)

---

## Level 1 Test Details

### Test File

`tests/smoke/appSmoke.test.ts`

**What It Does:**

1. Navigates to `/` (app root)
2. Waits for `#root` element to be visible
3. Logs page title as confirmation

**Why This Test:**

- ✅ Validates Testcontainers started
- ✅ Validates dev server connected to test DB
- ✅ Validates app renders (no server errors)
- ✅ Simplest possible test (easiest to debug)

**Expected Outcome:**

```
✅ app shell loads at root route with blank database
```

---

## Environment Variables

### During Test Execution

| Variable             | Value                        | Set By                      | Read By                                  |
| -------------------- | ---------------------------- | --------------------------- | ---------------------------------------- |
| `DATABASE__HOST`     | `localhost`                  | `playwright.globalSetup.ts` | Migration runner (`Pot.Data.Migrations`) |
| `DATABASE__PORT`     | `55432` (fixed)              | `playwright.globalSetup.ts` | Migration runner (`Pot.Data.Migrations`) |
| `DATABASE__NAME`     | `pot_e2e`                    | `playwright.globalSetup.ts` | Migration runner (`Pot.Data.Migrations`) |
| `DATABASE__USERNAME` | `test_user`                  | `playwright.globalSetup.ts` | Migration runner (`Pot.Data.Migrations`) |
| `DATABASE__PASSWORD` | `test_pass`                  | `playwright.globalSetup.ts` | Migration runner (`Pot.Data.Migrations`) |
| `DATABASE__SSLMODE`  | `disable`                    | `playwright.globalSetup.ts` | Migration runner (`Pot.Data.Migrations`) |
| `NODE_ENV`           | (depends on config)          | Playwright                  | Dev Server / Vite                        |
| `CI`                 | `undefined` (local) / `true` | Environment                 | Playwright                               |

> **Note**: The API (dev server) does **not** read these `DATABASE__*` env vars. It receives
> all DB config via CLI args hardcoded in `playwright.config.ts`. These env vars are only
> consumed by the migration runner child process spawned from `playwright.globalSetup.ts`.

### How to Debug Environment Variables

Add logging to `playwright.globalSetup.ts`:

```typescript
console.log('✅ Environment variables set for migration runner');
console.log('DATABASE__HOST:', process.env.DATABASE__HOST);
console.log('DATABASE__PORT:', process.env.DATABASE__PORT);
console.log('DATABASE__NAME:', process.env.DATABASE__NAME);
console.log('DATABASE__USERNAME:', process.env.DATABASE__USERNAME);
console.log('DATABASE__PASSWORD:', process.env.DATABASE__PASSWORD);
console.log('DATABASE__SSLMODE:', process.env.DATABASE__SSLMODE);
```

---

## How to Build the Server Image for E2E (Phase B: Future)

> **Status**: Planned for Phase B infrastructure expansion
>
> Currently (Phase A/early Phase B), E2E tests run against the dev server (`npm run dev`).
> In a future phase, we'll extend Testcontainers to also spin up the server container for full-stack isolated tests.

The future server image already includes the migrations tool. The Dockerfile publishes both the API and `Pot.Data.Migrations`, then copies them into the image so the entrypoint can run migrations before starting the API.

### Preparing for Full-Stack E2E (Phase B)

When ready, you'll build the server Docker image locally using the VS Code task:

1. Open the Command Palette: `Ctrl+Shift+P`
2. Run `Tasks: Run Task`
3. Select `docker-build-server-e2e`
4. Wait for ~60 seconds for the build to complete

**Manual build (if needed):**

```bash
cd Source
docker build --no-cache -t pot-server:test-e2e -f Docker/Server/Dockerfile .
```

### Full-Stack Orchestration Flow (Phase B)

When implemented, the flow will be:

1. **Run E2E tests**: `npm run e2e`
2. **Playwright starts Testcontainers Postgres**: Disposable, isolated database
3. **Playwright starts Testcontainers server**: Using local `pot-server:test-e2e` image
4. **Environment variables passed to server**:
   - `DATABASE__HOST`, `DATABASE__PORT`, `DATABASE__NAME`
   - `DATABASE__USERNAME`, `DATABASE__PASSWORD`, `DATABASE__SSLMODE`
5. **Browser tests run** against the full-stack
6. **Cleanup**: All containers stopped and removed

### Example: Full-Stack Testcontainers Setup (Phase B Reference)

```typescript
// This is a reference implementation for Phase B
import {
  PostgreSqlContainer,
  StartedPostgreSqlContainer,
} from '@testcontainers/postgresql';
import { GenericContainer, StartedTestContainer, Wait } from 'testcontainers';

let postgres: StartedPostgreSqlContainer;
let server: StartedTestContainer;

// Start Postgres
postgres = await new PostgreSqlContainer('postgres:16')
  .withDatabase('pot_e2e')
  .withUsername('test_user')
  .withPassword('test_pass')
  .start();

// Start server (using local image)
server = await new GenericContainer('pot-server:test-e2e')
  .withEnv('DATABASE__HOST', postgres.getHost())
  .withEnv('DATABASE__PORT', postgres.getMappedPort(5432).toString())
  .withEnv('DATABASE__NAME', 'pot_e2e')
  .withEnv('DATABASE__USERNAME', 'test_user')
  .withEnv('DATABASE__PASSWORD', 'test_pass')
  .withEnv('DATABASE__SSLMODE', 'disable')
  .withExposedPorts(5241)
  .withWaitStrategy(Wait.forLogMessage('Now listening on'))
  .start();

const serverPort = server.getMappedPort(5241);
const serverHost = server.getHost();
const baseURL = `http://${serverHost}:${serverPort}`;
```

---

## VS Code Task: Build Server Image for E2E

**Task Details:**

- **Label:** `docker-build-server-e2e`
- **Working Directory:** `${workspaceFolder}/Source`
- **Command:** `docker build --no-cache -t pot-server:test-e2e -f Docker/Server/Dockerfile .`

**Key Points:**

- `--no-cache` forces a fresh rebuild (ignores Docker layer cache)
- Build context is set to `Source/` (matches docker-compose behavior)
- Image tag `pot-server:test-e2e` is used by Testcontainers in the E2E setup
- First build takes ~60 seconds; subsequent builds with cache would be faster

**When to rebuild:**

- After making code changes to your server
- Before running E2E tests to ensure you're testing the latest code
- As part of your E2E test workflow: rebuild → run tests

---

## Next Action Items

### Immediate (This Session)

- [x] Build server image locally (task created: `docker-build-server-e2e`)
- [x] Discover how dev server reads DATABASE_URL (uses `DATABASE__*` env vars)
- [x] Verify connection string format matches server expectations (.NET Configuration binding)
- [ ] Run Level 1 test and observe startup flow (next step after confirming server image)

### Phase B Completion

- [ ] Create `fixtures/db.ts` (test-scoped database fixture)
- [ ] Create `fixtures/auth.ts` (worker-scoped auth fixture)
- [ ] Move `tests/smoke/appSmoke.test.ts` to proper location
- [ ] Add `.auth/` and `test-results/` to `.gitignore`
- [ ] Update `playwright.config.ts` to reference fixtures

### Phase C (Scenario Tests)

- [ ] Implement Level 2 test: login fails with blank DB
- [ ] Implement Level 3 test: login succeeds with seeded data
- [ ] Implement PRD 011 account filter scenarios

---

## Reference Documentation

- **Playwright Docs**: https://playwright.dev/docs/test-global-setup-teardown
- **Testcontainers Docs**: https://node.testcontainers.org/modules/postgresql/
- **PRD 012**: `Docs/Future/012-playwright-e2e-testing-infrastructure.md`
- **POT Architecture**: `Docs/ARCHITECTURE.md`
- **Local Setup**: `Docs/LOCAL-SETUP.md`

---

## Questions to Explore

1. **How does the dev server currently read connection strings?**

   - Environment variable?
   - Config file?
   - Hardcoded for dev?

2. **Is there an existing way to override DATABASE_URL in dev mode?**

   - This would be the easiest path to integration

3. **Does Vite proxy forwards environment variables to the backend?**

   - Check `vite.config.ts` for environment handling

4. **What happens if DATABASE_URL is not set?**
   - Does server fail gracefully?
   - Does it fall back to a default connection string?

---

**Last Updated**: 2026-05-16  
**Author**: E2E Infrastructure Planning & PRD 012  
**Status**: Living Document - In Active Development (Phase 0-1)

---

## PENDING UPDATES (Next Iteration)

Sections marked **PENDING** need to be filled in once decisions are made. These are tracked in the main TODO checklist.

- **PENDING**: Database mode npm scripts (`npm run e2e:blank`, `npm run e2e:seeded-users`, `npm run e2e:fully-seeded`)

  - Requires: Phase 1 environment setup and playwright.config.ts configuration
  - Tracked in: TODO Phase 4.2.4

- **PENDING**: Level 3 smoke test (`seedData.test.ts`); Level 2 is now complete (`baselineUsersImport.test.ts`)

  - Requires: SQL seed file generation, auth fixtures, database mode orchestration
  - Tracked in: TODO Phase 5.4

- **PENDING**: Exact import/renewal API contracts

  - Required before: Phase 4 implementation
  - Tracked in: TODO "DECISION REQUIRED" section

- **PENDING**: `.github/instructions/playwright-e2e.instructions.md` creation

  - Requires: Phase 7 documentation lock
  - Tracked in: TODO Phase 7.2

- **PENDING**: `e2e/AUTHORING.md` creation
  - Requires: Phase 7 documentation lock
  - Tracked in: TODO Phase 7.3

**How to identify PENDING items**: Search for "PENDING" or "⏳" in this document. Cross-reference with the TODO checklist to see what's blocking each item.

---

## Reference Documentation

### Hierarchical Order (Read in This Order)

1. **This README** (`e2e/README.md`)

   - Quick start, database modes, test organization
   - For: first-time users, quick reference

2. **E2E TODO Checklist** (`Docs/Future/playwright-e2e-todo.md`)

   - Phase-by-phase implementation plan
   - Dependencies and decision points
   - For: developers implementing infrastructure

3. **PRD 012** (`Docs/Future/012-playwright-e2e-testing-infrastructure.md`)

   - Design decisions (D1-D9), requirements (R1-R7), patterns
   - For: understanding rationale and deep dive

4. **Playwright Official Docs**

   - Global setup/teardown: https://playwright.dev/docs/test-global-setup-teardown
   - Fixtures: https://playwright.dev/docs/test-fixtures
   - Authentication: https://playwright.dev/docs/auth
   - For: Playwright-specific API reference

5. **Testcontainers Node Docs**
   - PostgreSQL module: https://node.testcontainers.org/modules/postgresql/
   - For: Container orchestration details

### POT-Specific Documentation (When Created)

- `.github/instructions/playwright-e2e.instructions.md` (PENDING Phase 7)
- `e2e/AUTHORING.md` (PENDING Phase 7)

### External References

- **Playwright Best Practices**: https://playwright.dev/docs/best-practices
- **Testcontainers Best Practices**: https://testcontainers.com/guides/
- **ASP.NET Core Configuration**: https://learn.microsoft.com/en-us/dotnet/core/configuration/

---

## Document Evolution

This README was created as part of Phase A/B E2E infrastructure planning (May 2026). It is a living document that will be updated as:

1. **Phase 1 completes**: Browser/env validation
2. **Phase 2 completes**: Seed data strategy locked
3. **Phase 3 completes**: Auth fixtures implemented
4. **Phase 4 completes**: Database lifecycle tested
5. **Phase 5 completes**: Core helpers released
6. **Phase 6-7 complete**: Documentation finalized

Each phase completion will add concrete examples and resolve PENDING items.

---

**Status**: Phase A/B Planning - Infrastructure not yet built  
**Current Blocker**: Seed data strategy (Phase 2) - See TODO for resolution  
**Next Steps**: Complete Phase 1-2 checklist items, then proceed to Phase 3
