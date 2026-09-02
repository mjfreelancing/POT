import { expect, test } from '../../fixtures/auth';

// Covers dashboard cards on mobile. Dashboard cards stack
// correctly on mobile. The dashboard's overview card grids use
// `grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-4` (AccountsOverview /
// ExpensesOverview / IncomesOverview) and `grid grid-cols-2 lg:grid-cols-4`
// (QuickActions): 2 columns on mobile, 3-4 on desktop. The vertical
// section stacking (`space-y-6`) is identical on every viewport, so the
// mobile-specific contract is the 2-up card grid fitting the viewport with no
// horizontal overflow.
//
// We measure the card column count via the Y positions of the account-card
// description headings (each AccountCard renders its description as an <h3>):
// on mobile cards 0 and 1 share a row and card 2 is on the next row; on
// desktop all three share a single row.
//
// Parallel-safe: read-only (page load + list GETs only).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

test('dashboard overview cards: 2 columns per row on mobile, 3-4 on desktop', async ({
  page,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  // The Accounts Overview section is open by default (no persisted local
  // storage on a fresh login) — wait for it and its cards.
  await page.goto('/dashboard');
  await expect(
    page.getByRole('heading', { name: 'Accounts Overview', exact: true }),
  ).toBeVisible();

  // The card grids load async (accounts/expenses/incomes fetches render
  // skeletons first). Wait until at least 3 card titles (h3) are present — the
  // 3 seeded account cards render before the (optional) expense/income cards —
  // so the measurement below never races the data load.
  await expect
    .poll(() => page.getByRole('heading', { level: 3 }).count())
    .toBeGreaterThanOrEqual(3);

  // The overview card titles (AccountCard / ExpenseCard / IncomeCard) are h3
  // headings. Group every visible h3 by its vertical position: cards on the
  // same grid row share a Y, so the largest group size = the number of columns
  // in the widest row. This is robust to API/rendering order differences.
  // (Quick Actions cards are h1 and section titles h2, so only the overview
  // cards count here.)
  const maxCardsPerRow = await page.evaluate(() => {
    const rows = new Map<number, number>();

    for (const heading of document.querySelectorAll('h3')) {
      const box = heading.getBoundingClientRect();
      if (box.width === 0 || box.height === 0) {
        continue; // skip any hidden/empty heading
      }
      const y = Math.round(box.y);
      rows.set(y, (rows.get(y) ?? 0) + 1);
    }

    return rows.size === 0 ? 0 : Math.max(...rows.values());
  });

  if (isMobile) {
    // Every dashboard card grid is `grid-cols-2` on mobile — never more than
    // two cards share a row.
    expect(maxCardsPerRow).toBe(2);
  } else {
    // lg:grid-cols-3 / xl:grid-cols-4 on desktop — at least three share a row.
    expect(maxCardsPerRow).toBeGreaterThanOrEqual(3);
  }
});

test('mobile: dashboard renders with no horizontal overflow', async ({
  page,
}, testInfo) => {
  test.skip(
    !isMobileProject(testInfo),
    'Horizontal-overflow guard is meaningful only on a narrow mobile viewport',
  );

  await page.goto('/dashboard');

  await expect(
    page.getByRole('heading', { name: 'Accounts Overview', exact: true }),
  ).toBeVisible();

  // The 2-up card grids must fit the viewport — nothing may bleed off-screen
  // horizontally (a common responsive regression when a fixed-width element
  // or grid does not collapse).
  const noHorizontalOverflow = await page.evaluate(
    () => document.documentElement.scrollWidth <= window.innerWidth + 1,
  );
  expect(noHorizontalOverflow).toBeTruthy();
});
