import { expect, test } from '../../fixtures/auth';

// Covers the app sidebar. On
// desktop it is a persistent inline rail whose nav links are always visible;
// on mobile (< 768 px, `useIsMobile`) it is hidden behind the `Toggle Sidebar`
// hamburger and opens as an overlay Sheet (`sidebar.tsx` renders the sidebar
// as a Sheet with `data-mobile="true"`), which auto-closes when a nav link is
// clicked (`MenuGroup` calls `setOpenMobile(false)` on link click).
//
// Scoped to `[data-slot="sidebar"]` so the link lookups only ever target the
// sidebar (inline rail on desktop, sheet content on mobile) and never a
// same-named link elsewhere on the page.
//
// Parallel-safe: read-only (navigation + visibility asserts only).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// The sidebar nav links live under `[data-slot="sidebar"]` on both desktop
// (inline rail) and mobile (Sheet content). When the mobile sheet is closed,
// the SheetContent is not rendered, so this resolves to zero elements.
const sidebarLink = (page: import('@playwright/test').Page, name: string) =>
  page
    .locator('[data-slot="sidebar"]')
    .getByRole('link', { name, exact: true });

test('sidebar: mobile opens as a sheet; desktop is inline', async ({
  page,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  await page.goto('/dashboard');

  const accountsLink = sidebarLink(page, 'Accounts');

  if (isMobile) {
    // Closed sheet -> nav links are not rendered at all.
    await expect(accountsLink).toHaveCount(0);

    // The hamburger trigger is present and opens the sheet.
    const trigger = page.getByRole('button', { name: 'Toggle Sidebar' });
    await expect(trigger).toBeVisible();
    await trigger.click();

    // Sheet open -> the link renders and is usable.
    await expect(accountsLink).toBeVisible();
    await accountsLink.click();

    // Navigation lands on the page and auto-closes the sheet.
    await expect(page).toHaveURL(/\/accounts$/);
    await expect(
      page.getByRole('heading', { name: 'Account Management', exact: true }),
    ).toBeVisible();
    await expect(accountsLink).toHaveCount(0);
  } else {
    // Desktop: the sidebar rail renders inline — links visible without any toggle.
    await expect(accountsLink).toBeVisible();
    await expect(
      page.getByRole('heading', { name: 'Financial Dashboard', exact: true }),
    ).toBeVisible();
  }
});

test('mobile: Toggle Sidebar opens the sheet; Escape closes it', async ({
  page,
}, testInfo) => {
  test.skip(
    !isMobileProject(testInfo),
    'Desktop has no overlay sheet; this verifies the mobile sheet lifecycle',
  );

  await page.goto('/dashboard');

  const accountsLink = sidebarLink(page, 'Accounts');
  await expect(accountsLink).toHaveCount(0);

  const trigger = page.getByRole('button', { name: 'Toggle Sidebar' });
  await trigger.click();
  await expect(accountsLink).toBeVisible(); // opened

  // Escape dismisses the shadcn Sheet.
  await page.keyboard.press('Escape');
  await expect(accountsLink).toHaveCount(0); // closed
});

test('desktop: Toggle Sidebar collapses and expands the sidebar rail', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'Mobile uses the overlay sheet; this verifies the desktop rail collapse',
  );

  await page.goto('/dashboard');

  const sidebar = page.locator('[data-slot="sidebar"]');
  const trigger = page.locator('[data-slot="sidebar-trigger"]');

  await expect(sidebar).toHaveAttribute('data-state', 'expanded');
  await trigger.click();
  await expect(sidebar).toHaveAttribute('data-state', 'collapsed');
  await trigger.click();
  await expect(sidebar).toHaveAttribute('data-state', 'expanded');
});
