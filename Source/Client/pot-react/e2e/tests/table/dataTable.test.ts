import { expect, test } from '../../fixtures/auth';

// Covers DataTable behaviors on the
// desktop table:
//   - column sorting (text + numeric columns; three-state asc -> desc -> clear);
//   - badge styling: the Next-Due status badges carry their semantic colors
//     (Overdue = red, Due Soon = orange — the shadcn-upgrade visual concern),
//     and frequency badges carry their per-frequency colors;
//   - row selection highlights the row and updates the bulk-actions bar count.
//     (The full bulk mark-as-paid flow is covered by crud/bulkActions.test.ts.)
//
// Desktop-only: mobile renders the card grids (covered by mobileCardGrids.test.ts).
// Parallel-safe: read-only (list GETs + client-side interactions only).
//
// Expected values are derived from the intercepted /api/expenses payload, never
// hardcoded (PRD R7). Date math mirrors the app's normalizeToLocalMidnight /
// getDaysDue so badge expectations match exactly (timezone-safe).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const moneyFormatter = new Intl.NumberFormat('en-AU', {
  style: 'currency',
  currency: 'AUD',
});
const formatMoney = (value: number) => moneyFormatter.format(value);

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

type Expense = {
  rowId: string;
  description: string;
  amount: number;
  nextDue: string;
  endDate: string | null;
  excludeFromCalcs: boolean;
  frequency: string;
  frequencyCount: number;
};

// Mirrors getStatusBadgeClass (src/lib/badgeStyles.ts): Overdue = red,
// Due Soon = orange, Due Today = amber, Ended/Excluded = slate.
const statusBadgeColor = (expense: Expense): string | null => {
  if (expense.excludeFromCalcs) return 'slate';
  if (expense.endDate && daysUntil(expense.endDate) < 0) return 'slate';
  const daysDue = daysUntil(expense.nextDue);
  if (daysDue < 0) return 'red';
  if (daysDue === 0) return 'amber';
  if (daysDue <= 7) return 'orange';
  return null;
};

const statusColorClass: Record<string, string> = {
  red: 'bg-red-100',
  orange: 'bg-orange-500',
  amber: 'bg-amber-500',
  slate: 'bg-slate-',
};

// Mirrors createFrequencyColumn's per-frequency colors (src/components/table).
const frequencyColorClass: Record<string, string> = {
  Days: 'bg-blue-50',
  Weeks: 'bg-green-50',
  Months: 'bg-purple-50',
  EndOfMonth: 'bg-purple-50',
  Years: 'bg-amber-50',
  OneTime: 'bg-pink-50',
};

const frequencySingular: Record<string, string> = {
  Days: 'Day',
  Weeks: 'Week',
  Months: 'Month',
  Years: 'Year',
  EndOfMonth: 'End of Month',
  OneTime: 'One Time',
};

const expectedFrequencyText = (expense: Expense): string =>
  expense.frequency === 'OneTime' || expense.frequency === 'EndOfMonth'
    ? frequencySingular[expense.frequency]
    : expense.frequencyCount === 1
      ? `1 ${frequencySingular[expense.frequency]}`
      : `${expense.frequencyCount} ${expense.frequency}`;

async function loadExpenses(
  page: import('@playwright/test').Page,
): Promise<Expense[]> {
  const responsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/expenses') &&
      response.request().method() === 'GET',
  );
  await page.goto('/expenses');
  const response = await responsePromise;
  expect(response.ok()).toBeTruthy();
  const expenses = (await response.json()) as Expense[];
  expect(expenses.length).toBeGreaterThan(0);
  return expenses;
}

const firstRowDescriptionCell = (page: import('@playwright/test').Page) =>
  page.locator('table tbody tr').first().locator('td').nth(1);

const firstRowAmountCell = (page: import('@playwright/test').Page) =>
  page.locator('table tbody tr').first().locator('td').nth(2);

