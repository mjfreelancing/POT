import type { APIRequestContext, Page } from '@playwright/test';

import {
  expect,
  quickActionsTest as test,
  viewerTest,
} from '../../fixtures/auth';
import {
  authHeaders,
  createAccountViaApi,
  createE2eRequestContext as createRequestContext,
  createExpenseViaApi,
  createIncomeViaApi,
  deleteExpenseViaApi,
  deleteIncomeViaApi,
} from '../../helpers/api';
import { toIsoDate } from '../../helpers/dates';

// Covers the dashboard quick actions: the two action cards on /dashboard
// — Renew Expenses and Renew Incomes — are driven by the accruals-status
// endpoint and only enabled when there is actionable data.
// Each card fires the matching API call and shows a success toast. The whole
// Quick Actions section is PermissionGuard gated (hidden for the read-only
// viewer).
//
// Fixture-managed, serial suite: overdue expenses/incomes are created through
// the API (deterministic dates) and removed in afterEach, so the suite is
// re-runnable without contamination. Each quick action gets its own
// test with fresh data, because acting on a card clears its own "required"
// state (renewing the overdue expenses makes Renew Expenses disabled again).
//
// Desktop-only: the dashboard quick-action cards are exercised on desktop;
// mobile layout is covered by mobileCardGrids.test.ts.
//
// Runs as e2e_quickactions (Admin on its OWN site, see baseline.sql) so the
// whole-site renew actions never sweep other suites' rows on the shared E2E
// site. It is CHROMIUM-ONLY in playwright.config.ts: chromium + edge
// running the same file against one DB would race each other's actions.

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// RowIds created during this serial suite, cleaned up in afterEach.
const createdExpenseRowIds: string[] = [];
const createdIncomeRowIds: string[] = [];

// The quick-actions suite runs as e2e_quickactions on its own isolated site,
// which has no seeded accounts (accounts are not part of the baseline seed).
// Create one on first use; later tests in the serial suite reuse it.
async function getOrCreateAccountRowId(
  request: APIRequestContext,
  accessToken: string,
): Promise<string> {
  const response = await request.get('/api/accounts', {
    headers: authHeaders(accessToken),
  });

  expect(response.ok()).toBeTruthy();

  const accounts = (await response.json()) as { rowId: string }[];

  if (accounts.length > 0) {
    return accounts[0].rowId;
  }

  const account = await createAccountViaApi(request, accessToken);

  return account.rowId;
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
      const accountRowId = await getOrCreateAccountRowId(request, accessToken);
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
      const accountRowId = await getOrCreateAccountRowId(request, accessToken);
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

  viewerTest(
    'viewer does not see quick actions (PermissionGuard)',
    async ({ page }, testInfo) => {
      viewerTest.skip(
        isMobileProject(testInfo),
        'Quick actions are desktop-only; mobile layout is covered by mobileCardGrids.test.ts',
      );

      await page.goto('/dashboard');

      // The whole Quick Actions section is hidden for the viewer (they lack
      // expense/income/account manage permissions).
      await expect(
        page.getByText('Quick Actions', { exact: true }),
      ).toHaveCount(0);
      await expect(
        page.getByRole('heading', { name: 'Renew Expenses' }),
      ).toHaveCount(0);

      // View-only sections are still visible.
      await expect(
        page.getByText('Accounts Overview', { exact: true }),
      ).toBeVisible();
    },
  );
});
