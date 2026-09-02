import { expect, pwChangeTest, test } from '../../fixtures/auth';

// Covers the user settings flow: profile (display name)
// update and password change from the "POT Settings" sheet (user menu ->
// Settings).
//
// Design notes:
// - BOTH tests run as the DEDICATED seeded identity e2e_pwchange (a canonical
//   seed user with Viewer role only; its password is E2E_pwchange-password,
//   matching the E2E_<user>-password pattern of the other canonical users). No
//   other test in the matrix logs in as e2e_pwchange,
//   so its etag is NOT churned by concurrent logins — this is why the profile
//   test cannot run as e2e_admin: every fresh admin login updates
//   LastLoggedInUtc, and DbContextBase.OnBeforeSave() bumps the Etag on every
//   Modified row, so in the full matrix the admin's etag races and the profile
//   save 409s (observed: full-matrix failure where the "User Details Updated"
//   toast never appeared).
// - The file is test.describe.configure({ mode: 'serial' }) so the two tests
//   (same shared user) never run concurrently.
// - The profile test changes the display name and RESTORES it afterwards (via
//   the API: GET /api/me for the post-save etag -> PUT /api/users/{rowId}, a
//   self-service update) so the password-change test's header lookup sees a
//   stable name.
// - The password-change test rotates the password (revokes sessions + bumps
//   TokenVersion) — safe because it only affects e2e_pwchange, not the shared
//   e2e_admin session every other fixture-managed suite depends on.
//
// CHROMIUM-ONLY (parallel-collision constraint): these two tests MUTATE the
// shared canonical identity e2e_pwchange (display name + password). Every E2E
// project in a run shares ONE Testcontainers Postgres, and Playwright runs
// projects concurrently (fullyParallel). When two projects run this file at the
// same time they collide on the shared user:
//   - Profile test: both projects edit the same display name from the same
//     starting etag -> etag/409 races plus interleaved header assertions.
//   - Password test: the first project's password change bumps TokenVersion,
//     invalidating the other project's access token -> 401.
// Every other suite avoids this by creating unique per-test data, but
// the settings sheet only edits the LOGGED-IN user's own profile and only the
// canonical users have known passwords, so unique data is not possible here.
// Running this file on chromium only keeps the full 4-project matrix green; the
// flows are identical on edge (same engine), so coverage is not meaningfully
// reduced. Verified 2026-08-27: chromium alone 2/2, edge alone 2/2, chromium +
// edge together 3 failed (collision).
//
// Enforcement: playwright.config.ts excludes this file from the edge and mobile
// projects (testIgnore) so it is never COLLECTED there. An in-body test.skip()
// is NOT enough on its own — the auth fixture (login) still runs for a skipped
// test and can itself collide/fail when the other project mutates the shared
// user first (observed: edge's e2e_pwchange fixture login 401'd because
// chromium had already changed that password).

// True on every non-chromium project (edge + both mobile projects).
const isNotChromium = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name !== 'chromium';

const apiBaseUrl = 'http://127.0.0.1:5242';

const authHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

// Both tests below mutate the SAME shared user (e2e_pwchange), so they must run
// one at a time in declaration order — never concurrently within a worker.
test.describe.configure({ mode: 'serial' });

