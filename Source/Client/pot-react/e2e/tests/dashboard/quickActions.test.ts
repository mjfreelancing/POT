import type { APIRequestContext, Page, Playwright } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

// Covers the dashboard quick actions: the four
// action cards on /dashboard — Renew Expenses, Renew Incomes, Accrue Expenses,
// Renew & Accrue — are driven by the accruals-status endpoint and only enabled
// when there is actionable data. Each card fires the matching API call and
// shows a success toast. The whole Quick Actions section is PermissionGuard
// gated (hidden for the read-only viewer).
//
// Fixture-managed, serial suite: overdue expenses/incomes are created through
// the API (deterministic dates) and removed in afterEach, so the suite is
// re-runnable without contamination. Each quick action gets its own
// test with fresh data, because acting on a card clears its own "required"
// state (renewing the overdue expenses makes Renew Expenses disabled again).
//
// Desktop-only: the dashboard quick-action cards are exercised on desktop;
// mobile layout is covered by mobileCardGrids.test.ts.

const apiBaseUrl = 'http://127.0.0.1:5242';

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// Local-date ISO (YYYY-MM-DD) so "overdue" classification matches the app's
// local-date handling.
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
      accrualPolicy: 'Automatic',
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

// The Quick Actions section is open by default (fresh login = no persisted
// localStorage). This helper waits for the lazy-loaded dashboard to mount and
// asserts the cards are actually visible before we interact with them.
async function waitForQuickActions(page: Page): Promise<void> {
  await expect(page.getByText('Quick Actions', { exact: true })).toBeVisible();

  await expect(
    page.getByRole('heading', { name: 'Renew Expenses' }),
  ).toBeVisible();
}

