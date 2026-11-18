import { expect, test } from '../../fixtures/auth';
import { collectApiFailures } from '../../helpers/api';

// The dashboard renders the same sections on every viewport (metric grids and
// account/expense/income cards are responsive), so this test runs on all projects.

const moneyFormatter = new Intl.NumberFormat('en-AU', {
  style: 'currency',
  currency: 'AUD',
});
const formatMoney = (value: number) => moneyFormatter.format(value);

// Mirrors the app's normalizeToLocalMidnight date arithmetic so the derived
// expectations match the rendered values exactly (timezone-safe day differences).
const normalizeToLocalMidnight = (date: string | Date): number => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  return new Date(
    dateObj.getFullYear(),
    dateObj.getMonth(),
    dateObj.getDate(),
  ).getTime();
};

// Mirrors the dashboard's period filter (ExpensesOverview/IncomesOverview
// filterExpenses/filterIncomes), which derives its window from getDaysDue: an
// item is shown when its calendar-day count (negative when overdue) is <= the
// selected period.
//
// The day count must be a ROUNDED calendar-day difference, not a floored
// millisecond difference: Math.floor((due - today) / 86_400_000) under-counts by
// one across a spring-forward (an N-day gap measures as N-1), which widened this
// test's window by a calendar day and selected a boundary item the dashboard
// correctly excludes. Because the raw /api/expenses response is unsorted, whether
// that boundary item came first varied run-to-run — that was the flake.
//
// "Today" is resolved per call (not at module load) so the window cannot go
// stale if a run straddles local midnight.
const calendarDaysUntil = (isoDate: string): number =>
  Math.round(
    (normalizeToLocalMidnight(isoDate) - normalizeToLocalMidnight(new Date())) /
      86_400_000,
  );

type Account = {
  description: string;
  balance: number;
  available: number;
  stableExpenseAccrual: number;
};

type RecurringItem = {
  description: string;
  amount: number;
  nextDue: string;
  excludeFromCalcs: boolean;
};

const dueWithinDays =
  (days: number) =>
  (item: RecurringItem): boolean =>
    !item.excludeFromCalcs && calendarDaysUntil(item.nextDue) <= days;

