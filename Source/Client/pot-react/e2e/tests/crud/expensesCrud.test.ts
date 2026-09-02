import type { APIRequestContext } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

// Covers the expenses CRUD flow:
// create -> edit -> delete end-to-end against the real stack, including the
// associated-account selector and a light date-picker exercise (the full
// EnrichedCalendar behavior is covered by picker/enrichedCalendar.test.ts),
// plus client-side validation.
//
// This is a fixture-managed, serial suite: every test cleans up the expenses it
// creates in afterEach via the API, so the suite is re-runnable without
// contamination.
//
// Desktop-only: the desktop flow uses the expenses table + row action menu; the
// mobile expense card flows are covered by mobileCardGrids.test.ts.

const apiBaseUrl = 'http://127.0.0.1:5242';

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// Unique description so repeated runs never collide with seed data
// or accounts from an earlier run.
const makeUniqueExpense = () => ({
  description: `E2E Expense ${Date.now()}`,
  amount: 120,
});

// RowIds created during this serial suite, cleaned up in afterEach.
const createdExpenseRowIds: string[] = [];

// Deletes a test-created expense through the API using the fixture's access
// token (no extra login). 404 is tolerated: the test may already have deleted
// it via the UI before cleanup runs.
async function deleteExpenseViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/expenses/${rowId}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  expect([200, 204, 404]).toContain(response.status());
}

test.describe.serial('Expenses CRUD (fixture-managed)', () => {
  test.afterEach(async ({ playwright, accessToken }) => {
    // 60s per-action timeout mirrors the test timeout (see filters.test.ts).
    const request = await playwright.request.newContext({
      baseURL: apiBaseUrl,
      timeout: 60_000,
    });

    try {
      for (const rowId of createdExpenseRowIds.splice(0)) {
        await deleteExpenseViaApi(request, accessToken, rowId);
      }
    } finally {
      await request.dispose();
    }
  });

  test('creates, edits, and deletes an expense end-to-end', async ({
    page,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Expenses table CRUD is desktop-only; mobile uses the card grid',
    );

    const expense = makeUniqueExpense();

    // ---- Create ----
    const createResponsePromise = page.waitForResponse(
      response =>
        response.url().endsWith('/api/expenses') &&
        response.request().method() === 'POST' &&
        response.status() < 300,
    );

    await page.goto('/expenses');
    await page.getByRole('button', { name: 'Add a new expense' }).click();
    await expect(page).toHaveURL(/\/expenses\/create$/);

    // Description: exact name avoids the expenses page 'Search expenses by
    // description' input.
    await page
      .getByRole('textbox', { name: 'Description', exact: true })
      .fill(expense.description);
    await page.getByLabel('Amount').fill(String(expense.amount));

    // Associated-account selection: open the Select and pick the first account.
    await page.getByLabel('Associated Account').click();
    await page.getByRole('option').first().click();

    // Light date-picker exercise: open Next Due and accept the selected date.
    await page.getByLabel('Next Due').click();
    await page.getByRole('button', { name: 'Accept' }).click();

    await page.getByRole('button', { name: 'Create' }).click();

    const createResponse = await createResponsePromise;
    expect(createResponse.ok()).toBeTruthy();

    const created = (await createResponse.json()) as {
      rowId: string;
      etag: number;
    };
    createdExpenseRowIds.push(created.rowId);

    // Back on the list with the new expense visible.
    await expect(page).toHaveURL(/\/expenses$/);
    await expect(
      page.getByText(expense.description, { exact: true }),
    ).toBeVisible();

    // ---- Edit ----
    const row = page.locator('tr').filter({ hasText: expense.description });
    await row.getByRole('button', { name: 'Open menu' }).click();
    await page.getByRole('menuitem', { name: 'Edit' }).click();
    await expect(page).toHaveURL(
      new RegExp(`/expenses/edit/${created.rowId}$`),
    );

    // Wait for the edit form to hydrate before editing.
    const descriptionInput = page.getByRole('textbox', {
      name: 'Description',
      exact: true,
    });
    await expect(descriptionInput).toBeEnabled();

    const editedDescription = `${expense.description} edited`;
    await descriptionInput.fill(editedDescription);

    const editResponsePromise = page.waitForResponse(
      response =>
        response.url().includes(`/api/expenses/${created.rowId}`) &&
        response.request().method() === 'PUT' &&
        response.status() < 300,
    );

    await page.getByRole('button', { name: 'Save' }).click();
    await editResponsePromise;

    await expect(page).toHaveURL(/\/expenses$/);
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
      deleteDialog.getByText('Delete Expense', { exact: true }),
    ).toBeVisible();

    const deleteResponsePromise = page.waitForResponse(
      response =>
        response.url().includes(`/api/expenses/${created.rowId}`) &&
        response.request().method() === 'DELETE' &&
        response.status() < 300,
    );

    await deleteDialog.getByRole('button', { name: 'Delete' }).click();
    await deleteResponsePromise;

    await expect(
      page.getByText(editedDescription, { exact: true }),
    ).toBeHidden();
  });

  test('blank create form surfaces required-field validation', async ({
    page,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Expenses table CRUD is desktop-only; mobile uses the card grid',
    );

    await page.goto('/expenses');
    await page.getByRole('button', { name: 'Add a new expense' }).click();
    await expect(page).toHaveURL(/\/expenses\/create$/);

    await page.getByRole('button', { name: 'Create' }).click();

    await expect(
      page.getByText('A description is required', { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText('An account is required', { exact: true }),
    ).toBeVisible();

    // Still on the create form — nothing was submitted.
    await expect(page).toHaveURL(/\/expenses\/create$/);
  });
});
