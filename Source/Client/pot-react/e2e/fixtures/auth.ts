import { test as baseTest, expect, type StorageState } from '@playwright/test';

import {
  adminCredentials,
  pwChangeCredentials,
  viewerCredentials,
} from '../helpers/secrets';

// API base URL for the E2E test server — must match playwright.config.ts webServer port.
const apiBaseUrl = 'http://127.0.0.1:5242';

type E2eCredentials = {
  username: string;
  password: string;
};

type LoginResult = {
  storageState: StorageState;
  accessToken: string;
};

/**
 * Builds a Playwright `test` extended with a per-test authenticated storage
 * state for the given canonical credentials.
 *
 * A fresh login per test is REQUIRED because the server rotates the refresh
 * token on every `/api/auth/refresh` call. A storage state's refresh cookie is
 * therefore single-use: once a test's page load refreshes the token, that cookie
 * is invalidated server-side, and any later test reusing the same state gets a
 * 401 and is redirected to the login page.
 *
 * (This replaces the earlier worker-scoped shared-file approach, which failed
 * intermittently when a worker was reused across multiple auth-using tests —
 * see the "D3 within-run stale session" finding.)
 *
 * The single per-test login is also exposed as an `accessToken` fixture (the
 * 15-minute JWT from the login response), so fixture-managed suites can reuse
 * that token for API setup/cleanup instead of performing an extra PBKDF2 login
 * per operation. This cuts the number of expensive password verifies per test
 * from ~3 to 1, which is a meaningful load reduction on the shared local stack.
 */
function createAuthenticatedTest(credentials: E2eCredentials) {
  return baseTest.extend<{ loginResult: LoginResult; accessToken: string }>({
    loginResult: [
      async ({ playwright }, use) => {
        console.log(
          `[auth] Logging in as ${credentials.username} for test (fresh session)...`,
        );

        // 60s per-action timeout mirrors the test timeout: on a loaded shared
        // stack the first (cold) PBKDF2 login can exceed Playwright's 30s
        // request default and fail the fixture before the test body runs.
        const requestContext = await playwright.request.newContext({
          baseURL: apiBaseUrl,
          timeout: 60_000,
        });

        // Tolerate ONE slow-but-healthy cold login (the API may still be
        // finishing JIT/EF warm-up). This is setup resilience, not a test retry.
        let response = await requestContext.post('/api/auth/login', {
          data: credentials,
        });

        if (!response.ok()) {
          console.warn(
            `[auth] login attempt 1 failed (${response.status()}); retrying once`,
          );
          await new Promise(resolve => setTimeout(resolve, 1000));
          response = await requestContext.post('/api/auth/login', {
            data: credentials,
          });
        }

        if (!response.ok()) {
          const body = await response.text();
          await requestContext.dispose();
          throw new Error(
            `Auth fixture login failed (${credentials.username}): ${response.status()} ${body}`,
          );
        }

        const body = (await response.json()) as { accessToken: string };
        const state = await requestContext.storageState();
        await requestContext.dispose();

        await use({ storageState: state, accessToken: body.accessToken });
      },
      { scope: 'test' },
    ],
    storageState: [
      async ({ loginResult }, use) => {
        await use(loginResult.storageState);
      },
      { scope: 'test' },
    ],
    accessToken: [
      async ({ loginResult }, use) => {
        await use(loginResult.accessToken);
      },
      { scope: 'test' },
    ],
  });
}

// Admin test: authenticated as e2e_admin (full access).
export const test = createAuthenticatedTest(adminCredentials);

// Viewer test: authenticated as e2e_viewer (read-only access).
export const viewerTest = createAuthenticatedTest(viewerCredentials);

// Password-change test: authenticated as e2e_pwchange, a canonical seed user
// (Viewer role) with its OWN unique password E2E_pwchange-password (see
// baseline.sql + secrets.ts). It exists so the user-settings password-change
// tests can rotate the password (bumping TokenVersion + revoking ITS sessions)
// without invalidating the shared admin session every other suite depends on.
export const pwChangeTest = createAuthenticatedTest(pwChangeCredentials);

export { expect };