test('Description column sorts ascending, descending, then clears', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'The data table is desktop-only; mobile uses card grids',
  );

  const expenses = await loadExpenses(page);

  // The page displays expenses sorted by nextDue (useApiGetAllExpenses sorts the
  // raw response via compareExpenseNextDue: nextDue asc, then description), so the
  // table's DEFAULT (and cleared) order is that sorted order — NOT the raw API order.
  const compareExpenseNextDue = (lhs: Expense, rhs: Expense): number => {
    const byDue = lhs.nextDue.localeCompare(rhs.nextDue);
    if (byDue !== 0) return byDue;
    return lhs.description.localeCompare(rhs.description, 'en', {
      sensitivity: 'base',
    });
  };
  const defaultDescriptions = [...expenses]
    .sort(compareExpenseNextDue)
    .map(expense => expense.description);
  const descriptions = expenses.map(expense => expense.description);
  const sortedAsc = [...descriptions].sort((a, b) => a.localeCompare(b));
  const sortedDesc = [...sortedAsc].reverse();

  const header = page.getByRole('button', { name: 'Description', exact: true });
  const firstCell = firstRowDescriptionCell(page);

  // Default order = the table's nextDue-sorted order.
  await expect(firstCell).toHaveText(defaultDescriptions[0]);

  // Click 1 -> ascending (by description).
  await header.click();
  await expect(firstCell).toHaveText(sortedAsc[0]);

  // Click 2 -> descending (by description).
  await header.click();
  await expect(firstCell).toHaveText(sortedDesc[0]);

  // Click 3 -> clears back to the table's default (nextDue-sorted) order.
  await header.click();
  await expect(firstCell).toHaveText(defaultDescriptions[0]);
});

test('Amount column sorts numerically ascending and descending', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'The data table is desktop-only; mobile uses card grids',
  );

  const expenses = await loadExpenses(page);
  const amounts = expenses.map(expense => expense.amount);
  const min = Math.min(...amounts);
  const max = Math.max(...amounts);

  const header = page.getByRole('button', { name: 'Amount', exact: true });
  const firstCell = firstRowAmountCell(page);

  await header.click(); // ascending
  await expect(firstCell).toHaveText(formatMoney(min));

  await header.click(); // descending
  await expect(firstCell).toHaveText(formatMoney(max));
});

test('Next Due status badges carry their semantic colors', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'The data table is desktop-only; mobile uses card grids',
  );

  const expenses = await loadExpenses(page);

  const target = expenses.find(expense => statusBadgeColor(expense) === 'red');
  expect(target, 'seed data must contain an Overdue expense').toBeTruthy();

  const overdueBadge = page
    .getByRole('row')
    .filter({ hasText: target!.description })
    .getByText('Overdue', { exact: true });
  await expect(overdueBadge).toBeVisible();
  await expect(overdueBadge).toHaveClass(new RegExp(statusColorClass.red));

  const dueSoon = expenses.find(
    expense => statusBadgeColor(expense) === 'orange',
  );
  expect(dueSoon, 'seed data must contain a Due Soon expense').toBeTruthy();

  const dueSoonBadge = page
    .getByRole('row')
    .filter({ hasText: dueSoon!.description })
    .getByText('Due Soon', { exact: true });
  await expect(dueSoonBadge).toBeVisible();
  await expect(dueSoonBadge).toHaveClass(new RegExp(statusColorClass.orange));
});

test('frequency badges carry their per-frequency colors', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'The data table is desktop-only; mobile uses card grids',
  );

  const expenses = await loadExpenses(page);
  const target = expenses.find(
    expense =>
      !expense.excludeFromCalcs && frequencyColorClass[expense.frequency],
  );
  expect(
    target,
    'seed data must contain a non-excluded frequency expense',
  ).toBeTruthy();

  const expectedClass = frequencyColorClass[target!.frequency];
  const expectedText = expectedFrequencyText(target!);

  const freqBadge = page
    .getByRole('row')
    .filter({ hasText: target!.description })
    .getByText(expectedText, { exact: true });
  await expect(freqBadge).toBeVisible();
  await expect(freqBadge).toHaveClass(new RegExp(expectedClass));
});

test('selecting a row highlights it and updates the bulk-actions count', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'The data table is desktop-only; mobile uses card grids',
  );

  const expenses = await loadExpenses(page);
  const target = expenses[0];

  const row = page.getByRole('row').filter({ hasText: target.description });
  const checkbox = page.getByRole('checkbox', {
    name: `Select row ${target.rowId}`,
  });

  await checkbox.check();
  await expect(row).toHaveAttribute('data-state', 'selected');
  await expect(page.getByText('1 selected', { exact: true })).toBeVisible();

  await checkbox.uncheck();
  await expect(row).not.toHaveAttribute('data-state', 'selected');
  await expect(
    page.getByText('No items selected', { exact: true }),
  ).toBeVisible();
});
