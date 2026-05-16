import { expect, test } from '../../fixtures/auth';

test('app shell loads at root route', async ({ page }) => {
  await page.goto('/');

  await expect(page.locator('#root')).toBeVisible();
});
