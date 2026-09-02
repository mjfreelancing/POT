import { expect, test, viewerTest } from '../../fixtures/auth';

test.describe('permission boundary (admin vs viewer)', () => {
  test('admin sees create affordances enabled', async ({ page }) => {
    await page.goto('/accounts');
    await expect(
      page.getByRole('button', { name: 'Add a new account' }),
    ).toBeEnabled();

    await page.goto('/expenses');
    await expect(
      page.getByRole('button', { name: 'Add a new expense' }).first(),
    ).toBeEnabled();

    await page.goto('/incomes');
    await expect(
      page.getByRole('button', { name: 'Add a new income' }).first(),
    ).toBeEnabled();
  });

  viewerTest('viewer sees create affordances disabled', async ({ page }) => {
    await page.goto('/accounts');
    await expect(
      page.getByRole('button', { name: 'Add a new account' }),
    ).toBeDisabled();

    await page.goto('/expenses');
    await expect(
      page.getByRole('button', { name: 'Add a new expense' }).first(),
    ).toBeDisabled();

    await page.goto('/incomes');
    await expect(
      page.getByRole('button', { name: 'Add a new income' }).first(),
    ).toBeDisabled();
  });
});
