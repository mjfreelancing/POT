import type { Browser } from '@playwright/test';

import { expect, test } from '../../fixtures/auth';

import { listExistingUsernames } from '../../helpers/databaseSeed';

const adminCredentials = {
  username: 'e2e_admin',
  password: 'E2E_admin-password',
};

const viewerCredentials = {
  username: 'e2e_viewer',
  password: 'E2E_viewer-password',
};

test('imports baseline users and allows both canonical users to login via UI', async ({
  browser,
}) => {
  const existingUsernames = await listExistingUsernames([
    'e2e_admin',
    'e2e_viewer',
  ]);

  expect(existingUsernames).toEqual(['e2e_admin', 'e2e_viewer']);

  await loginViaUi(browser, adminCredentials);
  await loginViaUi(browser, viewerCredentials);
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