pwChangeTest(
  'updates the profile display name and restores it',
  async ({ page, accessToken, playwright }, testInfo) => {
    test.skip(
      isNotChromium(testInfo),
      'chromium-only: mutates the shared e2e_pwchange identity; parallel projects collide on the shared E2E DB',
    );

    const originalDisplayName = 'e2e_pwchange';
    const originalEmail = 'e2e_pwchange@local.test';
    const updatedDisplayName = `E2E Profile ${Date.now()}`;

    // 60s per-action timeout mirrors the test timeout (see filters.test.ts).
    const request = await playwright.request.newContext({
      baseURL: apiBaseUrl,
      timeout: 60_000,
    });

    try {
      await page.goto('/dashboard');

      // Open the user menu, then the POT Settings sheet.
      await page.getByRole('button', { name: originalDisplayName }).click();
      await page.getByRole('menuitem', { name: 'Settings' }).click();
      await expect(
        page.getByText('POT Settings', { exact: true }),
      ).toBeVisible();

      // Open the "User Details" section and confirm the pre-filled display name.
      await page.getByRole('button', { name: /User Details/ }).click();

      const displayNameInput = page.getByLabel('Display Name', { exact: true });
      await expect(displayNameInput).toHaveValue(originalDisplayName);

      // Change the display name and save.
      await displayNameInput.fill(updatedDisplayName);
      await page
        .getByRole('button', { name: 'Update User Details', exact: true })
        .click();

      // The success toast confirms the update. Generous timeout: under full-matrix
      // load the shared stack (one Vite dev server + one API + one Postgres) can
      // delay the save response and client render well past the 10s default.
      await expect(
        page.getByText('User Details Updated', { exact: true }),
      ).toBeVisible({ timeout: 20000 });
    } finally {
      // ALWAYS restore the original display name via the API (self-service
      // UpdateUser — no permission required), so a RETRY (or the password-change
      // test that follows, serialized) sees a clean state even if the UI
      // assertions above failed after the save had already committed. The UI save
      // bumped the etag, so fetch the current row from /api/me first (works for a
      // Viewer). Done via the API rather than a second sheet visit because the
      // close/reopen flow proved load-sensitive on the shared stack.
      try {
        const meResponse = await request.get('/api/me', {
          headers: authHeaders(accessToken),
        });
        expect(meResponse.ok()).toBeTruthy();

        const me = (await meResponse.json()) as {
          rowId: string;
          etag: number;
        };
        expect(me.rowId).toBeTruthy();

        const restoreResponse = await request.put(`/api/users/${me.rowId}`, {
          headers: authHeaders(accessToken),
          data: {
            displayName: originalDisplayName,
            email: originalEmail,
            etag: me.etag,
          },
        });
        expect(restoreResponse.ok()).toBeTruthy();
      } finally {
        await request.dispose();
      }
    }
  },
);

pwChangeTest(
  'changes the password and shows the success dialog',
  async ({ page }, testInfo) => {
    test.skip(
      isNotChromium(testInfo),
      'chromium-only: mutates the shared e2e_pwchange identity; parallel projects collide on the shared E2E DB',
    );

    const newPassword = 'E2E_new-password-1!';

    await page.goto('/dashboard');

    // Open the user menu, then the POT Settings sheet.
    await page.getByRole('button', { name: 'e2e_pwchange' }).click();
    await page.getByRole('menuitem', { name: 'Settings' }).click();
    await expect(page.getByText('POT Settings', { exact: true })).toBeVisible();

    // Open the "Change Password" section.
    await page.getByRole('button', { name: /Change Password/ }).click();

    // Fill the current password (E2E_pwchange-password, the canonical value from
    // secrets.ts) plus a new password that meets the schema requirements.
    await page
      .getByLabel('Current Password', { exact: true })
      .fill('E2E_pwchange-password');
    await page.getByLabel('New Password', { exact: true }).fill(newPassword);
    await page
      .getByLabel('Confirm New Password', { exact: true })
      .fill(newPassword);

    await page
      .getByRole('button', { name: 'Change Password', exact: true })
      .click();

    // The dialog only closes via "Sign In with New Password", so assert it stays.
    // Generous timeout (30s): the change-password POST does 2x PBKDF2 (verify the
    // current password + hash the new one) plus session revocation on the server,
    // which under full-matrix CPU load took ~10s (observed in the server log: the
    // handler started 10s before the 10s assertion timed out and committed just
    // after it — the retry then failed at the fixture login because the password
    // had already rotated). A long wait here keeps attempt 1 green and avoids that
    // one-way-mutation retry trap. The dialog renders right after the response, so
    // this does NOT add 30s on top of the POST latency.
    await expect(
      page.getByText('Password Changed Successfully', { exact: true }),
    ).toBeVisible({ timeout: 30000 });
    await expect(
      page.getByRole('button', { name: 'Sign In with New Password' }),
    ).toBeVisible({ timeout: 30000 });
  },
);
