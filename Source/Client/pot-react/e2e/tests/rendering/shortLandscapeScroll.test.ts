import { expect, test } from '../../fixtures/auth';

// Short-landscape regression coverage.
//
// A phone rotated to landscape is ~390px tall but wider than the md breakpoint
// (>=768px), so the app renders its DESKTOP layout (sidebar + data table) at a
// very short height. Two related regressions lived there:
//   1. The list pages (accounts / expenses / incomes) used a fixed
//      `h-screen overflow-hidden` layout where only the deeply nested table
//      scrolls; the fixed chrome (page header + toolbar + paddings +
//      bulk-actions bar) consumed the whole height and collapsed the scroll
//      region to 0px, leaving rows unreachable and nothing to scroll.
//   2. The projections chart's md "fill the card" sizing collapsed the Recharts
//      responsive container to 0px when landing directly in landscape, so no
//      chart rendered.
//
// jsdom unit tests cannot measure layout (clientHeight/scrollHeight are 0), so
// this file guards both regressions in a real browser at a phone-landscape
// viewport. It runs on the DESKTOP projects only (chromium / edge); the mobile
// projects are portrait phone emulation and are excluded in
// playwright.config.ts (testIgnore) so no logins are wasted.
//
// Parallel-safe: read-only (page loads + list GETs), no mutations.

type ScrollMetrics = {
  scrollable: boolean;
  clientHeight: number;
  scrollHeight: number;
  afterScrollTop: number;
};

test.describe('short landscape viewport (desktop layout, ~390px tall)', () => {
  test.use({ viewport: { width: 844, height: 390 } });

  for (const route of ['/expenses', '/incomes', '/accounts'] as const) {
    test(`${route} keeps its list scrollable instead of collapsing to 0px`, async ({
      page,
    }) => {
      await page.goto(route);

      // Desktop data table renders at this width (>=768px).
      await expect(page.getByRole('table')).toBeVisible();
      await expect(page.locator('tbody tr').first()).toBeVisible();

      // The content below the fixed header must be a real, overflowing scroll
      // region (positive height we can actually scroll), not a 0px collapse
      // that leaves rows unreachable.
      const metrics = await measureContentScroll(
        page.getByRole('toolbar').first(),
      );

      expect(metrics.scrollable).toBe(true);
      expect(metrics.clientHeight).toBeGreaterThan(0);
      expect(metrics.scrollHeight).toBeGreaterThan(metrics.clientHeight);
      expect(metrics.afterScrollTop).toBeGreaterThan(0);
    });
  }

  test('projections renders a sized chart when opened directly in landscape', async ({
    page,
  }) => {
    await page.goto('/projections');

    await expect(
      page.getByRole('heading', { name: 'Projections', exact: true }),
    ).toBeVisible();

    // The default metric title renders only after projection data arrives.
    await expect(
      page.getByText('Account Balances', { exact: true }).first(),
    ).toBeVisible();

    // The chart SVG has no accessible name, so measure its rendered height.
    // Before the fix it was absent/0px when landing directly in landscape.
    const chartHeight = await page.evaluate(() => {
      const surface = document.querySelector('.recharts-surface');

      return surface ? surface.getBoundingClientRect().height : 0;
    });

    expect(chartHeight).toBeGreaterThan(200);

    // The surrounding page content must scroll so the chart is reachable.
    // The chart SVG is nested inside the page's scrollable content area, so it
    // is a reliable anchor for walking up to that scroll container.
    const metrics = await measureContentScroll(page.locator('.recharts-surface'));

    expect(metrics.scrollable).toBe(true);
    expect(metrics.clientHeight).toBeGreaterThan(0);
    expect(metrics.scrollHeight).toBeGreaterThan(metrics.clientHeight);
    expect(metrics.afterScrollTop).toBeGreaterThan(0);
  });
});

// Helpers
async function measureContentScroll(
  anchor: import('@playwright/test').Locator,
): Promise<ScrollMetrics> {
  // Anchor elements sit inside the scrollable content area below the fixed
  // header (list pages: the page Toolbar; projections: the chart SVG). Walk up
  // from the anchor to the first ancestor that can actually scroll (overflow-y
  // auto/scroll with overflowing content).
  await expect(anchor).toBeVisible();

  return anchor.evaluate(el => {
    let node = el.parentElement;

    while (node && node !== document.documentElement) {
      const style = window.getComputedStyle(node);
      const canScroll =
        (style.overflowY === 'auto' || style.overflowY === 'scroll') &&
        node.scrollHeight > node.clientHeight + 1;

      if (canScroll) {
        node.scrollTop = node.scrollHeight;

        return {
          scrollable: true,
          clientHeight: node.clientHeight,
          scrollHeight: node.scrollHeight,
          afterScrollTop: node.scrollTop,
        };
      }

      node = node.parentElement;
    }

    return {
      scrollable: false,
      clientHeight: 0,
      scrollHeight: 0,
      afterScrollTop: 0,
    };
  });
}
