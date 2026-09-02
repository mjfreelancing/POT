# POT E2E Testing — Architecture & Conventions

**Status**: Active — full test catalog implemented and validated (auth, navigation,
rendering, feedback, CRUD, bulk actions, quick actions, filters, mobile, custom controls,
PWA/theme/empty states, matrix & prodlike validation).
**Last Updated**: 2026-09-02

This is the central reference for how POT's Playwright E2E suite is architected and how to
extend it. Requirements and decisions live in PRD 012
(`Docs/Future/PRD-012-playwright-e2e-testing-infrastructure.md`); the implemented design,
current state, and design history (what was tried and rejected) live in ADR 012
(`Docs/Future/ADR-012-playwright-e2e-testing-infrastructure.md`). The **agent-facing rules**
that load automatically when editing files under this directory live in
`.github/instructions/playwright-e2e.instructions.md`.

---

## Quick Start

All scripts run from `Source/Client/pot-react`. Exact definitions are in `package.json`
— the list below mirrors them (do not guess from memory):

```bash
npm run e2e                 # alias of `npm run e2e:chromium` (chromium project, line reporter)
npm run e2e:dev             # ALL 4 projects via playwright.config.ts, line reporter
npm run e2e:all             # alias of `npm run e2e:all:dev`
npm run e2e:all:dev         # full 4-project matrix (chromium, edge, mobile-chrome, mobile-safari), line+html reporter
npm run e2e:prodlike        # all 4 projects via playwright.prod.config.ts (built client), line reporter
npm run e2e:all:prodlike    # full matrix via the prodlike (built-client) config, line+html reporter
npm run e2e:chromium        # chromium project only, line reporter
npm run e2e:edge            # edge (msedge channel) project only, line reporter
npm run e2e:mobile          # mobile-chrome + mobile-safari projects only, line reporter
npm run e2e:smoke           # e2e/tests/smoke only, line reporter
npm run e2e:smoke:headed    # smoke tests, visible browser
npm run e2e:headed          # visible browser (debugging)
npm run e2e:debug           # step-through debug mode
npm run e2e:ui              # Playwright UI mode
npm run e2e:report          # open the last HTML report (playwright show-report)
npm run e2e:install         # install Playwright browsers (chromium, webkit)
```

The default config (`playwright.config.ts`) targets the **Vite dev server**; the prodlike
config targets a **built client**. Both run the same 4 projects.

Full matrix requires Docker Desktop + the .NET SDK. Each run does a heavy setup:
Testcontainers Postgres (port `55432`) → migrations → `baseline.sql` seed → financial
import (63 records) → renewal/accrual.

---

## Architecture

```
Playwright (@1.59.1)
   ├─ webServer[0]: ASP.NET Core API  (dotnet run -c Release, http://127.0.0.1:5242)
   ├─ webServer[1]: Vite dev server   (http://127.0.0.1:5175, proxies /api -> 5242)
   └─ globalSetup: Testcontainers Postgres (host port 55432)
        └─ migrations -> baseline.sql seed -> financial import -> renewal/accrual
              └─ GET /_health/ready gate (waitForDatabaseReady) before any test runs
              └─ shared-stack pre-warm (warmUpSharedStack: mounts every lazy route
                 in a real browser so the first test never pays the cold start)
```

**ONE shared stack for the whole matrix**: one API process, one Vite dev server, one
Testcontainers Postgres — shared by every worker and all four projects. There are **no
per-test containers**. All test isolation comes from classification + teardown (below).

Key contracts:

- E2E ports: client `5175` (Vite dev server), API `5242` (E2E webServer), Testcontainers
  Postgres `55432`. (The docker-compose dev stack uses different ports — this doc covers
  the E2E harness only.)
- API auth: HTTP-only rotating refresh cookie + Bearer access token (15-min JWT, in-memory only).
- The server **rotates the refresh token on every `/api/auth/refresh`** — a refresh cookie is
  single-use. This is why each test must log in fresh (no worker-scoped shared auth state).
- Canonical users: `e2e_admin`/`E2E_admin-password` (Admin, full access),
  `e2e_viewer`/`E2E_viewer-password` (Viewer, read-only), and
  `e2e_pwchange`/`E2E_pwchange-password` (Viewer — dedicated identity so the
  user-settings password-change test can rotate its own credentials without
  invalidating the shared admin session). Platform admin
  `58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5`. Credentials live in `e2e/helpers/secrets.ts`.

### Current `playwright.config.ts` values (2026-08-27)

