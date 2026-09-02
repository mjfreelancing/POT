import { expect, test } from '../../fixtures/auth';

// Covers Sonner toasts for mutation success/failure. Both paths use
// intercepted API responses so nothing is persisted:
// - Success: Renew Expenses Quick Action (POST /api/expenses/renew mocked 200)
//   -> SuccessToast "Expense Renewal Complete".
// - Failure: Export modal (GET /api/maintenance/export mocked 500) -> ErrorToast
//   "Export Failed". The Export... entry is hidden on mobile, so that test is
//   desktop-only.

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

test('renew expenses mutation success shows a success toast', async ({
  page,
}) => {
  await page.route('**/api/expenses/renew', route => {
    if (route.request().method() === 'POST') {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: '{}',
      });
    }

    return route.continue();
  });

  await page.goto('/dashboard');

  // The Quick Action must have data (overdue expenses from the seed) to fire.
  const renewExpensesButton = page.getByRole('button', {
    name: 'Renew Expenses',
  });
  await expect(renewExpensesButton).toBeEnabled();

  await renewExpensesButton.click();

  await expect(
    page.getByText('Expense Renewal Complete', { exact: true }),
  ).toBeVisible();
  await expect(
    page.getByText('All expense renewals have successfully processed', {
      exact: true,
    }),
  ).toBeVisible();
});

test('export mutation failure shows an error toast', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'Export... is hidden on mobile (mobile file systems); covered on desktop',
  );

  await page.route('**/api/maintenance/export', route => {
    if (route.request().method() === 'GET') {
      return route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ detail: 'E2E injected export failure' }),
      });
    }

    return route.continue();
  });

  await page.goto('/dashboard');

  await page.getByRole('button', { name: 'Export...' }).click();

  await page.getByRole('button', { name: 'Export Data' }).click();

  await expect(page.getByText('Export Failed', { exact: true })).toBeVisible();
  await expect(
    page.getByText('There was an error exporting the data.', { exact: true }),
  ).toBeVisible();
});
