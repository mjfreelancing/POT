import type { APIRequestContext, Playwright } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

// Covers the approvals flow: approve/reject a pending
// platform approval from /approvals/pending.
//
// The seed has no pending users, so each test creates its own via the API:
// POST /api/users/invite (creates a Pending user) -> flip to status 'Approval'
// via PUT /api/users/{id}/status (the pending-approvals list filters on
// UserStatus.Approval). The invite returns no body, so the created user is
// located through GET /api/users by its unique username.
//
// KNOWN GAP (documented — do not silently rely on it): there is no delete-user
// API, so the approved/rejected test user stays in the database for the rest of
// the run. This is safe today because (a) every run starts from a FRESH
// disposable Testcontainers Postgres (no cross-run accumulation), (b) usernames
// are unique per run, and (c) no other test asserts the real /users list
// content. Revisit if a delete-user API is added or /users gains real E2E
// coverage.
//
// Desktop-only: the approvals table + row menu are desktop; mobile card flows
// are covered by mobileCardGrids.test.ts.

const apiBaseUrl = 'http://127.0.0.1:5242';

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// Unauthenticated API request context; auth comes from the accessToken fixture.
async function createRequestContext(
  playwright: Playwright,
): Promise<APIRequestContext> {
  // 60s per-action timeout mirrors the test timeout (same rationale as
  // filters.test.ts): a cold first API call on the loaded shared stack can
  // exceed Playwright's 30s request default.
  return playwright.request.newContext({
    baseURL: apiBaseUrl,
    timeout: 60_000,
  });
}

const authHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

// Invites a user, flips them to status 'Approval' (so they appear on
// /approvals/pending), and returns the unique username used to locate the row.
async function createPendingApprovalUser(
  request: APIRequestContext,
  accessToken: string,
): Promise<{ username: string }> {
  const stamp = Date.now();
  const username = `e2e_approval_${stamp}`;
  const email = `${username}@local.test`;

  // The invite needs at least one role id — use the Viewer role.
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
    data: { username, email, roleIds: [viewerRole!.rowId] },
  });
  expect(inviteResponse.ok()).toBeTruthy();

  // The invite returns no body — locate the created user by username.
  const usersResponse = await request.get('/api/users', {
    headers: authHeaders(accessToken),
  });
  expect(usersResponse.ok()).toBeTruthy();

  const users = (await usersResponse.json()) as {
    rowId: string;
    etag: number;
    username: string;
  }[];
  const invited = users.find(user => user.username === username);
  expect(invited).toBeTruthy();

  // Flip Pending -> Approval so the user appears on /approvals/pending.
  const statusResponse = await request.put(
    `/api/users/${invited!.rowId}/status`,
    {
      headers: authHeaders(accessToken),
      data: { etag: invited!.etag, status: 'Approval' },
    },
  );
  expect(statusResponse.ok()).toBeTruthy();

  return { username };
}

test.describe.serial('Platform approvals (API-seeded, no delete gap)', () => {
  test('approve a pending approval removes it and shows the success toast', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Approvals table is desktop-only; mobile uses the card grid',
    );

    const request = await createRequestContext(playwright);
    const { username } = await createPendingApprovalUser(request, accessToken);
    await request.dispose();

    await page.goto('/approvals/pending');

    const row = page.getByRole('row').filter({ hasText: username });
    await expect(row).toBeVisible();

    await row.getByRole('button', { name: 'Open menu' }).click();
    await page.getByRole('menuitem', { name: 'Approve' }).click();

    await expect(
      page.getByText('User Approved', { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText(new RegExp(`${username} has been approved`)),
    ).toBeVisible();

    // The approved user leaves the pending list.
    await expect(
      page.getByRole('row').filter({ hasText: username }),
    ).toHaveCount(0);
  });

  test('reject a pending approval removes it and shows the success toast', async ({
    page,
    playwright,
    accessToken,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Approvals table is desktop-only; mobile uses the card grid',
    );

    const request = await createRequestContext(playwright);
    const { username } = await createPendingApprovalUser(request, accessToken);
    await request.dispose();

    await page.goto('/approvals/pending');

    const row = page.getByRole('row').filter({ hasText: username });
    await expect(row).toBeVisible();

    await row.getByRole('button', { name: 'Open menu' }).click();
    await page.getByRole('menuitem', { name: 'Reject' }).click();

    await expect(
      page.getByText('User Rejected', { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText(new RegExp(`${username} has been rejected`)),
    ).toBeVisible();

    await expect(
      page.getByRole('row').filter({ hasText: username }),
    ).toHaveCount(0);
  });
});
