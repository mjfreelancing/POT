import type { Browser, Locator, Page, TestInfo } from '@playwright/test';

import { expect, siteDeleteTest, test, viewerTest } from '../../fixtures/auth';
import { listExistingUsernames } from '../../helpers/databaseSeed';
import {
  adminCredentials,
  siteDeleteCredentials,
  siteDeleteSiteName,
  viewerCredentials,
} from '../../helpers/secrets';

// Covers the site deletion row in the danger zone: who is offered it, the three-part confirmation, the
// successful deletion of a throwaway site, and the server refusal for a site that holds the last enabled
// platform administrator.
//
// The successful deletion runs as the DEDICATED seeded identity e2e_sitedelete (an Admin on its own site,
// see baseline.sql) because the journey destroys the site. It is chromium-only for the same reason
// accountDeletion.test.ts is: every project in a run shares ONE Testcontainers Postgres, so a second project
// would find the site already gone. The configs exclude this file from the other projects.
//
// NO RETRIES: the site and its administrator are gone after the first attempt, so a retry could never sign
// in and would surface as a misleading auth-fixture failure. The seed is verified below instead, because a
// regenerated baseline.sql that drops the identity is the usual reason it goes missing.
//
// The shared E2E site cannot be deleted in the harness because it holds the seeded platform administrator;
// that safeguard is asserted below.
//
// Desktop-only: the settings sheet is opened from the user menu, which the mobile projects render
// differently; the row itself is shared markup.

const isMobileProject = (testInfo: TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const isNotChromium = (testInfo: TestInfo) =>
  testInfo.project.name !== 'chromium';

const sharedSiteName = 'POT E2E Site';

// Declared in the file so the CLI --retries flag cannot re-enable them.
test.describe.configure({ retries: 0 });

test.beforeAll(async () => {
  const existingUsernames = await listExistingUsernames([
    siteDeleteCredentials.username,
  ]);

  if (!existingUsernames.includes(siteDeleteCredentials.username)) {
    throw new Error(
      `${siteDeleteCredentials.username} is missing from the E2E seed. Regenerate e2e/seed/baseline.sql.`,
    );
  }
});

viewerTest(
  'offers only the account deletion to a non-administrator',
  async ({ page }, testInfo) => {
    test.skip(isMobileProject(testInfo), 'Desktop settings sheet');

    await openDangerZone(page, viewerCredentials.username);

    await expect(
      page.getByRole('button', { name: 'Delete my account' }),
    ).toBeVisible();
    await expect(
      page.getByRole('button', { name: 'Delete this site' }),
    ).toHaveCount(0);
  },
);

siteDeleteTest(
  'deletes a site of its own and blocks further sign-in',
  async ({ page, browser }, testInfo) => {
    test.skip(
      isNotChromium(testInfo),
      'chromium-only: deletes the shared seeded e2e_sitedelete identity',
    );

    await openDangerZone(page, siteDeleteCredentials.username);

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
      dialog.getByRole('heading', { name: `Delete ${siteDeleteSiteName}` }),
    ).toBeVisible();

    const confirmButton = dialog.getByRole('button', {
      name: 'Delete Site',
      exact: true,
    });

    await expect(confirmButton).toBeDisabled();

    await fillSiteConfirmation(
      dialog,
      siteDeleteSiteName,
      siteDeleteCredentials.username,
    );

    await expect(confirmButton).toBeDisabled();

    await dialog
      .getByLabel('Your password', { exact: true })
      .fill(siteDeleteCredentials.password);

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

    await expectSignInRefused(
      browser,
      siteDeleteCredentials.username,
      siteDeleteCredentials.password,
    );
  },
);

test(
  'refuses to delete a site that holds the last platform administrator',
  async ({ page }, testInfo) => {
    test.skip(isMobileProject(testInfo), 'Desktop settings sheet');

    await openDangerZone(page, adminCredentials.username);
    await page.getByRole('button', { name: 'Delete this site' }).click();

    const dialog = page.getByRole('alertdialog');
    await expect(dialog).toBeVisible();

    await fillSiteConfirmation(
      dialog,
      sharedSiteName,
      adminCredentials.username,
    );
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
  },
);

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
