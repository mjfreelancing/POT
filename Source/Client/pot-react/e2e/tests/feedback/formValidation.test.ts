import { expect, test } from '../../fixtures/auth';

// Covers form validation: the account create form (zod + react-hook-form)
// surfaces field errors on submit with blank/invalid values. Validation is
// client-side, so no mutation is fired (parallel-safe).
// Runs on all projects.

test.describe('account create form validation', () => {
  test('shows field errors when submitting an empty form', async ({ page }) => {
    // Validation is client-side and needs no accounts data, so navigate straight
    // to the create route. Going via /accounts makes the test depend on the list
    // toolbar mounting, which is load-sensitive under the shared-stack matrix.
    await page.goto('/accounts/create');
    await expect(page.getByRole('button', { name: 'Create' })).toBeVisible();

    await page.getByRole('button', { name: 'Create' }).click();

    // Description min(1) failure.
    await expect(
      page.getByText('Description is required', { exact: true }),
    ).toBeVisible();
  });
});
