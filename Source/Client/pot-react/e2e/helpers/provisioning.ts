import { randomUUID } from 'node:crypto';

import type { APIRequestContext, APIResponse } from '@playwright/test';

import { authHeaders, createE2eRequestContext } from './api';
import { oneTimeCode, temporaryPassword, waitForMessage } from './mail';
import { adminCredentials } from './secrets';

/**
 * Provisions a tenant for a browser journey that destroys one.
 *
 * Each function drives the product's own routes - signing up, being approved, being invited - so the browser
 * journey starts from a tenant the application created rather than one the seed had to supply. Two secrets are
 * generated along the way and neither is guessable: the sign-up's temporary password and the invitation's. Both
 * are read from the messages the application sent, through the sink, because a suite that cannot read them cannot
 * provision at all.
 *
 * Every identity is named uniquely per call, so the same journey can run in several browser projects at once
 * against one database, and again on a later run.
 */

type StorageState = Awaited<ReturnType<APIRequestContext['storageState']>>;

// The part of the worker fixture these helpers use, taken from the existing helper so the shape is stated once.
type E2eRequestFactory = Parameters<typeof createE2eRequestContext>[0];

export type ProvisionedTenant = {
  /** The username to sign in with, and the one the danger zone asks a person to retype. */
  username: string;
  /** The password the application generated for this identity. */
  password: string;
  /** The address the application sent this identity's message to. */
  email: string;
  /** The display name the header shows, which is how the user menu is opened. */
  displayName: string;
  /** The site the identity belongs to, read from the application rather than assumed. */
  siteName: string;
  /** A JWT for API calls made on this identity's behalf. */
  accessToken: string;
  /** The signed-in cookies, for a browser context that continues this session. */
  storageState: StorageState;
};

type Identity = {
  username: string;
  email: string;
};

type RoleResponse = {
  rowId: string;
  name: string;
};

type PendingApprovalResponse = {
  rowId: string;
  etag: number;
  username: string;
};

type SignUpSendResponse = {
  referenceCode?: string;
};

type LoginResponse = {
  accessToken?: string;
};

type CurrentUserResponse = {
  displayName: string;
  site: {
    name: string;
  };
};

/**
 * Provisions a viewer on the caller's own site by invitation.
 *
 * Intended for a journey that destroys the identity and nothing else: the invitation joins an existing site, so
 * deleting the account leaves that site as it was.
 *
 * @param playwright The worker's request factory, for reaching the API directly.
 * @param label Names the invitation, so a failure says which journey caused it.
 * @returns The provisioned identity, signed in.
 */
export async function provisionInvitedViewer(
  playwright: E2eRequestFactory,
  label: string,
): Promise<ProvisionedTenant> {
  const request = await createE2eRequestContext(playwright);

  try {
    const identity = newIdentity(label);

    const inviter = await signIn(
      request,
      adminCredentials.username,
      adminCredentials.password,
    );

    const rolesResponse = await request.get('/api/roles', {
      headers: authHeaders(inviter.accessToken),
    });

    await expectOk(rolesResponse, 'list the roles an invitation can offer');

    const roles = (await rolesResponse.json()) as RoleResponse[];
    const viewerRole = roles.find(role => role.name === 'Viewer');

    if (!viewerRole) {
      throw new Error(
        `The application offers no 'Viewer' role to invite with (offered: ${roles.map(role => role.name).join(', ') || 'none'}).`,
      );
    }

    const inviteResponse = await request.post('/api/users/invite', {
      headers: authHeaders(inviter.accessToken),
      data: {
        username: identity.username,
        email: identity.email,
        roleIds: [viewerRole.rowId],
      },
    });

    await expectOk(inviteResponse, 'invite the identity');

    const invitation = await waitForMessage({
      to: identity.email,
      subjectContains: 'Invitation',
    });

    // The invitation's generated password exists nowhere else, so the message is the source of it.
    return await completeSignIn(
      request,
      identity,
      temporaryPassword(invitation),
    );
  } finally {
    await request.dispose();
  }
}

/**
 * Provisions a site of its own with an administrator, by signing up and being approved.
 *
 * Intended for a journey that destroys the site: the sign-up creates the site, so the tenant and its only
 * administrator belong to the journey and no seeded site is touched.
 *
 * @param playwright The worker's request factory, for reaching the API directly.
 * @param label Names the sign-up, so a failure says which journey caused it.
 * @returns The provisioned administrator, signed in, and the site it administers.
 */
