import { expect, test as unauthenticatedTest } from '@playwright/test';

import { test as authenticatedTest } from '../../fixtures/auth';

// Unauthenticated visitors are redirected away from protected routes.
unauthenticatedTest(
  'redirects unauthenticated visitors to /login from protected routes',
  async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login$/);

    await page.goto('/expenses');
    await expect(page).toHaveURL(/\/login$/);
  },
);

// An authenticated session can deep-link straight into a protected page.
authenticatedTest(
  'loads a protected page when authenticated (deep link)',
  async ({ page }) => {
    await page.goto('/expenses');

    await expect(page).toHaveURL(/\/expenses$/);
    await expect(
      page.getByRole('button', { name: 'Add a new expense' }).first(),
    ).toBeVisible();
  },
);
