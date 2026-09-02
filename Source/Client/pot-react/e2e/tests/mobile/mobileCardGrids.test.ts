import { expect, test } from '../../fixtures/auth';

// Covers the responsive data-table/card-grid switch. On narrow viewports the
// accounts / expenses / incomes pages swap their desktop `DataTable` for a
// 2-column mobile card grid (`AccountCardGrid` / `ExpenseCardGrid` /
// `IncomeCardGrid`, driven by `useIsMobile()` which flips at < 768 px
// viewport width). Desktop-vs-mobile layout is exactly what unit tests cannot
// validate, so each test asserts BOTH sides of the switch on the real browser:
//   - mobile projects (Pixel 7 / iPhone 14, ~390–412 px) → card grid renders,
//     the `<table>` is absent, and the card-only "Due:" / "Balance:"-style
//     labels are present;
//   - desktop projects (chromium / edge, 1280 px) → the `<table>` renders and
//     the card-only labels are absent.
//
// Parallel-safe: read-only (list GETs + page load only), no mutations, no
// teardown — safe to run concurrently on the shared stack.
//
// Data-readiness uses a DIRECT API prefetch via the accessToken (the
// fixture-managed convention) instead of `page.waitForResponse`: the browser's
// own GET under full-matrix load can be starved past the test timeout (a
// documented load-sensitive race — the incomes GET timed out on 2026-09-01).
// The list JSON comes from the request context; the UI assertions are web-first
// (they poll the rendered DOM), so no network wait is needed on the page side.

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const apiBaseUrl = 'http://127.0.0.1:5242';

const authHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

test('expenses page: data table on desktop, card grid on mobile', async ({
  page,
  playwright,
  accessToken,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  const expensesRequest = await playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
  const expensesResponse = await expensesRequest.get('/api/expenses', {
    headers: authHeaders(accessToken),
  });
  expect(expensesResponse.ok()).toBeTruthy();
  const expenses = (await expensesResponse.json()) as {
    description: string;
    excludeFromCalcs: boolean;
  }[];
  await expensesRequest.dispose();
  expect(expenses.length).toBeGreaterThan(0);

  // A stable target: the first non-excluded expense (renders its description).
  const target =
    expenses.find(expense => !expense.excludeFromCalcs) ?? expenses[0];

  await page.goto('/expenses');

  // Substring match: the table's description cell renders the text alongside a
  // possible note-popover, and the mobile card alongside its action icons, so
  // an exact-text match is not guaranteed.
  await expect(page.getByText(target.description).first()).toBeVisible();

  if (isMobile) {
    // Card grid: the desktop <table> is NOT rendered; the mobile card's
    // "Due:" / "Amount:" field labels ARE (the table uses "Next Due"/"Amount"
    // column headers, so the exact "Due:" text is card-only).
    await expect(page.getByRole('table')).toHaveCount(0);
    await expect(page.getByText('Due:', { exact: true }).first()).toBeVisible();
    await expect(
      page.getByText('Amount:', { exact: true }).first(),
    ).toBeVisible();
  } else {
    // Data table: the <table> renders and the mobile card labels are absent.
    await expect(page.getByRole('table')).toBeVisible();
    await expect(page.getByText('Due:', { exact: true })).toHaveCount(0);
  }
});

test('incomes page: data table on desktop, card grid on mobile', async ({
  page,
  playwright,
  accessToken,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  const incomesRequest = await playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
  const incomesResponse = await incomesRequest.get('/api/incomes', {
    headers: authHeaders(accessToken),
  });
  expect(incomesResponse.ok()).toBeTruthy();
  const incomes = (await incomesResponse.json()) as {
    description: string;
    excludeFromCalcs: boolean;
  }[];
  await incomesRequest.dispose();
  expect(incomes.length).toBeGreaterThan(0);

  const target = incomes.find(income => !income.excludeFromCalcs) ?? incomes[0];

  await page.goto('/incomes');

  // Substring match — see the expenses test above for why exact is not safe.
  await expect(page.getByText(target.description).first()).toBeVisible();

  if (isMobile) {
    await expect(page.getByRole('table')).toHaveCount(0);
    await expect(page.getByText('Due:', { exact: true }).first()).toBeVisible();
    await expect(
      page.getByText('Amount:', { exact: true }).first(),
    ).toBeVisible();
  } else {
    await expect(page.getByRole('table')).toBeVisible();
    await expect(page.getByText('Due:', { exact: true })).toHaveCount(0);
  }
});

test('accounts page: data table on desktop, card grid on mobile', async ({
  page,
  playwright,
  accessToken,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  const accountsRequest = await playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
  const accountsResponse = await accountsRequest.get('/api/accounts', {
    headers: authHeaders(accessToken),
  });
  expect(accountsResponse.ok()).toBeTruthy();
  const accounts = (await accountsResponse.json()) as {
    description: string;
    bsb: string;
  }[];
  await accountsRequest.dispose();
  expect(accounts.length).toBeGreaterThan(0);

  await page.goto('/accounts');

  // Substring match: the accounts table's description cell renders the text
  // alongside linked-data StatusBadges, and the mobile card as a heading — so
  // an exact-text match is not guaranteed.
  await expect(page.getByText(accounts[0].description).first()).toBeVisible();

  if (isMobile) {
    // Card grid: no <table>; the mobile account card's "BSB:"/"Balance:"
    // labels render (the table shows those values in plain cells / column
    // headers without the colon, so the "BSB:" text is card-only). The card
    // renders "BSB: <value>" in ONE text node, so match by substring.
    await expect(page.getByRole('table')).toHaveCount(0);
    await expect(page.getByText(/BSB:/).first()).toBeVisible();
    await expect(
      page.getByText('Balance:', { exact: true }).first(),
    ).toBeVisible();
  } else {
    await expect(page.getByRole('table')).toBeVisible();
    await expect(page.getByText(/BSB:/)).toHaveCount(0);
  }
});
