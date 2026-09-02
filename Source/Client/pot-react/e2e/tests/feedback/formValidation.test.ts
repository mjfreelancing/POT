import { expect, test } from '../../fixtures/auth';

// Covers form validation: the account create form (zod + react-hook-form)
// surfaces field errors on submit with blank/invalid values. Validation is
// client-side, so no mutation is fired (parallel-safe).
// Runs on all projects.

test.describe('account create form validation', () => {
  test('shows field errors when submitting an empty form', async ({ page }) => {
    await page.goto('/accounts');
    await page.getByRole('button', { name: 'Add a new account' }).click();
    await expect(page).toHaveURL(/\/accounts\/create$/);

    await page.getByRole('button', { name: 'Create' }).click();

    // BSB regex failure.
    await expect(
      page.getByText('BSB must be in the format XXX-XXX', { exact: true }),
    ).toBeVisible();

    // Account Number and Description min(1) failures.
    await expect(
      page
        .getByText('String must contain at least 1 character(s)', {
          exact: true,
        })
        .first(),
    ).toBeVisible();
  });

  test('shows a BSB format error for an invalid BSB', async ({ page }) => {
    await page.goto('/accounts');
    await page.getByRole('button', { name: 'Add a new account' }).click();
    await expect(page).toHaveURL(/\/accounts\/create$/);

    await page.getByLabel('BSB').fill('123');
    await page.getByRole('button', { name: 'Create' }).click();

    await expect(
      page.getByText('BSB must be in the format XXX-XXX', { exact: true }),
    ).toBeVisible();
  });
});
