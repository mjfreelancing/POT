import type { APIRequestContext } from '@playwright/test';
import { addDays, format } from 'date-fns';

import { expect, test } from '../../fixtures/auth';
import {
  createAccountViaApi,
  createE2eRequestContext as createRequestContext,
  createExpenseViaApi,
  deleteAccountViaApi,
  deleteExpenseViaApi,
} from '../../helpers/api';
import { toIsoDate } from '../../helpers/dates';

// Covers the projection bar-chart detail interaction:
// - clicking an account bar, the Total (All Accounts) bar, or the blank area of
//   a day column opens the right-hand detail sheet for that day across ALL
//   legend-visible accounts (not just the clicked account), and
// - accounts hidden via the legend ("Show" toggles) are excluded from the sheet,
//   even when the Total bar is clicked.
// Regression guard: the previous behaviour scoped the sheet to the clicked
// account only, showed hidden accounts when the Total bar was clicked, and left
// blank areas with no click handler at all.
//
// Fixture-managed and serial: each test creates its own two accounts (plus one
// monthly expense each on the same future date) so the target day is
// unambiguous and isolated, and removes them in a finally block, so the suite
// is re-runnable.
//
// Desktop-only: the projection legend is hidden behind a filters toggle on
// mobile (covered by mobileCardGrids.test.ts).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

type ProjectionAccount = {
  rowId: string;
  description: string;
  dates: { date: string }[];
};

type ProjectionDaySetup = {
  accountARowId: string;
  accountBRowId: string;
  accountDescriptionA: string;
  accountDescriptionB: string;
  expectedSheetTitle: string;
  createdAccountRowIds: string[];
  createdExpenseRowIds: string[];
};

// Creates two accounts, each with one monthly expense due on the same future
// date (dayOffset days ahead), so the projection has a single unambiguous day
// that carries an expense on two different accounts.
async function createProjectionDay(
  request: APIRequestContext,
  accessToken: string,
  dayOffset: number,
): Promise<ProjectionDaySetup> {
  const stamp = `${Date.now()}${Math.floor(Math.random() * 1000)}`;
  const accountDescriptionA = `E2E Projection Acct A ${stamp}`;
  const accountDescriptionB = `E2E Projection Acct B ${stamp}`;
  const detailDate = addDays(new Date(), dayOffset);
  const detailDateKey = toIsoDate(detailDate);
  const expectedSheetTitle = format(detailDate, 'MMMM d, yyyy');

  const createdAccountRowIds: string[] = [];
  const createdExpenseRowIds: string[] = [];

  const accountA = await createAccountViaApi(
    request,
    accessToken,
    accountDescriptionA,
  );
  createdAccountRowIds.push(accountA.rowId);

  const accountB = await createAccountViaApi(
    request,
    accessToken,
    accountDescriptionB,
  );
  createdAccountRowIds.push(accountB.rowId);

  const expenseA = await createExpenseViaApi(
    request,
    accessToken,
    accountA.rowId,
    {
      description: `E2E Projection Expense A ${stamp}`,
      nextDue: detailDateKey,
      amount: 12.34,
    },
  );
  createdExpenseRowIds.push(expenseA.rowId);

  const expenseB = await createExpenseViaApi(
    request,
    accessToken,
    accountB.rowId,
    {
      description: `E2E Projection Expense B ${stamp}`,
      nextDue: detailDateKey,
      amount: 56.78,
    },
  );
  createdExpenseRowIds.push(expenseB.rowId);

  return {
    accountARowId: accountA.rowId,
    accountBRowId: accountB.rowId,
    accountDescriptionA,
    accountDescriptionB,
    expectedSheetTitle,
    createdAccountRowIds,
    createdExpenseRowIds,
  };
}

async function teardownProjectionDay(
  request: APIRequestContext,
  accessToken: string,
  setup: ProjectionDaySetup,
): Promise<void> {
  for (const rowId of setup.createdExpenseRowIds) {
    await deleteExpenseViaApi(request, accessToken, rowId);
  }

  for (const rowId of setup.createdAccountRowIds) {
    await deleteAccountViaApi(request, accessToken, rowId);
  }
}

