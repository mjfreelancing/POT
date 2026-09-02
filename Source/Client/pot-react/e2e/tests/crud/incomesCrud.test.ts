import type { APIRequestContext } from '@playwright/test';
import { expect, test } from '../../fixtures/auth';

// Covers the incomes CRUD flow:
// create -> edit -> delete end-to-end against the real stack.
//
// This is a fixture-managed, serial suite: the test cleans up the income it
// creates in afterEach via the API, so the suite is re-runnable without
// contamination.
//
// Desktop-only: the desktop flow uses the incomes table + row action menu; the
// mobile income card flows are covered by mobileCardGrids.test.ts.

const apiBaseUrl = 'http://127.0.0.1:5242';

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

// Unique description so repeated runs never collide with seed data
// or incomes from an earlier run.
const makeUniqueIncome = () => ({
  description: `E2E Income ${Date.now()}`,
  amount: 250,
});

// RowId created during this serial suite, cleaned up in afterEach.
const createdIncomeRowIds: string[] = [];

// Deletes a test-created income through the API using the fixture's access
// token (no extra login). 404 is tolerated: the test may already have deleted
// it via the UI before cleanup runs.
async function deleteIncomeViaApi(
  request: APIRequestContext,
  accessToken: string,
  rowId: string,
): Promise<void> {
  const response = await request.delete(`/api/incomes/${rowId}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  expect([200, 204, 404]).toContain(response.status());
}

test.describe.serial('Incomes CRUD (fixture-managed)', () => {
  test.afterEach(async ({ playwright, accessToken }) => {
    // 60s per-action timeout mirrors the test timeout (see filters.test.ts).
    const request = await playwright.request.newContext({
      baseURL: apiBaseUrl,
      timeout: 60_000,
    });

    try {
      for (const rowId of createdIncomeRowIds.splice(0)) {
        await deleteIncomeViaApi(request, accessToken, rowId);
      }
    } finally {
      await request.dispose();
    }
  });

  test('creates, edits, and deletes an income end-to-end', async ({
    page,
  }, testInfo) => {
    test.skip(
      isMobileProject(testInfo),
      'Incomes table CRUD is desktop-only; mobile uses the card grid',
    );

    const income = makeUniqueIncome();

    // ---- Create ----
    const createResponsePromise = page.waitForResponse(
      response =>
        response.url().endsWith('/api/incomes') &&
        response.request().method() === 'POST' &&
        response.status() < 300,
    );

    await page.goto('/incomes');
    await page.getByRole('button', { name: 'Add a new income' }).click();
    await expect(page).toHaveURL(/\/incomes\/create$/);

    // Description: exact name avoids the incomes page 'Search incomes by
    // description' input.
    await page
      .getByRole('textbox', { name: 'Description', exact: true })
      .fill(income.description);
    await page.getByLabel('Amount').fill(String(income.amount));

    // Associated-account selection: open the Select and pick the first account.
    await page.getByLabel('Associated Account').click();
    await page.getByRole('option').first().click();

    await page.getByRole('button', { name: 'Create' }).click();

    const createResponse = await createResponsePromise;
    expect(createResponse.ok()).toBeTruthy();

    const created = (await createResponse.json()) as {
      rowId: string;
      etag: number;
    };
    createdIncomeRowIds.push(created.rowId);

    // Back on the list with the new income visible.
    await expect(page).toHaveURL(/\/incomes$/);
    await expect(
      page.getByText(income.description, { exact: true }),
    ).toBeVisible();

    // ---- Edit ----
    const row = page.locator('tr').filter({ hasText: income.description });
    await row.getByRole('button', { name: 'Open menu' }).click();
    await page.getByRole('menuitem', { name: 'Edit' }).click();
    await expect(page).toHaveURL(new RegExp(`/incomes/edit/${created.rowId}$`));

    // Wait for the edit form to hydrate before editing.
    const descriptionInput = page.getByRole('textbox', {
      name: 'Description',
      exact: true,
    });
    await expect(descriptionInput).toBeEnabled();

    const editedDescription = `${income.description} edited`;
    await descriptionInput.fill(editedDescription);

    const editResponsePromise = page.waitForResponse(
      response =>
        response.url().includes(`/api/incomes/${created.rowId}`) &&
        response.request().method() === 'PUT' &&
        response.status() < 300,
    );

    await page.getByRole('button', { name: 'Save' }).click();
    await editResponsePromise;

    await expect(page).toHaveURL(/\/incomes$/);
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
      deleteDialog.getByText('Delete Income', { exact: true }),
    ).toBeVisible();

    const deleteResponsePromise = page.waitForResponse(
      response =>
        response.url().includes(`/api/incomes/${created.rowId}`) &&
        response.request().method() === 'DELETE' &&
        response.status() < 300,
    );

    await deleteDialog.getByRole('button', { name: 'Delete' }).click();
    await deleteResponsePromise;

    await expect(
      page.getByText(editedDescription, { exact: true }),
    ).toBeHidden();
  });
});
