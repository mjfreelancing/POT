import { expect, test } from '../../fixtures/auth';

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
});
