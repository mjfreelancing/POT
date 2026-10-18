import type { Browser } from '@playwright/test';

import { expect, selfDeleteTest as test } from '../../fixtures/auth';
import { listExistingUsernames } from '../../helpers/databaseSeed';
import { selfDeleteCredentials } from '../../helpers/secrets';

// Covers the self-service account deletion journey: the settings block, the
// typed confirmation with the caller's own password, the return to the login
// page and the account being unusable afterwards.
//
// Runs as the DEDICATED seeded identity e2e_selfdelete (Viewer on the shared E2E
// site, see baseline.sql) because the journey deletes the account. It is
// chromium-only for the same reason userSettings.test.ts is: every project in a
// run shares ONE Testcontainers Postgres, so a second project would find the
// account already gone. The configs exclude this file from the other projects.
//
// NO RETRIES: the account is gone after the first attempt, so a retry could never sign in and would
// surface as a misleading auth-fixture failure. The seed is verified below instead, because a regenerated
// baseline.sql that drops the identity is the usual reason it goes missing.

const isNotChromium = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name !== 'chromium';

// Declared in the file so the CLI --retries flag cannot re-enable them.
test.describe.configure({ retries: 0 });

test.beforeAll(async () => {
  const existingUsernames = await listExistingUsernames([
    selfDeleteCredentials.username,
  ]);

  if (!existingUsernames.includes(selfDeleteCredentials.username)) {
    throw new Error(
      `${selfDeleteCredentials.username} is missing from the E2E seed. Regenerate e2e/seed/baseline.sql.`,
    );
  }
});

test('deleting your own account returns you to the login page and blocks sign-in', async ({
  page,
  browser,
}, testInfo) => {
  test.skip(
    isNotChromium(testInfo),
    'chromium-only: deletes the shared seeded e2e_selfdelete identity',
  );

  await page.goto('/dashboard');

  await page
    .getByRole('button', { name: selfDeleteCredentials.username, exact: true })
    .click();
  await page.getByRole('menuitem', { name: 'Settings', exact: true }).click();

  await page.getByRole('button', { name: 'Danger Zone' }).click();
  await page.getByRole('button', { name: 'Delete my account' }).click();

  const dialog = page.getByRole('alertdialog');
  await expect(dialog).toBeVisible();

  const confirmButton = dialog.getByRole('button', {
    name: 'Delete Account',
    exact: true,
  });
  await expect(confirmButton).toBeDisabled();

  await dialog
    .getByLabel('Username', { exact: true })
    .fill(selfDeleteCredentials.username);
  await dialog
    .getByLabel('Your password', { exact: true })
    .fill(selfDeleteCredentials.password);

  await expect(confirmButton).toBeEnabled();

  const deleteResponse = page.waitForResponse(
    response =>
      response.url().includes('/api/me') &&
      response.request().method() === 'DELETE',
  );

  await confirmButton.click();

  const deleted = await deleteResponse;
  expect(deleted.ok()).toBeTruthy();
  await deleted.finished();

  await expect(page).toHaveURL(/\/login$/);
  await expect(
    page.getByText('Your account has been deleted.', { exact: true }),
  ).toBeVisible();

  await expectSignInRefused(browser);
});

/**
 * A refused sign-in leaves the visitor on the login page with an authentication error, so the successful
 * login response is what distinguishes a deleted account from a working one.
 */
async function expectSignInRefused(browser: Browser): Promise<void> {
  const context = await browser.newContext({
    storageState: { cookies: [], origins: [] },
  });

  const page = await context.newPage();

  await page.goto('/login');
  await page.getByLabel('Username').fill(selfDeleteCredentials.username);
  await page.getByLabel('Password').fill(selfDeleteCredentials.password);

  const loginResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/auth/login') &&
      response.request().method() === 'POST',
  );

  await page.getByRole('button', { name: 'Login' }).click();

  const loginResponse = await loginResponsePromise;

  expect(loginResponse.ok()).toBeFalsy();
  await expect(page).toHaveURL(/\/login$/);

  await context.close();
}
