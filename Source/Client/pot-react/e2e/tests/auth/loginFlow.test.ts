import { expect, test } from '@playwright/test';

import { adminCredentials } from '../../helpers/secrets';

test.describe('login flow', () => {
  // Plain fill-and-submit. A retry loop was previously needed because the Vite
  // dev server could full-reload the page under parallel load, wiping the form
  // mid-click. E2E now runs with Vite HMR disabled (E2E=1 -> server.hmr:false),
  // so the page can never be reloaded behind a test and no retry is required.
  // The valid test waits for /dashboard; the invalid test waits for the inline
  // error while staying on /login.
  async function fillAndSubmitLogin(
    page: import('@playwright/test').Page,
    password: string,
    expectDashboard: boolean,
  ) {
    await page.goto('/login');

    await page.getByLabel('Username').fill(adminCredentials.username);
    await page.getByLabel('Password').fill(password);
    await page.getByRole('button', { name: 'Login' }).click();

    if (expectDashboard) {
      await expect(page).toHaveURL(/\/dashboard$/);
    } else {
      await expect(page.getByRole('alert')).toBeVisible();
    }
  }

  test('logs in with valid credentials and reaches the dashboard', async ({
    page,
  }) => {
    await fillAndSubmitLogin(page, adminCredentials.password, true);

    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test('shows an inline error for invalid credentials and stays on login', async ({
    page,
  }) => {
    await fillAndSubmitLogin(page, 'definitely-wrong-password', false);

    // Invalid credentials surface as an inline role="alert" in the login form.
    await expect(page.getByRole('alert')).toBeVisible();
    await expect(page).toHaveURL(/\/login$/);
  });
});
