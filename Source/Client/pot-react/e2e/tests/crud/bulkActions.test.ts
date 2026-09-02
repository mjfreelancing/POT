import type { APIRequestContext, Playwright } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

// Covers the bulk mark-as-paid/received flow: selecting a
// mixed overdue + future set, the smart confirmation dialog showing the
// per-type breakdown, and the per-type advance behaviour (Future vs Overdue
// renew calls, then nextDue actually advancing).
//
// Fixture-managed, serial suite: created expenses/incomes are set up through
// the API (deterministic dates) and removed in afterEach, so the suite is
// re-runnable without contamination.
//
// Desktop-only: the bulk table toolbar is desktop; mobile card bulk flows are
// covered by mobileCardGrids.test.ts.

const apiBaseUrl = 'http://127.0.0.1:5242';

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// Local-date ISO (YYYY-MM-DD) so the classification (overdue/future vs today)
// matches the app's local-date handling.
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
// login per operation.
async function createRequestContext(
  playwright: Playwright,
): Promise<APIRequestContext> {
  // 60s per-action timeout mirrors the test timeout (same rationale as
  // filters.test.ts): a cold first API call on the loaded shared stack can
  // exceed Playwright's 30s request default.
  return playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
}

const authHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

async function getFirstAccountRowId(
  request: APIRequestContext,
  accessToken: string,
): Promise<string> {
  const response = await request.get('/api/accounts', {
    headers: authHeaders(accessToken),
  });

  expect(response.ok()).toBeTruthy();

  const accounts = (await response.json()) as { rowId: string }[];
  expect(accounts.length).toBeGreaterThan(0);

  return accounts[0].rowId;
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

test.describe.serial('Bulk mark-as-paid/received (fixture-managed)', () => {
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

  test('bulk mark-as-paid shows the breakdown and advances overdue + future expenses', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Bulk table actions are desktop-only; mobile uses the card grid',
    );

    const now = new Date();
    const overdueDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() - 5),
    );
    const futureDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() + 30),
    );

    const request = await createRequestContext(playwright);

    try {
      const accountRowId = await getFirstAccountRowId(request, accessToken);
      const stamp = Date.now();

      const overdueExpense = await createExpenseViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E Bulk Overdue ${stamp}`,
          nextDue: overdueDate,
          amount: 50,
        },
      );
      const futureExpense = await createExpenseViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E Bulk Future ${stamp}`,
          nextDue: futureDate,
          amount: 60,
        },
      );

      createdExpenseRowIds.push(overdueExpense.rowId, futureExpense.rowId);

      // ---- UI: select both rows and open the bulk action ----
      await page.goto('/expenses');

      await page
        .getByRole('checkbox', { name: `Select row ${overdueExpense.rowId}` })
        .check();
      await page
        .getByRole('checkbox', { name: `Select row ${futureExpense.rowId}` })
        .check();

      await page
        .getByRole('button', { name: 'Open bulk actions menu' })
        .click();
      await page.getByRole('menuitem', { name: 'Mark as Paid' }).click();

      // ---- Smart confirmation dialog shows the per-type breakdown ----
      const dialog = page.getByRole('alertdialog');
      await expect(dialog).toBeVisible();
      await expect(
        dialog.getByText('Mark Expenses as Paid', { exact: true }),
      ).toBeVisible();
      await expect(dialog.getByText(/1 overdue expense/)).toBeVisible();
      await expect(dialog.getByText(/1 expense not yet due/)).toBeVisible();

      // Capture the renew calls so we can assert per-type behaviour.
      const renewRequests: Array<{ mode?: string; rowIds?: string[] }> = [];
      page.on('request', req => {
        if (
          req.url().includes('/api/expenses/renew') &&
          req.method() === 'POST'
        ) {
          renewRequests.push(req.postDataJSON());
        }
      });

      await dialog.getByRole('button', { name: 'Proceed' }).click();

      // Success toast with the processed summary.
      await expect(
        page.getByText('Expenses Processed', { exact: true }),
      ).toBeVisible();

      // Two renew calls: Future (future expense) + Overdue (overdue expense).
      await expect.poll(() => renewRequests.length, { timeout: 10000 }).toBe(2);

      const futureCall = renewRequests.find(call => call.mode === 'Future');
      const overdueCall = renewRequests.find(call => call.mode === 'Overdue');

      expect(futureCall?.rowIds).toContain(futureExpense.rowId);
      expect(overdueCall?.rowIds).toContain(overdueExpense.rowId);

      // ---- Per-type advance: nextDue moved forward for both ----
      const expensesResponse = await request.get('/api/expenses', {
        headers: authHeaders(accessToken),
      });
      expect(expensesResponse.ok()).toBeTruthy();

      const expenses = (await expensesResponse.json()) as {
        rowId: string;
        nextDue: string;
      }[];

      const overdueAfter = expenses.find(
        expense => expense.rowId === overdueExpense.rowId,
      );
      const futureAfter = expenses.find(
        expense => expense.rowId === futureExpense.rowId,
      );

      expect(overdueAfter?.nextDue).toBeTruthy();
      expect(futureAfter?.nextDue).toBeTruthy();
      // ISO date strings compare lexicographically.
      expect(overdueAfter!.nextDue > overdueDate).toBeTruthy();
      expect(futureAfter!.nextDue > futureDate).toBeTruthy();
    } finally {
      await request.dispose();
    }
  });

  test('bulk mark-as-received advances a future income', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Bulk table actions are desktop-only; mobile uses the card grid',
    );

    const now = new Date();
    const futureDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() + 30),
    );

    const request = await createRequestContext(playwright);

    try {
      const accountRowId = await getFirstAccountRowId(request, accessToken);
      const stamp = Date.now();

      const futureIncome = await createIncomeViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E Bulk Future Income ${stamp}`,
          nextDue: futureDate,
          amount: 100,
        },
      );

      createdIncomeRowIds.push(futureIncome.rowId);

      // ---- UI: select the row and run the bulk action ----
      await page.goto('/incomes');

      await page
        .getByRole('checkbox', { name: `Select row ${futureIncome.rowId}` })
        .check();

      await page
        .getByRole('button', { name: 'Open bulk actions menu' })
        .click();
      await page.getByRole('menuitem', { name: 'Mark as Received' }).click();

      const dialog = page.getByRole('alertdialog');
      await expect(dialog).toBeVisible();
      await expect(
        dialog.getByText('Mark Incomes as Received', { exact: true }),
      ).toBeVisible();
      await expect(dialog.getByText(/1 income not yet due/)).toBeVisible();

      const renewRequests: Array<{ mode?: string; rowIds?: string[] }> = [];
      page.on('request', req => {
        if (
          req.url().includes('/api/incomes/renew') &&
          req.method() === 'POST'
        ) {
          renewRequests.push(req.postDataJSON());
        }
      });

      await dialog.getByRole('button', { name: 'Proceed' }).click();

      await expect(
        page.getByText('Incomes Processed', { exact: true }),
      ).toBeVisible();

      await expect.poll(() => renewRequests.length, { timeout: 10000 }).toBe(1);
      expect(renewRequests[0]?.mode).toBe('Future');
      expect(renewRequests[0]?.rowIds).toContain(futureIncome.rowId);

      // ---- Per-type advance ----
      const incomesResponse = await request.get('/api/incomes', {
        headers: authHeaders(accessToken),
      });
      expect(incomesResponse.ok()).toBeTruthy();

      const incomes = (await incomesResponse.json()) as {
        rowId: string;
        nextDue: string;
      }[];

      const incomeAfter = incomes.find(
        income => income.rowId === futureIncome.rowId,
      );

      expect(incomeAfter?.nextDue).toBeTruthy();
      expect(incomeAfter!.nextDue > futureDate).toBeTruthy();
    } finally {
      await request.dispose();
    }
  });
});
