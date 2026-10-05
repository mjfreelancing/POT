import type { APIRequestContext } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';
import { authHeaders, createE2eRequestContext } from '../../helpers/api';

// Covers the Users page administration journeys that call the user-mutation routes:
// POST /api/users/invite (setup), POST /api/users/{id}/resend-invite and
// PUT /api/users/{id}/status. These are the journeys the server-side site binding and the
// protected-identity rules must not break, and the list a row-level delete action will extend.
//
// Fixture-managed: each run invites its own target user and cannot delete it (there is no
// delete-user route), so the shared Users list gains rows over time. Assertions are therefore
// scoped to the run's own row by its unique email and never count or order the list.
//
// Desktop-only: the row action menu belongs to the users table; the mobile projects render a
// card grid with its own action surface (covered by the mobile card-grid suite).

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

type SiteUserRow = {
  rowId: string;
  username: string;
  etag: number;
  status: string;
};

test.describe.configure({ mode: 'serial' });

test('resends an invitation for a pending user from the users list', async ({
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
    await expect(row.getByText('Pending', { exact: true })).toBeVisible();

    // The resend endpoint only QUEUES the invitation: submission writes to an unbounded channel and
    // returns, and the dispatch loop swallows send failures. It therefore succeeds with no SMTP server
    // configured, which is the case in this harness. This test asserts the HTTP contract and the client
    // outcome, not mail delivery — delivery is verified manually where SMTP is configured.
    const resendResponse = page.waitForResponse(
      response =>
        response.url().includes(`/api/users/${target.rowId}/resend-invite`) &&
        response.request().method() === 'POST',
    );

    await row.getByRole('button', { name: 'Open menu' }).click();
    await page
      .getByRole('menuitem', { name: 'Resend Invitation', exact: true })
      .click();

    const response = await resendResponse;
    expect(response.ok()).toBeTruthy();
    await response.finished();

    // The success toast confirms the client rendered the outcome (generous timeout: the
    // shared stack can delay the render under full-matrix load).
    await expect(
      page.getByText('Invitation Resent', { exact: true }),
    ).toBeVisible({ timeout: 30000 });
  } finally {
    await request.dispose();
  }
});

test('disables and re-enables a user from the users list', async ({
  page,
  accessToken,
  playwright,
}, testInfo) => {
  test.skip(isMobileProject(testInfo), 'Desktop users table row menu');

  const request = await createE2eRequestContext(playwright);

  try {
    const target = await invitePendingUser(request, accessToken);

    // The status toggle is offered for Enabled/Disabled rows only, so enable the invited user
    // first; the invite leaves them Pending.
    const enableResponse = await request.put(
      `/api/users/${target.rowId}/status`,
      {
        headers: authHeaders(accessToken),
        data: { etag: target.etag, status: 'Enabled' },
      },
    );
    expect(enableResponse.ok()).toBeTruthy();

    await page.goto('/users');

    const row = usersRow(page, target.username);
    await expect(row).toBeVisible();
    await expect(row.getByText('Enabled', { exact: true })).toBeVisible();

    const disableResponse = page.waitForResponse(
      response =>
        response.url().includes(`/api/users/${target.rowId}/status`) &&
        response.request().method() === 'PUT',
    );

    await row.getByRole('button', { name: 'Open menu' }).click();
    await page
      .getByRole('menuitem', { name: 'Disable User', exact: true })
      .click();

    const disabled = await disableResponse;
    expect(disabled.ok()).toBeTruthy();
    await disabled.finished();

    await expect(page.getByText('Status Updated', { exact: true })).toBeVisible(
      { timeout: 30000 },
    );
    await expect(row.getByText('Disabled', { exact: true })).toBeVisible();

    // Re-enable, leaving the target in the state the run created it in.
    const reEnableResponse = page.waitForResponse(
      response =>
        response.url().includes(`/api/users/${target.rowId}/status`) &&
        response.request().method() === 'PUT',
    );

    await row.getByRole('button', { name: 'Open menu' }).click();
    await page
      .getByRole('menuitem', { name: 'Enable User', exact: true })
      .click();

    const reEnabled = await reEnableResponse;
    expect(reEnabled.ok()).toBeTruthy();
    await reEnabled.finished();

    await expect(row.getByText('Enabled', { exact: true })).toBeVisible();
  } finally {
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
  const username = `e2e_useradmin_${Date.now()}_${Math.floor(Math.random() * 1000)}`;

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
