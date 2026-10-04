import { expect, test } from '../../fixtures/auth';
import { selectRadixOption } from '../../helpers/radix';

// The projection chart's full legend strip is desktop-oriented; the mobile
// layout keeps the same controls but in a compact two-column stack.
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
      `Hide ${escapeRegExp(firstAccountDescription)} account on chart`,
    ),
  });

  await expect(legendToggle).toBeVisible();
});

// The Include switches live behind a compact trigger whose label states the
// current basis; on phones the trigger itself sits inside the Options
// disclosure.
test('projections include switches are reachable on any viewport', async ({
  page,
}, testInfo) => {
  // Register the response wait before navigating (the repo's pattern for
  // API-backed UI). Under full-matrix load the projections fetch can outrun the
  // default expect timeout, so wait on the response rather than the chart text.
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

  if (isMobileProject(testInfo)) {
    await page.getByRole('button', { name: 'Show options' }).click();
  }

  await page.getByRole('button', { name: /^Include:/ }).click();

  for (const name of INCLUDE_SWITCH_NAMES) {
    await expect(page.getByRole('switch', { name })).toBeVisible();
  }
});

// Each switch subtracts its own component from every series. The combined
// "Total (Selected Accounts)" line sums the visible accounts' components, so
// toggling any switch moves it; the exact per-component magnitudes are pinned
// by the chart data hook's reference-data tests, and this proves the end-to-end
// wiring.
test('toggling each include switch moves the plotted total and restoring it returns the line', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'Plotted-line comparison is desktop-only; the mobile switch reachability is covered above',
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

  // Open the compact Include trigger so the switches are mounted.
  await page.getByRole('button', { name: /^Include:/ }).click();

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

// The combined series is the sum of the legend-visible accounts, so hiding an
// account must move the plotted total and showing it again must move it back.
test('toggling an account off/on changes the plotted total', async ({
  page,
}, testInfo) => {
  test.skip(
    isMobileProject(testInfo),
    'Plotted-line comparison is desktop-only; the mobile legend is covered by mobileCardGrids.test.ts',
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
    accounts: { description: string }[];
  };
  expect(projection.accounts.length).toBeGreaterThan(0);

  const accountDescription = projection.accounts[0].description;

  await expect(
    page.getByText('Account Balances', { exact: true }).first(),
  ).toBeVisible();

  // The last rendered line is the combined total series.
  const totalLine = page.locator('.recharts-line').last();
  await expect(totalLine).toBeVisible();

  const totalPath = totalLine.locator('path').first();

  // Wait until the plotted total stops moving (entrance and domain animations).
  const baselinePath = await waitForStablePath(totalPath);

  const hideButton = page.getByRole('button', {
    name: new RegExp(
      `^Hide ${escapeRegExp(accountDescription)} account on chart$`,
    ),
  });
  const showButton = page.getByRole('button', {
    name: new RegExp(
      `^Show ${escapeRegExp(accountDescription)} account on chart$`,
    ),
  });

  await hideButton.click();
  await expect(showButton).toBeVisible();

  // The plotted total moves once the account is removed from the sum...
  await expect.poll(() => totalPath.getAttribute('d')).not.toBe(baselinePath);
  const hiddenPath = await waitForStablePath(totalPath);

  await showButton.click();
  await expect(hideButton).toBeVisible();

  // ...and moves back once the account is shown again. Recharts recomputes the Y
  // domain across series changes, so the exact SVG path is not compared here; the
  // per-account contribution is pinned by the chart-data hook tests and the
  // total-tooltip spec above.
  await expect.poll(() => totalPath.getAttribute('d')).not.toBe(hiddenPath);
});

// The total's tooltip lists only the accounts currently shown in the legend, so
// hiding an account removes it from the hover breakdown.
test('the total tooltip lists only the plotted accounts', async ({
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
    accounts: { description: string }[];
  };
  expect(projection.accounts.length).toBeGreaterThan(0);

  const accountDescription = projection.accounts[0].description;

  // The Expenses metric is a bar chart, whose total bar carries its own tooltip.
  await page
    .getByRole('combobox', { name: 'Select chart metric to display' })
    .click();
  await selectRadixOption(
    page.getByRole('option', { name: 'Expenses', exact: true }),
    page.getByRole('option', { name: 'Account Balances', exact: true }),
  );

  // Let the bar entrance animation finish so the geometry is final.
  await page.waitForTimeout(1200);

  expect(await hoverTotalBar(page)).toBeTruthy();

  const tooltip = page.locator('.recharts-tooltip-wrapper');
  await expect(tooltip).toContainText('Total (Selected Accounts)');
  await expect(tooltip).toContainText(accountDescription);

  await page
    .getByRole('button', {
      name: new RegExp(
        `^Hide ${escapeRegExp(accountDescription)} account on chart$`,
      ),
    })
    .click();
  await page.waitForTimeout(600);

  expect(await hoverTotalBar(page)).toBeTruthy();

  await expect(tooltip).toContainText('Total (Selected Accounts)');
  await expect(tooltip).not.toContainText(accountDescription);
});

// Waits until a line's `d` attribute stops changing between reads and returns
// the settled path. Recharts animates the line and the Y domain on mount and on
// series changes, so comparing paths without this races the animation.
async function waitForStablePath(
  pathLocator: import('@playwright/test').Locator,
): Promise<string> {
  let previousPath = '';

  await expect
    .poll(
      async () => {
        const currentPath = await pathLocator.getAttribute('d');
        const isStable = currentPath !== null && currentPath === previousPath;

        previousPath = currentPath ?? '';

        return isStable;
      },
      { timeout: 15_000, intervals: [250, 250, 250, 500, 500, 1000] },
    )
    .toBe(true);

  const stablePath = await pathLocator.getAttribute('d');

  expect(stablePath).toBeTruthy();

  return stablePath!;
}

// Dispatches a hover over the tallest bar of the final (total) series. Bars are
// sub-1px wide on a dense chart, so the event carries the bar's real client
// coordinates rather than relying on a pointer landing on the shape.
async function hoverTotalBar(
  page: import('@playwright/test').Page,
): Promise<boolean> {
  return page.evaluate(() => {
    const series = document.querySelectorAll('.recharts-bar');
    const bar = series[series.length - 1];

    if (!bar) {
      return false;
    }

    const rectangles = Array.from(
      bar.querySelectorAll('path.recharts-rectangle'),
    );
    let target: Element | null = null;
    let targetHeight = 0;

    for (const rectangle of rectangles) {
      const height = rectangle.getBoundingClientRect().height;

      if (height > targetHeight) {
        targetHeight = height;
        target = rectangle;
      }
    }

    if (!target) {
      return false;
    }

    const box = target.getBoundingClientRect();
    const clientX = box.x + box.width / 2;
    const clientY = box.y + box.height / 2;
    const wrapper = document.querySelector('.recharts-wrapper') ?? target;

    wrapper.dispatchEvent(
      new MouseEvent('mouseover', {
        bubbles: true,
        cancelable: true,
        view: window,
        clientX,
        clientY,
      }),
    );
    wrapper.dispatchEvent(
      new MouseEvent('mousemove', {
        bubbles: true,
        cancelable: true,
        view: window,
        clientX,
        clientY,
      }),
    );

    return true;
  });
}
