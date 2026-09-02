import { expect, test } from '../../fixtures/auth';

// Covers the top-level ErrorBoundary (src/App.tsx) catches render errors.
// A lazy page chunk that fails to load makes React.lazy throw during render,
// which the boundary converts to the "Something went wrong !" fallback and
// records as an app-level ErrorSheet ("Application Error") that can be dismissed.
// Runs on all projects (the boundary is app-wide).

test('lazy page chunk failure surfaces the ErrorBoundary fallback', async ({
  page,
}) => {
  // Block the AccountsPage module so its dynamic import rejects.
  await page.route('**/src/features/accounts/AccountsPage.tsx*', route =>
    route.abort(),
  );

  await page.goto('/accounts');

  // The boundary fallback (role="alert") with the fixed message.
  const fallbackAlert = page.getByRole('alert');
  await expect(fallbackAlert).toBeVisible();
  await expect(
    fallbackAlert.getByText('Something went wrong !', { exact: true }),
  ).toBeVisible();

  // handleError() also records an app-level ErrorSheet that can be dismissed.
  await expect(
    page.getByText('Application Error', { exact: true }),
  ).toBeVisible();

  const dismissButton = page.getByRole('button', { name: 'Dismiss' });
  await expect(dismissButton).toBeVisible();

  await dismissButton.click();

  await expect(
    page.getByText('Application Error', { exact: true }),
  ).toBeHidden();
});
