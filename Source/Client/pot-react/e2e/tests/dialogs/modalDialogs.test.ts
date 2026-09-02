import {
  test as baseTest,
  expect,
  type Browser,
  type Page,
} from '@playwright/test';

import { test as adminTest } from '../../fixtures/auth';

// Covers the MODAL dialog interaction
// contract on the shadcn Dialog (src/components/ui/dialog.tsx, built on Radix
// Dialog). Unlike the always-open CREATE SHEETS covered by mobileSheetsDialogs.test.ts
// (modal={false}, NO onOpenChange — closed only via the form's Cancel or by
// navigating back), these dialogs open from a trigger and CAN be closed three
// ways:
//   - Escape (Radix Dialog Root closes on Escape unless onEscapeKeyDown is
//     prevented — always enabled for these dialogs);
//   - backdrop click (the Root is modal by default and these DialogContent
//     usages do NOT pass the custom `modal` prop, so onPointerDownOutside is
//     undefined and Radix's default outside-pointer-down close applies);
//   - an explicit footer action (Cancel).
//
// Catalog targets:
//   1-3. SignupDialog (src/features/auth/signup/SignupDialog.tsx), opened from
//        the login page's "Sign up" link. Uses a truly unauthenticated context
//        (same pattern as baselineUsersImport.loginViaUi — explicit empty
//        storageState), so there are NO auth/data/permission dependencies and
//        the tests run on all 4 projects. Phase-1 contract: role=dialog, title
//        "Create Account", description "Enter your username and email to get
//        started", modal backdrop present. It has no X close button
//        (showCloseButton defaults false) and no phase-1 Cancel, so closing is
//        via Escape / backdrop click.
//   4.   UserRoleDialog (src/features/users/components/UserRoleDialog.tsx),
//        opened from a user row's "Open menu" -> "Change Role". Desktop-only
//        here (the mobile user-card grid opens the SAME dialog via its own
//        "Open menu" — structurally identical; the desktop table is the
//        primary target). Asserts the dialog reflects the SELECTED
//        user's username, then closes via Escape AND via its explicit "Cancel"
//        footer button. Read-only (never submits "Update Role").
//
// Documented, NOT E2E-tested (consistent with the projection-detail-sheet
// decision in mobileSheetsDialogs.test.ts): the settings "Unsaved Changes"
// dialog passes the custom `modal`
// prop to DialogContent, which sets onPointerDownOutside={e => e.preventDefault()}
// -> a backdrop click does NOT close it (only Escape / the footer actions do).
// Reaching it requires a settings dirty-flow (edit a field, then attempt to
// close), so it is documented rather than tested here.
//
// Parallel-safe: read-only (no mutations) across all 4 projects.

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// Opens the login page in a truly unauthenticated context (no auth fixture, so
// no login/PBKDF2 cost — the login page needs no data). Returns the page; the
// caller closes the context via page.context().close().
async function openLoginPage(browser: Browser): Promise<Page> {
  const context = await browser.newContext({
    storageState: { cookies: [], origins: [] },
  });
  const page = await context.newPage();
  await page.goto('/login');
  return page;
}

const signUpButton = (page: Page) =>
  page.getByRole('button', { name: 'Sign up', exact: true });

baseTest(
  'sign-up dialog opens from the login page (role, title, description)',
  async ({ browser }) => {
    const page = await openLoginPage(browser);
    try {
      await signUpButton(page).click();

      const dialog = page.getByRole('dialog');
      await expect(dialog).toBeVisible();
      await expect(
        dialog.getByRole('heading', { name: 'Create Account', exact: true }),
      ).toBeVisible();
      await expect(
        dialog.getByText('Enter your username and email to get started', {
          exact: true,
        }),
      ).toBeVisible();

      // The modal backdrop is rendered behind the dialog.
      await expect(page.locator('[data-slot="dialog-overlay"]')).toBeVisible();
    } finally {
      await page.context().close();
    }
  },
);

baseTest('sign-up dialog closes on Escape', async ({ browser }) => {
  const page = await openLoginPage(browser);
  try {
    await signUpButton(page).click();

    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();

    await page.keyboard.press('Escape');

    await expect(dialog).toBeHidden();
    await expect(page.locator('[data-slot="dialog-overlay"]')).toBeHidden();
  } finally {
    await page.context().close();
  }
});

baseTest(
  'sign-up dialog closes on a backdrop click (outside the dialog)',
  async ({ browser }) => {
    const page = await openLoginPage(browser);
    try {
      await signUpButton(page).click();

      const dialog = page.getByRole('dialog');
      await expect(dialog).toBeVisible();

      // The dialog content is centered, so the overlay's top-left corner is
      // outside it on every project viewport (on the narrowest mobile the
      // content's left edge is ~91px and top edge ~43px, well clear of (5,5)).
      // Clicking there is a pointer-down-outside -> the modal Radix Dialog
      // closes.
      await page
        .locator('[data-slot="dialog-overlay"]')
        .click({ position: { x: 5, y: 5 } });

      await expect(dialog).toBeHidden();
    } finally {
      await page.context().close();
    }
  },
);

adminTest(
  'Change Role dialog opens from a user row menu; Escape and Cancel close it',
  async ({ page }, testInfo) => {
    adminTest.skip(
      isMobileProject(testInfo),
      'Desktop users table + row menu; mobile user cards open the same dialog via their own menu',
    );

    await page.goto('/users');

    // e2e_viewer is a canonical seed user; its row menu is available because
    // the acting user (e2e_admin) is not the row's user (self-row has no menu).
    const row = page.getByRole('row').filter({ hasText: 'e2e_viewer' });
    await expect(row).toBeVisible();

    const openDialog = async () => {
      await row.getByRole('button', { name: 'Open menu' }).click();
      await page
        .getByRole('menuitem', { name: 'Change Role', exact: true })
        .click();

      const dialog = page.getByRole('dialog');
      await expect(dialog).toBeVisible();
      return dialog;
    };

    // Open -> the dialog reflects the SELECTED user's username.
    const dialog = await openDialog();
    await expect(
      dialog.getByRole('heading', { name: 'Change Role', exact: true }),
    ).toBeVisible();
    await expect(
      dialog.getByText(/Update the role assignment for e2e_viewer/),
    ).toBeVisible();

    // Close via Escape.
    await page.keyboard.press('Escape');
    await expect(dialog).toBeHidden();

    // Reopen -> close via the explicit Cancel footer button.
    const reopenedDialog = await openDialog();
    await reopenedDialog
      .getByRole('button', { name: 'Cancel', exact: true })
      .click();
    await expect(reopenedDialog).toBeHidden();
  },
);
