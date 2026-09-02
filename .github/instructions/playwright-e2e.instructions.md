---
applyTo: "Source/Client/pot-react/e2e/**"
---

# Playwright E2E Instructions

## Core Rules

### Locator and Click Strategy

- Prefer user-facing locators in this order: `getByRole` -> `getByLabel` -> `getByText` -> stable test id.
- If reliable locators are missing, suggest minimal product-code improvements that strengthen testability (for example accessible names/roles or stable `data-testid` attributes).
- When in scope, implement those locator improvements in product code with minimal surface-area changes; when out of scope, record the recommendation with rationale.
- Use CSS/xpath locators only when no accessible locator exists.
- Click actionable controls directly; avoid clicking container wrappers when a child control is available.
- Scope locators to a stable region/dialog before clicking to reduce ambiguity.
- Do not use forced clicks unless the test explicitly validates blocked/hidden interaction behavior.

### Click Reliability

- For Playwright test code, prefer `await locator.click()` over coordinate clicks.
- Assert outcome immediately after click (URL, dialog, network response, or UI state change).
- Do not chain ambiguous clicks without assertions between them.
- For controls that open overlays, assert the overlay role/heading before continuing.
- For Radix / shadcn `Select` components, prefer the retrying keyboard/AT activation path
  over `.click()` and over `locator.evaluate()` event dispatch. Radix's mouse path selects on
  pointerup only if a same-node pointerdown set its `pointerTypeRef` to 'mouse' (state that
  can be lost under load), so `.click()` (separate CDP pointerdown/pointerup round-trips) is
  flaky; and `locator.evaluate()` dispatch has NO actionability retry — it silently drops the
  event if the option node is transient mid-positioning (expert-verified), so it
  too flakes on a loaded machine. The reliable path is
  `selectRadixOption(option, selectedOption)` in `e2e/tests/filters/filters.test.ts`: gate on
  the currently-selected option having `data-highlighted` (Radix's settle signal), then
  `option.focus()` + `toBeFocused` + `option.press('Enter')` (SelectItem's `onKeyDown` →
  `handleSelect`, Radix's own unit-tested path), then fail-fast `toBeHidden` (handleSelect
  closes the dropdown). Playwright's focus/press retry on instability; evaluate() silently drops.

### Assertions and Waiting

- Use web-first assertions (`toHaveText`, `toContainText`, `toHaveURL`, `toHaveCount`) instead of manual polling.
- Rely on Playwright auto-waiting; do not add hard sleeps for synchronization.
- Use visibility assertions only when visibility itself is the behavior under test.
- When UI values are API-driven, assert from intercepted response payloads where practical.

### Test Structure and Readability

- Group tests by user behavior and feature intent.
- Use descriptive test titles that read as behavior statements.
- Keep each test focused on one user journey outcome.
- Add comments only for non-obvious setup or complex assertions.
- Place all `test(...)` and `test.describe(...)` blocks before any private helper functions in the file. Helpers go at the bottom so tests are visible without scrolling.

### Stability and Determinism

- Avoid selectors tied to styling classes, animation classes, or fragile DOM structure.
- Avoid assumptions about timing/order across parallel workers.
- Keep tests independent from prior test data unless a fixture explicitly defines shared setup.
- Prefer deterministic fixture setup over in-test ad hoc data mutation.

### Test Isolation Classification

- Classify every test as one of **parallel-safe** or **fixture-managed** before writing it.
- A test is **parallel-safe** only when it never calls a state-changing endpoint and never asserts on shared mutable state. No special configuration required. Exception: calls that produce only transient server-side state (such as an authenticated session) with no observable effect on other tests are treated as parallel-safe, provided any browser context created solely for that call is closed immediately after the assertion.
- A test is **fixture-managed** when it creates or deletes its own records. Wrap in `test.describe.serial()`; clean up in `afterEach` (not `afterAll`) so cleanup runs even on failure. Reuse the `accessToken` fixture for API setup/cleanup with ONE request context per suite (no per-row logins — see the auth-fixture note below).
- CI `workers: 1` does not make a test parallel-safe. Classify for local parallel execution; CI serialisation is a safety net, not the isolation mechanism.
- There are no per-test isolated containers. All workers in a run share one API process (port `5242`) and one Testcontainers database (port `55432`).

### Browser Tooling Interactions

- For browser-driven exploration with `mcp_playwright_browser_click`:
  - Capture a fresh page snapshot before targeting an element.
  - Use exact target references from the snapshot when available.
  - Provide a human-readable `element` description that reflects user intent.
  - Re-snapshot after significant click-driven UI changes to avoid stale references.

## Expansion Notes

- Keep reusable Playwright guidance above in Core Rules.
- Keep project-specific route anchors, selector anchors, fixture contracts, and run commands below.
  - POT: Keep POT-specific behavior, routes, and selector anchors below.
  - POT: Route anchors currently include `/login`, `/dashboard`, `/projections`, `/accounts`, `/expenses`, `/incomes`, `/users`, `/approvals/pending` from `src/routes/AppRoutes.tsx`.
  - POT: Sidebar navigation labels are stable user-facing anchors (`Dashboard`, `Projections`, `Accounts`, `Expenses`, `Income`, `Users`, `Approvals`, `Export...`, `Import...`) from `src/components/nav/AppSidebarMenus.tsx` and `src/components/nav/MenuGroup.tsx`.
  - POT: Prefer `getByRole('link', { name: '<menu label>' })` for sidebar navigation clicks before fallback selectors.
  - POT: Use explicit aria labels already present in UI for resilient clicks:
    - `Add a new account` (`src/features/accounts/AccountsPage.tsx`)
    - `Add a new expense` (`src/features/expenses/ExpensesPage.tsx`)
    - `Add a new income` (`src/features/incomes/IncomesPage.tsx`)
    - `Filter by account` (`src/components/filters/AccountFilter.tsx`)
    - `Clear search input` (`src/components/filters/SearchInput.tsx`)
    - `Toggle Sidebar` (`src/components/ui/sidebar.tsx`)
    - `Navigate to Pay On Time homepage` (`src/components/nav/AppSidebarHeader.tsx`)
  - POT: Login uses labeled fields (`Username`, `Password`) and a `Login` submit button in `src/features/auth/LoginForm.tsx`; prefer `getByLabel` and role-based button clicks.
  - POT: For account-filter scenarios (PRD 011), assert both URL query behavior and visible filter state after each click-driven navigation.
  - POT: Use the `selectRadixOption` helper in `e2e/tests/filters/filters.test.ts` for the `Filter by account` Radix `Select` — it drives the retrying keyboard/AT path (`data-highlighted` gate + focus + Enter + `toBeHidden`; see Core Rules → Click Reliability).
  - POT: When clicking controls that open sheets/dialogs (create/edit/invite/import/export), assert the sheet/dialog role and heading before proceeding.
  - POT: When click actions are permission-gated (`WithPermission`), assert enabled/disabled affordance before click and expected rejection path when disabled.
  - POT: Calendar picker (EnrichedCalendar, react-day-picker v9): day cells are `button[name="day"]` (`role="gridcell"`), adjacent-month days carry `.day-outside` (exclude them), the popover is `[data-slot="popover-content"]`, and the footer/nav buttons are `Previous Year`/`Previous Month`/`Next Month`/`Next Year`/`Today`/`Accept`/`Cancel`. shadcn styling hooks: today = `day_today`/`bg-accent`, selected = `day_selected`/`bg-primary`. NOTE: `locator.filter({ hasText })` is a SUBSTRING match — a day "1" also matches 10-19/21 — so anchor with an exact regex (`new RegExp('^' + dayNumber + '$')`) when the text could be a prefix/substring of siblings (see `enrichedCalendar.test.ts`).
  - POT: Sheets/dialogs (shadcn/Radix): always-open CREATE sheets are `modal={false}` with NO `onOpenChange` (closed via form Cancel / back-nav, NOT Escape); MODAL dialogs (SignupDialog, UserRoleDialog, …) close on Escape + backdrop click unless they pass the custom `modal` prop to `DialogContent` (which prevents backdrop-close). `DialogContent` defaults `showCloseButton=false`. Useful anchors: `[data-slot="dialog-overlay"]`, `[data-slot="sidebar"]`, `[data-slot="sidebar-footer"]` (see `dialogs/modalDialogs.test.ts`, `theme/themeToggle.test.ts`).
  - POT: Data-readiness — prefer PREFETCHING read-only lists via the `accessToken` request context over `page.waitForResponse` for a list GET (`waitForResponse` resolves on headers and can be load-starved past the timeout; see `mobileCardGrids.test.ts`). When asserting a UI signal that appears AFTER a slow POST (e.g. a success toast), capture the response, assert `response.ok()`, `await response.finished()`, then assert with documented headroom `{ timeout: 30_000 }` (see `quickActions.test.ts`).
  - POT: PWA — dev serves NO manifest/SW (vite-plugin-pwa gates both behind `devOptions.enabled`; `registerServiceWorker()` short-circuits on `import.meta.env.DEV`); the BUILT client serves the manifest + registers the SW. The mode-aware `e2e/tests/pwa/pwaContract.test.ts` asserts whichever contract applies (runs under both the dev and prodlike configs).
  - POT: Use API-backed expectation flow from PRD 012 R9:
    - Register `waitForResponse` before click/navigation that triggers fetch.
    - Validate response contract shape.
    - Assert UI values rendered from payload-derived expectations.

  - POT: For browser-tool exploration sessions using `mcp_playwright_browser_click`:
    - Prefer targeting links/buttons by accessible name from snapshot output.
    - If a click does not produce expected state change, check dialog presence and network requests before retrying.

  - POT: Keep line reporter for non-interactive agent runs (`npm run e2e` currently uses `--reporter=line` in `package.json`).
  - POT: The `/api/auth/login` endpoint creates a transient session only; smoke tests that call it and immediately close the browser context qualify for the parallel-safe transient-session exception.
  - POT: Current Playwright config is in `Source/Client/pot-react/playwright.config.ts` with `testDir: './e2e'`; keep new tests under `Source/Client/pot-react/e2e`. Prodlike (`playwright.prod.config.ts`, built client) is an ON-DEMAND gate — run `npm run e2e:prodlike` when the client production build changes, not per-change.
  - POT: Central architecture/conventions doc: `Source/Client/pot-react/e2e/README.md` (read it before adding tests). Requirements + decisions: `Docs/Future/PRD-012-playwright-e2e-testing-infrastructure.md` (PRD); implemented design + design history: `Docs/Future/ADR-012-playwright-e2e-testing-infrastructure.md` (ADR).
  - POT: Auth fixture (`e2e/fixtures/auth.ts`) exposes `test` (admin) / `viewerTest` (viewer) / `pwChangeTest` (e2e_pwchange, viewer). Each authenticated test logs in ONCE and receives BOTH `storageState` (browser context) and `accessToken` (the 15-min JWT, for API setup/cleanup). Reuse `accessToken` for API calls; do NOT log in per operation (PBKDF2 CPU under load) and do NOT share storage state across tests/workers (the refresh cookie rotates on every `/api/auth/refresh`).
  - POT: Current config values: `workers: process.env.CI ? 1 : 2`, `retries: process.env.CI ? 2 : 1`, `timeout: 60_000`, `expect.timeout: 10_000`, `video: 'on-first-retry'`, API webServer `dotnet run -c Release`. Do NOT raise `timeout` to chase flakes (it worsens shared-stack contention); retries are a safety net, not a fix.
