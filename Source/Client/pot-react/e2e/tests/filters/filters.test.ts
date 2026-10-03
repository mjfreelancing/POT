import type { APIRequestContext } from '@playwright/test';

import { expect, test } from '../../fixtures/auth';
import {
    authHeaders,
    createE2eRequestContext as createRequestContext,
    createExpenseViaApi,
    createIncomeViaApi,
    deleteExpenseViaApi,
    deleteIncomeViaApi,
} from '../../helpers/api';
import { toIsoDate } from '../../helpers/dates';
import { selectRadixOption } from '../../helpers/radix';

// Covers the filters: the SearchInput (with its
// "Clear search input" button) and the AccountFilter ("Filter by account")
// select, including its URL query-string contract — the `accountId`
// URL parameter is the single render-time source of truth (RULE 1), so
// selecting an account writes `?accountId=<id>`, choosing "All Accounts" clears
// it, deep links pre-filter, and invalid account ids fall back to unfiltered.
//
// Fixture-managed, serial suite: unique expenses/incomes are created through
// the API and removed in afterEach, so the suite is re-runnable without
// contamination.
//
// Desktop-only: filter UI is exercised on desktop; mobile filter/layout
// behavior is covered by mobileCardGrids.test.ts.

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// RowIds created during this serial suite, cleaned up in afterEach.
const createdExpenseRowIds: string[] = [];
const createdIncomeRowIds: string[] = [];

async function getAccountsViaApi(
  request: APIRequestContext,
  accessToken: string,
): Promise<{ rowId: string; description: string }[]> {
  const response = await request.get('/api/accounts', {
    headers: authHeaders(accessToken),
  });

  expect(response.ok()).toBeTruthy();

  const accounts = (await response.json()) as {
    rowId: string;
    description: string;
  }[];

  expect(accounts.length).toBeGreaterThan(1);

  return accounts;
}

