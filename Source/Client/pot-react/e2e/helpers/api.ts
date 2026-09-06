import type { APIRequestContext, Playwright } from '@playwright/test';
import { expect } from '@playwright/test';

/**
 * Shared E2E API helpers used by the fixture-managed suites.
 *
 * Auth always comes from the per-test `accessToken` fixture (the single login),
 * so helpers never perform an extra PBKDF2 login per operation.
 */
export const E2E_API_BASE_URL = 'http://127.0.0.1:5242';

/**
 * Creates an unauthenticated API request context with a 60s per-action timeout.
 *
 * The timeout mirrors the E2E test timeout: a cold first API call on the loaded
 * shared stack can exceed Playwright's 30s request default (see the rationale
 * previously documented in filters.test.ts / quickActions.test.ts).
 */
export async function createE2eRequestContext(
  playwright: Playwright,
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

export type AccrualPolicy = 'Automatic' | 'None';

/**
 * Creates an expense and returns its rowId.
 *
 * @param accrualPolicy default 'None' (matches bulk/filters); pass 'Automatic'
 *   when the test drives accrual (quick actions).
 */
export async function createExpenseViaApi(
  request: APIRequestContext,
  accessToken: string,
  accountRowId: string,
  payload: ExpenseViaApiPayload,
  accrualPolicy: AccrualPolicy = 'None',
): Promise<{ rowId: string }> {
  const response = await request.post('/api/expenses', {
    headers: authHeaders(accessToken),
    data: {
      description: payload.description,
      nextDue: payload.nextDue,
      accrualStart: null,
      accrualPolicy,
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