export async function provisionSiteAdministrator(
  playwright: E2eRequestFactory,
  label: string,
): Promise<ProvisionedTenant> {
  const request = await createE2eRequestContext(playwright);

  try {
    const identity = newIdentity(label);

    const sendResponse = await request.post('/api/auth/signup/send', {
      data: { username: identity.username, email: identity.email },
    });

    await expectOk(sendResponse, 'start the sign-up');

    const sendBody = (await sendResponse.json()) as SignUpSendResponse;

    if (!sendBody.referenceCode) {
      throw new Error(
        'Starting the sign-up returned no reference code, so the sign-up cannot be completed.',
      );
    }

    const signUpMessage = await waitForMessage({
      to: identity.email,
      subjectContains: 'Signup',
    });

    const password = temporaryPassword(signUpMessage);

    const completeResponse = await request.post('/api/auth/signup/complete', {
      data: {
        username: identity.username,
        referenceCode: sendBody.referenceCode,
        verificationCode: oneTimeCode(signUpMessage),
      },
    });

    await expectOk(completeResponse, 'complete the sign-up');

    await approve(request, identity.username);

    // The password is usable only once the sign-up is approved, so signing in here is what proves it arrived.
    return await completeSignIn(request, identity, password);
  } finally {
    await request.dispose();
  }
}

/**
 * Approves a pending sign-up as the platform administrator, which is the only caller allowed to decide one.
 *
 * @param request The request context to use.
 * @param username The identity awaiting approval.
 */
async function approve(
  request: APIRequestContext,
  username: string,
): Promise<void> {
  const approver = await signIn(
    request,
    adminCredentials.username,
    adminCredentials.password,
  );

  const pendingResponse = await request.get('/api/approvals/pending', {
    headers: authHeaders(approver.accessToken),
  });

  await expectOk(pendingResponse, 'list the pending approvals');

  const pending = (await pendingResponse.json()) as PendingApprovalResponse[];
  const approval = pending.find(item => item.username === username);

  if (!approval) {
    throw new Error(
      `'${username}' was not among the ${pending.length} pending approval(s), so it cannot be approved. The sign-up must have completed before it is approved.`,
    );
  }

  const updateResponse = await request.put(
    `/api/approvals/${approval.rowId}/status`,
    {
      headers: authHeaders(approver.accessToken),
      data: { etag: approval.etag, status: 'Approved' },
    },
  );

  await expectOk(updateResponse, 'approve the sign-up');
}

/**
 * Signs the provisioned identity in and reads the tenant the application gave it.
 *
 * The site name is read rather than derived from the username: the journey types it into a confirmation, so a
 * change to how a site is named must fail here rather than half-way through the journey.
 *
 * @param request The request context to use.
 * @param identity The identity to sign in.
 * @param password The password the application generated for it.
 * @returns The provisioned tenant, signed in.
 */
async function completeSignIn(
  request: APIRequestContext,
  identity: Identity,
  password: string,
): Promise<ProvisionedTenant> {
  const session = await signIn(request, identity.username, password);

  const meResponse = await request.get('/api/me', {
    headers: authHeaders(session.accessToken),
  });

  await expectOk(meResponse, 'read the provisioned identity');

  const currentUser = (await meResponse.json()) as CurrentUserResponse;

  return {
    username: identity.username,
    password,
    email: identity.email,
    displayName: currentUser.displayName,
    siteName: currentUser.site.name,
    accessToken: session.accessToken,
    storageState: session.storageState,
  };
}

/**
 * Signs in, returning the session a browser context can continue.
 *
 * @param request The request context to use. Its cookies become the session's storage state.
 * @param username The username to sign in with.
 * @param password The password to sign in with.
 * @returns The access token and the signed-in storage state.
 */
async function signIn(
  request: APIRequestContext,
  username: string,
  password: string,
): Promise<{ accessToken: string; storageState: StorageState }> {
  const response = await request.post('/api/auth/login', {
    data: { username, password },
  });

  await expectOk(response, `sign in as '${username}'`);

  const body = (await response.json()) as LoginResponse;

  if (!body.accessToken) {
    throw new Error(
      `Signing in as '${username}' returned no access token, so the session cannot be used.`,
    );
  }

  return {
    accessToken: body.accessToken,
    storageState: await request.storageState(),
  };
}

/**
 * Fails with the step that did not hold and what the application answered, so a provisioning failure is never
 * mistaken for a defect in the journey.
 *
 * @param response The response to check.
 * @param step The step being attempted, in the form the failure should read in.
 */
async function expectOk(response: APIResponse, step: string): Promise<void> {
  if (response.ok()) {
    return;
  }

  const body = await response.text();

  throw new Error(
    `Could not ${step}: ${response.status()} ${response.statusText()} ${body}`,
  );
}

/**
 * Builds a username and address no previous run has used, so the same journey can run again, and in several
 * browser projects at once, against one database.
 *
 * @param label Names the identity after the journey that owns it.
 * @returns The username and email to provision.
 */
function newIdentity(label: string): Identity {
  const suffix = randomUUID().replace(/-/g, '').slice(0, 12);
  const username = `e2e-${label}-${suffix}`;

  return { username, email: `${username}@example.com` };
}