test.describe.serial('Filters (fixture-managed)', () => {
  test.afterEach(async ({ playwright, accessToken }) => {
    const request = await createRequestContext(playwright);

    try {
      for (const rowId of createdExpenseRowIds.splice(0)) {
        await deleteExpenseViaApi(request, accessToken, rowId);
      }

      for (const rowId of createdIncomeRowIds.splice(0)) {
        await deleteIncomeViaApi(request, accessToken, rowId);
      }
    } finally {
      await request.dispose();
    }
  });

  test('expense search filter narrows, clears, and shows the no-match empty state', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Filter toolbar is desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const now = new Date();
    const futureDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() + 30),
    );

    const request = await createRequestContext(playwright);

    try {
      const accounts = await getAccountsViaApi(request, accessToken);
      const stamp = Date.now();
      const description = `E2E Filter Alpha ${stamp}`;

      const expense = await createExpenseViaApi(
        request,
        accessToken,
        accounts[0].rowId,
        { description, nextDue: futureDate, amount: 30 },
      );

      createdExpenseRowIds.push(expense.rowId);

      // ---- Type a matching substring into the search input.
      await page.goto('/expenses');

      const searchInput = page.getByRole('textbox', {
        name: 'Search expenses by description',
      });

      await searchInput.fill(description);

      await expect(
        page.getByRole('row').filter({ hasText: description }),
      ).toBeVisible();
      await expect(page.getByText(/Showing 1 of \d+ items/)).toBeVisible();

      // ---- Clear via the dedicated clear button.
      await page.getByRole('button', { name: 'Clear search input' }).click();

      await expect(searchInput).toHaveValue('');

      // ---- A nonsense term shows the no-match empty state with a reset action.
      await searchInput.fill('zzz-filter-no-match-zzz');

      await expect(
        page.getByText('No matching expenses', { exact: true }),
      ).toBeVisible();

      await page.getByRole('button', { name: 'Clear filters' }).click();

      await expect(searchInput).toHaveValue('');
      await expect(
        page.getByText('No matching expenses', { exact: true }),
      ).toHaveCount(0);
    } finally {
      await request.dispose();
    }
  });

  test('account filter writes the URL and narrows rows; All Accounts clears it', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Filter toolbar is desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const now = new Date();
    const futureDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() + 30),
    );

    const request = await createRequestContext(playwright);

    try {
      const accounts = await getAccountsViaApi(request, accessToken);
      const accountA = accounts[0];
      const accountB = accounts[1];
      const stamp = Date.now();
      const descriptionA = `E2E Filter Acct A ${stamp}`;
      const descriptionB = `E2E Filter Acct B ${stamp}`;

      const expenseA = await createExpenseViaApi(
        request,
        accessToken,
        accountA.rowId,
        { description: descriptionA, nextDue: futureDate, amount: 30 },
      );
      const expenseB = await createExpenseViaApi(
        request,
        accessToken,
        accountB.rowId,
        { description: descriptionB, nextDue: futureDate, amount: 40 },
      );

      createdExpenseRowIds.push(expenseA.rowId, expenseB.rowId);

      await page.goto('/expenses');

      // ---- Select account A from the filter -> URL gains ?accountId=A.
      // selectRadixOption drives Radix's AT/keyboard path (focus + Enter) — no
      // pointer events, immune to the pointerTypeRef race and to the silent
      // evaluate-dispatch drop that made earlier approaches flaky on slow machines.
      await page.getByLabel('Filter by account').click();
      const accountOption = page.getByRole('option', {
        name: accountA.description,
        exact: true,
      });
      const allAccountsOption = page.getByRole('option', {
        name: 'All Accounts',
        exact: true,
      });
      // On this first open the currently-selected option is "All Accounts"
      // (value 'all') — pass it so we gate on focus steering settling.
      await selectRadixOption(accountOption, allAccountsOption);

      await expect(page).toHaveURL(new RegExp(`accountId=${accountA.rowId}`));

      // Combine with search to make the row-level assertion deterministic.
      await page
        .getByRole('textbox', { name: 'Search expenses by description' })
        .fill(descriptionA);

      await expect(
        page.getByRole('row').filter({ hasText: descriptionA }),
      ).toBeVisible();
      await expect(
        page.getByRole('row').filter({ hasText: descriptionB }),
      ).toHaveCount(0);

      // ---- Choose All Accounts -> URL loses accountId.
      await page
        .getByRole('textbox', { name: 'Search expenses by description' })
        .fill('');

      // Same AT-path selection as the account-A step. On this SECOND open the
      // currently-selected option is account A — pass it so we gate on focus
      // steering settling before activating "All Accounts".
      await page.getByLabel('Filter by account').click();
      await selectRadixOption(allAccountsOption, accountOption);

      await expect(page).not.toHaveURL(/accountId=/);
    } finally {
      await request.dispose();
    }
  });

  test('account deep link pre-filters; an invalid account id falls back to unfiltered', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Filter toolbar is desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const request = await createRequestContext(playwright);

    try {
      const accounts = await getAccountsViaApi(request, accessToken);
      const accountA = accounts[0];

      // ---- Valid deep link: the filter reflects the URL account.
      await page.goto(`/expenses?accountId=${accountA.rowId}`);

      await expect(page).toHaveURL(new RegExp(`accountId=${accountA.rowId}`));
      await expect(page.getByLabel('Filter by account')).toContainText(
        accountA.description,
      );

      // ---- Invalid deep link: falls back to unfiltered (URL accountId cleared).
      await page.goto('/expenses?accountId=not-a-real-account');

      await expect(page).not.toHaveURL(/accountId=/);
      await expect(page).toHaveURL(/\/expenses$/);
    } finally {
      await request.dispose();
    }
  });

  test('income search filter narrows and clears', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Filter toolbar is desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const now = new Date();
    const futureDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() + 30),
    );

    const request = await createRequestContext(playwright);

    try {
      const accounts = await getAccountsViaApi(request, accessToken);
      const stamp = Date.now();
      const description = `E2E Filter Income ${stamp}`;

      const income = await createIncomeViaApi(
        request,
        accessToken,
        accounts[0].rowId,
        { description, nextDue: futureDate, amount: 100 },
      );

      createdIncomeRowIds.push(income.rowId);

      await page.goto('/incomes');

      const searchInput = page.getByRole('textbox', {
        name: 'Search incomes by description',
      });

      await searchInput.fill(description);

      await expect(
        page.getByRole('row').filter({ hasText: description }),
      ).toBeVisible();
      await expect(page.getByText(/Showing 1 of \d+ items/)).toBeVisible();

      await page.getByRole('button', { name: 'Clear search input' }).click();

      await expect(searchInput).toHaveValue('');
    } finally {
      await request.dispose();
    }
  });
});
