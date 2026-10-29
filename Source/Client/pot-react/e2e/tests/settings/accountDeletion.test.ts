import type { Browser } from '@playwright/test';
import { expect, test } from '@playwright/test';

import { listExistingUsernames } from '../../helpers/databaseSeed';
import { provisionInvitedViewer } from '../../helpers/provisioning';
import { adminCredentials, viewerCredentials } from '../../helpers/secrets';

// Covers the self-service account deletion journey: the settings block, the typed confirmation with the caller's
// own password, the return to the login page and the account being unusable afterwards.
//
// The identity is PROVISIONED for the journey rather than seeded. An invitation creates a viewer on the shared E2E
// site and the journey deletes it, so the file can run in every browser project against one database - and again
// on a later run - without consuming an identity the seed would have to keep supplying.
//
// The seeded identities are asserted intact at the end: consuming one is the failure this arrangement prevents.

test('deleting your own account returns you to the login page and blocks sign-in', async ({
  browser,
  playwright,
}) => {
  const account = await provisionInvitedViewer(playwright, 'account-deletion');

  const context = await browser.newContext({
    storageState: account.storageState,
  });

  const page = await context.newPage();

  await page.goto('/dashboard');

  await page
    .getByRole('button', { name: account.displayName, exact: true })
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

  await dialog.getByLabel('Username', { exact: true }).fill(account.username);
  await dialog
    .getByLabel('Your password', { exact: true })
    .fill(account.password);

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

  await context.close();

  await expectSignInRefused(browser, account.username, account.password);

  const seededUsernames = await listExistingUsernames([
    adminCredentials.username,
    viewerCredentials.username,
  ]);

  expect(seededUsernames.slice().sort()).toEqual(
    [adminCredentials.username, viewerCredentials.username].sort(),
  );
});

/**
 * A refused sign-in leaves the visitor on the login page with an authentication error, so the successful
 * login response is what distinguishes a deleted account from a working one.
 */
async function expectSignInRefused(
  browser: Browser,
  username: string,
  password: string,
): Promise<void> {
  const context = await browser.newContext({
    storageState: { cookies: [], origins: [] },
  });

  const page = await context.newPage();

  await page.goto('/login');
  await page.getByLabel('Username').fill(username);
  await page.getByLabel('Password').fill(password);

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
