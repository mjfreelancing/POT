import { expect, test } from '../../fixtures/auth';

// Covers the API-failure -> ErrorSheet path via useErrorContext and
// network/500 handling: each list page surfaces a 5xx API failure in a single
// top-of-viewport ErrorSheet (the page-level one), and the sheet can be
// dismissed. Runs on all projects (the ErrorSheet is a universal fixed overlay).
//
// A single ErrorSheet is asserted explicitly: the shared error context is now
// rendered once per page (the page-level ErrorSheet) — the child components
// (table/card grid/actions) only write to the context via setError.

const injectServerFailure = async (page: import('@playwright/test').Page) => {
  await page.route('**/api/accounts', route => {
    if (route.request().method() === 'GET') {
      return route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ detail: 'E2E injected server failure' }),
      });
    }

    return route.continue();
  });
};

test.describe('ErrorSheet on API failure (5xx)', () => {
  test('accounts page shows a single ErrorSheet and can dismiss it', async ({
    page,
  }) => {
    await injectServerFailure(page);

    await page.goto('/accounts');

    // ServerError code -> ErrorSheet title; detail -> description.
    // Exactly one ErrorSheet is rendered (page-level only).
    await expect(page.getByText('Server Error', { exact: true })).toHaveCount(
      1,
    );
    await expect(
      page.getByText('E2E injected server failure', { exact: true }),
    ).toHaveCount(1);

    const dismissButton = page.getByRole('button', { name: 'Dismiss' });
    await expect(dismissButton).toHaveCount(1);

    await dismissButton.click();

    await expect(
      page.getByText('E2E injected server failure', { exact: true }),
    ).toBeHidden();
  });

  test('expenses page shows the ErrorSheet on API failure', async ({
    page,
  }) => {
    await page.route('**/api/expenses', route => {
      if (route.request().method() === 'GET') {
        return route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ detail: 'E2E injected server failure' }),
        });
      }

      return route.continue();
    });

    await page.goto('/expenses');

    await expect(page.getByText('Server Error', { exact: true })).toHaveCount(
      1,
    );
    await expect(
      page.getByText('E2E injected server failure', { exact: true }),
    ).toHaveCount(1);
    await expect(page.getByRole('button', { name: 'Dismiss' })).toHaveCount(1);
  });

  test('incomes page shows the ErrorSheet on API failure', async ({ page }) => {
    await page.route('**/api/incomes', route => {
      if (route.request().method() === 'GET') {
        return route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ detail: 'E2E injected server failure' }),
        });
      }

      return route.continue();
    });

    await page.goto('/incomes');

    await expect(page.getByText('Server Error', { exact: true })).toHaveCount(
      1,
    );
    await expect(
      page.getByText('E2E injected server failure', { exact: true }),
    ).toHaveCount(1);
    await expect(page.getByRole('button', { name: 'Dismiss' })).toHaveCount(1);
  });

  test('users page shows the ErrorSheet on API failure', async ({ page }) => {
    await page.route('**/api/users', route => {
      if (route.request().method() === 'GET') {
        return route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ detail: 'E2E injected server failure' }),
        });
      }

      return route.continue();
    });

    await page.goto('/users');

    await expect(page.getByText('Server Error', { exact: true })).toHaveCount(
      1,
    );
    await expect(
      page.getByText('E2E injected server failure', { exact: true }),
    ).toHaveCount(1);
    await expect(page.getByRole('button', { name: 'Dismiss' })).toHaveCount(1);

    await page.getByRole('button', { name: 'Dismiss' }).click();

    await expect(
      page.getByText('E2E injected server failure', { exact: true }),
    ).toBeHidden();
  });

  test('pending approvals page shows the ErrorSheet on API failure', async ({
    page,
  }) => {
    await page.route('**/api/approvals/pending', route => {
      if (route.request().method() === 'GET') {
        return route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ detail: 'E2E injected server failure' }),
        });
      }

      return route.continue();
    });

    await page.goto('/approvals/pending');

    await expect(page.getByText('Server Error', { exact: true })).toHaveCount(
      1,
    );
    await expect(
      page.getByText('E2E injected server failure', { exact: true }),
    ).toHaveCount(1);
    await expect(page.getByRole('button', { name: 'Dismiss' })).toHaveCount(1);
  });
});
