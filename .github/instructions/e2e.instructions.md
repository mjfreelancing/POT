---
description: "Playwright E2E conventions, isolation and app anchors"
applyTo: "Source/Client/pot-react/e2e/**"
---

# Playwright E2E

Architecture, fixtures and conventions are documented in the files below. Read them before adding tests.

- [e2e README](../../Source/Client/pot-react/e2e/README.md)
- [e2e AUTHORING](../../Source/Client/pot-react/e2e/AUTHORING.md)

Config: `playwright.config.ts` (`testDir: './e2e'`; keep new tests under `e2e/`). Run `npm run e2e` from `Source/Client/pot-react` (line reporter, suited to non-interactive runs). Prodlike (`playwright.prod.config.ts`, built client) is an on-demand gate: run `npm run e2e:prodlike` when the client production build changes, not per change.

## Locators and clicks

- Prefer `getByRole` -> `getByLabel` -> `getByText` -> stable test id. CSS/xpath only when no accessible locator exists.
- If reliable locators are missing, make minimal product-code improvements (accessible names/roles, stable `data-testid`) when in scope; otherwise record the recommendation with rationale.
- Click the actionable control, not a wrapper. Scope locators to a stable region/dialog. No forced clicks unless the test validates blocked/hidden interaction. Use `await locator.click()`, never coordinates.
- Assert the outcome right after a click (URL, dialog, network response, UI state). Do not chain ambiguous clicks without assertions between them. For controls that open overlays, assert the overlay role/heading before continuing.
- Radix / shadcn `Select`: do not use `.click()` or `locator.evaluate()` event dispatch. Radix selects on pointerup only if a same-node pointerdown set `pointerTypeRef` to `'mouse'` (state that can be lost under load), and `evaluate()` has no actionability retry so it silently drops the event if the option node is mid-positioning. Use `selectRadixOption(option, selectedOption)` in `e2e/tests/filters/filters.test.ts`: gate on the selected option having `data-highlighted`, then `option.focus()` + `toBeFocused` + `option.press('Enter')`, then a fail-fast `toBeHidden`.

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
- CI `workers: 1` does not make a test parallel-safe; classify for local parallel execution. All workers share one API process (port `5242`) and one Testcontainers database (port `55432`).

## Fixtures and config

- `e2e/fixtures/auth.ts` exposes `test` (admin), `viewerTest` (viewer), `pwChangeTest` (`e2e_pwchange`, viewer). Each authenticated test logs in ONCE and receives both `storageState` and `accessToken` (15-min JWT for API setup/cleanup). Do not log in per operation (PBKDF2 CPU under load) and do not share storage state across tests/workers (the refresh cookie rotates on every `/api/auth/refresh`).
- Config: `workers: process.env.CI ? 1 : 2`, `retries: process.env.CI ? 2 : 1`, `timeout: 60_000`, `expect.timeout: 10_000`, `video: 'on-first-retry'`, API webServer `dotnet run -c Release`. Do not raise `timeout` to chase flakes (it worsens shared-stack contention); retries are a safety net, not a fix.
- PWA: dev serves no manifest/SW (`vite-plugin-pwa` gates both behind `devOptions.enabled`; `registerServiceWorker()` short-circuits on `import.meta.env.DEV`); the built client serves the manifest and registers the SW. `e2e/tests/pwa/pwaContract.test.ts` asserts whichever contract applies.

## App anchors

- Routes (`src/routes/AppRoutes.tsx`): `/login`, `/dashboard`, `/projections`, `/accounts`, `/expenses`, `/incomes`, `/users`, `/approvals/pending`.
- Sidebar links (`src/components/nav/AppSidebarMenus.tsx`, `MenuGroup.tsx`): `Dashboard`, `Projections`, `Accounts`, `Expenses`, `Income`, `Users`, `Approvals`, `Export...`, `Import...`. Use `getByRole('link', { name: '<menu label>' })` first.
- Aria labels: `Add a new account` (`AccountsPage.tsx`), `Add a new expense` (`ExpensesPage.tsx`), `Add a new income` (`IncomesPage.tsx`), `Filter by account` (`AccountFilter.tsx`), `Clear search input` (`SearchInput.tsx`), `Toggle Sidebar` (`src/components/ui/sidebar.tsx`), `Navigate to Pay On Time homepage` (`AppSidebarHeader.tsx`).
- Login (`src/features/auth/LoginForm.tsx`): labeled fields `Username` and `Password`, `Login` button; use `getByLabel` and role-based clicks.
- Account-filter scenarios: assert both the URL query and the visible filter state after each click-driven navigation. Use `selectRadixOption` for the `Filter by account` select.
- Sheets/dialogs (create/edit/invite/import/export): assert the dialog role and heading before proceeding. Permission-gated actions (`WithPermission`): assert enabled/disabled affordance before clicking, and the rejection path when disabled.
- shadcn/Radix dialogs: always-open CREATE sheets are `modal={false}` with no `onOpenChange` (close via form Cancel / back-nav, not Escape). Modal dialogs (SignupDialog, UserRoleDialog, ...) close on Escape and backdrop click unless they pass the custom `modal` prop to `DialogContent`. `DialogContent` defaults `showCloseButton=false`. Anchors: `[data-slot="dialog-overlay"]`, `[data-slot="sidebar"]`, `[data-slot="sidebar-footer"]` (see `dialogs/modalDialogs.test.ts`, `theme/themeToggle.test.ts`).
- Calendar picker (EnrichedCalendar, react-day-picker v9): day cells are `button[name="day"]` (`role="gridcell"`); adjacent-month days carry `.day-outside` (exclude them); popover is `[data-slot="popover-content"]`; footer/nav buttons are `Previous Year` / `Previous Month` / `Next Month` / `Next Year` / `Today` / `Accept` / `Cancel`. Today = `day_today` / `bg-accent`, selected = `day_selected` / `bg-primary`. `locator.filter({ hasText })` is a substring match (day "1" matches 10-19 and 21), so anchor with an exact regex `new RegExp('^' + dayNumber + '$')` (see `enrichedCalendar.test.ts`).

## Browser-tool exploration (Playwright MCP)

- Take a fresh page snapshot before targeting an element; use exact refs from it and a human-readable `element` description; re-snapshot after significant UI changes.
- Target links/buttons by accessible name. If a click does not change state, check for a dialog and network requests before retrying.