| Setting          | Value                    | Rationale                                                                     |
| ---------------- | ------------------------ | ----------------------------------------------------------------------------- |
| `fullyParallel`  | `true`                   | Max throughput on the shared stack                                            |
| `workers`        | `process.env.CI ? 1 : 2` | Bounded local parallelism so the shared stack stays responsive                |
| `retries`        | `process.env.CI ? 2 : 1` | Safety net for the residual cold-browser contention blip                      |
| `timeout`        | `60_000`                 | Do NOT raise it — it makes shared-stack contention worse (see Load Reduction) |
| `expect.timeout` | `10_000`                 | Cold-browser first loads (Edge/mobile) exceed the 5s default                  |
| `video`          | `'on-first-retry'`       | Recording every test on all 4 projects was a real CPU cost                    |
| API webServer    | `dotnet run -c Release`  | Release build lowers per-request latency across the matrix                    |

### Prodlike config (`playwright.prod.config.ts`, validated 2026-09-01)

Runs the same 4 projects against a **built client** (`npm run build && npm run preview`; the
preview server inherits `server.proxy`, so `/api` still reaches the API, and
`VITE_API_BASE_URL=/api` from `.env.production`). Bounded `workers: CI ? 1 : 2`; testIgnores
the dev-only/conflicting tests (`**/feedback/errorBoundary.test.ts` on all projects;
`**/settings/userSettings.test.ts` on edge + mobile; `**/filters/filters.test.ts` on mobile).
The PWA contract is covered by the mode-aware `e2e/tests/pwa/pwaContract.test.ts` (dev: no
manifest/SW; built: manifest + SW). Prodlike is an **on-demand gate** — run it when the client
production build changes (vite config/manifest/PWA, `.env.production`, Docker build args) and
before releases; the dev matrix remains the default (validated 3x consecutive clean at
262 passed / 72 skipped / 0 flaky).

---

## Auth fixtures

`e2e/fixtures/auth.ts` builds `test` (admin), `viewerTest` (viewer), and
`pwChangeTest` (e2e_pwchange) via `createAuthenticatedTest(credentials)`. Each
authenticated test performs **one fresh login**
(`loginResult` fixture) which produces **both**:

- `storageState` — used to create the browser context (the refresh cookie).
- `accessToken` — the login response's 15-min JWT, exposed for API setup/cleanup.

Fixture-managed suites must reuse the `accessToken` fixture for API calls (setup + cleanup)
instead of logging in again — each extra login is a PBKDF2 verify (~50–100ms CPU) and the
logins are the single biggest server-side CPU cost under parallel load. See the Load
Reduction note.

Do **not** share storage state across tests/workers — the refresh cookie is single-use (see
Architecture).

---

## Test isolation model

Every test is one of two classes (the suite runs in a single fully-seeded mode):

### Parallel-safe (read-only)

GET-only / UI-only; never writes to shared state. Safe to run concurrently on the shared
stack. Smoke, navigation, rendering, and read-only view tests.

### Fixture-managed (writes its own records)

Creates/updates/deletes **its own** records against the shared DB, then cleans up so the
baseline is restored. The established convention used by the interaction suites:

1. Wrap the suite in `test.describe.serial('... (fixture-managed)', ...)` so its steps never
   interleave with other workers.
2. Clean up in `test.afterEach` (never `afterAll`) so teardown runs even on failure.
3. Create records with deterministic, unique descriptions (e.g. `E2E Foo ${Date.now()}`) so
   repeated runs never collide with seed data or prior runs.
4. Track created rowIds in module-level arrays; `afterEach` deletes them through the API
   using the **`accessToken` fixture + ONE request context** (no per-row logins).
5. Desktop-only where the flow uses the table/desktop UI: `test.skip(isMobileProject(testInfo),
'...')` — mobile uses the card grid (see `e2e/tests/mobile/`).

Example skeleton (mirrors `e2e/tests/crud/expensesCrud.test.ts`):

