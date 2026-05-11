# POT E2E Testing Infrastructure

**Status**: Level 1 (App Loads) - In Progress  
**Last Updated**: 2026-05-11  
**Phase**: Phase B Infrastructure Baseline

This document explains the E2E testing setup, how it works, and how to troubleshoot issues.

---

## Quick Start

### Using npm Scripts (From `Source/Client/pot-react`)

```bash
npm run e2e                    # Run all tests (headless, default)
npm run e2e:all                # Run the full browser/device matrix
npm run e2e:headed            # Run tests with browser visible (debugging)
npm run e2e:smoke             # Run smoke tests only
npm run e2e:smoke:headed      # Run smoke tests with browser visible
npm run e2e:edge              # Run tests using Microsoft Edge channel
npm run e2e:mobile            # Run mobile emulation (Chrome + Safari profiles)
npm run e2e:ui                # Open Playwright UI mode
npm run e2e:debug             # Step-through debug mode
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
1. npm run e2e
2. Playwright reads playwright.config.ts
3. Finds globalSetup: './playwright.globalSetup.ts'
4. Playwright knows to run globalSetup FIRST
```

### Phase 2: Global Setup (Container Startup)

```
5. playwright.globalSetup.ts executes
6. Creates PostgreSqlContainer("postgres:16")
7. Starts container with:
   - Database: pot_e2e
   - Username: test_user
   - Password: test_pass
8. Gets connection URI: postgresql://test_user:test_pass@localhost:RANDOM_PORT/pot_e2e
9. Parses URI and sets individual environment variables:
   - DATABASE__HOST, DATABASE__PORT, DATABASE__NAME
   - DATABASE__USERNAME, DATABASE__PASSWORD, DATABASE__SSLMODE
10. Forces .NET host environment to Production:
   - DOTNET_ENVIRONMENT=Production
   - ASPNETCORE_ENVIRONMENT=Production
11. Runs Pot.Data.Migrations with `dotnet run --no-launch-profile`
    against the Testcontainers database
12. Returns control to Playwright
```

### Phase 3: Dev Server Startup

```
13. Playwright reads webServer config from playwright.config.ts
14. Starts: npm run dev (your Vite dev server)
15. Dev server reads DATABASE__* environment variables (set by globalSetup)
16. ASP.NET Core configuration maps DATABASE__HOST → Database:Host, etc.
17. Backend connects to the migrated Testcontainers Postgres instance
18. Dev server listens on http://127.0.0.1:5175
```

### Phase 4: Tests Run

```
19. Playwright spawns browser
20. Tests execute (e.g., appSmoke.test.ts)
21. Tests navigate to http://127.0.0.1:5175 (hits dev server)
22. Dev server queries test database
23. App renders and responds to test interactions
```

### Phase 5: Cleanup

```
24. Tests complete
25. playwright.globalTeardown runs
26. Container stops (Testcontainers auto-cleanup)
27. Temporary database is discarded
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
webServer: {
  command: 'npm run dev',
  port: 5175,
  reuseExistingServer: !process.env.CI,
  timeout: 120000,
}
```

- `command`: Starts the dev server
- `port`: Dev server must listen on 5175
- `reuseExistingServer`: In local mode, reuse if already running (no restart)
- `timeout`: Wait up to 2 minutes for dev server to start

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
process.env.DATABASE__SSLMODE = 'Disable';

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

```
Testcontainers Postgres
   ↓ (provides: postgresql://test_user:test_pass@localhost:XXXXX/pot_e2e)
playwright.globalSetup.ts (parses URI)
   ↓ (sets: DATABASE__HOST, DATABASE__PORT, etc.)
Environment Variables
   ↓ (read by .NET Core configuration: Database:Host, Database:Port, etc.)
ASP.NET Core DatabaseConfiguration
   ↓ (builds connection string)
Backend Server (connects to test DB)
   ↓
Dev Server on http://127.0.0.1:5175
```

### Why This Works

- ✅ No changes to backend code needed
- ✅ Backend's existing configuration reads the environment variables
- ✅ .NET Core automatically maps `DATABASE__HOST` → `Database:Host` (double underscore becomes colon)
- ✅ The backend validates all required fields are present
- ✅ Connection string is built by the backend using the same logic as production

### Current State

✅ Testcontainers Postgres starts  
✅ `DATABASE__*` environment variables are set  
✅ `Pot.Data.Migrations` runs against the test database  
✅ Database schema is created before server startup  
✅ Individual environment variables set  
✅ Dev server reads them and connects

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
│   │   └── appLoads.test.ts          # Level 1: app renders
│   ├── core/                         # (future)
│   ├── auth/                         # (future)
│   └── concurrency/                  # (future)
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

**Cause**: Environment variables not set correctly, or backend didn't pick them up  
**Fix**:

1. Verify global setup ran and logged the environment variables (should see `✅ Environment variables set for backend`)
2. Check that variables are in the correct format: `DATABASE__HOST` (double underscore, uppercase)
3. Look for errors in the dev server output mentioning missing Database configuration
4. Check that all six DATABASE\_\_\* variables are set (HOST, PORT, NAME, USERNAME, PASSWORD, SSLMODE)

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

| Variable             | Value                         | Set By                      | Read By              |
| -------------------- | ----------------------------- | --------------------------- | -------------------- |
| `DATABASE__HOST`     | `localhost` or container host | `playwright.globalSetup.ts` | Dev Server (ASP.NET) |
| `DATABASE__PORT`     | Dynamic port (e.g., `55432`)  | `playwright.globalSetup.ts` | Dev Server (ASP.NET) |
| `DATABASE__NAME`     | `pot_e2e`                     | `playwright.globalSetup.ts` | Dev Server (ASP.NET) |
| `DATABASE__USERNAME` | `test_user`                   | `playwright.globalSetup.ts` | Dev Server (ASP.NET) |
| `DATABASE__PASSWORD` | `test_pass`                   | `playwright.globalSetup.ts` | Dev Server (ASP.NET) |
| `DATABASE__SSLMODE`  | `Disable`                     | `playwright.globalSetup.ts` | Dev Server (ASP.NET) |
| `NODE_ENV`           | (depends on config)           | Playwright                  | Dev Server / Vite    |
| `CI`                 | `undefined` (local) / `true`  | Environment                 | Playwright           |

### How to Debug Environment Variables

Add logging to `playwright.globalSetup.ts`:

```typescript
console.log('✅ Environment variables set for backend');
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
  .withEnv('DATABASE__SSLMODE', 'Disable')
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

**Last Updated**: 2026-05-11  
**Author**: Implementation Phase B  
**Status**: Living Document - Updates as we progress
