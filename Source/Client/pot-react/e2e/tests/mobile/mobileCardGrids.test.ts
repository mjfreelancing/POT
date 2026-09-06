import { expect, test } from '../../fixtures/auth';
import {
  authHeaders,
  createAccountViaApi,
  createE2eRequestContext as createRequestContext,
  createExpenseViaApi,
  createIncomeViaApi,
  deleteAccountViaApi,
  deleteExpenseViaApi,
  deleteIncomeViaApi,
} from '../../helpers/api';
import { toIsoDate } from '../../helpers/dates';

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
// Fixture-managed: each test creates its OWN uniquely-named row via the API,
// asserts it appears in the UI (also exercising the write → list → render
// path), then removes it in `finally`. Owning the asserted row keeps the suite
// safe from other suites creating/deleting rows on the shared site mid-test (a
// previous version sampled the "first" live row, which raced with the filters
// suite and flaked).
//
// Data-readiness uses a DIRECT API prefetch via the accessToken (the
// fixture-managed convention) instead of `page.waitForResponse`: the browser's
// own GET under full-matrix load can be starved past the test timeout (a
// documented load-sensitive race — the incomes GET timed out on 2026-09-01).
// The UI assertions are web-first (they poll the rendered DOM).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

test('expenses page: data table on desktop, card grid on mobile', async ({
  page,
  playwright,
  accessToken,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  const request = await createRequestContext(playwright);
  const description = `E2E CardGrid Expense ${Date.now()}`;
  let createdRowId: string | undefined;

  try {
    // Use the first (stable, seed-owned) account for the created expense.
    const accountsResponse = await request.get('/api/accounts', {
      headers: authHeaders(accessToken),
    });
    expect(accountsResponse.ok()).toBeTruthy();
    const accounts = (await accountsResponse.json()) as { rowId: string }[];
    expect(accounts.length).toBeGreaterThan(0);

    const expense = await createExpenseViaApi(
      request,
      accessToken,
      accounts[0].rowId,
      { description, nextDue: toIsoDate(new Date()), amount: 123 },
    );
    createdRowId = expense.rowId;

    await page.goto('/expenses');

    // Substring match: the table's description cell renders the text alongside
    // a possible note-popover, and the mobile card alongside its action icons,
    // so an exact-text match is not guaranteed.
    await expect(page.getByText(description).first()).toBeVisible();

    if (isMobile) {
      // Card grid: the desktop <table> is NOT rendered; the mobile card's
      // "Due:" / "Amount:" field labels ARE (the table uses "Next Due"/"Amount"
      // column headers, so the exact "Due:" text is card-only).
      await expect(page.getByRole('table')).toHaveCount(0);
      await expect(
        page.getByText('Due:', { exact: true }).first(),
      ).toBeVisible();
      await expect(
        page.getByText('Amount:', { exact: true }).first(),
      ).toBeVisible();
    } else {
      // Data table: the <table> renders and the mobile card labels are absent.
      await expect(page.getByRole('table')).toBeVisible();
      await expect(page.getByText('Due:', { exact: true })).toHaveCount(0);
    }
  } finally {
    if (createdRowId) {
      await deleteExpenseViaApi(request, accessToken, createdRowId);
    }
    await request.dispose();
  }
});

test('incomes page: data table on desktop, card grid on mobile', async ({
  page,
  playwright,
  accessToken,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  const request = await createRequestContext(playwright);
  const description = `E2E CardGrid Income ${Date.now()}`;
  let createdRowId: string | undefined;

  try {
    const accountsResponse = await request.get('/api/accounts', {
      headers: authHeaders(accessToken),
    });
    expect(accountsResponse.ok()).toBeTruthy();
    const accounts = (await accountsResponse.json()) as { rowId: string }[];
    expect(accounts.length).toBeGreaterThan(0);

    const income = await createIncomeViaApi(
      request,
      accessToken,
      accounts[0].rowId,
      { description, nextDue: toIsoDate(new Date()), amount: 234 },
    );
    createdRowId = income.rowId;

    await page.goto('/incomes');

    // Substring match — see the expenses test above for why exact is not safe.
    await expect(page.getByText(description).first()).toBeVisible();

    if (isMobile) {
      await expect(page.getByRole('table')).toHaveCount(0);
      await expect(
        page.getByText('Due:', { exact: true }).first(),
      ).toBeVisible();
      await expect(
        page.getByText('Amount:', { exact: true }).first(),
      ).toBeVisible();
    } else {
      await expect(page.getByRole('table')).toBeVisible();
      await expect(page.getByText('Due:', { exact: true })).toHaveCount(0);
    }
  } finally {
    if (createdRowId) {
      await deleteIncomeViaApi(request, accessToken, createdRowId);
    }
    await request.dispose();
  }
});

test('accounts page: data table on desktop, card grid on mobile', async ({
  page,
  playwright,
  accessToken,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  const request = await createRequestContext(playwright);
  const description = `E2E CardGrid Account ${Date.now()}`;
  let createdRowId: string | undefined;

  try {
    const account = await createAccountViaApi(request, accessToken, description);
    createdRowId = account.rowId;

    await page.goto('/accounts');

    // Substring match: the accounts table's description cell renders the text
    // alongside linked-data StatusBadges, and the mobile card as a heading — so
    // an exact-text match is not guaranteed.
    await expect(page.getByText(description).first()).toBeVisible();

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
  } finally {
    if (createdRowId) {
      await deleteAccountViaApi(request, accessToken, createdRowId);
    }
    await request.dispose();
  }
});
