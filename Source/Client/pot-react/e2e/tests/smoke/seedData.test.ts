import { expect, test } from '../../fixtures/auth';

test('dashboard renders seeded financial accounts from the API', async ({
  page,
}) => {
  // Register the response listener before navigating (PRD R7 pattern).
  const accountsResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/accounts') &&
      response.request().method() === 'GET',
  );

  await page.goto('/dashboard');

  const accountsResponse = await accountsResponsePromise;
  expect(accountsResponse.ok()).toBeTruthy();

  const accounts = (await accountsResponse.json()) as Array<{
    description: string;
  }>;

  // The fully-seeded database contains financial accounts (imported + renewed).
  expect(accounts.length).toBeGreaterThan(0);

  // The first seeded account's name is rendered on the dashboard (exact name).
  await expect(
    page.getByRole('heading', { name: accounts[0].description, exact: true }),
  ).toBeVisible();

  // The Accounts Overview section header confirms the summary rendered.
  await expect(
    page.getByRole('heading', { name: 'Accounts Overview', exact: true }),
  ).toBeVisible();
});
