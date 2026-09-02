import { expect, test } from '../../fixtures/auth';

// The projection chart's legend row is only visible on desktop; on mobile the
// filters (including the legend) are collapsed behind a "Show filters" button
// (covered by mobileCardGrids.test.ts).
const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const escapeRegExp = (value: string) =>
  value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

test('projections renders the chart and account legend from the API', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'Projection chart legend is desktop-only; mobile projection filters are covered by mobileCardGrids.test.ts',
  );
  const projectionsResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/projections') &&
      response.request().method() === 'GET',
  );

  await page.goto('/projections');

  const projectionsResponse = await projectionsResponsePromise;
  expect(projectionsResponse.ok()).toBeTruthy();

  const projection = (await projectionsResponse.json()) as {
    accounts: { rowId: string; description: string; dates: unknown[] }[];
    global: unknown[];
  };

  expect(projection.accounts.length).toBeGreaterThan(0);

  // Page header and default metric chart title (Account Balances).
  await expect(
    page.getByRole('heading', { name: 'Projections', exact: true }),
  ).toBeVisible();
  await expect(
    page.getByText('Account Balances', { exact: true }).first(),
  ).toBeVisible();

  // The legend toggle for the first account is derived from the payload.
  const firstAccountDescription = projection.accounts[0].description;
  const legendToggle = page.getByRole('button', {
    name: new RegExp(
      `Hide ${escapeRegExp(firstAccountDescription)} series on chart`,
    ),
  });

  await expect(legendToggle).toBeVisible();
});
