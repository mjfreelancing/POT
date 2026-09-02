import { expect, test } from '../../fixtures/auth';

// The dashboard renders the same sections on every viewport (metric grids and
// account/expense/income cards are responsive), so this test runs on all projects.

const moneyFormatter = new Intl.NumberFormat('en-AU', {
  style: 'currency',
  currency: 'AUD',
});
const formatMoney = (value: number) => moneyFormatter.format(value);

// Mirrors the app's normalizeToLocalMidnight/getDaysDue date arithmetic so the
// derived expectations match exactly (timezone-safe day differences).
const normalizeToLocalMidnight = (date: string | Date): number => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  return new Date(
    dateObj.getFullYear(),
    dateObj.getMonth(),
    dateObj.getDate(),
  ).getTime();
};

const localTodayEpoch = normalizeToLocalMidnight(new Date());

const daysUntil = (isoDate: string): number =>
  Math.floor(
    (normalizeToLocalMidnight(isoDate) - localTodayEpoch) / 86_400_000,
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
    !item.excludeFromCalcs && daysUntil(item.nextDue) <= days;

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

  // "Total Next 30 Days" rollups are derived from the expense/income payloads
  // (the dashboard's default period is 30 days).
  const dueWithin30 = dueWithinDays(30);

  const expenses30Total = expenses
    .filter(dueWithin30)
    .reduce((sum, expense) => sum + expense.amount, 0);
  await expect(
    page.getByText(formatMoney(expenses30Total), { exact: true }).first(),
  ).toBeVisible();

  const incomes30Total = incomes
    .filter(dueWithin30)
    .reduce((sum, income) => sum + income.amount, 0);
  await expect(
    page.getByText(formatMoney(incomes30Total), { exact: true }).first(),
  ).toBeVisible();

  // An upcoming (due within 30 days) expense renders with its amount.
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
});
