import type { APIRequestContext, Page } from '@playwright/test';
import { expect } from '@playwright/test';

/**
 * Shared E2E API helpers used by the fixture-managed suites.
 *
 * Auth always comes from the per-test `accessToken` fixture (the single login),
 * so helpers never perform an extra PBKDF2 login per operation.
 */
export const E2E_API_BASE_URL = 'http://127.0.0.1:5242';

/**
 * Structural type for the part of the Playwright worker fixture this helper
 * uses. (`Playwright` is not exported by name from '@playwright/test', so we
 * type only the `request` factory we call.)
 */
type E2eRequestFactory = {
  request: {
    newContext(options?: {
      baseURL?: string;
      timeout?: number;
    }): Promise<APIRequestContext>;
  };
};

/**
 * Creates an unauthenticated API request context with a 60s per-action timeout.
 *
 * The timeout mirrors the E2E test timeout: a cold first API call on the loaded
 * shared stack can exceed Playwright's 30s request default (see the rationale
 * previously documented in filters.test.ts / quickActions.test.ts).
 */
export async function createE2eRequestContext(
  playwright: E2eRequestFactory,
): Promise<APIRequestContext> {
  return playwright.request.newContext({
    baseURL: E2E_API_BASE_URL,
    timeout: 60_000,
  });
}

export const authHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

export type ExpenseViaApiPayload = {
  description: string;
  nextDue: string;
  amount: number;
};

/** Creates an expense and returns its rowId. */
export async function createExpenseViaApi(
  request: APIRequestContext,
  accessToken: string,
  accountRowId: string,
  payload: ExpenseViaApiPayload,
): Promise<{ rowId: string }> {
  const response = await request.post('/api/expenses', {
    headers: authHeaders(accessToken),
    data: {
      description: payload.description,
      nextDue: payload.nextDue,
      accrualStart: null,
      accrualPolicy: 'None',
      endDate: null,
      frequency: 'Months',
      frequencyCount: 1,
      amount: payload.amount,
      note: null,
      accountRowId,
    },
  });

  expect(response.ok()).toBeTruthy();

  return (await response.json()) as { rowId: string };
}

export type IncomeViaApiPayload = ExpenseViaApiPayload;

/** Creates an income and returns its rowId. */
export async function createIncomeViaApi(
  request: APIRequestContext,
  accessToken: string,
  accountRowId: string,
  payload: IncomeViaApiPayload,
): Promise<{ rowId: string }> {
  const response = await request.post('/api/incomes', {
    headers: authHeaders(accessToken),
    data: {
      description: payload.description,
      nextDue: payload.nextDue,
      endDate: null,
      frequency: 'Months',
      frequencyCount: 1,
      amount: payload.amount,
      note: null,
      accountRowId,
    },
  });

  expect(response.ok()).toBeTruthy();

  return (await response.json()) as { rowId: string };
}

/**
 * Deletes a test-created expense. 404 is tolerated: the test may already have
 * deleted the row via the UI before cleanup runs.
 */
export async function deleteExpenseViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/expenses/${rowId}`, {
    headers: authHeaders(accessToken),
  });

  expect([200, 204, 404]).toContain(response.status());
}

/**
 * Deletes a test-created income. 404 is tolerated (see deleteExpenseViaApi).
 */
export async function deleteIncomeViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/incomes/${rowId}`, {
    headers: authHeaders(accessToken),
  });

  expect([200, 204, 404]).toContain(response.status());
}

/**
 * Creates an account and returns its rowId.
 *
 * @param description unique per suite/test so the created row can be asserted
 *   in the UI (and so cleanup never touches another suite's rows). Account
 *   descriptions are unique per site, so suites sharing a site must pass
 *   distinct values.
 */
export async function createAccountViaApi(
  request: APIRequestContext,
  accessToken: string,
  description = 'E2E Quick Actions Account',
): Promise<{ rowId: string }> {
  const response = await request.post('/api/accounts', {
    headers: authHeaders(accessToken),
    data: {
      description,
      balance: 0,
      reserved: 0,
    },
  });

  expect(response.ok()).toBeTruthy();

  return (await response.json()) as { rowId: string };
}

/**
 * Deletes a test-created account. 404 is tolerated (see deleteExpenseViaApi).
 */
export async function deleteAccountViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/accounts/${rowId}`, {
    headers: authHeaders(accessToken),
  });

  expect([200, 204, 404]).toContain(response.status());
}

/**
 * A request the page made that the API answered with a failure.
 */
export type ApiFailure = {
  method: string;
  url: string;
  status: number;
};

/**
 * Records every failed `/api/` response the page receives, so a test can assert the journey it drove made no
 * failing call.
 *
 * A test that leaves a request failing is green for the wrong reason: assertions on the figures it cares about
 * still pass while an unrelated request 4xxs, and Playwright fails neither on a failed response nor on the
 * console error that follows. Call this before the navigation, exclude the failures the test deliberately
 * provokes, and assert the result is empty once the assertions are done.
 *
 * @param page The page to watch
 * @param expected Identifies failures the test provokes on purpose; those are not recorded
 * @returns The failures recorded so far, filled as the page makes its requests
 */
export function collectApiFailures(
  page: Page,
  expected: (failure: ApiFailure) => boolean = () => false,
): ApiFailure[] {
  const failures: ApiFailure[] = [];

  page.on('response', response => {
    if (!response.url().includes('/api/')) {
      return;
    }

    const status = response.status();

    if (status < 400) {
      return;
    }

    const failure: ApiFailure = {
      method: response.request().method(),
      url: response.url(),
      status,
    };

    if (!expected(failure)) {
      failures.push(failure);
    }
  });

  return failures;
}
