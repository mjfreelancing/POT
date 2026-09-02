import { expect, test } from '../../fixtures/auth';

// Desktop-only: the /incomes page renders a data table on desktop but mobile
// cards on mobile, and the mobile card shows "(N days)" instead of a "Due Soon"
// badge. The mobile list rendering is covered by mobileCardGrids.test.ts.
const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

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

type Income = {
  description: string;
  amount: number;
  nextDue: string;
  endDate: string | null;
  excludeFromCalcs: boolean;
  frequency: string;
  frequencyCount: number;
};

// Mirrors createFrequencyColumn in dataTableColumnFactories.tsx.
const frequencySingular: Record<string, string> = {
  Days: 'Day',
  Weeks: 'Week',
  Months: 'Month',
  Years: 'Year',
  EndOfMonth: 'End of Month',
  OneTime: 'One Time',
};

const expectedFrequency = (frequency: string, count: number): string => {
  if (frequency === 'OneTime' || frequency === 'EndOfMonth') {
    return frequencySingular[frequency];
  }

  return count === 1
    ? `1 ${frequencySingular[frequency]}`
    : `${count} ${frequency}`;
};

// Mirrors createNextDueStatusColumn in dataTableColumnFactories.tsx.
const expectedStatusBadge = (income: Income): string | null => {
  if (income.excludeFromCalcs) {
    return 'Excluded';
  }

  if (income.endDate && daysUntil(income.endDate) < 0) {
    return 'Ended';
  }

  const daysDue = daysUntil(income.nextDue);

  if (daysDue < 0) {
    return 'Overdue';
  }

  if (daysDue === 0) {
    return 'Due Today';
  }

  if (daysDue <= 7) {
    return 'Due Soon';
  }

  return null;
};

test('incomes list renders seeded rows, amounts, status and frequency', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'Income list table is desktop-only; mobile uses the card grid',
  );

  const incomesResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/incomes') &&
      response.request().method() === 'GET',
  );

  await page.goto('/incomes');

  const incomesResponse = await incomesResponsePromise;
  expect(incomesResponse.ok()).toBeTruthy();

  const incomes = (await incomesResponse.json()) as Income[];
  expect(incomes.length).toBeGreaterThan(0);

  // Pick a stable target row: the first non-excluded income.
  const target = incomes.find(income => !income.excludeFromCalcs) ?? incomes[0];

  // The row renders the description and formatted amount.
  await expect(
    page.getByText(target.description, { exact: true }).first(),
  ).toBeVisible();
  await expect(
    page.getByText(formatMoney(target.amount), { exact: true }).first(),
  ).toBeVisible();

  // The status badge matches the payload-derived expectation.
  const badge = expectedStatusBadge(target);

  if (badge) {
    await expect(page.getByText(badge, { exact: true }).first()).toBeVisible();
  }

  // The frequency badge matches the payload-derived expectation.
  const frequencyLabel = expectedFrequency(
    target.frequency,
    target.frequencyCount,
  );
  await expect(
    page.getByText(frequencyLabel, { exact: true }).first(),
  ).toBeVisible();
});