// Dispatches a click on the bar of `seriesIndex` whose column centre is nearest
// to `targetX`. Bars are sub-1px wide on a dense chart, so a real mouse click
// would not reliably land on the shape; the chart routes clicks by position,
// so the dispatched event carries the bar's real client coordinates.
async function clickBarNearestColumn(
  page: import('@playwright/test').Page,
  seriesIndex: number,
  targetX: number,
): Promise<boolean> {
  return page.evaluate(
    ({ seriesIndex, targetX }) => {
      const series = document.querySelectorAll('.recharts-bar');
      const bar = series[seriesIndex];

      if (!bar) {
        return false;
      }

      const rects = Array.from(bar.querySelectorAll('path.recharts-rectangle'));

      if (rects.length === 0) {
        return false;
      }

      let nearestRect = rects[0];
      let nearestDistance = Infinity;

      for (const rect of rects) {
        const rectBox = rect.getBoundingClientRect();
        const centreX = rectBox.x + rectBox.width / 2;
        const distance = Math.abs(centreX - targetX);

        if (distance < nearestDistance) {
          nearestDistance = distance;
          nearestRect = rect;
        }
      }

      const box = nearestRect.getBoundingClientRect();

      nearestRect.dispatchEvent(
        new MouseEvent('click', {
          bubbles: true,
          cancelable: true,
          view: window,
          clientX: box.x + box.width / 2,
          clientY: box.y + box.height / 2,
        }),
      );

      return true;
    },
    { seriesIndex, targetX },
  );
}

// Opens the projections page and switches to the Expenses (bar) metric over a
// one-month window. Returns the projection payload captured on first load.
async function openExpensesBarChart(page: import('@playwright/test').Page) {
  const projectionsResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/projections') &&
      response.request().method() === 'GET',
  );

  await page.goto('/projections');

  const projectionsResponse = await projectionsResponsePromise;
  expect(projectionsResponse.ok()).toBeTruthy();

  const payload = (await projectionsResponse.json()) as {
    accounts: ProjectionAccount[];
  };

  await page
    .getByRole('combobox', { name: 'Select chart metric to display' })
    .click();
  await page.getByRole('option', { name: 'Expenses', exact: true }).click();
  await page.getByRole('radio', { name: 'Set chart period to 1 mo' }).click();

  return payload;
}

async function expectSheetHeading(
  dialog: import('@playwright/test').Locator,
  expectedSheetTitle: string,
): Promise<void> {
  await expect(
    dialog.getByRole('heading', { name: expectedSheetTitle, exact: true }),
  ).toBeVisible();
}

