import { expect, test } from '../../fixtures/auth';
import { authHeaders, createE2eRequestContext } from '../../helpers/api';

// Covers the Site Details section of the POT Settings sheet, the only client caller of
// PUT /api/sites/{id}. The route updates the caller's current site, so this journey must keep
// working for the signed-in site administrator.
//
// Mutates the shared site's description and restores it in `finally` via the API, so a retry or a
// failure after the save has committed still leaves the site as the run found it. The site name is
// deliberately left alone: it is the sheet's more load-sensitive field and other suites read it.
//
// Desktop-only: mobile opens the same sheet through its own layout, covered by the mobile
// sheets/dialogs suite.

const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

type MeInfo = {
  site: {
    rowId: string;
    name: string;
    description: string | null;
    etag: number;
  };
};

test('updates the site description and restores it', async ({
  page,
  accessToken,
  playwright,
}, testInfo) => {
  test.skip(isMobileProject(testInfo), 'Desktop settings sheet');

  const request = await createE2eRequestContext(playwright);

  // Hoisted so the restore below can use them, and so a failure before the save never triggers a
  // restore that would overwrite the site with a value nothing read.
  let originalDescription = '';
  let savedDescription = false;

  try {
    const me = await getMeInfo(request, accessToken);
    originalDescription = me.site.description ?? '';
    const updatedDescription = `E2E site description ${Date.now()}`;

    await page.goto('/dashboard');

    // Open the user menu, then the POT Settings sheet.
    await page.getByRole('button', { name: 'e2e_admin' }).click();
    await page.getByRole('menuitem', { name: 'Settings' }).click();
    await expect(page.getByText('POT Settings', { exact: true })).toBeVisible();

    await page.getByRole('button', { name: /Site Details/ }).click();

    // Scope to the section's own form: other sheet sections also render a "Description" field.
    const siteForm = page.locator('form').filter({
      has: page.getByRole('button', {
        name: 'Update Site Details',
        exact: true,
      }),
    });

    const descriptionInput = siteForm.getByLabel('Description', {
      exact: true,
    });
    await expect(descriptionInput).toHaveValue(originalDescription);

    await descriptionInput.fill(updatedDescription);

    // Register the response wait BEFORE the click, then assert the contract before the UI.
    const saveResponse = page.waitForResponse(
      response =>
        response.url().includes(`/api/sites/${me.site.rowId}`) &&
        response.request().method() === 'PUT',
    );

    await page
      .getByRole('button', { name: 'Update Site Details', exact: true })
      .click();

    const saved = await saveResponse;
    expect(saved.ok()).toBeTruthy();
    savedDescription = true;
    await saved.finished();

    await expect(
      page.getByText('Site Details Updated', { exact: true }),
    ).toBeVisible({ timeout: 30000 });

    // Confirm the server holds the new value (the UI toast alone does not prove persistence,
    // because the site row feeds the next read).
    const afterSave = await getMeInfo(request, accessToken);
    expect(afterSave.site.description).toBe(updatedDescription);
  } finally {
    // Restore only if the save committed, using the row's CURRENT etag: the UI save bumped it, so
    // the value read before the save would conflict. Runs even when an assertion above failed.
    try {
      if (savedDescription) {
        const current = await getMeInfo(request, accessToken);
        const restoreResponse = await request.put(
          `/api/sites/${current.site.rowId}`,
          {
            headers: authHeaders(accessToken),
            data: {
              etag: current.site.etag,
              name: current.site.name,
              description: originalDescription,
            },
          },
        );
        expect(restoreResponse.ok()).toBeTruthy();
      }
    } finally {
      await request.dispose();
    }
  }
});

async function getMeInfo(
  request: import('@playwright/test').APIRequestContext,
  accessToken: string,
): Promise<MeInfo> {
  const response = await request.get('/api/me', {
    headers: authHeaders(accessToken),
  });
  expect(response.ok()).toBeTruthy();

  return (await response.json()) as MeInfo;
}
