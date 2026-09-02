import type { Browser } from '@playwright/test';

import { expect, test } from '../../fixtures/auth';

import { listExistingUsernames } from '../../helpers/databaseSeed';
import { adminCredentials, viewerCredentials } from '../../helpers/secrets';

test.describe('E2E baseline smoke', () => {
  test('baseline seed contains all canonical users', async () => {
    // e2e_pwchange is a dedicated identity for the user-settings password-change
    // E2E test, with its own unique password E2E_pwchange-password (Viewer role;
    // see baseline.sql + secrets.ts).
    const existingUsernames = await listExistingUsernames([
      'e2e_admin',
      'e2e_viewer',
      'e2e_pwchange',
    ]);

    expect(existingUsernames).toEqual([
      'e2e_admin',
      'e2e_pwchange',
      'e2e_viewer',
    ]);
  });

  test('e2e_admin can log in via UI and reach the dashboard', async ({
    browser,
  }) => {
    await loginViaUi(browser, adminCredentials);
  });

  test('e2e_viewer can log in via UI and reach the dashboard', async ({
    browser,
  }) => {
    await loginViaUi(browser, viewerCredentials);
  });
});

async function loginViaUi(
  browser: Browser,
  credentials: { username: string; password: string },
): Promise<void> {
  // Pass an explicit empty storageState to prevent Playwright from inheriting the
  // worker's auth cookie. This test must verify login from a truly unauthenticated state.
  const context = await browser.newContext({
    storageState: { cookies: [], origins: [] },
  });

  const page = await context.newPage();

  await page.goto('/login');
  await page.getByLabel('Username').fill(credentials.username);
  await page.getByLabel('Password').fill(credentials.password);

  const loginResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/auth/login') &&
      response.request().method() === 'POST',
  );

  await page.getByRole('button', { name: 'Login' }).click();

  const loginResponse = await loginResponsePromise;
  const loginResponseBody = await loginResponse.text();

  expect(
    loginResponse.ok(),
    `Login API failed for ${credentials.username}: ${loginResponse.status()} ${loginResponseBody}`,
  ).toBeTruthy();

  await expect(page).toHaveURL(/\/dashboard$/);

  const cookies = await context.cookies();
  expect(cookies.length).toBeGreaterThan(0);

  await context.close();
}
