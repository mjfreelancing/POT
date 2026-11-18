---
description: "Playwright E2E conventions, isolation and app anchors"
applyTo: "Source/Client/pot-react/e2e/**"
---

# Playwright E2E

Architecture, fixtures and conventions are documented in the files below. Read them before adding tests.

- [e2e README](../../Source/Client/pot-react/e2e/README.md) — architecture, config values, quick start, run hygiene, known traps and troubleshooting
- [e2e AUTHORING](../../Source/Client/pot-react/e2e/AUTHORING.md) — the day-to-day recipes

This file holds the rules an agent must follow. The reasons, the failure modes and the diagnosis steps
live in the README.

Config: `playwright.config.ts` (`testDir: './e2e'`; keep new tests under `e2e/`). Run `npm run e2e` from `Source/Client/pot-react` (the config's non-blocking reporters, suited to non-interactive runs). Prodlike (`playwright.prod.config.ts`, built client) is not run per change. It is the completion gate for client-affecting work: run the full matrix with `npm run e2e:all:prodlike` when such a PRD completes, and `npm run e2e:prodlike` on demand when the client production build changes (ADR-0041).

## Locators and clicks

- Prefer `getByRole` -> `getByLabel` -> `getByText` -> stable test id. CSS/xpath only when no accessible locator exists.
- If reliable locators are missing, make minimal product-code improvements (accessible names/roles, stable `data-testid`) when in scope; otherwise record the recommendation with rationale.
- Click the actionable control, not a wrapper. Scope locators to a stable region/dialog. No forced clicks unless the test validates blocked/hidden interaction. Use `await locator.click()`, never coordinates.
- Assert the outcome right after a click (URL, dialog, network response, UI state). Do not chain ambiguous clicks without assertions between them. For controls that open overlays, assert the overlay role/heading before continuing.
- Radix / shadcn `Select`: never `.click()` or `locator.evaluate()` an option. Click the `combobox` trigger, then select through `selectRadixOption(option, selectedOption?)` in `e2e/helpers/radix.ts`, passing the current value's option when there is one. Never re-implement it in a spec — the sequence it performs is in the README's known traps.

## Assertions, structure, determinism

- Use web-first assertions (`toHaveText`, `toContainText`, `toHaveURL`, `toHaveCount`); rely on auto-waiting, no hard sleeps. Assert visibility only when visibility is the behavior. For API-driven UI values, assert from the intercepted response payload.
- Group tests by user behavior; titles read as behavior statements; one user-journey outcome per test. Comment only non-obvious setup. Put all `test(...)` / `test.describe(...)` blocks before private helpers.
- Avoid selectors tied to styling/animation classes or fragile DOM structure. Do not assume timing or order across parallel workers. Prefer deterministic fixture setup over in-test data mutation.
- Data readiness: prefer prefetching read-only lists via the `accessToken` request context over `page.waitForResponse` (see `mobileCardGrids.test.ts`). For a UI signal after a slow POST (such as a success toast), capture the response, assert `response.ok()`, `await response.finished()`, then assert with `{ timeout: 30_000 }` (see `quickActions.test.ts`).
- API-backed expectations: register `waitForResponse` before the click/navigation that triggers the fetch, validate the response contract shape, then assert UI values derived from the payload.
- A page-driving test also asserts the page made no failed API call: `collectApiFailures(page)` from `e2e/helpers/api.ts`, called before the navigation and asserted empty after the last assertion (before any `finally` cleanup). Playwright fails on neither a failed response nor the console error that follows one, so an assertion on one figure passes while an unrelated request 4xxs. A spec whose purpose is that failure — authorization, validation, rate limiting, concurrency — passes the `expected` predicate instead; never drop the assertion to make such a spec pass.

## Isolation classification

Classify every test before writing it (ADR-0042).

- **Parallel-safe**: never calls a state-changing endpoint and never asserts on shared mutable state. Exception: calls producing only transient server state with no effect on other tests (for example `/api/auth/login`) qualify if any browser context created solely for that call is closed right after the assertion.
- **Fixture-managed**: creates or deletes its own records. Wrap in `test.describe.serial()`; clean up in `afterEach` (not `afterAll`) so cleanup runs on failure. Use ONE request context per suite and reuse the `accessToken` fixture.
- **Destructive**: destroys the tenant or identity it runs as (account deletion, site deletion). It must PROVISION that tenant through the product's routes with `e2e/helpers/provisioning.ts` (`provisionInvitedViewer`, `provisionSiteAdministrator`) and drive the journey in its own `browser.newContext({ storageState })` — never an auth fixture over a seeded identity, which can only be destroyed once per database. Such a spec runs in every browser project and is expected to pass twice in a row without reseeding; assert the seeded identities still exist once it finishes.
- CI `workers: 1` does not make a test parallel-safe; classify for local parallel execution. All workers share one API process (port `5242`) and one Testcontainers database (port `55432`), which is the shared stack the classification assumes (ADR-0040).

## Fixtures and config

- `e2e/fixtures/auth.ts` exposes `test` (admin), `viewerTest` (viewer), `pwChangeTest` (`e2e_pwchange`, viewer) and `quickActionsTest` (`e2e_quickactions`, Admin on its own site). Each authenticated test logs in ONCE and receives both `storageState` and `accessToken` (15-min JWT for API setup/cleanup). Do not log in per operation and do not share storage state across tests/workers (the refresh cookie rotates on every `/api/auth/refresh`). A journey that changes its own account runs as a seeded identity nothing else logs in as (ADR-0045).
- Config values are in `playwright.config.ts` and stated in the README; do not raise `timeout` to chase flakes, and treat retries as a safety net rather than a fix.
- PWA: dev serves no manifest/SW (`vite-plugin-pwa` gates both behind `devOptions.enabled`; `registerServiceWorker()` short-circuits on `import.meta.env.DEV`); the built client serves the manifest and registers the SW. `e2e/tests/pwa/pwaContract.test.ts` asserts whichever contract applies.

## Reporting and process lifecycle

- Reporters are config-owned (`[['line'], ['html', { open: 'never' }]]` in both configs). Never pass `--reporter` from an npm script: a CLI flag replaces the config's array outright, which silently drops the HTML report or re-introduces the one that blocks. Open the report with `npm run e2e:report`.
- Both webServers use `reuseExistingServer: false` (API 5242, Vite 5175), so never run two stacks at once.
- After any run that died abnormally, run `npm run e2e:clean` before the next one. `npm run e2e:all:dev:log` clears a leftover Playwright daemon itself and keeps the run's evidence.
- The run starts and stops its own mail sink container (`pot-e2e-mail-sink`). Read messages with `e2e/helpers/mail.ts` — `waitForMessage({ to, subjectContains })`, then `oneTimeCode` or `temporaryPassword`. Never parse a message body by hand, and never assume a message exists: a sign-up's and an invitation's credential exist nowhere else, so the wait is the assertion.
- A global hook file must export its hook as `default`. Playwright reads only the default export, so a named `globalTeardown` beside a default `globalSetup` is silently ignored and the setup then runs a second time as the "teardown". The hooks are separate modules: `playwright.globalSetup.ts` and `playwright.globalTeardown.ts`.
- `npm run e2e:scripts` (Pester 5, `e2e/scripts/tests/`) covers what the scripts decide: which ports are required, reclaimable or tolerated, what counts as a leftover, what clean refuses to do, and how a running container is classified. Keep the port numbers in `e2e/scripts/e2e-ports.ps1` in step with the configs.
- Diagnose with `npm run e2e:preflight` (read-only; exits 1 when something would block a run) and keep the evidence with `npm run e2e:all:dev:log`, which writes `e2e/logs/e2e-<timestamp>.log` and `e2e-ports-<timestamp>.log`. Add `-DebugWebServer` when the client was unreachable and the log has no `[WebServer]` lines.

## App anchors

- Routes (`src/routes/AppRoutes.tsx`): `/login`, `/dashboard`, `/projections`, `/accounts`, `/expenses`, `/incomes`, `/users`, `/approvals/pending`.
- Sidebar links (`src/components/nav/AppSidebarMenus.tsx`, `MenuGroup.tsx`): `Dashboard`, `Projections`, `Accounts`, `Expenses`, `Income`, `Users`, `Approvals`, `Export...`, `Import...`. Use `getByRole('link', { name: '<menu label>' })` first.
- Aria labels: `Add a new account` (`AccountsPage.tsx`), `Add a new expense` (`ExpensesPage.tsx`), `Add a new income` (`IncomesPage.tsx`), `Filter by account` (`AccountFilter.tsx`), `Clear search input` (`SearchInput.tsx`), `Toggle Sidebar` (`src/components/ui/sidebar.tsx`), `Navigate to Pay On Time homepage` (`AppSidebarHeader.tsx`).
- Login (`src/features/auth/LoginForm.tsx`): labeled fields `Username` and `Password`, `Login` button; use `getByLabel` and role-based clicks.
- Account-filter scenarios: assert both the URL query and the visible filter state after each click-driven navigation. Use `selectRadixOption` (`e2e/helpers/radix.ts`) for the `Filter by account` select.
- Sheets/dialogs (create/edit/invite/import/export): assert the dialog role and heading before proceeding. Permission-gated actions (`WithPermission`): assert enabled/disabled affordance before clicking, and the rejection path when disabled.
- shadcn/Radix dialogs: always-open CREATE sheets are `modal={false}` with no `onOpenChange` (close via form Cancel / back-nav, not Escape). Modal dialogs (SignupDialog, UserRoleDialog, ...) close on Escape and backdrop click unless they pass the custom `modal` prop to `DialogContent`. `DialogContent` defaults `showCloseButton=false`. Anchors: `[data-slot="dialog-overlay"]`, `[data-slot="sidebar"]`, `[data-slot="sidebar-footer"]` (see `dialogs/modalDialogs.test.ts`, `theme/themeToggle.test.ts`).
- Calendar picker (EnrichedCalendar, react-day-picker v10): each day is a grid cell containing a `button[@data-day]` whose text is the bare day number; adjacent-month buttons carry an ancestor cell with the `rdp-outside` class (exclude them); popover is `[data-slot="popover-content"]`; footer/nav buttons are `Previous Year` / `Previous Month` / `Next Month` / `Next Year` / `Today` / `Accept` / `Cancel`. Today = the cell carries `rdp-today` and `bg-accent`; selected = the button has `data-selected-single` and `bg-primary`. Anchor a day with an exact whole-text XPath (`normalize-space(.)='N'`) — `locator.filter({ hasText })` is a substring match (day "1" matches 10-19 and 21) (see `enrichedCalendar.test.ts`).

## Browser-tool exploration (Playwright MCP)

- Take a fresh page snapshot before targeting an element; use exact refs from it and a human-readable `element` description; re-snapshot after significant UI changes.
- Target links/buttons by accessible name. If a click does not change state, check for a dialog and network requests before retrying.
