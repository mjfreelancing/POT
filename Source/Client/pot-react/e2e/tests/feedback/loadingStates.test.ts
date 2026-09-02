import { expect, test } from '../../fixtures/auth';

// Covers loading states: while the accounts list request is in flight the
// LoadingOverlay (role="status", aria-label="Loading") is visible, then it
// disappears once the data resolves. The request is delayed via an intercepted
// passthrough so the assertion window is deterministic.
// Runs on all projects.

test('accounts page shows the loading overlay while data is fetching', async ({
  page,
}) => {
  await page.route('**/api/accounts', async route => {
    if (route.request().method() !== 'GET') {
      return route.continue();
    }

    await new Promise(resolve => setTimeout(resolve, 2500));

    return route.continue();
  });

  await page.goto('/accounts');

  const loadingSpinner = page.getByRole('status', { name: 'Loading' });
  await expect(loadingSpinner).toBeVisible();

  // Once the delayed response resolves, the overlay is removed.
  await expect(loadingSpinner).toBeHidden();

  // The underlying data still renders (real response passed through).
  await expect(
    page.getByRole('heading', { name: 'Account Management', exact: true }),
  ).toBeVisible();
});
