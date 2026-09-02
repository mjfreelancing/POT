# ADR 012 — Playwright E2E Testing Infrastructure: Design & Current State

**Status:** Accepted — implemented and validated (test catalog complete, matrix green)
**Date:** 2026-09-02

## Context

[PRD 012](PRD-012-playwright-e2e-testing-infrastructure.md) defines **what** the E2E testing
infrastructure must deliver — goals, non-goals, requirements, and acceptance criteria. This
ADR records **how** those requirements are met: the implemented architecture and its current
state. Read the PRD for the requirements and their rationale; read this ADR for the concrete
design, operational detail, and design history.

## Companion documents

- [PRD 012 — Playwright E2E Testing Infrastructure](PRD-012-playwright-e2e-testing-infrastructure.md)
  — requirements (what/why).
- `Source/Client/pot-react/e2e/README.md` — day-to-day architecture & conventions reference.
- `Source/Client/pot-react/e2e/AUTHORING.md` — how to add/update a test.
- `.github/instructions/playwright-e2e.instructions.md` — agent-facing rules that load
  automatically when editing files under `e2e/`.

Earlier iterations of this work (a multi-mode database model, an "isolated-sequence" test
class, a database-reset helper, and a mode-selection script matrix) were tried and rejected.
They are **not** part of the current design and are not referenced in the body of this
document; the full history — what was attempted and why it was abandoned — is captured in
[Appendix A: Design History](#appendix-a-design-history--what-was-tried-and-rejected).

---

## Table of Contents

1. [Purpose and Scope](#1-purpose-and-scope)
2. [Fixed Constraints](#2-fixed-constraints)
3. [Architecture Overview](#3-architecture-overview)
4. [Database Mode](#4-database-mode)
5. [Canonical Seeded Identities](#5-canonical-seeded-identities)
6. [Authentication Strategy](#6-authentication-strategy)
7. [Test Classification](#7-test-classification)
8. [Global Setup Behaviour](#8-global-setup-behaviour)
9. [Seed Infrastructure](#9-seed-infrastructure)
10. [Configuration and npm Scripts](#10-configuration-and-npm-scripts)
11. [Directory Structure](#11-directory-structure)
12. [Implementation Patterns](#12-implementation-patterns)
13. [Operational Notes and Known Risks](#13-operational-notes-and-known-risks)
14. [Coverage Boundaries](#14-coverage-boundaries)
15. [Appendix A: Design History — What Was Tried and Rejected](#appendix-a-design-history--what-was-tried-and-rejected)

---

## 1. Purpose and Scope

### 1.1 Purpose

Define the Playwright E2E testing infrastructure for the POT application: database
orchestration, authentication strategy, test-data management, the two-class isolation
model, execution config, and authoring conventions. Written for developers and coding
agents.

### 1.2 In Scope

- Local developer-machine execution via Docker (Testcontainers-managed Postgres).
- A **single fully-seeded database mode** for every test run.
- Two browser-config targets: the **dev matrix** (Vite dev server) and the **prodlike
  gate** (built client via `vite preview`).
- Per-test fresh authentication with admin / viewer / password-change fixtures.
- Two-class execution-safety model: **parallel-safe** and **fixture-managed**.
- UI expectations derived from intercepted API responses (never hardcoded values).
- Four-project browser matrix: `chromium`, `edge`, `mobile-chrome`, `mobile-safari`.

### 1.3 Out of Scope

- CI pipeline integration (out of scope per the feature PRD; revisit after release gates).
- Cloud-hosted execution.
- Per-test isolated containers (all tests share one stack; isolation is by classification).
- New server endpoints created solely for test convenience.
- OTP / email-based flows (password reset, signup) — deferred indefinitely (see
  [Appendix A](#appendix-a-design-history--what-was-tried-and-rejected)).

---

## 2. Fixed Constraints

Non-negotiable rules that apply to the whole suite:

- **Docker-only execution.** Every E2E run targets a Testcontainers-managed Postgres. No
  tests target a shared manual-test database or a running dev-server database.
- **No test-only server endpoints.** Tests exercise only endpoints that exist in the
  production application.
- **Test data via committed artifacts only.** Site and user data comes from
  `e2e/seed/baseline.sql`; financial data comes from importing the committed
  `e2e/seed/financial.export` artifact through the server API. No ad-hoc API data creation
  in setup unless the creation call itself is the behaviour under test.
- **No per-test isolated containers.** All workers in a run share one API process (`5242`)
  and one Testcontainers database (`55432`). Isolation is achieved through test
  classification and teardown discipline.
- **One database mode.** There is no mode-selection variable; global setup always runs the
  fully-seeded path.
- **Server behaviour is not modified for tests.** OTP/security code paths are unchanged
  (OTP flows are simply not E2E-tested).

---

## 3. Architecture Overview

```
Playwright (@^1.59.1)
   ├─ webServer[0]: ASP.NET Core API   (dotnet run -c Release, http://127.0.0.1:5242)
   ├─ webServer[1]: client             (dev: Vite dev server, :5175  |  prodlike: build + preview, :5175)
   └─ globalSetup: Testcontainers Postgres (postgres:16, fixed host port 55432)
        └─ migrations → baseline.sql seed → financial import (63 records)
              └─ renewal/accrual → GET /_health/ready gate → shared-stack pre-warm
```

**ONE shared stack for the whole matrix**: one API process, one client server, one
Testcontainers Postgres — shared by every worker and all four projects. There are no
per-test containers. All test isolation comes from classification + teardown.

Key contracts:

- **Ports:** client `5175`, API `5242` (E2E webServer), Testcontainers Postgres `55432`.
  (The docker-compose dev stack uses different ports; this document covers the E2E harness.)
- **API auth:** HTTP-only rotating refresh cookie + Bearer access token (15-minute JWT, held
  in memory only — never stored in `storageState` files for reuse).
- **Refresh-token rotation:** the server rotates the refresh token on every
  `/api/auth/refresh`. A captured refresh cookie is therefore **single-use** — this is why
  each test logs in fresh (no worker-scoped shared auth state; see
  [Section 6](#6-authentication-strategy)).
- **Canonical users:** `e2e_admin` (Admin), `e2e_viewer` (Viewer), `e2e_pwchange` (Viewer,
  dedicated for the password-change test), plus platform admin
  `58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5` (see [Section 5](#5-canonical-seeded-identities)).
- **Backend:** .NET 10 (`net10.0`); API project `Pot.AspNetCore`, migrations project
  `Pot.Data.Migrations`. Client: React 19 + Vite 6.

### 3.1 Current config values (2026-08-27, dev config)

| Setting           | Value                    | Rationale                                                       |
| ----------------- | ------------------------ | --------------------------------------------------------------- |
| `fullyParallel`   | `true`                   | Max throughput on the shared stack                              |
| `workers`         | `process.env.CI ? 1 : 2` | Bounded local parallelism keeps the shared stack responsive     |
| `retries`         | `process.env.CI ? 2 : 1` | Safety net for a residual cold-browser contention blip          |
| `timeout`         | `60_000`                 | Do NOT raise — it makes shared-stack contention worse (see §13) |
| `expect.timeout`  | `10_000`                 | Cold-browser first loads (Edge/mobile) exceed the 5s default    |
| `video` / `trace` | `on-first-retry`         | Recording every test on 4 projects was a real CPU cost          |
| API webServer     | `dotnet run -c Release`  | Release build lowers per-request latency across the matrix      |

### 3.2 Prodlike config (`playwright.prod.config.ts`, validated 2026-09-01)

Runs the same four projects against a **built client** (`npm run build && npm run preview`;
the preview server inherits `server.proxy`, so `/api` still reaches the API, and
`VITE_API_BASE_URL=/api` comes from `.env.production`). Bounded `workers: CI ? 1 : 2`;
`retries: CI ? 2 : 0`; `video: retain-on-failure`. It ignores the tests that only make sense
against the dev source or desktop:

- `**/feedback/errorBoundary.test.ts` on all projects (it aborts a dev-source lazy chunk URL;
  a built client serves hashed chunk names so the route never matches).
- `**/settings/userSettings.test.ts` on edge + mobile (chromium-only — mutates shared
  canonical identities).
- `**/filters/filters.test.ts` on mobile (desktop-only; excluding avoids wasted logins).

The PWA contract is covered in both configs by the mode-aware
`e2e/tests/pwa/pwaContract.test.ts` (dev: no manifest/SW; built: manifest + SW registers).
Prodlike is an **on-demand gate** — run it when the client production build changes
(vite config/manifest/PWA, `.env.production`, Docker build args) and before releases. The
dev matrix remains the default.

---

## 4. Database Mode

The suite runs in a **single fully-seeded mode**. Every run, global setup performs:

1. Start Testcontainers Postgres on fixed host port `55432` (with `citext` installed before
   first connection — see §13).
2. Run EF Core migrations (dedicated migrations project).
3. Load `e2e/seed/baseline.sql` (site + canonical users).
4. Import `e2e/seed/financial.export` through the server API (63 records).
5. Run renewal/accrual to advance the data to the current local date.

There is no `users-only` / `blank` mode and no mode-selection npm scripts. Most tests
benefit from seeded data; login and auth flows work fine against a seeded database.

---

## 5. Canonical Seeded Identities

Established by `e2e/seed/baseline.sql`; fixed. Passwords must match the hashes committed in
that file and must not be changed without regenerating it. Credentials are centralised in
`e2e/helpers/secrets.ts` (DRY, not security — these are test identities).

| Field                | Value                                    |
| -------------------- | ---------------------------------------- |
| Site display name    | `POT E2E Site`                           |
| Admin username/pass  | `e2e_admin` / `E2E_admin-password`       |
| Admin email          | `e2e_admin@local.test`                   |
| Viewer username/pass | `e2e_viewer` / `E2E_viewer-password`     |
| Viewer email         | `e2e_viewer@local.test`                  |
| PW-change user/pass  | `e2e_pwchange` / `E2E_pwchange-password` |
| Platform admin ID    | `58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5`   |

`e2e_pwchange` is a seeded Viewer with its **own** password so the user-settings
password-change test can rotate its credentials (bumping `TokenVersion` and revoking _its_
sessions) without invalidating the shared admin session other suites depend on.

**Regeneration triggers:** regenerate `baseline.sql` when canonical identities change, role
assignments change, or non-nullable columns without defaults are added to covered tables.
Additive schema changes (new nullable columns/tables) do not require regeneration. When
regenerating, strip non-`SET` psql metacommands from the dump header/footer.

---

## 6. Authentication Strategy

### 6.1 Fresh login per test

Each authenticated test performs **one fresh login** (the `loginResult` fixture), which
yields **both**:

- `storageState` — used to create the browser context (carries the refresh cookie).
- `accessToken` — the login response's 15-minute JWT, exposed for API setup/cleanup.

A fresh login per test is **required** because the server rotates the refresh token on every
`/api/auth/refresh`; a storage state's refresh cookie is single-use. Reusing one shared state
across tests causes 401s and redirects to the login page once the first page load rotates the
token.

### 6.2 Fixtures

`e2e/fixtures/auth.ts` builds the authenticated tests via
`createAuthenticatedTest(credentials)`:

| Export         | Identity       | Role   | Used by                               |
| -------------- | -------------- | ------ | ------------------------------------- |
| `test`         | `e2e_admin`    | Admin  | Most suites (full access)             |
| `viewerTest`   | `e2e_viewer`   | Viewer | Permission-boundary tests (read-only) |
| `pwChangeTest` | `e2e_pwchange` | Viewer | user-settings password-change tests   |

The login fixture tolerates one slow-but-healthy cold login (the API may still be finishing
JIT/EF warm-up) — setup resilience, not a test retry. Fixture-managed suites **reuse the
`accessToken` fixture** for API setup/cleanup instead of logging in again: each extra login
is a PBKDF2 verify (~50–100ms CPU), the single biggest server-side CPU cost under parallel
load.

### 6.3 Logged-out / auth-under-test pages

Tests that exercise the login UI, signup/password-reset dialogs, or the PWA contract do
**not** use an auth fixture. They create a context with an empty storage state
(`storageState: { cookies: [], origins: [] }`) and navigate to `/login` directly. Name the
base import `baseTest` and use `adminTest.skip(...)` (a bare `test(`/`test.skip` raises a
`ReferenceError`).

---

## 7. Test Classification

Every test belongs to one of two classes. Classification determines whether it can run
concurrently with other tests against the shared stack.

### 7.1 Parallel-safe (read-only)

GET-only or UI-only; never writes to shared state. May run with any number of workers.

- Examples: smoke, navigation, rendering, read-only list/view tests.

### 7.2 Fixture-managed (owns its data)

Creates/updates/deletes **its own** records against the shared DB, then cleans up so the
baseline is restored. Convention (used by every Phase 6 suite):

1. Wrap the suite in `test.describe.serial('... (fixture-managed)', ...)` so its steps never
   interleave with other workers.
2. Clean up in `test.afterEach` (never `afterAll`) so teardown runs even on failure.
3. Create records with deterministic, unique descriptions (e.g. `E2E Foo ${Date.now()}`) so
   repeated runs never collide with seed data or prior runs.
4. Track created `rowId`s in module-level arrays; `afterEach` deletes them through the API
   using the **`accessToken` fixture + ONE request context** (no per-row logins).
5. Desktop-only where the flow uses the table/desktop UI: `test.skip(isMobileProject(testInfo),
'...')`.

### 7.3 Classification rules

- Classification must be correct for **local parallel execution**, not just CI serialised
  runs (`workers: 1` on CI hides parallelism bugs that resurface at higher worker counts).
- A test is parallel-safe **only** if it never calls a state-changing endpoint and never
  asserts shared mutable state. When in doubt, classify as fixture-managed.
- There are no per-test isolated containers — all workers share one database.
- **Out of scope as an E2E class:** destructive/aggregate "sequences" (import/accrual/
  renewal/projection recalculation). Those calculations are owned by the server unit +
  integration tests. Read-only UI rendering/correctness of seeded data (dashboard,
  projections, lists) **is** in scope and is parallel-safe.

---

## 8. Global Setup Behaviour

`playwright.globalSetup.ts` provides both the `default` (setup) and the named
`globalTeardown` export (Playwright ≥ 1.44 requirement; repo pins `^1.59.1`).

### 8.1 Startup sequence

Playwright starts the webServers **before** globalSetup runs (see §13 for the ordering
contract), then globalSetup:

1. Deletes `e2e/.auth/` (clears stale auth state from prior runs).
2. Reads `e2e/.runtime/testcontainers.json` for a stale container from a hard-killed run;
   if found, removes it with `docker rm -f` (not `docker stop` — Windows TIME_WAIT, see
   §13) and polls until port `55432` is released (`waitForPortReleased`, bounded).
3. Sets `DOTNET_ENVIRONMENT` / `ASPNETCORE_ENVIRONMENT` to `Production` for the migration
   child process.
4. Starts Testcontainers `postgres:16` on fixed host port `55432`, with an init script that
   installs `citext` before the first connection.
5. Parses the connection URI and sets `DATABASE__*`, `DATABASE_URL`, and
   `DATABASE_CONTAINER_ID` env vars for child processes and test helpers. (The already-running
   API webServer does **not** receive these — its config is passed as CLI args in the config.)
6. Writes the container ID to the runtime state file.
7. Runs EF Core migrations (`dotnet run --project Pot.Data.Migrations/...`).
8. Loads `baseline.sql` via `docker exec ... psql` (data-only insert).
9. Polls `GET /_health/ready` until the server confirms a live DB connection
   (`waitForDatabaseReady`, bounded to ~2 minutes — see §13).
10. Imports the financial artifact and runs renewal/accrual via the API
    (`runFinancialImportAndRenewal`): login as `e2e_admin` → import → accrue expenses →
    renew incomes (mode `Overdue`, as of the local date) → renew expenses. Throws if
    `e2e/seed/financial.export` is missing.
11. Pre-warms the shared stack (`warmUpSharedStack`): mounts every authenticated route in a
    real browser once, forcing Vite to transform lazy chunks and the API to serve first reads
    before the first test runs. Non-fatal on failure (tests would just pay the cold cost).

### 8.2 Teardown

`globalTeardown` deletes the runtime state file **only**. The Testcontainers **Ryuk reaper**
removes the container after the Playwright process exits (deliberately not `docker stop`,
which leaves port `55432` in TIME_WAIT on Docker Desktop for Windows). For hard-killed runs
where Ryuk cannot fire, the next run's stale-container step (§8.1 step 2) cleans up.

---

## 9. Seed Infrastructure

| Artifact           | Location                    | Contents                                        | Managed by                                  |
| ------------------ | --------------------------- | ----------------------------------------------- | ------------------------------------------- |
| `baseline.sql`     | `e2e/seed/baseline.sql`     | Data-only dump: `Site`, `User`, `UserRole` rows | Committed; regenerated on identity changes  |
| `financial.export` | `e2e/seed/financial.export` | Financial data export artifact (63 records)     | Committed; refreshed when test data changes |

`baseline.sql` is applied by piping its content via stdin into `psql` inside the running
container (`docker exec -i ... psql -v ON_ERROR_STOP=1 ...`). It is data-only; schema comes
from migrations. No `-f` flag (no filesystem path inside the container). The seed file must
not contain schema DDL, `CREATE DATABASE`, or `\connect` metacommands.

`citext` is installed by the container init script before the API's first connection so that
Npgsql loads the type into its cache (otherwise login fails later with a "citext not found"
type-cache error).

---

## 10. Configuration and npm Scripts

All scripts run from `Source/Client/pot-react`. Exact definitions live in `package.json`.

### 10.1 Scripts

| Script             | Command                                                                                                                                                     | Notes                           |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------- |
| `e2e`              | `npm run e2e:chromium`                                                                                                                                      | Alias (chromium, line reporter) |
| `e2e:dev`          | `playwright test --reporter=line`                                                                                                                           | All 4 projects, line reporter   |
| `e2e:all`          | `npm run e2e:all:dev`                                                                                                                                       | Alias                           |
| `e2e:all:dev`      | `playwright test --project chromium --project edge --project mobile-chrome --project mobile-safari --reporter=line,html`                                    | Full dev matrix (the gate)      |
| `e2e:prodlike`     | `playwright test --config=playwright.prod.config.ts --reporter=line`                                                                                        | Prodlike, line reporter         |
| `e2e:all:prodlike` | `playwright test --config=playwright.prod.config.ts --project chromium --project edge --project mobile-chrome --project mobile-safari --reporter=line,html` | Full prodlike matrix            |
| `e2e:chromium`     | `playwright test --reporter=line --project chromium`                                                                                                        | Fastest loop (one project)      |
| `e2e:edge`         | `playwright test --reporter=line --project edge`                                                                                                            | Edge (msedge channel)           |
| `e2e:mobile`       | `playwright test --reporter=line --project mobile-chrome --project mobile-safari`                                                                           | Mobile projects                 |
| `e2e:smoke`        | `playwright test --reporter=line e2e/tests/smoke`                                                                                                           | Smoke tests only                |
| `e2e:smoke:headed` | `playwright test e2e/tests/smoke --headed`                                                                                                                  | Smoke, visible browser          |
| `e2e:headed`       | `playwright test --headed`                                                                                                                                  | Debugging                       |
| `e2e:debug`        | `playwright test --debug`                                                                                                                                   | Step-through debug              |
| `e2e:ui`           | `playwright test --ui`                                                                                                                                      | Playwright UI mode              |
| `e2e:report`       | `playwright show-report`                                                                                                                                    | Open last HTML report           |
| `e2e:install`      | `playwright install chromium webkit`                                                                                                                        | Install browsers                |

### 10.2 Validation workflow

Validate the way the repo does it: targeted `chromium` → all four projects (`e2e:all:dev`) →
(for a gate) the full matrix twice more, then the on-demand `e2e:all:prodlike` when the
production build changes. Full matrix requires Docker Desktop + the .NET SDK.

### 10.3 Load-sensitivity rationale

The shared stack caps how much parallel work it can absorb; the config is tuned to reduce
per-test/per-request cost:

1. **Reduced PBKDF2 logins** — the auth fixture exposes `accessToken`; fixture-managed
   suites reuse it for setup + cleanup (one login per test).
2. **Release API** (`dotnet run -c Release`) — lowers per-request latency.
3. **`video: 'on-first-retry'`** — records only on retry (recording every test on 4
   projects is a real CPU cost).
4. **`workers: 2` locally**, **`retries: 1`** as a safety net.
5. **Vite HMR disabled for E2E** (`E2E=1` → `server.hmr: false`) — the page can never be
   reloaded behind a test.

---

## 11. Directory Structure

```
Source/Client/pot-react/
  playwright.config.ts        # dev matrix config
  playwright.prod.config.ts   # prodlike (built-client) config — on-demand gate
  playwright.globalSetup.ts   # global setup + teardown (shared module)
  e2e/
    .runtime/                 # gitignored; testcontainers.json runtime state
    .auth/                    # gitignored; per-test auth states (transient)
    fixtures/
      auth.ts                 # test (admin), viewerTest, pwChangeTest
    helpers/
      secrets.ts              # canonical credentials (DRY)
      databaseSeed.ts         # read-only listExistingUsernames()
    seed/
      baseline.sql            # site + canonical users
      financial.export        # financial artifact (63 records)
    scripts/                  # shared harness scripts
    tests/
      smoke/  auth/  navigation/  rendering/  feedback/
      crud/   dashboard/  filters/  approvals/  settings/  maintenance/
      mobile/ picker/  table/  dialogs/  theme/  emptystates/  pwa/
    README.md                 # architecture & conventions reference
    AUTHORING.md              # authoring guide (day-to-day)
```

Test files follow `<area>/<name>.test.ts`, e.g. `e2e/tests/crud/expensesCrud.test.ts`.

---

## 12. Implementation Patterns

### 12.1 Container lifecycle (global setup)

The Postgres container starts in global setup and lives for the whole run. Removal is
delegated to the Testcontainers Ryuk reaper after process exit; globalTeardown only removes
the runtime state file. See §8 and §13.

### 12.2 Auth fixture

`createAuthenticatedTest(credentials)` extends `baseTest` with a `loginResult` fixture
(test-scoped) that performs one fresh API login and exposes `{ storageState, accessToken }`,
plus `storageState` and `accessToken` conveniences that read from it. See §6.

### 12.3 Fixture-managed suite skeleton

```ts
import type { APIRequestContext } from "@playwright/test";
import { expect, test } from "../../fixtures/auth";

const apiBaseUrl = "http://127.0.0.1:5242";
const isMobileProject = (t: import("@playwright/test").TestInfo) =>
  t.project.name.startsWith("mobile");

const createdRowIds: string[] = [];

async function createRequestContext(
  playwright: import("@playwright/test").Playwright,
) {
  // 60s per-action timeout mirrors the test timeout: a cold first API call on the
  // loaded shared stack can exceed the 30s default.
  return playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
}

test.describe.serial("X (fixture-managed)", () => {
  test.afterEach(async ({ playwright, accessToken }) => {
    const request = await createRequestContext(playwright);
    try {
      for (const rowId of createdRowIds.splice(0)) {
        await request.delete(`/api/.../${rowId}`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
      }
    } finally {
      await request.dispose();
    }
  });

  test("does the thing", async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      "desktop-only; mobile covered by mobile tests",
    );
    const request = await createRequestContext(playwright);
    try {
      // create via API using accessToken; drive the UI; assert
    } finally {
      await request.dispose();
    }
  });
});
```

### 12.4 UI expectations from API responses

Tests derive expected values from intercepted API payloads, not hardcoded numbers:

- **Read-only lists:** PREFETCH via `playwright.request.newContext({ baseURL: apiBaseUrl,
timeout: 60_000 })` + `GET /api/<list>` with the `accessToken`, then assert the UI
  web-first. `page.waitForResponse` resolves on headers and can be load-starved past the
  test timeout.
- **After a slow POST** (renew/accrue/change-password): register the response listener
  **before** the action, assert `response.ok()`, `await response.finished()` (wait for the
  body), THEN assert the UI signal (toast/dialog) with documented headroom
  `{ timeout: 30_000 }`.

Register `waitForResponse` listeners before the navigation/action that fires the request.

---

## 13. Operational Notes and Known Risks

### 13.1 Server-before-DB startup ordering

Playwright launches the API webServer **before** globalSetup creates the Postgres container,
so the API starts pointing at a DB that does not exist yet (port `55432`). It survives via
Npgsql's connection retry — the HTTP listener opens before the DB connection is established.
`waitForDatabaseReady()` on `GET /_health/ready` is the barrier that prevents tests from
starting until the server confirms a live DB connection. Do **not** remove that barrier.

- If the server exhausts its Npgsql retry budget before the container is ready, look in
  `Source/Server/e2e-server.log` for connection-refused messages.

### 13.2 Windows TIME_WAIT and container teardown

On Docker Desktop for Windows, `docker stop` leaves the host port in TCP `TIME_WAIT`, so the
next run reports `55432` as "already allocated". Therefore:

- Teardown does **not** stop the container — the Ryuk reaper does it after process exit.
- Stale containers (hard-killed runs) are removed with `docker rm -f`, then the next run
  polls until the port is released (`waitForPortReleased`) instead of a blind fixed sleep.

### 13.3 `citext` first-connection type cache

`citext` must exist before the API's first connection; Npgsql caches type metadata on first
connect. The container init script creates the extension before first start. Without it,
login fails later with a "citext could not be found in the types loaded by Npgsql" error.

### 13.4 Readiness gate is bounded

`waitForDatabaseReady` is bounded (~60 attempts, ~2 minutes) and throws with a pointer to
`e2e-server.log` on timeout. An unbounded loop was the worst failure mode (infinite hang
with no diagnostics); it is fixed.

### 13.5 Do not raise the per-test timeout

Raising `timeout` makes shared-stack contention worse — heavy tests run longer → higher
sustained load → other tests starve. Keep it at `60_000` and reduce per-test cost instead
(§10.3).

### 13.6 API webServer log redirection

The API webServer redirects stdout/stderr to `Source/Server/e2e-server.log` (overwritten
each run); `stdout`/`stderr` are `'ignore'` so Playwright does not capture the redirected
output. The prodlike config must match the dev config **exactly** (Release build, the
`--PlatformAdmin:UserIds` grant, the log redirect, and a 180s start timeout) — drift broke
platform-admin flows and piped output under load in the past.

---

## 14. Coverage Boundaries

### 14.1 What the suite covers (test areas)

`smoke` (app shell, baseline, seed data), `auth` (login, logout, protected routes, permission
gating), `navigation` (sidebar, catch-all), `rendering` (read-only data), `feedback`
(ErrorSheet, ErrorBoundary, toasts, loading, form validation), `crud` (accounts/expenses/
incomes lifecycles), `dashboard` (quick actions), `filters`, `approvals` (platform admin),
`settings` (profile + password change), `maintenance` (export), `mobile` (card grids,
sidebar, sheets/dialogs, dashboard cards), `picker` (EnrichedCalendar), `table` (sorting/
badges), `dialogs` (modal sheets/dialogs), `theme` (light/dark toggle), `emptystates`, `pwa`
(dev vs built contract).

### 14.2 What is deliberately not E2E-tested

- **Renewal/accrual/projection calculations, import/export server behaviour, auth endpoint
  semantics, CORS, rate limiting, security headers, health checks** — owned by server unit +
  integration tests. E2E verifies the UI **renders** the data correctly, not the maths.
- **OTP / email flows** (password reset, signup) — deferred indefinitely.
- **CI integration, per-test isolated containers, cloud execution** — out of scope.

---

## Appendix A: Design History — What Was Tried and Rejected

> This appendix is retained so the reasoning behind the current design survives the
> retirement of the original working plan. It is **historical** — nothing here is part of
> the current implementation. Do not reintroduce these patterns.

| #   | Idea tried                                                                                                                                                        | What happened / why rejected                                                                                                                                                                                                                               | Outcome & lesson                                                                                                                                     |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **Multi-mode database seeding** — an `E2E_MODE` variable selecting `users-only` / `fully-seeded` (and earlier, a "blank" mode), plus a 3-mode npm-script matrix.  | Most tests benefit from seeded data, and login/signup work fine on a seeded DB. The mode matrix added configuration surface and script sprawl for no coverage gain.                                                                                        | **Rejected (S1).** Single fully-seeded mode; no `E2E_MODE`; no mode scripts.                                                                         |
| 2   | **"Isolated-sequence" test class** — dedicated-invocation sequences (separate npm script) that trigger import/accrual/renewal/projection and assert global state. | The calculations are already covered by server unit + integration tests; duplicating them in E2E added an execution class that required dedicated invocations and DB resets.                                                                               | **Rejected (S4).** No destructive-sequence E2E class. Read-only UI rendering/correctness of seeded data remains in scope as parallel-safe.           |
| 3   | **`applyDatabaseSeedMode()` DB-reset helper** — reset the DB to a known state from test `beforeAll` hooks (for isolated sequences).                               | Dead code once sequences were removed. It also had an unfixed truncation bug (inserts collided with duplicate primary keys because it reloaded `baseline.sql` without truncating first).                                                                   | **Removed (S5).** Helper trimmed to the read-only `listExistingUsernames()`.                                                                         |
| 4   | **Worker-scoped shared auth storage** — one login per parallel worker saving `.auth/{index}.json`, reused by every test in that worker.                           | The server **rotates the refresh token on every `/api/auth/refresh`**, so a shared storage state is single-use: once any test's page load refreshed the token, later tests reusing the file got a 401 → login page (intermittent, worker-reuse dependent). | **Replaced.** Fresh login per test exposing `storageState` + `accessToken`.                                                                          |
| 5   | **Unbounded / 4 / 3 worker counts** for the local matrix.                                                                                                         | Too many browsers hit the ONE shared API/Vite/Postgres: load-sensitive tests flaked (login page not rendering in 60s, `waitForResponse` timeouts, mobile `ERR_ABORTED`). Raising the timeout made it worse; 3 workers still flaked.                        | **2 workers locally (1 on CI).** The tightest bound that reliably fits the shared stack.                                                             |
| 6   | **Recording video/trace on every test** on all 4 projects.                                                                                                        | Edge + WebKit capture is expensive CPU on the shared local stack.                                                                                                                                                                                          | **`on-first-retry`.** Diagnostics preserved on retries; screenshots still capture failures.                                                          |
| 7   | **Built-client (`vite preview`) as the local-matrix default** (to remove dev-server HMR/transform load).                                                          | Regressed the matrix: the PWA service worker installs on every fresh per-test context (per-test overhead), and the errorBoundary test aborts a dev-source lazy chunk URL that doesn't exist in a built bundle.                                             | **Kept Vite dev server** for the local matrix (HMR disabled via `E2E=1`); built-client is the separate on-demand prodlike config.                    |
| 8   | **OTP / MailPit e2e** (password reset, signup) — new Docker service + SMTP config + OTP capture helper.                                                           | Genuinely useful but requires new infrastructure and was not needed for the core catalog.                                                                                                                                                                  | **Deferred indefinitely (S3).** Revisit only if signup/reset e2e becomes a priority.                                                                 |
| 9   | **Unbounded `waitForDatabaseReady()` loop.**                                                                                                                      | No exit condition → infinite hang with no diagnostics when setup broke (container, migrations, or server connection).                                                                                                                                      | **Fixed.** Bounded to ~2 minutes; throws with a pointer to `e2e-server.log`.                                                                         |
| 10  | **Blind 3s sleep after stale-container removal.**                                                                                                                 | Fixed delay was either too short (race) or wasteful on fast machines.                                                                                                                                                                                      | **Replaced** with an active port-release poll (`waitForPortReleased`, bounded).                                                                      |
| 11  | **`docker stop` during teardown.**                                                                                                                                | Left port `55432` in TIME_WAIT on Docker Desktop for Windows → next run "port already allocated".                                                                                                                                                          | **Avoided.** Ryuk reaper removes the container post-exit; stale containers are `docker rm -f`'d at next startup.                                     |
| 12  | **Prodlike API webServer drifting from the dev config** (Debug build, missing `--PlatformAdmin:UserIds`, piped stdout, short timeout).                            | Broke platform-admin flows (approvals/user settings) and piped output could fill under load in the full matrix.                                                                                                                                            | **Fixed.** The prodlike API webServer must mirror the dev config exactly: Release, the PlatformAdmin grant, a log-file redirect, and a 180s timeout. |
| 13  | **Per-test isolated containers.**                                                                                                                                 | Contradicts the shared-stack model; test isolation is achieved by classification + teardown instead.                                                                                                                                                       | **Out of scope** (PRD).                                                                                                                              |
| 14  | **Separate planning artifacts** (an `_orig` PRD, a direction-change doc, a coverage-discovery doc, a code-review-findings tracker).                               | Content was superseded by the implemented suite and this document.                                                                                                                                                                                         | **Retired/deleted.** Single source: this design record + `e2e/README.md` + `e2e/AUTHORING.md`.                                                       |
