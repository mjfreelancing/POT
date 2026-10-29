import type { Browser, Locator, Page } from '@playwright/test';

import { expect, test, viewerTest } from '../../fixtures/auth';
import { listExistingUsernames } from '../../helpers/databaseSeed';
import { provisionSiteAdministrator } from '../../helpers/provisioning';
import { adminCredentials, viewerCredentials } from '../../helpers/secrets';

// Covers the site deletion row in the danger zone: who is offered it, the three-part confirmation, the successful
// deletion of a throwaway site, and the server refusal for a site that holds the last enabled platform
// administrator.
//
// The site and its administrator are PROVISIONED for the journey rather than seeded: a sign-up creates a site whose
// only administrator is the sign-up, and the journey deletes that site. Provisioning is what lets the file run in
// every browser project against one database - and again on a later run - without consuming the seeded identity it
// used to destroy.
//
// The seeded identities and the shared site are asserted intact at the end: consuming one is the failure this
// arrangement prevents.
//
// The shared E2E site cannot be deleted in the harness because it holds the seeded platform administrator; that
// safeguard is asserted without changing anything, so it stays parallel-safe.

const sharedSiteName = 'POT E2E Site';

viewerTest(
  'offers only the account deletion to a non-administrator',
  async ({ page }) => {
    await openDangerZone(page, viewerCredentials.username);

    await expect(
      page.getByRole('button', { name: 'Delete my account' }),
    ).toBeVisible();
    await expect(
      page.getByRole('button', { name: 'Delete this site' }),
    ).toHaveCount(0);
  },
);

test('deletes a site of its own and blocks further sign-in', async ({
  browser,
  playwright,
}) => {
  const administrator = await provisionSiteAdministrator(
    playwright,
    'site-deletion',
  );

  const context = await browser.newContext({
    storageState: administrator.storageState,
  });

  const page = await context.newPage();

  await openDangerZone(page, administrator.displayName);

  // The account action keeps its position for every role; the site action escalates below it.
  const actions = page.getByRole('button', {
    name: /Delete my account|Delete this site/,
  });

  await expect(actions).toHaveCount(2);
  await expect(actions.first()).toHaveText('Delete my account');
  await expect(actions.last()).toHaveText('Delete this site');

  await page.getByRole('button', { name: 'Delete this site' }).click();

  const dialog = page.getByRole('alertdialog');
  await expect(dialog).toBeVisible();
  await expect(
    dialog.getByRole('heading', { name: `Delete ${administrator.siteName}` }),
  ).toBeVisible();

  const confirmButton = dialog.getByRole('button', {
    name: 'Delete Site',
    exact: true,
  });

  await expect(confirmButton).toBeDisabled();

  await fillSiteConfirmation(
    dialog,
    administrator.siteName,
    administrator.username,
  );

  await expect(confirmButton).toBeDisabled();

  await dialog
    .getByLabel('Your password', { exact: true })
    .fill(administrator.password);

  await expect(confirmButton).toBeEnabled();

  const deleteResponse = page.waitForResponse(
    response =>
      response.url().includes('/api/sites/current') &&
      response.request().method() === 'DELETE',
  );

  await confirmButton.click();

  const deleted = await deleteResponse;

  expect(deleted.ok()).toBeTruthy();
  await deleted.finished();

  await expect(page).toHaveURL(/\/login$/);
  await expect(
    page.getByText('Your site has been deleted.', { exact: true }),
  ).toBeVisible();

  await context.close();

  await expectSignInRefused(
    browser,
    administrator.username,
    administrator.password,
  );

  const seededUsernames = await listExistingUsernames([
    adminCredentials.username,
    viewerCredentials.username,
  ]);

  expect(seededUsernames.slice().sort()).toEqual(
    [adminCredentials.username, viewerCredentials.username].sort(),
  );
});

test('refuses to delete a site that holds the last platform administrator', async ({
  page,
}) => {
  await openDangerZone(page, adminCredentials.username);
  await page.getByRole('button', { name: 'Delete this site' }).click();

  const dialog = page.getByRole('alertdialog');
  await expect(dialog).toBeVisible();

  await fillSiteConfirmation(dialog, sharedSiteName, adminCredentials.username);
  await dialog
    .getByLabel('Your password', { exact: true })
    .fill(adminCredentials.password);

  const deleteResponse = page.waitForResponse(
    response =>
      response.url().includes('/api/sites/current') &&
      response.request().method() === 'DELETE',
  );

  await dialog
    .getByRole('button', { name: 'Delete Site', exact: true })
    .click();

  const refused = await deleteResponse;

  expect(refused.status()).toBe(400);
  await refused.finished();

  // The refusal is read inside the confirmation, which stays open, and the caller stays signed in.
  await expect(dialog.getByRole('alert')).toHaveText(
    'This site contains the last enabled platform administrator. Another platform administrator must be configured first.',
    { timeout: 30000 },
  );
  await expect(dialog).toBeVisible();

  await dialog.getByRole('button', { name: 'Cancel', exact: true }).click();
  await expect(dialog).toBeHidden();
  await expect(page).toHaveURL(/\/dashboard$/);
});

/** Types the site name and username, which are two of the three parts of the confirmation. */
async function fillSiteConfirmation(
  dialog: Locator,
  siteName: string,
  username: string,
) {
  await dialog.getByLabel('Site name', { exact: true }).fill(siteName);
  await dialog.getByLabel('Username', { exact: true }).fill(username);
}

/**
 * A refused sign-in leaves the visitor on the login page with an authentication error, so the successful
 * login response is what distinguishes a deleted site from a working one.
 */
async function expectSignInRefused(
  browser: Browser,
  username: string,
  password: string,
) {
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

/** Opens the settings sheet from the user menu and expands the danger zone. */
async function openDangerZone(page: Page, displayName: string) {
  await page.goto('/dashboard');

  await page.getByRole('button', { name: displayName, exact: true }).click();
  await page.getByRole('menuitem', { name: 'Settings', exact: true }).click();

  const dangerZone = page.getByRole('button', { name: /Danger Zone/ });

  await dangerZone.scrollIntoViewIfNeeded();
  await dangerZone.click();
}
