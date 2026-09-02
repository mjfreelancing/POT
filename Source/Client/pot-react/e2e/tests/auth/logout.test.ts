import { expect, test } from '../../fixtures/auth';

test('logs out from an authenticated session and returns to login', async ({
  page,
}) => {
  // The fixture authenticates this test as e2e_admin — confirm we are in.
  await page.goto('/dashboard');
  await expect(page).toHaveURL(/\/dashboard$/);

  // /logout triggers the global logout manager, clears the session, and redirects.
  await page.goto('/logout');

  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('button', { name: 'Login' })).toBeVisible();
});
