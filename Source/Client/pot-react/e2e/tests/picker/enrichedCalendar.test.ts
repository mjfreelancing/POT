import { format } from 'date-fns';
import { expect, test } from '../../fixtures/auth';

// Covers the EnrichedCalendar date
// picker (react-day-picker grid + custom month/year nav header +
// Today/Cancel/Accept footer), exercised through the expense create form's
// "Next Due" picker (an EnrichedDatePicker popover).
//
// The component's month-maintenance / min-max logic is already covered by unit
// tests (tests/components/picker/EnrichedCalendar.test.tsx and
// EnrichedCalendar.VisualBehavior.test.tsx — jsdom). This file adds real
// browser coverage: navigation, selection/accept, cancel semantics, the Today
// jump, and — because the user has hit shadcn-upgrade regressions — structural
// APPEARANCE assertions (day-grid markup, the shadcn `day_today`/`day_selected`
// styling hooks) that run against the real rendered DOM.
//
// NOTE on visual appearance: class/markup assertions catch shadcn regressions
// that change structure or the styling-hook classes. Pure pixel/color/layout
// drift is a screenshot-baseline concern (not covered here).
//
// Parallel-safe: opens the create sheet + calendar but never submits the form
// (no mutations, no teardown).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const today = new Date();

// dateIsoFormat mirrors the app's helper (local date -> YYYY-MM-DD).
const dateIsoFormat = (date: Date): string => format(date, 'yyyy-MM-dd');

// The picker trigger renders format(selectedDate, 'PPP') after the app's
// ISO round-trip (local -> YYYY-MM-DD -> new Date(iso) = UTC midnight), so the
// expected string must mirror that exact path to be timezone-safe.
const expectedTriggerText = (date: Date): string =>
  format(new Date(dateIsoFormat(date)), 'PPP');

// The custom nav header shows the display month as "MMM yyyy".
const monthLabel = (monthOffset: number): string =>
  format(
    new Date(today.getFullYear(), today.getMonth() + monthOffset, 1),
    'MMM yyyy',
  );

const calendarPopover = (page: import('@playwright/test').Page) =>
  page.locator('[data-slot="popover-content"]');

// react-day-picker v9 day cells are <button name="day" role="gridcell">; the
// adjacent-month (outside) days carry the `day-outside` class and are excluded
// so a day number always resolves to the current month's cell.
//
// hasText is a SUBSTRING match: a bare '1' also matches 10-19/21/31, so any
// day passed to this helper must be anchored to its exact value or the locator
// fails with a strict-mode violation. This bit hard on the FIRST day of a month
// (today.getDate() === 1 -> 12 matching cells) and is date-boundary-dependent:
// it only stayed green because earlier runs landed on days like 31 whose string
// is unambiguous. Anchoring to `^dayNumber$` makes every day unambiguous.
const dayButton = (page: import('@playwright/test').Page, dayNumber: number) =>
  page
    .locator('button[name="day"]:not(.day-outside)')
    .filter({ hasText: new RegExp(`^${dayNumber}$`) });

async function openNextDueCalendar(page: import('@playwright/test').Page) {
  // The calendar popover (a floating-ui popper anchored to the field) can
  // extend below the fold on SHORT viewports (e.g. iPhone 14's 844px height),
  // putting the Accept/Cancel footer buttons below the screen edge. A real
  // user CAN reach them by scrolling the page (the anchored popover repositions
  // as the sheet scrolls), but Playwright's click auto-scrolls only the
  // popover's own (non-scrollable) ancestors, so it reports "outside of the
  // viewport". Give the tests a viewport tall enough for the full calendar
  // while KEEPING the project's width, so the mobile/desktop layout switch
  // (useIsMobile: width < 768px) is unaffected and no extra scroll step is
  // needed.
  const viewport = page.viewportSize();
  if (viewport) {
    await page.setViewportSize({
      width: viewport.width,
      height: Math.max(viewport.height, 1100),
    });
  }

  await page.goto('/expenses');
  await page.getByRole('button', { name: 'Add a new expense' }).first().click();
  await expect(
    page.getByRole('heading', { name: 'Create Expense', exact: true }),
  ).toBeVisible();

  await page.getByLabel('Next Due').click();

  const popover = calendarPopover(page);
  await expect(popover.getByRole('button', { name: 'Accept' })).toBeVisible();
}