test('dashboard renders seeded account rollups and upcoming items', async ({
  page,
}) => {
  const apiFailures = collectApiFailures(page);

  const accountsResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/accounts') &&
      response.request().method() === 'GET',
  );
  const expensesResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/expenses') &&
      response.request().method() === 'GET',
  );
  const incomesResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/incomes') &&
      response.request().method() === 'GET',
  );

  await page.goto('/dashboard');

  const [accountsResponse, expensesResponse, incomesResponse] =
    await Promise.all([
      accountsResponsePromise,
      expensesResponsePromise,
      incomesResponsePromise,
    ]);

  expect(accountsResponse.ok()).toBeTruthy();
  expect(expensesResponse.ok()).toBeTruthy();
  expect(incomesResponse.ok()).toBeTruthy();

  const accounts = (await accountsResponse.json()) as Account[];
  const expenses = (await expensesResponse.json()) as RecurringItem[];
  const incomes = (await incomesResponse.json()) as RecurringItem[];

  expect(accounts.length).toBeGreaterThan(0);

  // Account rollups: Total Balance and Daily Need are sums of all accounts.
  const totalBalance = accounts.reduce(
    (sum, account) => sum + account.balance,
    0,
  );
  const dailyNeed = accounts.reduce(
    (sum, account) => sum + account.stableExpenseAccrual,
    0,
  );

  await expect(
    page.getByText(formatMoney(totalBalance), { exact: true }).first(),
  ).toBeVisible();
  await expect(
    page.getByText(formatMoney(dailyNeed), { exact: true }).first(),
  ).toBeVisible();

  // The first account's card renders its name, balance, and available funds.
  const firstAccount = accounts[0];
  await expect(
    page.getByText(firstAccount.description, { exact: true }).first(),
  ).toBeVisible();
  await expect(
    page.getByText(formatMoney(firstAccount.balance), { exact: true }).first(),
  ).toBeVisible();
  await expect(
    page
      .getByText(formatMoney(firstAccount.available), { exact: true })
      .first(),
  ).toBeVisible();

  // Section headers confirm all overview sections rendered.
  await expect(
    page.getByRole('heading', { name: 'Accounts Overview', exact: true }),
  ).toBeVisible();
  await expect(
    page.getByRole('heading', { name: 'Expenses Overview', exact: true }),
  ).toBeVisible();
  await expect(
    page.getByRole('heading', { name: 'Incomes Overview', exact: true }),
  ).toBeVisible();

  // An upcoming (due within 30 days) expense renders with its amount.
  // The dashboard's default period is 30 days, and dueWithinDays mirrors the
  // dashboard's own filter, so ANY row it matches is guaranteed to be rendered
  // (the raw /api/expenses response order is not relied upon).
  const dueWithin30 = dueWithinDays(30);
  const upcomingExpense = expenses.find(dueWithin30);

  if (upcomingExpense) {
    await expect(
      page.getByText(upcomingExpense.description, { exact: true }).first(),
    ).toBeVisible();
    await expect(
      page
        .getByText(formatMoney(upcomingExpense.amount), { exact: true })
        .first(),
    ).toBeVisible();
  }

  // An upcoming (due within 30 days) income renders with its amount.
  const upcomingIncome = incomes.find(dueWithin30);

  if (upcomingIncome) {
    await expect(
      page.getByText(upcomingIncome.description, { exact: true }).first(),
    ).toBeVisible();
    await expect(
      page
        .getByText(formatMoney(upcomingIncome.amount), { exact: true })
        .first(),
    ).toBeVisible();
  }

  // The dashboard reads more than the figures asserted above, and an assertion on one of them passes even when
  // another request is refused.
  expect(apiFailures).toEqual([]);
});

// The account total is the sum the client computes itself, and summing two-decimal values lands a fraction of a
// cent away from what the cards add up to: 0.3 - 0.1 - 0.2 is -2.78e-17. Formatting that raw is where the browser
// renders '-$0.00', so the assertion belongs here rather than only in a unit test.
//
// The payload is fulfilled rather than seeded because no sequence of writes through the API produces those
// figures, and creating accounts on the seeded site would change the totals the test above reads.
test('dashboard renders an account total that sums to nothing as $0.00', async ({
  page,
}) => {
  const apiFailures = collectApiFailures(page);

  // The ids are GUIDs because the dashboard passes them to /api/accruals/status, which rejects anything else.
  const account = (rowId: string, available: number) => ({
    rowId,
    etag: 0,
    description: `Rounding ${available}`,
    balance: 1,
    reserved: 0,
    totalExpenseAccrued: 0,
    totalArrears: 0,
    totalCommitted: 0,
    stableExpenseAccrual: 0,
    available,
    linkedExpenses: 0,
    linkedIncomes: 0,
  });

  await page.route('**/api/accounts', route =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        account('8f14e45f-ceea-467f-a8b8-1d6a5c0b7e21', 0.3),
        account('c9f0f895-fb98-4c98-8e9b-6a1c4d2f5b03', -0.1),
        account('45c48cce-2e2d-4fbd-aa1d-7b6e5c3a9f14', -0.2),
      ]),
    }),
  );

  await page.goto('/dashboard');

  await expect(
    page.getByText('Total Available', { exact: true }),
  ).toBeVisible();

  // The cards carry their own figures, so the only total that can be -$0.00 is the available one.
  await expect(page.getByText('-$0.00', { exact: true })).toHaveCount(0);
  await expect(page.getByText('$0.00', { exact: true }).first()).toBeVisible();

  // The dashboard reads more than the accounts it totals, and an assertion on the total passes even when one of
  // those other requests is refused.
  expect(apiFailures).toEqual([]);
});

