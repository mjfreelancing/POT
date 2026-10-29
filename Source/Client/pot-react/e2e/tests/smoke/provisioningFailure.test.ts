import type { APIRequestContext, APIResponse } from '@playwright/test';
import { expect, test } from '@playwright/test';

import {
  ProvisioningFailure,
  provisionInvitedViewer,
} from '../../helpers/provisioning';

// Proves a failure caused by provisioning says so, so it is not read as a product defect.
//
// The request context is a stub that refuses everything, so the failure happens before any real call: what is
// asserted is the shape of the failure a journey reports, not the provisioning flow itself, which the destructive
// suites exercise against a real stack.

function refusingResponse(): APIResponse {
  return {
    ok: () => false,
    status: () => 403,
    statusText: () => 'Forbidden',
    text: async () => 'The caller is not permitted to do that.',
    json: async () => ({}),
  } as unknown as APIResponse;
}

function refusingRequestContext(): APIRequestContext {
  return {
    get: async () => refusingResponse(),
    post: async () => refusingResponse(),
    put: async () => refusingResponse(),
    storageState: async () => ({ cookies: [], origins: [] }),
    dispose: async () => undefined,
  } as unknown as APIRequestContext;
}

const refusingApi = {
  request: {
    newContext: async () => refusingRequestContext(),
  },
};

test('a provisioning failure says it is the harness rather than the product', async () => {
  let failure: unknown;

  try {
    await provisionInvitedViewer(refusingApi, 'diagnostics');
  } catch (error) {
    failure = error;
  }

  if (!(failure instanceof ProvisioningFailure)) {
    throw new Error(
      `Expected a ProvisioningFailure, but provisioning ${
        failure === undefined
          ? 'did not fail'
          : `failed with: ${String(failure)}`
      }`,
    );
  }

  // The first line names the cause, and the step that failed follows it.
  expect(failure.message).toContain('harness failure and not a product defect');
  expect(failure.message).toContain("Provisioning failed for 'diagnostics'");
  expect(failure.message).toContain('sign in as');
  expect(failure.message).toContain('403');
});
