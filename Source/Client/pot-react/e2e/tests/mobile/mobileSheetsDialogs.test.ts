import { expect, test } from '../../fixtures/auth';

// Covers sheets/dialogs on mobile.
//
// The create/edit form sheets (AccountSheet / ExpenseSheet / IncomeSheet, a
// shared FORM_SHEET_STYLES.SHEET_CONTENT) are RIGHT-side sheets that are
// `modal={false}` and always-open controlled sheets (no `onOpenChange` — they
// close by navigating back, e.g. the form's Cancel button; Escape is not
// wired up), sized `w-3/4` on mobile and `sm:max-w-sm/lg` (capped ≤ 512px) on
// desktop, with `overflow-y-auto` content. This file validates:
//   1. the sheet opens as a dialog with its title and closes via Cancel
//      (the app's real dismissal path for these non-modal, always-open sheets);
//   2. the sheet is sized responsively (3/4 viewport width on mobile,
//      constrained ≤ 512px on desktop) — measured via boundingBox;
//   3. the long expense form sheet actually scrolls on a short mobile viewport
//      (scrollHeight > clientHeight on the overflow-y-auto content).
//
// NOTE: the projection detail sheets (ExpenseDetails / IncomeDetails) use the
// true full-width-on-mobile pattern (`w-full md:max-w-md`), but they only open
// from a chart-bar click (recharts Bar onClick — no accessible locator), so a
// deterministic test would be coordinate-based and flaky; their full-width
// class is documented rather than tested.
//
// Parallel-safe: opening a create sheet triggers only read-only GETs (no
// mutation until the form is submitted, which these tests never do).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const sheetContent = (page: import('@playwright/test').Page) =>
  page.locator('[data-slot="sheet-content"]');

test('create sheet opens as a dialog with its title and closes via Cancel', async ({
  page,
}) => {
  await page.goto('/accounts');
  await page.getByRole('button', { name: 'Add a new account' }).click();

  const sheet = sheetContent(page);
  await expect(sheet).toBeVisible();
  await expect(page.getByRole('dialog')).toBeVisible();
  await expect(
    page.getByRole('heading', { name: 'Create Account', exact: true }),
  ).toBeVisible();

  // The create sheets are always-open controlled sheets (`modal={false}`, no
  // `onOpenChange`): Escape is NOT wired up — they close by navigating back,
  // e.g. the form's Cancel button. So the sheet-lifecycle assertion uses the
  // app's real dismissal path rather than Escape.
  await page.getByRole('button', { name: 'Cancel' }).click();
  await expect(page).toHaveURL(/\/accounts$/);
  await expect(sheet).toHaveCount(0);
});

test('form sheet is sized responsively (3/4 width on mobile, constrained on desktop)', async ({
  page,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  await page.goto('/accounts');
  await page.getByRole('button', { name: 'Add a new account' }).click();

  const sheet = sheetContent(page);
  await expect(sheet).toBeVisible();

  const box = await sheet.boundingBox();
  const viewport = page.viewportSize();
  expect(box).not.toBeNull();
  expect(viewport).not.toBeNull();

  if (isMobile) {
    // Base SheetContent is `w-3/4` → 75% of the viewport width on mobile.
    expect(Math.abs(box!.width - viewport!.width * 0.75)).toBeLessThan(25);
  } else {
    // On desktop it is capped by `sm:max-w-*` (≤ 512px), well under the
    // 1280px viewport — a constrained side sheet, not full-width.
    expect(box!.width).toBeLessThan(600);
  }
});

test('mobile: the long expense form sheet scrolls', async ({
  page,
}, testInfo) => {
  test.skip(
    !isMobileProject(testInfo),
    'Verifies the mobile long-form scroll behaviour (short viewport)',
  );

  await page.goto('/expenses');
  await page.getByRole('button', { name: 'Add a new expense' }).click();

  const sheet = sheetContent(page);
  await expect(sheet).toBeVisible();

  // The SheetContent is `overflow-y-auto`; a long create form on a ~800px-tall
  // mobile viewport overflows, so the content must scroll rather than clip.
  const isScrollable = await sheet.evaluate(
    element => element.scrollHeight > element.clientHeight,
  );
  expect(isScrollable).toBeTruthy();
});
