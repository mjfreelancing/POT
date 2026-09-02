import { expect, test } from '@playwright/test';

test('app boots and routes unauthenticated visitors to the login page', async ({
  page,
}) => {
  // This test intentionally does NOT use the authenticated `test` fixture. It
  // verifies the app shell from a genuinely unauthenticated visitor, so `test`
  // is imported from '@playwright/test' (no auth storage state is applied to
  // the context).
  await page.goto('/');

  // The root route sits behind ProtectedRoute, so an unauthenticated visitor is
  // redirected to the login page — proving the app booted and routing works.
  await expect(page).toHaveURL(/\/login$/);

  // User-facing anchor: the login form rendered (replaces the fragile #root check).
  await expect(page.getByRole('button', { name: 'Login' })).toBeVisible();
});