```ts
import type { APIRequestContext } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

const apiBaseUrl = 'http://127.0.0.1:5242';
const isMobileProject = (t: import('@playwright/test').TestInfo) =>
  t.project.name.startsWith('mobile');

const createdExpenseRowIds: string[] = [];

async function createRequestContext(
  playwright: import('@playwright/test').Playwright,
) {
  // 60s per-action timeout mirrors the test timeout (see filters.test.ts): a
  // cold first API call on the loaded shared stack can exceed the 30s default.
  return playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
}

test.describe.serial('X (fixture-managed)', () => {
  test.afterEach(async ({ playwright, accessToken }) => {
    const request = await createRequestContext(playwright);
    try {
      for (const rowId of createdExpenseRowIds.splice(0)) {
        await request.delete(`/api/expenses/${rowId}`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
      }
    } finally {
      await request.dispose();
    }
  });

  test('does the thing', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'desktop-only; mobile uses the card grid',
    );
    const request = await createRequestContext(playwright);
    try {
      /* create via API using accessToken, drive the UI, assert */
    } finally {
      await request.dispose();
    }
  });
});
```

---

## Conventions & known traps (lessons)

- **Locator strategy**: `getByRole` → `getByLabel` → `getByText` → stable test id. See the
  instruction file for the full POT anchor catalog.
- **`getByRole('textbox', { name: 'Description', exact: true })`** — the create/edit form's
  Description field collides with the page "Search … by description" input; always use the
  exact-name form.
- **Checkbox row selection** for bulk actions: `getByRole('checkbox', { name: 'Select row ${rowId}' })`
  (row id = `createRowIdGetter` = `String(row.rowId)`).
- **Capture requests/responses BEFORE the action that fires them** — e.g. register
  `page.waitForResponse(...)` before clicking a submit/bulk-action button; assert toasts only
  after the response completes (the accrue-expenses call is the slowest).
- **`PermissionGuard` HIDES** on missing perms (returns null); `WithPermission` DISABLES.
  Assert accordingly (e.g. viewer should not see the Quick Actions section at all).
- **DataTable has no pagination** — filtered-out rows are removed from the DOM
  (`toHaveCount(0)` works); visible rows can be asserted regardless of scroll.
- **`FilterResultsCount` always renders** "Showing X of Y items".
- **React hydration/remount races**: don't branch on `isVisible()` before a lazy chunk
  mounts. Vite HMR is disabled for E2E — `vite.config.ts` sets `server.hmr: false` when
  `E2E=1` (the Vite webServer sets `env: { E2E: '1' }`) — so the page can never be reloaded
  behind a test.
- **URL state**: the `accountId` query param is the single render-time source of truth for
  account filters (PRD 011) — assert URL + visible state after each filter change.

---

## Config rationale (load-sensitivity)

The shared stack (one API, one Vite server, one Postgres) caps how much parallel work it can
absorb, so the config is tuned to **reduce per-test/per-request cost**:

1. **Reduced PBKDF2 logins** — the auth fixture exposes `accessToken`; fixture-managed suites
   reuse it for setup + cleanup (one login per test).
2. **Release API** (`dotnet run -c Release`) — lowers per-request latency.
3. **`video: 'on-first-retry'`** — records only on retry (recording every test on all 4
   projects is a real CPU cost).
4. **`workers: 2`** locally, **`retries: 1`** as a safety net.
5. **Vite HMR disabled for E2E** (`E2E=1` → `server.hmr: false`) — the page can never be
   reloaded behind a test.

**Do NOT**: raise `timeout` to chase flakes (it makes contention worse — heavy tests run
longer and starve others); rely on retries as the fix (safety net only); share auth state
across workers. **Prodlike (built client) is a supported ON-DEMAND gate, not the default**:
the Vite dev server is the default matrix; `playwright.prod.config.ts` runs the built client
with bounded workers, dev-only tests excluded via `testIgnore`, and a mode-aware
`pwaContract.test.ts` that asserts the dev negative contract (no manifest/SW) under the dev
config and the built positive contract (manifest + SW) under prodlike. Run `e2e:prodlike`
when the client production build changes.

---

## Troubleshooting

- **Port 55432 stuck in TIME_WAIT / stale container**: the global teardown leaves the
  Testcontainers container to Ryuk; globalSetup detects and removes a stale container before
  starting a new one.
- **API `/_health/ready` returns 503**: expected during startup — globalSetup polls it as the
  readiness gate; the API starts before the container exists and recovers via Npgsql retry.
- **A test lands on the login page unexpectedly**: the per-test refresh cookie was consumed
  (single-use rotation) or an auth request was slow under load. Each test must log in fresh;
  do not share storage state.
- **Random full-matrix flakes**: almost always shared-stack contention. Check the Load
  Reduction note; re-run the matrix once (retries: 1) before assuming a code bug.
- **Browser downloads / PWA**: the suite targets the dev server; export/download tests use
  Playwright's `page.waitForEvent('download')` (see 8.7).
