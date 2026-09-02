import type { APIRequestContext, Playwright } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

// Covers the filters: the SearchInput (with its
// "Clear search input" button) and the AccountFilter ("Filter by account")
// select, including the PRD 011 URL query-string contract — the `accountId`
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

const apiBaseUrl = 'http://127.0.0.1:5242';

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// Local-date ISO (YYYY-MM-DD) so created records classify the same way the app
// does.
const toIsoDate = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
};

// RowIds created during this serial suite, cleaned up in afterEach.
const createdExpenseRowIds: string[] = [];
const createdIncomeRowIds: string[] = [];

// Creates an unauthenticated API request context. Auth comes from the
// `accessToken` fixture (the single per-test login), avoiding an extra PBKDF2
// login per operation. The 60s per-action timeout mirrors the test timeout so a
// cold first API call on a loaded machine does not throw before the test can
// complete (observed attempt-1 setup failures on a loaded local stack).
async function createRequestContext(
  playwright: Playwright,
): Promise<APIRequestContext> {
  return playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
}

const authHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

// Selects a Radix Select option via the AT/keyboard activation path — no pointer
// events (immune to Radix's pointerTypeRef mouse-selection contract, which .click()
// and atomic pointerdown/pointerup dispatch both race on slow machines). This is
// machine-speed-independent because it uses Playwright's RETRYING primitives and
// gates on Radix's own settle signal:
//   1. waits for Radix's open-time focus steering to settle — the currently
//      SELECTED option receives `data-highlighted` (on the 1st open that is
//      "All Accounts"; on later opens it is the previously-selected account);
//   2. focuses the target and PROVES focus landed (`toBeFocused`), closing the
//      window where Radix's async focusFirst() could steal focus;
//   3. activates with `Enter` -> SelectItem onKeyDown -> handleSelect(), Radix's
//      own unit-tested activation path.
// A bare option.evaluate() synthetic-click dispatch was NOT enough: evaluate() has
// no actionability retry, so if the option node is transient mid-positioning on a
// loaded machine the click is silently dropped (the "dropdown stays open + URL
// unchanged" failure). focus()/press() re-resolve and retry on instability.
async function selectRadixOption(
  option: import('@playwright/test').Locator,
  selectedOption?: import('@playwright/test').Locator,
): Promise<void> {
  await expect(option).toBeAttached();
  await expect(option).toBeVisible();

  // Gate on Radix's open-time focus steering settling (the selected option is
  // highlighted). Without this, Enter can land before the async focusFirst()
  // completes and be delivered to the wrong node.
  if (selectedOption) {
    await expect(selectedOption).toHaveAttribute('data-highlighted', '');
  }

  // Focus the target, prove it, then activate via Enter.
  await option.focus();
  await expect(option).toBeFocused();
  await option.press('Enter');

  // Fail-fast: handleSelect also closes the dropdown. If it never ran, the
  // option stays visible (dropdown still open).
  await expect(option).toBeHidden({ timeout: 5_000 });
}

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

async function createExpenseViaApi(
  request: APIRequestContext,
  accessToken: string,
  accountRowId: string,
  payload: { description: string; nextDue: string; amount: number },
): Promise<{ rowId: string }> {
  const response = await request.post('/api/expenses', {
    headers: authHeaders(accessToken),
    data: {
      description: payload.description,
      nextDue: payload.nextDue,
      accrualStart: null,
      accrualPolicy: 'None',
      endDate: null,
      frequency: 'Months',
      frequencyCount: 1,
      amount: payload.amount,
      note: null,
      accountRowId,
    },
  });

  expect(response.ok()).toBeTruthy();

  return (await response.json()) as { rowId: string };
}

async function createIncomeViaApi(
  request: APIRequestContext,
  accessToken: string,
  accountRowId: string,
  payload: { description: string; nextDue: string; amount: number },
): Promise<{ rowId: string }> {
  const response = await request.post('/api/incomes', {
    headers: authHeaders(accessToken),
    data: {
      description: payload.description,
      nextDue: payload.nextDue,
      endDate: null,
      frequency: 'Months',
      frequencyCount: 1,
      amount: payload.amount,
      note: null,
      accountRowId,
    },
  });

  expect(response.ok()).toBeTruthy();

  return (await response.json()) as { rowId: string };
}

async function deleteExpenseViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/expenses/${rowId}`, {
    headers: authHeaders(accessToken),
  });

  expect([200, 204, 404]).toContain(response.status());
}

async function deleteIncomeViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/incomes/${rowId}`, {
    headers: authHeaders(accessToken),
  });

  expect([200, 204, 404]).toContain(response.status());
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