test.describe.serial('Projection bar chart details (fixture-managed)', () => {
  test('clicking an account bar, the Total bar, or blank area opens the day for all visible accounts', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Projection chart legend is desktop-only; mobile projection filters are covered by mobileCardGrids.test.ts',
    );

    const request = await createRequestContext(playwright);
    const setup = await createProjectionDay(request, accessToken, 6);

    try {
      const payload = await openExpensesBarChart(page);

      // The chart renders one series per account, in payload order, followed by
      // the total series. Find the series index of our first created account so
      // we can click its bar.
      const accountAIndex = payload.accounts.findIndex(
        account => account.rowId === setup.accountARowId,
      );
      expect(accountAIndex).toBeGreaterThanOrEqual(0);

      // Sanity check: the created expense date is inside the fetched projection.
      expect(
        payload.accounts.some(account =>
          account.dates.some(
            day =>
              day.date === toIsoDate(addDays(new Date(), 6)) &&
              account.rowId === setup.accountARowId,
          ),
        ),
      ).toBeTruthy();

      const accountABarSeries = page
        .locator('.recharts-bar')
        .nth(accountAIndex);
      const accountABar = accountABarSeries.locator('path.recharts-rectangle');

      await expect(accountABar.first()).toBeVisible();

      // Let the bar entrance animation finish so measured geometry is final.
      await page.waitForTimeout(1200);

      // In a one-month window each new monthly expense contributes exactly one
      // bar, so the account bar's column centre is the day's unambiguous column.
      const accountABarBox = await accountABar.first().boundingBox();
      expect(accountABarBox).not.toBeNull();
      const { x, width } = accountABarBox!;
      const barCenterX = x + width / 2;

      const gridBox = await page
        .locator('.recharts-cartesian-grid')
        .boundingBox();
      expect(gridBox).not.toBeNull();

      // --- Click the actual account bar -------------------------------------
      const clickedAccountBar = await clickBarNearestColumn(
        page,
        accountAIndex,
        barCenterX,
      );
      expect(clickedAccountBar).toBeTruthy();

      let dialog = page.getByRole('dialog');
      await expectSheetHeading(dialog, setup.expectedSheetTitle);

      // The sheet must show BOTH visible accounts for the day — not just the
      // account whose bar was clicked.
      await expect(
        dialog.getByText(setup.accountDescriptionA, { exact: true }),
      ).toBeVisible();
      await expect(
        dialog.getByText(setup.accountDescriptionB, { exact: true }),
      ).toBeVisible();

      await page.getByRole('button', { name: 'Close expense details' }).click();
      await expect(dialog).toBeHidden();

      // --- Click the Total (All Accounts) bar in the same column ------------
      // The total series is the last rendered bar series.
      const totalSeriesIndex =
        (await page.locator('.recharts-bar').count()) - 1;
      const clickedTotalBar = await clickBarNearestColumn(
        page,
        totalSeriesIndex,
        barCenterX,
      );
      expect(clickedTotalBar).toBeTruthy();

      dialog = page.getByRole('dialog');
      await expectSheetHeading(dialog, setup.expectedSheetTitle);
      await expect(
        dialog.getByText(setup.accountDescriptionA, { exact: true }),
      ).toBeVisible();
      await expect(
        dialog.getByText(setup.accountDescriptionB, { exact: true }),
      ).toBeVisible();

      await page.getByRole('button', { name: 'Close expense details' }).click();
      await expect(dialog).toBeHidden();

      // --- Click the blank area above the bars in the same day column -------
      // Blank area near the top of the grid is above every bar in the column.
      await page.mouse.click(barCenterX, gridBox!.y + 4);
      await page.waitForTimeout(400);

      dialog = page.getByRole('dialog');
      await expectSheetHeading(dialog, setup.expectedSheetTitle);
      await expect(
        dialog.getByText(setup.accountDescriptionA, { exact: true }),
      ).toBeVisible();
      await expect(
        dialog.getByText(setup.accountDescriptionB, { exact: true }),
      ).toBeVisible();

      await page.getByRole('button', { name: 'Close expense details' }).click();
      await expect(dialog).toBeHidden();

      // --- Line metrics must NOT open the details sheet on click ------------
      await page
        .getByRole('combobox', { name: 'Select chart metric to display' })
        .click();
      await page
        .getByRole('option', { name: 'Account Balances', exact: true })
        .click();
      await expect(page.locator('.recharts-line').first()).toBeVisible();
      await page.waitForTimeout(600);

      await page.mouse.click(barCenterX, gridBox!.y + 4);
      await expect(page.getByRole('dialog')).toHaveCount(0);
    } finally {
      try {
        await teardownProjectionDay(request, accessToken, setup);
      } finally {
        await request.dispose();
      }
    }
  });

  test('hiding an account in the legend excludes it from the day sheet (incl. via the Total bar)', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Projection chart legend is desktop-only; mobile projection filters are covered by mobileCardGrids.test.ts',
    );

    const request = await createRequestContext(playwright);
    const setup = await createProjectionDay(request, accessToken, 9);

    try {
      const payload = await openExpensesBarChart(page);

      const accountAIndex = payload.accounts.findIndex(
        account => account.rowId === setup.accountARowId,
      );
      expect(accountAIndex).toBeGreaterThanOrEqual(0);

      const accountABarSeries = page
        .locator('.recharts-bar')
        .nth(accountAIndex);
      const accountABar = accountABarSeries.locator('path.recharts-rectangle');

      await expect(accountABar.first()).toBeVisible();
      await page.waitForTimeout(1200);

      const accountABarBox = await accountABar.first().boundingBox();
      expect(accountABarBox).not.toBeNull();
      const { x, width } = accountABarBox!;
      const barCenterX = x + width / 2;

      const totalSeriesIndex =
        (await page.locator('.recharts-bar').count()) - 1;

      // --- Total bar click with all accounts visible ------------------------
      const clickedTotalBar = await clickBarNearestColumn(
        page,
        totalSeriesIndex,
        barCenterX,
      );
      expect(clickedTotalBar).toBeTruthy();

      let dialog = page.getByRole('dialog');
      await expectSheetHeading(dialog, setup.expectedSheetTitle);
      await expect(
        dialog.getByText(setup.accountDescriptionA, { exact: true }),
      ).toBeVisible();
      await expect(
        dialog.getByText(setup.accountDescriptionB, { exact: true }),
      ).toBeVisible();

      await page.getByRole('button', { name: 'Close expense details' }).click();
      await expect(dialog).toBeHidden();

      // --- Hide account B via the legend, then click the Total bar ----------
      await page
        .getByRole('button', {
          name: `Hide ${setup.accountDescriptionB} series on chart`,
        })
        .click();
      await page.waitForTimeout(400);

      // Hiding a series removes its rendered bar layer, which shifts the index
      // of the trailing total series, so recompute it after the legend change.
      const hiddenTotalSeriesIndex =
        (await page.locator('.recharts-bar').count()) - 1;

      const clickedTotalBarAfterHide = await clickBarNearestColumn(
        page,
        hiddenTotalSeriesIndex,
        barCenterX,
      );
      expect(clickedTotalBarAfterHide).toBeTruthy();

      dialog = page.getByRole('dialog');
      await expectSheetHeading(dialog, setup.expectedSheetTitle);

      // Only the still-enabled account must be listed; the hidden one must not
      // appear, even though its amount still contributes to the Total bar.
      await expect(
        dialog.getByText(setup.accountDescriptionA, { exact: true }),
      ).toBeVisible();
      await expect(
        dialog.getByText(setup.accountDescriptionB, { exact: true }),
      ).toHaveCount(0);

      await page.getByRole('button', { name: 'Close expense details' }).click();
      await expect(dialog).toBeHidden();
    } finally {
      try {
        await teardownProjectionDay(request, accessToken, setup);
      } finally {
        await request.dispose();
      }
    }
  });
});
