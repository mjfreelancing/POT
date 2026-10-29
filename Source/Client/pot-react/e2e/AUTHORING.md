# POT E2E — Authoring Guide (day-to-day)

How to add or update a Playwright E2E test for POT. Read this + `e2e/README.md` (architecture)
before writing a test. The agent rules that load automatically are in
`.github/instructions/playwright-e2e.instructions.md`.

---

## Where tests live

`Source/Client/pot-react/e2e/tests/<area>/<name>.test.ts`, grouped by behavior:
`auth/`, `smoke/`, `rendering/`, `feedback/`, `crud/`, `dashboard/`, `filters/`,
`approvals/`, `settings/`, `maintenance/`, `mobile/`, `picker/`, `table/`, `dialogs/`,
`theme/`, `emptystates/`, `pwa/`. Follow the existing naming (`<noun><verb>.test.ts`).

## Pick the test class FIRST

| Class               | Use when                                                 | Structure                                                                                        |
| ------------------- | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| **parallel-safe**   | read-only: GETs / UI-only, never mutates shared state    | plain `test(...)`; no teardown                                                                   |
| **fixture-managed** | creates/deletes its OWN records                          | `test.describe.serial(...)`; `afterEach` cleanup; reuse `accessToken` via ONE request context    |
| **destructive**     | destroys a tenant or an identity (account/site deletion) | `e2e/helpers/provisioning.ts` creates the tenant, the journey destroys it; runs in every project |

A test is **parallel-safe only** if it never calls a state-changing endpoint and never asserts
shared mutable state. When in doubt, make it **fixture-managed**. A journey that destroys what
it runs against is **destructive**: it must provision that tenant rather than consume a seeded
one (see below), because a seeded identity can only be destroyed once per database.

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

## Destructive journeys provision their own tenant

A journey that deletes the account or the site it runs as must create that tenant itself.
`e2e/helpers/provisioning.ts` drives the product's own routes to do it:

- `provisionInvitedViewer(playwright, label)` invites a viewer onto the shared E2E site — for a
  journey that deletes the identity and nothing else.
- `provisionSiteAdministrator(playwright, label)` signs up, approves and signs in — for a journey
  that deletes the site, because a sign-up creates the site and its only administrator.

Both return `{ username, password, displayName, siteName, accessToken, storageState }`, read from
the application rather than assumed, and both are uniquely named per call. That is what lets the
file run in every browser project against one database, and again on a later run, without a seed
identity being consumed. Drive the journey in your own context:

```ts
test('deletes its own tenant', async ({ browser, playwright }) => {
  const administrator = await provisionSiteAdministrator(
    playwright,
    'site-deletion',
  );
  const context = await browser.newContext({
    storageState: administrator.storageState,
  });
  const page = await context.newPage();
  // ... use administrator.siteName / username / password in the confirmation
  await context.close();
});
```

The two secrets a provisioning flow needs (the sign-up's temporary password and the invitation's)
exist only in the message the application sent, so they are read from the sink
(`e2e/helpers/mail.ts`). Assert the seeded identities are still present at the end: consuming one
is the failure this arrangement exists to prevent.

## Locator preference

`getByRole` -> `getByLabel` -> `getByText` -> stable test id. Never style classes as
selectors. Full POT anchor catalog in the instruction file.

## Data-readiness (avoid waitForResponse races)

- **Read-only lists**: PREFETCH via `playwright.request.newContext({ baseURL: apiBaseUrl,
timeout: 60_000 })` + `GET /api/<list>` with the accessToken, then assert the UI web-first
  (see `mobileCardGrids.test.ts`). `page.waitForResponse` resolves on headers and can be
  load-starved past the test timeout.
- **After a slow POST** (renew/change-password): capture the response, assert
  `response.ok()`, `await response.finished()` (wait for the body — `waitForResponse` resolves
  on headers), THEN assert the UI signal (toast/dialog) with documented headroom
  `{ timeout: 30_000 }` (see `quickActions.test.ts`).

## Common traps (condensed)

- `locator.filter({ hasText })` is a **substring** match — anchor with `^...$` when the text
  could be a prefix/substring of siblings (calendar day "1" also matches 10-19/21).
- Create/edit form "Description" collides with the page "Search … by description" — use
  `getByRole('textbox', { name: 'Description', exact: true })`.
- Radix `Select`: use `selectRadixOption` (`e2e/helpers/radix.ts`) — click the `combobox`
  trigger, then the keyboard/AT path (`data-highlighted` gate + focus + Enter + `toBeHidden`).
  Never `.click()` or `evaluate()` dispatch an option.
- MODAL dialogs close on Escape/backdrop; always-open create sheets do NOT (form Cancel /
  back-nav only). `DialogContent` has no X by default (`showCloseButton=false`).
- A table description cell often shares text with badges/popovers — use substring
  `getByText`, not `{ exact: true }`.
- Desktop-only flows: `test.skip(isMobileProject(testInfo), '...')` (mobile uses the card
  grid — see `e2e/tests/mobile/`).

## Run commands (from `Source/Client/pot-react`)

```bash
npm run e2e:preflight                         # environment check before a run (read-only)
npm run e2e:chromium                          # fastest loop (one project)
npx playwright test <path> --project=chromium # targeted file
npm run e2e:mobile                            # mobile projects
npm run e2e:all:dev                           # full 4-project matrix (the gate)
npm run e2e:all:dev:log                       # as e2e:all:dev, capturing console + port timeline
npm run e2e:prodlike                          # built-client subset (on-demand)
```

Validate the way the repo does: targeted chromium -> all 4 projects -> (for a gate) the full
matrix twice / three consecutive clean runs. **No flaky tests**: fix the root cause; a retry
is a safety net, not a fix.

