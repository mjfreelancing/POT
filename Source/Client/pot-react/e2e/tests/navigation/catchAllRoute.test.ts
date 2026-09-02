import { expect, test } from '../../fixtures/auth';

test.describe('catch-all routing', () => {
  test('unknown paths redirect to /dashboard', async ({ page }) => {
    await page.goto('/this-page-does-not-exist');

    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(
      page.getByRole('heading', { name: 'Financial Dashboard', exact: true }),
    ).toBeVisible();
  });

  test('root path redirects to /dashboard', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(
      page.getByRole('heading', { name: 'Financial Dashboard', exact: true }),
    ).toBeVisible();
  });
});
