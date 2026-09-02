import { expect, test } from '../../fixtures/auth';

// Desktop-only: on mobile the sidebar is hidden behind a hamburger trigger and
// opens as a Sheet (covered by mobileSidebar.test.ts). The skip is
// evaluated inside the test body because the mobile project name is only
// available via `testInfo`.
const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const sidebarLinks = [
  {
    linkName: 'Dashboard',
    pathname: '/dashboard',
    heading: 'Financial Dashboard',
  },
  {
    linkName: 'Projections',
    pathname: '/projections',
    heading: 'Projections',
  },
  {
    linkName: 'Accounts',
    pathname: '/accounts',
    heading: 'Account Management',
  },
  {
    linkName: 'Expenses',
    pathname: '/expenses',
    heading: 'Expense Management',
  },
  {
    linkName: 'Income',
    pathname: '/incomes',
    heading: 'Income Management',
  },
  {
    linkName: 'Users',
    pathname: '/users',
    heading: 'Users',
  },
  {
    linkName: 'Approvals',
    pathname: '/approvals/pending',
    heading: 'Pending Approvals',
  },
];

const urlRegexFor = (pathname: string): RegExp =>
  new RegExp(`${pathname.replace(/\//g, '\\/')}/?$`);

test.describe('sidebar navigation (admin)', () => {
  for (const target of sidebarLinks) {
    test(`sidebar '${target.linkName}' link opens the ${target.heading} page`, async ({
      page,
    }, testInfo) => {
      test.skip(
        isMobileProject(testInfo),
        'Sidebar navigation is desktop-only; mobile sidebar is covered by mobileSidebar.test.ts',
      );

      await page.goto('/dashboard');

      const navLink = page
        .locator('[data-slot="sidebar"]')
        .getByRole('link', { name: target.linkName, exact: true });

      await expect(navLink).toBeVisible();
      await navLink.click();

      await expect(page).toHaveURL(urlRegexFor(target.pathname));
      await expect(
        page.getByRole('heading', { name: target.heading, exact: true }),
      ).toBeVisible();
    });
  }
});
