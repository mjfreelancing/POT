import { expect, test } from '../../fixtures/auth';

// Covers the ThemeProvider light/dark/system toggle
// (src/components/theme/ThemeProvider.tsx + ThemeToggle.tsx). The toggle lives
// in the sidebar footer (AppSidebar -> SidebarFooter -> ThemeToggle): on
// desktop the rail footer is always visible; on mobile the sidebar is a closed
// Sheet until `Toggle Sidebar` is clicked (see mobileSidebar.test.ts), so the footer is reached
// inside `[data-slot="sidebar"]` after opening the sheet.
//
// Behavior (verified against the source):
// - ThemeToggle shows the Sun icon in light mode and the Moon icon in dark
//   mode; clicking the VISIBLE icon switches: Sun -> dark, Moon -> light.
// - setTheme() persists to localStorage under a per-user key
//   `pot:{env}:user:{userId}:theme` (App.tsx ThemedApp -> buildUserScopedKey
//   when signed in; env-scoped `pot:{env}:theme` when logged out) and applies
//   the `dark`/`light` class to <html>.
// - system theme resolves via `prefers-color-scheme` (Playwright default:
//   light) and is NOT written to localStorage.
//
// Each test gets a fresh context (fresh localStorage), so there is no
// cross-test contamination and no restore step is needed.
//
// Parallel-safe: read-only (local theme state only).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// The footer lives under the sidebar rail (desktop) or the mobile sheet content;
// both carry `data-slot="sidebar"`.
const sidebarFooter = (page: import('@playwright/test').Page) =>
  page.locator('[data-slot="sidebar"] [data-slot="sidebar-footer"]');

async function revealThemeToggle(
  page: import('@playwright/test').Page,
  isMobile: boolean,
): Promise<void> {
  // On mobile the sidebar is a closed Sheet until the hamburger opens it; the
  // footer is only reachable inside it. On desktop the rail footer is always
  // visible (the Toggle Sidebar button would COLLAPSE the rail, so it is not
  // clicked here).
  if (isMobile) {
    await page.getByRole('button', { name: 'Toggle Sidebar' }).click();
  }
  await expect(sidebarFooter(page)).toBeVisible();
}

const themeStorageValue = (page: import('@playwright/test').Page) =>
  page.evaluate(() => {
    const key = Object.keys(localStorage).find(key => key.endsWith(':theme'));
    return key ? localStorage.getItem(key) : null;
  });

test('theme toggle switches dark and light and persists per user across reloads', async ({
  page,
}, testInfo) => {
  const isMobile = isMobileProject(testInfo);

  await page.goto('/expenses');
  // Web-first: the expenses page (authenticated) is mounted.
  await expect(
    page.getByRole('textbox', { name: 'Search expenses by description' }),
  ).toBeVisible();

  const html = page.locator('html');
  const sun = sidebarFooter(page).locator('svg.lucide-sun');
  const moon = sidebarFooter(page).locator('svg.lucide-moon');

  // Light (system default) -> click the visible Sun -> dark.
  await revealThemeToggle(page, isMobile);
  await sun.click();
  await expect(html).toHaveClass(/\bdark\b/);
  expect(await themeStorageValue(page)).toBe('dark');

  // Persists across a reload (read back from the per-user storage key).
  await page.reload();
  await expect(html).toHaveClass(/\bdark\b/);

  // Dark -> click the visible Moon -> light, and it persists too.
  await revealThemeToggle(page, isMobile);
  await moon.click();
  await expect(html).not.toHaveClass(/\bdark\b/);
  expect(await themeStorageValue(page)).toBe('light');

  await page.reload();
  await expect(html).not.toHaveClass(/\bdark\b/);
});

test('system theme applies the OS color-scheme class on first load', async ({
  page,
}) => {
  await page.goto('/expenses');
  await expect(
    page.getByRole('textbox', { name: 'Search expenses by description' }),
  ).toBeVisible();

  // With no persisted theme, the provider resolves 'system' via
  // prefers-color-scheme (Playwright default colorScheme is light) and applies
  // exactly that class to <html>.
  const applied = await page.evaluate(() => ({
    htmlHasDark: document.documentElement.classList.contains('dark'),
    prefersDark: window.matchMedia('(prefers-color-scheme: dark)').matches,
  }));

  expect(applied.htmlHasDark).toBe(applied.prefersDark);
});
