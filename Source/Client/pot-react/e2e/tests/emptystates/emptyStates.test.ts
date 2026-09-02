import { expect, test } from '../../fixtures/auth';

// Covers the EmptyStateMessage contract
// (src/components/feedback/message/EmptyStateMessage.tsx) on the
// "filtered-out" (no matches) branch — the only branch reachable in the shared
// seeded E2E DB. The seed always has accounts/expenses/incomes, so the true
// "no data at all" states ("No accounts yet", "No expenses yet", "No incomes
// yet", "Create an account to get started") are NOT E2E-reachable without a
// fresh empty DB — documented rather than tested.
//
// The expense "No matching expenses" empty state is already covered by
// filters.test.ts. This file adds the accounts + incomes no-matches states,
// including their action buttons ("Clear search" / "Clear filters"), which
// restore the list.
//
// Parallel-safe: read-only (search + clear; no mutations).

const noMatchStamp = () => `e2e_no_match_${Date.now()}`;

test('accounts: no matching accounts empty state with a Clear search action', async ({
  page,
}) => {
  await page.goto('/accounts');
  const search = page.getByRole('textbox', {
    name: 'Search accounts by description',
  });
  await expect(search).toBeVisible();

  await search.fill(noMatchStamp());

  const emptyTitle = page.getByText('No matching accounts', { exact: true });
  await expect(emptyTitle).toBeVisible();
  await expect(
    page.getByText('Try adjusting your search to find the account you need.', {
      exact: true,
    }),
  ).toBeVisible();

  // The action button clears the search and restores the list.
  await page.getByRole('button', { name: 'Clear search', exact: true }).click();
  await expect(emptyTitle).toHaveCount(0);
  await expect(search).toHaveValue('');
  await expect(page.getByText(/Showing \d+ of \d+ items/)).toBeVisible();
});

test('incomes: no matching incomes empty state with a Clear filters action', async ({
  page,
}) => {
  await page.goto('/incomes');
  const search = page.getByRole('textbox', {
    name: 'Search incomes by description',
  });
  await expect(search).toBeVisible();

  await search.fill(noMatchStamp());

  const emptyTitle = page.getByText('No matching incomes', { exact: true });
  await expect(emptyTitle).toBeVisible();
  await expect(
    page.getByText('Try adjusting your search to find matching incomes.', {
      exact: true,
    }),
  ).toBeVisible();

  await page
    .getByRole('button', { name: 'Clear filters', exact: true })
    .click();
  await expect(emptyTitle).toHaveCount(0);
  await expect(search).toHaveValue('');
  await expect(page.getByText(/Showing \d+ of \d+ items/)).toBeVisible();
});