test('calendar navigates months and years with the header controls', async ({
  page,
}) => {
  await openNextDueCalendar(page);
  const popover = calendarPopover(page);

  await expect(popover.getByText(monthLabel(0), { exact: true })).toBeVisible();

  await popover.getByRole('button', { name: 'Previous Month' }).click();
  await expect(
    popover.getByText(monthLabel(-1), { exact: true }),
  ).toBeVisible();

  await popover.getByRole('button', { name: 'Next Year' }).click();
  await expect(
    popover.getByText(monthLabel(11), { exact: true }),
  ).toBeVisible();

  await popover.getByRole('button', { name: 'Next Month' }).click();
  await expect(
    popover.getByText(monthLabel(12), { exact: true }),
  ).toBeVisible();

  await popover.getByRole('button', { name: 'Previous Year' }).click();
  await expect(popover.getByText(monthLabel(0), { exact: true })).toBeVisible();
});

test('selecting a day and Accept applies it to the field', async ({ page }) => {
  await openNextDueCalendar(page);
  const popover = calendarPopover(page);

  const selectedDay = dayButton(page, 15);
  await selectedDay.click();
  await expect(selectedDay).toHaveAttribute('aria-selected', 'true');

  await popover.getByRole('button', { name: 'Accept' }).click();
  await expect(popover).toHaveCount(0); // picker closed

  const expected = expectedTriggerText(
    new Date(today.getFullYear(), today.getMonth(), 15),
  );
  await expect(page.getByLabel('Next Due')).toContainText(expected);
});

test('Cancel discards the in-progress selection', async ({ page }) => {
  await openNextDueCalendar(page);
  const popover = calendarPopover(page);
  const trigger = page.getByLabel('Next Due');
  const originalText = (await trigger.textContent())?.trim() ?? '';

  await dayButton(page, 15).click();
  await popover.getByRole('button', { name: 'Cancel' }).click();
  await expect(popover).toHaveCount(0); // picker closed

  // Cancel does NOT change the field value.
  await expect(trigger).toHaveText(originalText);
});

test('Today jumps to today and Accept applies it', async ({ page }) => {
  await openNextDueCalendar(page);
  const popover = calendarPopover(page);

  await popover.getByRole('button', { name: 'Today' }).click();

  const todayDay = dayButton(page, today.getDate());
  await expect(todayDay).toHaveAttribute('aria-selected', 'true');

  await popover.getByRole('button', { name: 'Accept' }).click();
  await expect(popover).toHaveCount(0);

  await expect(page.getByLabel('Next Due')).toContainText(
    expectedTriggerText(today),
  );
});

test('calendar renders the expected day-grid structure and styling', async ({
  page,
}) => {
  await openNextDueCalendar(page);
  const popover = calendarPopover(page);

  // The four custom header navigation controls are present.
  for (const name of [
    'Previous Year',
    'Previous Month',
    'Next Month',
    'Next Year',
  ]) {
    await expect(popover.getByRole('button', { name })).toBeVisible();
  }

  // react-day-picker renders the month as a table grid.
  await expect(popover.locator('table')).toBeVisible();

  // A full month of in-month day cells renders (>= 28 days).
  const inMonthDayCount = await popover
    .locator('button[name="day"]:not(.day-outside)')
    .count();
  expect(inMonthDayCount).toBeGreaterThanOrEqual(28);

  // Today's cell carries the shadcn "today" accent styling hook (day_today).
  const todayButton = dayButton(page, today.getDate());
  await expect(todayButton).toHaveClass(/bg-accent/);

  // Selecting a day applies the shadcn "selected" styling hook (day_selected).
  await dayButton(page, 15).click();
  const selectedButton = dayButton(page, 15);
  await expect(selectedButton).toHaveAttribute('aria-selected', 'true');
  await expect(selectedButton).toHaveClass(/bg-primary/);
});
