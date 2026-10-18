import type { APIRequestContext } from '@playwright/test';

import { expect, test } from '../../fixtures/auth';
import { authHeaders, createE2eRequestContext } from '../../helpers/api';
import { adminCredentials } from '../../helpers/secrets';

// Covers the Delete User journey added to the users list: DELETE /api/users/{id}
// behind the reusable destructive confirmation (typed username + the acting
// administrator's own password).
//
// Fixture-managed: each run invites its own throwaway user and deletes it, so no
// seeded user is consumed and the shared Users list does not grow. Assertions are
// scoped to the run's own row by its unique email and never count or order the list.
//
// Desktop-only: the row action menu belongs to the users table; the mobile card
// grid has its own action surface (covered by the mobile card-grid suite). The
// confirmation dialog itself is shared and unit-tested separately.

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

type SiteUserRow = {
  rowId: string;
  username: string;
  etag: number;
  status: string;
};

test('deletes a throwaway user after the typed confirmation', async ({
  page,
  accessToken,
  playwright,
}, testInfo) => {
  test.skip(isMobileProject(testInfo), 'Desktop users table row menu');

  const request = await createE2eRequestContext(playwright);

  try {
    const target = await invitePendingUser(request, accessToken);

    await page.goto('/users');

    const row = usersRow(page, target.username);
    await expect(row).toBeVisible();

    await row.getByRole('button', { name: 'Open menu' }).click();
    await page
      .getByRole('menuitem', { name: 'Delete User…', exact: true })
      .click();

    const dialog = page.getByRole('alertdialog');
    await expect(dialog).toBeVisible();

    const confirmButton = dialog.getByRole('button', {
      name: 'Delete User',
      exact: true,
    });
    await expect(confirmButton).toBeDisabled();

    // Cancel is the first action and takes focus when the dialog opens, so Enter is never destructive.
    await expect(
      dialog.getByRole('button', { name: 'Cancel', exact: true }),
    ).toBeFocused();

    // A click outside the dialog must not dismiss the confirmation.
    await page.mouse.click(10, 10);
    await expect(dialog).toBeVisible();
    await expect(confirmButton).toBeDisabled();

    await dialog.getByLabel('Username', { exact: true }).fill(target.username);
    await dialog.getByLabel('Your password', { exact: true }).fill(
      adminCredentials.password,
    );

    await expect(confirmButton).toBeEnabled();

    const deleteResponse = page.waitForResponse(
      response =>
        response.url().includes(`/api/users/${target.rowId}`) &&
        response.request().method() === 'DELETE',
    );

    await confirmButton.click();

    const deleted = await deleteResponse;
    expect(deleted.ok()).toBeTruthy();
    await deleted.finished();

    await expect(page.getByText('User Deleted', { exact: true })).toBeVisible({
      timeout: 30000,
    });

    await expect(dialog).toBeHidden();
    await expect(row).toBeHidden();

    // The row disappearing is the client's view; the record is gone server-side.
    const usersResponse = await request.get('/api/users', {
      headers: authHeaders(accessToken),
    });
    expect(usersResponse.ok()).toBeTruthy();

    const users = (await usersResponse.json()) as SiteUserRow[];
    expect(users.some(user => user.username === target.username)).toBe(false);
  } finally {
    await request.dispose();
  }
});

test('keeps the user when the confirmation password is wrong', async ({
  page,
  accessToken,
  playwright,
}, testInfo) => {
  test.skip(isMobileProject(testInfo), 'Desktop users table row menu');

  const request = await createE2eRequestContext(playwright);
  let target: SiteUserRow | undefined;

  try {
    const invitedUser = await invitePendingUser(request, accessToken);
    target = invitedUser;

    await page.goto('/users');

    const row = usersRow(page, invitedUser.username);
    await expect(row).toBeVisible();

    await row.getByRole('button', { name: 'Open menu' }).click();
    await page
      .getByRole('menuitem', { name: 'Delete User…', exact: true })
      .click();

    const dialog = page.getByRole('alertdialog');
    await expect(dialog).toBeVisible();

    await dialog
      .getByLabel('Username', { exact: true })
      .fill(invitedUser.username);
    await dialog.getByLabel('Your password', { exact: true }).fill(
      'not-the-admin-password',
    );

    const deleteResponse = page.waitForResponse(
      response =>
        response.url().includes(`/api/users/${invitedUser.rowId}`) &&
        response.request().method() === 'DELETE',
    );

    await dialog
      .getByRole('button', { name: 'Delete User', exact: true })
      .click();

    // The refusal is a 400 with the password error in the ProblemDetails detail, shown inside the
    // open confirmation. The dialog stays open so the refusal is read in place, and the user is still listed.
    const refused = await deleteResponse;
    expect(refused.status()).toBe(400);
    await refused.finished();

    await expect(dialog.getByRole('alert')).toHaveText(
      'The password is incorrect.',
      { timeout: 30000 },
    );
    await expect(dialog).toBeVisible();

    // Everything outside an open modal is hidden from the accessibility tree, so
    // close the dialog before asserting the row survived.
    await dialog.getByRole('button', { name: 'Cancel', exact: true }).click();
    await expect(dialog).toBeHidden();
    await expect(row).toBeVisible();
    await expect(row.getByText('Pending', { exact: true })).toBeVisible();
  } finally {
    if (target) {
      // Cleanup through the API so a refused run does not leave its user behind.
      await request.delete(`/api/users/${target.rowId}`, {
        headers: authHeaders(accessToken),
        data: { password: adminCredentials.password },
      });
    }

    await request.dispose();
  }
});

/**
 * Locates the run's own row by its unique email. `hasText` is a substring match, so anchoring on
 * `username@local.test` keeps the locator to exactly one row even when earlier runs left users
 * behind with a username that is a prefix of this one.
 */
function usersRow(
  page: import('@playwright/test').Page,
  username: string,
): import('@playwright/test').Locator {
  return page.getByRole('row').filter({ hasText: `${username}@local.test` });
}

/**
 * Invites a user into the acting administrator's site and returns the created row. The invite
 * response carries no body, so the row is located through the site users list by its unique
 * username.
 */
async function invitePendingUser(
  request: APIRequestContext,
  accessToken: string,
): Promise<SiteUserRow> {
  const username = `e2e_userdel_${Date.now()}_${Math.floor(Math.random() * 1000)}`;

  const rolesResponse = await request.get('/api/roles', {
    headers: authHeaders(accessToken),
  });
  expect(rolesResponse.ok()).toBeTruthy();

  const roles = (await rolesResponse.json()) as {
    rowId: string;
    name: string;
  }[];
  const viewerRole = roles.find(role => role.name === 'Viewer');
  expect(viewerRole).toBeTruthy();

  const inviteResponse = await request.post('/api/users/invite', {
    headers: authHeaders(accessToken),
    data: {
      username,
      email: `${username}@local.test`,
      roleIds: [viewerRole!.rowId],
    },
  });
  expect(inviteResponse.ok()).toBeTruthy();

  const usersResponse = await request.get('/api/users', {
    headers: authHeaders(accessToken),
  });
  expect(usersResponse.ok()).toBeTruthy();

  const users = (await usersResponse.json()) as SiteUserRow[];
  const invited = users.find(user => user.username === username);
  expect(invited).toBeTruthy();

  return invited!;
}
