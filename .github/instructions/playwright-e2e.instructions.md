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

### Stability and Determinism

- Avoid selectors tied to styling classes, animation classes, or fragile DOM structure.
- Avoid assumptions about timing/order across parallel workers.
- Keep tests independent from prior test data unless a fixture explicitly defines shared setup.
- Prefer deterministic fixture setup over in-test ad hoc data mutation.

### Browser Tooling Interactions

- For browser-driven exploration with `mcp_microsoft_pla_browser_click`:
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
  - POT: When clicking controls that open sheets/dialogs (create/edit/invite/import/export), assert the sheet/dialog role and heading before proceeding.
  - POT: When click actions are permission-gated (`WithPermission`), assert enabled/disabled affordance before click and expected rejection path when disabled.
  - POT: Use API-backed expectation flow from PRD 012 R7:
    - Register `waitForResponse` before click/navigation that triggers fetch.
    - Validate response contract shape.
    - Assert UI values rendered from payload-derived expectations.

  - POT: For browser-tool exploration sessions using `mcp_microsoft_pla_browser_click`:
    - Prefer targeting links/buttons by accessible name from snapshot output.
    - If a click does not produce expected state change, check dialog presence and network requests before retrying.

  - POT: Keep line reporter for non-interactive agent runs (`npm run e2e` currently uses `--reporter=line` in `package.json`).
  - POT: Current Playwright config is in `Source/Client/pot-react/playwright.config.ts` with `testDir: './e2e'`; keep new tests under `Source/Client/pot-react/e2e`.
