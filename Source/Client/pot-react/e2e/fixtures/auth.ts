import { test as baseTest, expect } from '@playwright/test';
import { existsSync, mkdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const currentFilePath = fileURLToPath(import.meta.url);
const currentDirectory = dirname(currentFilePath);

// Resolved path to the e2e/.auth directory, one level up from e2e/fixtures.
const authDirectory = resolve(currentDirectory, '../.auth');

// API base URL for the E2E test server — must match playwright.config.ts webServer port.
const apiBaseUrl = 'http://127.0.0.1:5242';

// Canonical admin credentials matching baseline.sql seed data.
const adminCredentials = {
  username: 'e2e_admin',
  password: 'E2E_admin-password',
};

type WorkerFixtures = {
  workerStorageState: string;
};

/**
 * Extended test fixture that provides worker-scoped authentication.
 *
 * Each Playwright worker logs in once via the API and saves the resulting
 * HTTP-only refresh token cookie to e2e/.auth/{parallelIndex}.json.
 * The saved state is reused for every test in that worker without re-logging in.
 *
 * The storageState override wires the saved file into each test's browser
 * context automatically, so tests start with the refresh token cookie in place.
 * The app then calls /api/auth/refresh on first use to obtain an access token.
 *
 * Auth state files are cleared by playwright.globalSetup.ts at the start of
 * each run to prevent stale sessions from prior runs (see D4 in PRD 012).
 */
export const test = baseTest.extend<{}, WorkerFixtures>({
  storageState: [
    async ({ workerStorageState }, use) => {
      await use(workerStorageState);
    },
    { scope: 'test' },
  ],

  workerStorageState: [
    async ({ playwright }, use, workerInfo) => {
      const authFile = resolve(
        authDirectory,
        `${workerInfo.parallelIndex}.json`,
      );

      if (!existsSync(authFile)) {
        console.log(
          `[auth] Creating new auth state for worker ${workerInfo.parallelIndex}`,
        );

        const requestContext = await playwright.request.newContext({
          baseURL: apiBaseUrl,
        });

        const response = await requestContext.post('/api/auth/login', {
          data: adminCredentials,
        });

        if (!response.ok()) {
          const body = await response.text();
          await requestContext.dispose();
          throw new Error(
            `Auth fixture login failed for worker ${workerInfo.parallelIndex}: ${response.status()} ${body}`,
          );
        }

        mkdirSync(authDirectory, { recursive: true });
        await requestContext.storageState({ path: authFile });
        await requestContext.dispose();
      } else {
        console.log(
          `[auth] Reusing existing auth state for worker ${workerInfo.parallelIndex}`,
        );
      }

      await use(authFile);
    },
    { scope: 'worker' },
  ],
});

export { expect };
