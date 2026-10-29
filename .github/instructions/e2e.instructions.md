---
description: "Playwright E2E conventions, isolation and app anchors"
applyTo: "Source/Client/pot-react/e2e/**"
---

# Playwright E2E

Architecture, fixtures and conventions are documented in the files below. Read them before adding tests.

- [e2e README](../../Source/Client/pot-react/e2e/README.md)
- [e2e AUTHORING](../../Source/Client/pot-react/e2e/AUTHORING.md)

Config: `playwright.config.ts` (`testDir: './e2e'`; keep new tests under `e2e/`). Run `npm run e2e` from `Source/Client/pot-react` (the config's non-blocking reporters, suited to non-interactive runs). Prodlike (`playwright.prod.config.ts`, built client) is not run per change. It is the completion gate for client-affecting work: run the full matrix with `npm run e2e:all:prodlike` when such a PRD completes, and `npm run e2e:prodlike` on demand when the client production build changes.

## Locators and clicks

- Prefer `getByRole` -> `getByLabel` -> `getByText` -> stable test id. CSS/xpath only when no accessible locator exists.
- If reliable locators are missing, make minimal product-code improvements (accessible names/roles, stable `data-testid`) when in scope; otherwise record the recommendation with rationale.
- Click the actionable control, not a wrapper. Scope locators to a stable region/dialog. No forced clicks unless the test validates blocked/hidden interaction. Use `await locator.click()`, never coordinates.
- Assert the outcome right after a click (URL, dialog, network response, UI state). Do not chain ambiguous clicks without assertions between them. For controls that open overlays, assert the overlay role/heading before continuing.
- Radix / shadcn `Select`: do not use `.click()` or `locator.evaluate()` event dispatch on an option. Radix selects on pointerup only if a same-node pointerdown set `pointerTypeRef` to `'mouse'` (state that can be lost under load), and `evaluate()` has no actionability retry so it silently drops the event if the option node is mid-positioning. Click the `combobox` trigger, then select through `selectRadixOption(option, selectedOption?)` in `e2e/helpers/radix.ts`: gate on the selected option having `data-highlighted` (pass the current value's option; omit it when nothing is selected yet), then `option.focus()` + `toBeFocused` + `option.press('Enter')`, then a fail-fast `toBeHidden`. Never re-implement this in a spec.

## Assertions, structure, determinism

- Use web-first assertions (`toHaveText`, `toContainText`, `toHaveURL`, `toHaveCount`); rely on auto-waiting, no hard sleeps. Assert visibility only when visibility is the behavior. For API-driven UI values, assert from the intercepted response payload.
- Group tests by user behavior; titles read as behavior statements; one user-journey outcome per test. Comment only non-obvious setup. Put all `test(...)` / `test.describe(...)` blocks before private helpers.
- Avoid selectors tied to styling/animation classes or fragile DOM structure. Do not assume timing or order across parallel workers. Prefer deterministic fixture setup over in-test data mutation.
- Data readiness: prefer prefetching read-only lists via the `accessToken` request context over `page.waitForResponse` (it resolves on headers and can be load-starved; see `mobileCardGrids.test.ts`). For a UI signal after a slow POST (such as a success toast), capture the response, assert `response.ok()`, `await response.finished()`, then assert with `{ timeout: 30_000 }` (see `quickActions.test.ts`).
- API-backed expectations: register `waitForResponse` before the click/navigation that triggers the fetch, validate the response contract shape, then assert UI values derived from the payload.

## Isolation classification

Classify every test before writing it.

- **Parallel-safe**: never calls a state-changing endpoint and never asserts on shared mutable state. Exception: calls producing only transient server state with no effect on other tests (for example `/api/auth/login`) qualify if any browser context created solely for that call is closed right after the assertion.
- **Fixture-managed**: creates or deletes its own records. Wrap in `test.describe.serial()`; clean up in `afterEach` (not `afterAll`) so cleanup runs on failure. Use ONE request context per suite and reuse the `accessToken` fixture.
- **Destructive**: destroys the tenant or identity it runs as (account deletion, site deletion). It must PROVISION that tenant through the product's routes with `e2e/helpers/provisioning.ts` (`provisionInvitedViewer`, `provisionSiteAdministrator`) and drive the journey in its own `browser.newContext({ storageState })` — never an auth fixture over a seeded identity, which can only be destroyed once per database. Such a spec runs in every browser project and is expected to pass twice in a row without reseeding; assert the seeded identities still exist once it finishes.
- CI `workers: 1` does not make a test parallel-safe; classify for local parallel execution. All workers share one API process (port `5242`) and one Testcontainers database (port `55432`).

## Fixtures and config

- `e2e/fixtures/auth.ts` exposes `test` (admin), `viewerTest` (viewer), `pwChangeTest` (`e2e_pwchange`, viewer). Each authenticated test logs in ONCE and receives both `storageState` and `accessToken` (15-min JWT for API setup/cleanup). Do not log in per operation (PBKDF2 CPU under load) and do not share storage state across tests/workers (the refresh cookie rotates on every `/api/auth/refresh`).
- Config: `workers: process.env.CI ? 1 : 2`, `retries: process.env.CI ? 2 : 1`, `timeout: 60_000`, `expect.timeout: 10_000`, `video: 'on-first-retry'`, API webServer `dotnet run -c Release`. Do not raise `timeout` to chase flakes (it worsens shared-stack contention); retries are a safety net, not a fix.
- PWA: dev serves no manifest/SW (`vite-plugin-pwa` gates both behind `devOptions.enabled`; `registerServiceWorker()` short-circuits on `import.meta.env.DEV`); the built client serves the manifest and registers the SW. `e2e/tests/pwa/pwaContract.test.ts` asserts whichever contract applies.

## Reporting and process lifecycle

- Reporters are configured in `playwright.config.ts` and `playwright.prod.config.ts`: `reporter: [['line'], ['html', { open: 'never' }]]`. `line` keeps console progress; `html` still writes `playwright-report/`, but `open: 'never'` means Playwright never serves it, so a failing run prints `To open last HTML report run: npx playwright show-report` and exits. Open it on demand with `npm run e2e:report`.
- Never pass `--reporter` from an npm script. A CLI flag REPLACES the whole config `reporter` array, so `--reporter=line` silently drops the HTML report and `--reporter=line,html` re-introduces the blocking one. Let the config govern.
- Why: when the HTML report is served, a failing or interrupted run stays resident on port 9323 ("Serving HTML report at http://localhost:9323. Press Ctrl+C to quit."). The next run then collides - symptom: a run that finishes in seconds with EVERY failure `net::ERR_CONNECTION_REFUSED` at `127.0.0.1:5175`.
- Both webServers use `reuseExistingServer: false` (API 5242, Vite 5175), so never run two stacks at once. A run that is hard-killed (agent timeout, closed terminal, `Stop-Process`) leaves the `dotnet run` API grandchild alive and the next run dies with `Error: http://localhost:5242 is already used` before any test starts; a clean completed run's teardown does stop it.
- A hard-killed run also leaves Playwright's `test-server` daemon (`node_modules/@playwright/test/cli.js test-server`). It holds no E2E port, so a port check never sees it, and the next run can attach to it — the abandoned job's teardown then stops the new run's Vite server mid-run (timeline `client=True` → `client=False`, every test `ERR_CONNECTION_REFUSED` while the API stays healthy). An idle daemon is otherwise normal (the VS Code Playwright discovery keeps one and respawns it), so the preflight only warns about it. After any run that dies abnormally, `npm run e2e:clean` (stops the daemon, a stray Vite server, the harness API and the harness's own mail sink container; never the development Docker stack, never while a matrix is running) before the next run. `npm run e2e:all:dev:log` clears the daemon itself before starting.
- The run starts and stops its own mail sink container (`pot-e2e-mail-sink`, maildev: SMTP `1025`, read back over HTTP on `1080`); the API is already configured to deliver there by `playwright.config.ts`. Read messages with `e2e/helpers/mail.ts` — `waitForMessage({ to, subjectContains })`, then `oneTimeCode` or `temporaryPassword`, which read the templates' monospaced blocks. Never parse a message body by hand, and never assume a message exists: a sign-up's and an invitation's credential exist nowhere else, so the wait is the assertion. A leftover sink is reported and replaced at the start of the next run; anything else holding `1025` or `1080` fails before any test and names the process.
- A global hook file must export its hook as `default`: Playwright resolves `globalSetup` and `globalTeardown` from the module's default export only (`requireOrImportDefaultFunction`), so a named `globalTeardown` export beside a default `globalSetup` is silently ignored and the setup then runs a second time as the "teardown" (re-migrating, re-seeding, re-importing and leaving a fresh sink). The hooks are therefore separate modules: `playwright.globalSetup.ts` and `playwright.globalTeardown.ts`. Adding a named hook to the setup module will not take effect.
- The scripts' decisions are covered by `npm run e2e:scripts` (Pester 5, `e2e/scripts/tests/`): which ports are required, reclaimable by the harness's own sink, or tolerated; which processes count as a leftover; what clean refuses to do; and how a running container is classified (`Get-E2eContainerClassification` in `e2e/scripts/e2e-processes.ps1` — the harness's own sink is named `pot-*` like the development stack, so it is identified by name first, and the stack is checked before the postgres image its `pot-postgres` shares with the harness). It runs in about a second with no Docker, browser or database, so a change to those rules is proved there rather than by observing a run. Keep the port numbers in `e2e/scripts/e2e-ports.ps1` in step with the configs.
- Diagnosing a failure: run `npm run e2e:preflight` first (read-only; reports any occupant of the E2E ports with the owning process, a running E2E session, leftover Playwright daemons, stray Vite processes, conflicting containers and Docker availability, and exits 1 when something would block a run). When you want the evidence kept, run `npm run e2e:all:dev:log` instead of `e2e:all:dev`: it mirrors the run to the console and writes `e2e/logs/e2e-<timestamp>.log` (console output, including the `[WebServer]` lines Playwright forwards from Vite - `webServer[1]` sets `stdout: 'pipe'`) plus `e2e-ports-<timestamp>.log` (client/API/Postgres/report ports, free memory and container count every 10s, never overwritten). Add `-DebugWebServer` to the script (sets `DEBUG=pw:webserver`) when the failure was the client being unreachable and the log has no `[WebServer]` lines: a killed process writes nothing. The HTML report holds no webServer output and `test-results/`/`playwright-report/` are cleared by the next run, so those two files are the only record of a run that failed to connect.

## App anchors

- Routes (`src/routes/AppRoutes.tsx`): `/login`, `/dashboard`, `/projections`, `/accounts`, `/expenses`, `/incomes`, `/users`, `/approvals/pending`.
- Sidebar links (`src/components/nav/AppSidebarMenus.tsx`, `MenuGroup.tsx`): `Dashboard`, `Projections`, `Accounts`, `Expenses`, `Income`, `Users`, `Approvals`, `Export...`, `Import...`. Use `getByRole('link', { name: '<menu label>' })` first.
- Aria labels: `Add a new account` (`AccountsPage.tsx`), `Add a new expense` (`ExpensesPage.tsx`), `Add a new income` (`IncomesPage.tsx`), `Filter by account` (`AccountFilter.tsx`), `Clear search input` (`SearchInput.tsx`), `Toggle Sidebar` (`src/components/ui/sidebar.tsx`), `Navigate to Pay On Time homepage` (`AppSidebarHeader.tsx`).
- Login (`src/features/auth/LoginForm.tsx`): labeled fields `Username` and `Password`, `Login` button; use `getByLabel` and role-based clicks.
- Account-filter scenarios: assert both the URL query and the visible filter state after each click-driven navigation. Use `selectRadixOption` (`e2e/helpers/radix.ts`) for the `Filter by account` select.
- Sheets/dialogs (create/edit/invite/import/export): assert the dialog role and heading before proceeding. Permission-gated actions (`WithPermission`): assert enabled/disabled affordance before clicking, and the rejection path when disabled.
- shadcn/Radix dialogs: always-open CREATE sheets are `modal={false}` with no `onOpenChange` (close via form Cancel / back-nav, not Escape). Modal dialogs (SignupDialog, UserRoleDialog, ...) close on Escape and backdrop click unless they pass the custom `modal` prop to `DialogContent`. `DialogContent` defaults `showCloseButton=false`. Anchors: `[data-slot="dialog-overlay"]`, `[data-slot="sidebar"]`, `[data-slot="sidebar-footer"]` (see `dialogs/modalDialogs.test.ts`, `theme/themeToggle.test.ts`).
- Calendar picker (EnrichedCalendar, react-day-picker v9): day cells are `button[name="day"]` (`role="gridcell"`); adjacent-month days carry `.day-outside` (exclude them); popover is `[data-slot="popover-content"]`; footer/nav buttons are `Previous Year` / `Previous Month` / `Next Month` / `Next Year` / `Today` / `Accept` / `Cancel`. Today = `day_today` / `bg-accent`, selected = `day_selected` / `bg-primary`. `locator.filter({ hasText })` is a substring match (day "1" matches 10-19 and 21), so anchor with an exact regex `new RegExp('^' + dayNumber + '$')` (see `enrichedCalendar.test.ts`).

## Browser-tool exploration (Playwright MCP)

- Take a fresh page snapshot before targeting an element; use exact refs from it and a human-readable `element` description; re-snapshot after significant UI changes.
- Target links/buttons by accessible name. If a click does not change state, check for a dialog and network requests before retrying.