test.describe.serial('Dashboard quick actions (fixture-managed)', () => {
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

  test('renew expenses quick action renews overdue expenses', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Quick actions are desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const now = new Date();
    const overdueDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() - 5),
    );

    const request = await createRequestContext(playwright);

    try {
      const accountRowId = await getFirstAccountRowId(request, accessToken);
      const stamp = Date.now();

      const overdueExpense1 = await createExpenseViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E QA Expense 1 ${stamp}`,
          nextDue: overdueDate,
          amount: 40,
        },
      );
      const overdueExpense2 = await createExpenseViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E QA Expense 2 ${stamp}`,
          nextDue: overdueDate,
          amount: 50,
        },
      );

      createdExpenseRowIds.push(overdueExpense1.rowId, overdueExpense2.rowId);

      // ---- UI: the card only becomes clickable once accruals status says
      // there are overdue expenses (role="button" appears when enabled).
      await page.goto('/dashboard');
      await waitForQuickActions(page);

      const renewResponsePromise = page.waitForResponse(
        response =>
          response.url().includes('/api/expenses/renew') &&
          response.request().method() === 'POST',
      );

      await page.getByRole('button', { name: 'Renew Expenses' }).click();

      const renewResponse = await renewResponsePromise;
      // Surface real server errors instead of silently missing the toast, and
      // wait for the response BODY (waitForResponse resolves on headers) so
      // React has processed the result before the toast is asserted.
      expect(renewResponse.ok()).toBeTruthy();
      await renewResponse.finished();
      const renewBody = renewResponse.request().postDataJSON() as {
        mode?: string;
        rowIds?: string[];
      };

      // The success toast renders after the renew POST; under full-matrix load
      // the server work + render can exceed the 10s default (documented
      // slow-POST precedent). Give this post-response UI signal headroom.
      await expect(
        page.getByText('Expense Renewal Complete', { exact: true }),
      ).toBeVisible({ timeout: 30_000 });

      expect(renewBody.mode).toBe('Overdue');
      expect(renewBody.rowIds).toContain(overdueExpense1.rowId);
      expect(renewBody.rowIds).toContain(overdueExpense2.rowId);

      // ---- Per-type advance: both expenses moved forward.
      const expensesResponse = await request.get('/api/expenses', {
        headers: authHeaders(accessToken),
      });
      expect(expensesResponse.ok()).toBeTruthy();

      const expenses = (await expensesResponse.json()) as {
        rowId: string;
        nextDue: string;
      }[];

      const expense1After = expenses.find(
        expense => expense.rowId === overdueExpense1.rowId,
      );
      const expense2After = expenses.find(
        expense => expense.rowId === overdueExpense2.rowId,
      );

      expect(expense1After?.nextDue).toBeTruthy();
      expect(expense2After?.nextDue).toBeTruthy();
      // ISO date strings compare lexicographically.
      expect(expense1After!.nextDue > overdueDate).toBeTruthy();
      expect(expense2After!.nextDue > overdueDate).toBeTruthy();
    } finally {
      await request.dispose();
    }
  });

  test('renew incomes quick action renews overdue incomes', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Quick actions are desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const now = new Date();
    const overdueDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() - 5),
    );

    const request = await createRequestContext(playwright);

    try {
      const accountRowId = await getFirstAccountRowId(request, accessToken);
      const stamp = Date.now();

      const overdueIncome1 = await createIncomeViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E QA Income 1 ${stamp}`,
          nextDue: overdueDate,
          amount: 100,
        },
      );
      const overdueIncome2 = await createIncomeViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E QA Income 2 ${stamp}`,
          nextDue: overdueDate,
          amount: 120,
        },
      );

      createdIncomeRowIds.push(overdueIncome1.rowId, overdueIncome2.rowId);

      await page.goto('/dashboard');
      await waitForQuickActions(page);

      const renewResponsePromise = page.waitForResponse(
        response =>
          response.url().includes('/api/incomes/renew') &&
          response.request().method() === 'POST',
      );

      await page.getByRole('button', { name: 'Renew Incomes' }).click();

      const renewResponse = await renewResponsePromise;
      // Surface real server errors instead of silently missing the toast, and
      // wait for the response BODY so React has processed the result first.
      expect(renewResponse.ok()).toBeTruthy();
      await renewResponse.finished();
      const renewBody = renewResponse.request().postDataJSON() as {
        mode?: string;
        rowIds?: string[];
      };

      // Post-response toast: headroom for the slow renew POST under load
      // (slow-POST precedent).
      await expect(
        page.getByText('Income Renewal Complete', { exact: true }),
      ).toBeVisible({ timeout: 30_000 });

      expect(renewBody.mode).toBe('Overdue');
      expect(renewBody.rowIds).toContain(overdueIncome1.rowId);
      expect(renewBody.rowIds).toContain(overdueIncome2.rowId);

      const incomesResponse = await request.get('/api/incomes', {
        headers: authHeaders(accessToken),
      });
      expect(incomesResponse.ok()).toBeTruthy();

      const incomes = (await incomesResponse.json()) as {
        rowId: string;
        nextDue: string;
      }[];

      const income1After = incomes.find(
        income => income.rowId === overdueIncome1.rowId,
      );
      const income2After = incomes.find(
        income => income.rowId === overdueIncome2.rowId,
      );

      expect(income1After?.nextDue).toBeTruthy();
      expect(income2After?.nextDue).toBeTruthy();
      expect(income1After!.nextDue > overdueDate).toBeTruthy();
      expect(income2After!.nextDue > overdueDate).toBeTruthy();
    } finally {
      await request.dispose();
    }
  });

  test('accrue expenses quick action accrues dirty accounts', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Quick actions are desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const now = new Date();
    const overdueDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() - 5),
    );

    const request = await createRequestContext(playwright);

    try {
      const accountRowId = await getFirstAccountRowId(request, accessToken);
      const stamp = Date.now();

      // Creating an expense marks the account accrual dirty (the seed already
      // accrued all accounts), so the account is in accountAccrualsRequired.
      const overdueExpense = await createExpenseViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E QA Accrue ${stamp}`,
          nextDue: overdueDate,
          amount: 60,
        },
      );

      createdExpenseRowIds.push(overdueExpense.rowId);

      await page.goto('/dashboard');
      await waitForQuickActions(page);

      const accrueResponsePromise = page.waitForResponse(
        response =>
          response.url().includes('/api/accruals/accrue-expenses') &&
          response.request().method() === 'POST',
      );

      await page.getByRole('button', { name: 'Accrue Expenses' }).click();

      const accrueResponse = await accrueResponsePromise;
      // Surface real server errors instead of silently missing the toast, and
      // wait for the response BODY so React has processed the result first.
      expect(accrueResponse.ok()).toBeTruthy();
      await accrueResponse.finished();
      const accrueBody = accrueResponse.request().postDataJSON() as {
        rowIds?: string[];
      };

      // Post-response toast: headroom for the slow accrue POST under load
      // (slow-POST precedent).
      await expect(
        page.getByText('Account Accruals Complete', { exact: true }),
      ).toBeVisible({ timeout: 30_000 });

      expect(accrueBody.rowIds).toContain(accountRowId);

      // ---- Post-condition: the account accrual is now clean.
      const statusUrl = `/api/accruals/status?accountRowIds=${accountRowId}`;

      await expect
        .poll(async () => {
          const statusResponse = await request.get(statusUrl, {
            headers: authHeaders(accessToken),
          });
          expect(statusResponse.ok()).toBeTruthy();

          const status = (await statusResponse.json()) as {
            accountAccrualsRequired: string[];
          };

          return status.accountAccrualsRequired;
        })
        .not.toContain(accountRowId);
    } finally {
      await request.dispose();
    }
  });

  test('renew & accrue quick action renews expenses + incomes and accrues accounts', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Quick actions are desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    const now = new Date();
    const overdueDate = toIsoDate(
      new Date(now.getFullYear(), now.getMonth(), now.getDate() - 5),
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
          description: `E2E QA R&A Expense ${stamp}`,
          nextDue: overdueDate,
          amount: 70,
        },
      );
      const overdueIncome = await createIncomeViaApi(
        request,
        accessToken,
        accountRowId,
        {
          description: `E2E QA R&A Income ${stamp}`,
          nextDue: overdueDate,
          amount: 150,
        },
      );

      createdExpenseRowIds.push(overdueExpense.rowId);
      createdIncomeRowIds.push(overdueIncome.rowId);

      await page.goto('/dashboard');
      await waitForQuickActions(page);

      const renewExpensesResponsePromise = page.waitForResponse(
        response =>
          response.url().includes('/api/expenses/renew') &&
          response.request().method() === 'POST',
      );
      const renewIncomesResponsePromise = page.waitForResponse(
        response =>
          response.url().includes('/api/incomes/renew') &&
          response.request().method() === 'POST',
      );
      const accrueResponsePromise = page.waitForResponse(
        response =>
          response.url().includes('/api/accruals/accrue-expenses') &&
          response.request().method() === 'POST',
      );

      // The title is "Renew&nbsp;& Accrue" (non-breaking spaces) — match with a
      // whitespace-tolerant regex.
      await page.getByRole('button', { name: /Renew\s*&\s*Accrue/ }).click();

      const renewExpensesResponse = await renewExpensesResponsePromise;
      const renewIncomesResponse = await renewIncomesResponsePromise;
      const accrueResponse = await accrueResponsePromise;
      // Surface real server errors instead of silently missing the toast, and
      // wait for the response BODIES so React has processed them first.
      expect(renewExpensesResponse.ok()).toBeTruthy();
      expect(renewIncomesResponse.ok()).toBeTruthy();
      expect(accrueResponse.ok()).toBeTruthy();
      await renewExpensesResponse.finished();
      await renewIncomesResponse.finished();
      await accrueResponse.finished();

      const renewExpensesBody = renewExpensesResponse
        .request()
        .postDataJSON() as {
        mode?: string;
        rowIds?: string[];
      };
      const renewIncomesBody = renewIncomesResponse
        .request()
        .postDataJSON() as {
        mode?: string;
        rowIds?: string[];
      };
      const accrueBody = accrueResponse.request().postDataJSON() as {
        rowIds?: string[];
      };

      // Post-response toast: headroom for the slow multi-POST action under load
      // (slow-POST precedent).
      await expect(
        page.getByText('Renew / Accruals Complete', { exact: true }),
      ).toBeVisible({ timeout: 30_000 });

      expect(renewExpensesBody.mode).toBe('Overdue');
      expect(renewExpensesBody.rowIds).toContain(overdueExpense.rowId);

      expect(renewIncomesBody.mode).toBe('Overdue');
      expect(renewIncomesBody.rowIds).toContain(overdueIncome.rowId);

      expect(accrueBody.rowIds).toContain(accountRowId);
    } finally {
      await request.dispose();
    }
  });

  test('viewer does not see quick actions (PermissionGuard)', async ({
    page,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Quick actions are desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
    );

    await page.goto('/dashboard');

    // The whole Quick Actions section is hidden for the viewer (they lack
    // expense/income/account manage permissions).
    await expect(page.getByText('Quick Actions', { exact: true })).toHaveCount(
      0,
    );
    await expect(
      page.getByRole('heading', { name: 'Renew Expenses' }),
    ).toHaveCount(0);

    // View-only sections are still visible.
    await expect(
      page.getByText('Accounts Overview', { exact: true }),
    ).toBeVisible();
  });
});
