import type { APIRequestContext } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

// Covers the accounts CRUD flow:
// create -> edit -> delete end-to-end against the real stack, plus the
// API-failure path. Client-side form validation for the create form is covered
// separately by `feedback/formValidation.test.ts`.
//
// This is a fixture-managed, serial suite: every test cleans up the accounts it
// creates in afterEach via the API, so the suite is re-runnable without
// contamination (repeated runs must stay green).
//
// Desktop-only: the desktop flow uses the accounts table + row action menu; the
// mobile account card flows are covered by mobileCardGrids.test.ts.

const apiBaseUrl = 'http://127.0.0.1:5242';

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// A unique BSB + number pair so repeated runs never collide with
// the seed data or with accounts from an earlier run.
const makeUniqueAccount = () => ({
  bsb: '111-222',
  number: `E2E${Date.now()}`.slice(0, 20),
  description: `E2E Account ${Date.now()}`,
  balance: 1234.5,
  reserved: 100,
});

// RowIds created during this serial suite, cleaned up in afterEach.
const createdAccountRowIds: string[] = [];

// Deletes a test-created account through the API using the fixture's access
// token (no extra login). 404 is tolerated: the test may already have deleted
// it via the UI before cleanup runs.
async function deleteAccountViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/accounts/${rowId}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  expect([200, 204, 404]).toContain(response.status());
}

test.describe.serial('Accounts CRUD (fixture-managed)', () => {
  test.afterEach(async ({ playwright, accessToken }) => {
    // 60s per-action timeout mirrors the test timeout (see filters.test.ts).
    const request = await playwright.request.newContext({
      baseURL: apiBaseUrl,
      timeout: 60_000,
    });

    try {
      for (const rowId of createdAccountRowIds.splice(0)) {
        await deleteAccountViaApi(request, accessToken, rowId);
      }
    } finally {
      await request.dispose();
    }
  });

  test('creates, edits, and deletes an account end-to-end', async ({
    page,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Accounts table CRUD is desktop-only; mobile uses the card grid',
    );

    const account = makeUniqueAccount();

    // ---- Create ----
    const createResponsePromise = page.waitForResponse(
      response =>
        response.url().endsWith('/api/accounts') &&
        response.request().method() === 'POST' &&
        response.status() < 300,
    );

    await page.goto('/accounts');
    await page.getByRole('button', { name: 'Add a new account' }).click();
    await expect(page).toHaveURL(/\/accounts\/create$/);

    await page.getByLabel('BSB').fill(account.bsb);
    await page.getByLabel('Account Number').fill(account.number);
    // exact name avoids the accounts page 'Search accounts by description' input.
    await page
      .getByRole('textbox', { name: 'Description', exact: true })
      .fill(account.description);
    await page.getByLabel('Balance').fill(String(account.balance));
    await page.getByLabel('Reserved').fill(String(account.reserved));

    await page.getByRole('button', { name: 'Create' }).click();

    const createResponse = await createResponsePromise;
    expect(createResponse.ok()).toBeTruthy();

    const created = (await createResponse.json()) as {
      rowId: string;
      etag: number;
    };
    createdAccountRowIds.push(created.rowId);

    // Back on the list with the new account visible.
    await expect(page).toHaveURL(/\/accounts$/);
    await expect(
      page.getByText(account.description, { exact: true }),
    ).toBeVisible();

    // ---- Edit ----
    const row = page.locator('tr').filter({ hasText: account.description });
    await row.getByRole('button', { name: 'Open menu' }).click();
    await page.getByRole('menuitem', { name: 'Edit' }).click();
    await expect(page).toHaveURL(
      new RegExp(`/accounts/edit/${created.rowId}$`),
    );

    // Wait for the edit form to hydrate before editing. exact name avoids the
    // accounts page 'Search accounts by description' input.
    const descriptionInput = page.getByRole('textbox', {
      name: 'Description',
      exact: true,
    });
    await expect(descriptionInput).toBeEnabled();

    const editedDescription = `${account.description} edited`;
    await descriptionInput.fill(editedDescription);

    const editResponsePromise = page.waitForResponse(
      response =>
        response.url().includes(`/api/accounts/${created.rowId}`) &&
        response.request().method() === 'PUT' &&
        response.status() < 300,
    );

    await page.getByRole('button', { name: 'Save' }).click();
    await editResponsePromise;

    await expect(page).toHaveURL(/\/accounts$/);
    await expect(
      page.getByText(editedDescription, { exact: true }),
    ).toBeVisible();

    // ---- Delete ----
    const editedRow = page.locator('tr').filter({ hasText: editedDescription });
    await editedRow.getByRole('button', { name: 'Open menu' }).click();
    await page.getByRole('menuitem', { name: 'Delete' }).click();

    const deleteDialog = page.getByRole('alertdialog');
    await expect(deleteDialog).toBeVisible();
    await expect(
      deleteDialog.getByText('Delete Account', { exact: true }),
    ).toBeVisible();

    const deleteResponsePromise = page.waitForResponse(
      response =>
        response.url().includes(`/api/accounts/${created.rowId}`) &&
        response.request().method() === 'DELETE' &&
        response.status() < 300,
    );

    await deleteDialog.getByRole('button', { name: 'Delete' }).click();
    await deleteResponsePromise;

    await expect(
      page.getByText(editedDescription, { exact: true }),
    ).toBeHidden();
  });

  test('create API failure surfaces the ErrorSheet and stays on the create form', async ({
    page,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Accounts table CRUD is desktop-only; mobile uses the card grid',
    );

    await page.route('**/api/accounts', route => {
      if (route.request().method() === 'POST') {
        return route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ detail: 'E2E injected create failure' }),
        });
      }

      return route.continue();
    });

    await page.goto('/accounts');
    await page.getByRole('button', { name: 'Add a new account' }).click();
    await expect(page).toHaveURL(/\/accounts\/create$/);

    await page.getByLabel('BSB').fill('111-222');
    await page.getByLabel('Account Number').fill('98765432');
    // exact name avoids the accounts page 'Search accounts by description' input.
    await page
      .getByRole('textbox', { name: 'Description', exact: true })
      .fill('E2E failure account');

    await page.getByRole('button', { name: 'Create' }).click();

    // 5xx -> ServerError code -> the page-level ErrorSheet; the form stays open.
    await expect(page.getByText('Server Error', { exact: true })).toHaveCount(
      1,
    );
    await expect(
      page.getByText('E2E injected create failure', { exact: true }),
    ).toHaveCount(1);
    await expect(page).toHaveURL(/\/accounts\/create$/);
  });
});
