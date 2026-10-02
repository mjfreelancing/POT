import { expect, test } from '../../fixtures/auth';

// The projection chart's legend row is only visible on desktop; on mobile the
// filters (including the legend) are collapsed behind a "Show filters" button
// (covered by mobileCardGrids.test.ts).
const isMobileProject = (testInfo: import('@playwright/test').TestInfo) =>
  testInfo.project.name.startsWith('mobile');

const escapeRegExp = (value: string) =>
  value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

const INCLUDE_SWITCH_NAMES = [
  'Include Reserved',
  'Include Accruals',
  'Include Arrears',
] as const;

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

// A-03: the Include switch row never collapses, so it renders on any viewport
// without the reader opening the mobile filters.
test('projections include switches render on any viewport without opening filters', async ({
  page,
}) => {
  await page.goto('/projections');

  await expect(
    page.getByText('Account Balances', { exact: true }).first(),
  ).toBeVisible();

  for (const name of INCLUDE_SWITCH_NAMES) {
    await expect(page.getByRole('switch', { name })).toBeVisible();
  }
});

// Each switch subtracts its own component from every series. The combined
// "Total (All Accounts)" line sums all accounts' components, so toggling any
// switch moves it; the exact per-component magnitudes are pinned by the chart
// data hook's reference-data tests, and this proves the end-to-end wiring.
test('toggling each include switch moves the plotted total and restoring it returns the line', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'Plotted-line comparison is desktop-only; the mobile switch visibility is covered above',
  );

  const projectionsResponsePromise = page.waitForResponse(
    response =>
      response.url().includes('/api/projections') &&
      response.request().method() === 'GET',
  );

  await page.goto('/projections');

  const projectionsResponse = await projectionsResponsePromise;
  expect(projectionsResponse.ok()).toBeTruthy();

  await expect(
    page.getByText('Account Balances', { exact: true }).first(),
  ).toBeVisible();

  // The last rendered line is the combined total series, which sums the
  // accounts' reserved, unpaid accrual and arrears.
  const totalLine = page.locator('.recharts-line').last();
  await expect(totalLine).toBeVisible();

  // Let the entrance animation finish so the measured path is final.
  await page.waitForTimeout(1200);

  const totalPath = totalLine.locator('path').first();
  const baselinePath = await totalPath.getAttribute('d');
  expect(baselinePath).toBeTruthy();

  for (const name of INCLUDE_SWITCH_NAMES) {
    const toggle = page.getByRole('switch', { name });
    const initialState = await toggle.getAttribute('data-state');

    await toggle.click();
    await expect(toggle).toHaveAttribute(
      'data-state',
      initialState === 'checked' ? 'unchecked' : 'checked',
    );

    // The plotted total moves once the switch is applied...
    await expect.poll(() => totalPath.getAttribute('d')).not.toBe(baselinePath);

    // ...and returns exactly when the switch is restored.
    await toggle.click();
    await expect.poll(() => totalPath.getAttribute('d')).toBe(baselinePath);
  }
});
