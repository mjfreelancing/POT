# POT E2E — Authoring Guide (day-to-day)

How to add or update a Playwright E2E test for POT. Read this + `e2e/README.md` (architecture)
before writing a test. Requirements live in PRD 012
(`Docs/Future/PRD-012-playwright-e2e-testing-infrastructure.md`); the implemented design and
design history are in ADR 012 (`Docs/Future/ADR-012-playwright-e2e-testing-infrastructure.md`);
the agent rules that load automatically are in
`.github/instructions/playwright-e2e.instructions.md`.

---

## Where tests live

`Source/Client/pot-react/e2e/tests/<area>/<name>.test.ts`, grouped by behavior:
`auth/`, `smoke/`, `rendering/`, `feedback/`, `crud/`, `dashboard/`, `filters/`,
`approvals/`, `settings/`, `maintenance/`, `mobile/`, `picker/`, `table/`, `dialogs/`,
`theme/`, `emptystates/`, `pwa/`. Follow the existing naming (`<noun><verb>.test.ts`).

## Pick the test class FIRST

| Class               | Use when                                              | Structure                                                                                     |
| ------------------- | ----------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| **parallel-safe**   | read-only: GETs / UI-only, never mutates shared state | plain `test(...)`; no teardown                                                                |
| **fixture-managed** | creates/deletes its OWN records                       | `test.describe.serial(...)`; `afterEach` cleanup; reuse `accessToken` via ONE request context |

A test is **parallel-safe only** if it never calls a state-changing endpoint and never asserts
shared mutable state. When in doubt, make it **fixture-managed**.

## Authentication

- `e2e/fixtures/auth.ts` exports `test` (admin), `viewerTest` (viewer), `pwChangeTest`
  (e2e_pwchange). Each does ONE fresh login and gives BOTH `storageState` and `accessToken`.
- For API setup/cleanup use `accessToken` — never a second login (logins are the biggest CPU
  cost under parallel load). Do NOT share storage state across tests/workers (the refresh
  cookie rotates on every `/api/auth/refresh`).
- For LOGGED-OUT pages (login page, signup/password-reset dialogs, PWA contract): do NOT use
  an auth fixture — `browser.newContext({ storageState: { cookies: [], origins: [] } })`,
  then `page.goto('/login')` (see `dialogs/modalDialogs.test.ts`). Name the base import
  `baseTest` and use `adminTest.skip(...)` (a bare `test(`/`test.skip` raises ReferenceError).

## Locator preference

`getByRole` -> `getByLabel` -> `getByText` -> stable test id. Never style classes as
selectors. Full POT anchor catalog in the instruction file.

## Data-readiness (avoid waitForResponse races)

- **Read-only lists**: PREFETCH via `playwright.request.newContext({ baseURL: apiBaseUrl,
timeout: 60_000 })` + `GET /api/<list>` with the accessToken, then assert the UI web-first
  (see `mobileCardGrids.test.ts`). `page.waitForResponse` resolves on headers and can be
  load-starved past the test timeout.
- **After a slow POST** (renew/accrue/change-password): capture the response, assert
  `response.ok()`, `await response.finished()` (wait for the body — `waitForResponse` resolves
  on headers), THEN assert the UI signal (toast/dialog) with documented headroom
  `{ timeout: 30_000 }` (see `quickActions.test.ts`).

## Common traps (condensed)

- `locator.filter({ hasText })` is a **substring** match — anchor with `^...$` when the text
  could be a prefix/substring of siblings (calendar day "1" also matches 10-19/21).
- Create/edit form "Description" collides with the page "Search … by description" — use
  `getByRole('textbox', { name: 'Description', exact: true })`.
- Radix `Select`: use `selectRadixOption` (keyboard/AT path: `data-highlighted` gate + focus +
  Enter + `toBeHidden`) — never `.click()` or `evaluate()` dispatch.
- MODAL dialogs close on Escape/backdrop; always-open create sheets do NOT (form Cancel /
  back-nav only). `DialogContent` has no X by default (`showCloseButton=false`).
- A table description cell often shares text with badges/popovers — use substring
  `getByText`, not `{ exact: true }`.
- Desktop-only flows: `test.skip(isMobileProject(testInfo), '...')` (mobile uses the card
  grid — see `e2e/tests/mobile/`).

## Run commands (from `Source/Client/pot-react`)

```bash
npm run e2e:chromium                          # fastest loop (one project)
npx playwright test <path> --project=chromium # targeted file
npm run e2e:mobile                            # mobile projects
npm run e2e:all:dev                           # full 4-project matrix (the gate)
npm run e2e:prodlike                          # built-client subset (on-demand)
```

Validate the way the repo does: targeted chromium -> all 4 projects -> (for a gate) the full
matrix twice / three consecutive clean runs. **No flaky tests**: fix the root cause; a retry
is a safety net, not a fix.
