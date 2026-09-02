import { test as baseTest, expect } from '@playwright/test';

// Covers the PWA contract in BOTH environments (dev and prodlike configs):
// - DEV (playwright.config.ts, Vite dev server, E2E=1): vite-plugin-pwa gates
//   BOTH the manifest <link> and the service worker behind `devOptions.enabled`
//   — NOT set here (verified in the plugin source: `webManifestData()` /
//   `registerSWData()` short-circuit on `ctx.devEnvironment && !devOptions.enabled`),
//   and the app itself short-circuits `registerServiceWorker()` when
//   `import.meta.env.DEV`. So dev serves NO manifest and registers NO SW — the
//   NEGATIVE contract.
// - BUILT (playwright.prod.config.ts, `npm run build && vite preview`): the
//   manifest IS injected and served at `/manifest.webmanifest` with the
//   identity declared in vite.config.ts, and a service worker IS registered —
//   the POSITIVE contract (this is why the old dev-only negative test could not
//   run against the built client).
//
// This single test detects which contract applies — a manifest <link> is
// present exactly when the app is a production build — and asserts the matching
// one, so it runs green under BOTH configs. No config-level testIgnore needed.
//
// Logged-out (login page) -> no auth/data deps; runs on all 4 projects.

baseTest(
  'PWA contract matches the environment (dev: none; built: manifest + SW)',
  async ({ browser }) => {
    const context = await browser.newContext({
      storageState: { cookies: [], origins: [] },
    });
    const page = await context.newPage();
    try {
      await page.goto('/login');
      // Web-first: the login form is the app shell's "logged-out" marker.
      await expect(
        page.getByRole('button', { name: 'Login', exact: true }),
      ).toBeVisible();

      // A production build injects <link rel="manifest">; dev does not.
      const manifestHref = await page.evaluate(
        () =>
          document
            .querySelector('link[rel="manifest"]')
            ?.getAttribute('href') ?? null,
      );

      if (manifestHref === null) {
        // DEV negative contract: no service worker is registered, none controls
        // the page, and there is no manifest link.
        const swState = await page.evaluate(async () => {
          const registrations =
            await navigator.serviceWorker.getRegistrations();
          return {
            count: registrations.length,
            hasController: navigator.serviceWorker.controller !== null,
          };
        });
        expect(swState.count).toBe(0);
        expect(swState.hasController).toBe(false);
        return;
      }

      // BUILT positive contract: the manifest is served with the identity
      // declared in vite.config.ts, and a service worker registers.
      const manifestUrl = new URL(manifestHref, page.url()).toString();
      const manifestResponse = await page.request.get(manifestUrl);
      expect(manifestResponse.ok()).toBeTruthy();
      const manifest = (await manifestResponse.json()) as {
        name?: string;
        short_name?: string;
        start_url?: string;
        display?: string;
      };
      expect(manifest.name).toBe('POT - Pay On Time');
      expect(manifest.short_name).toBe('POT');
      expect(manifest.start_url).toBe('/');
      expect(manifest.display).toBe('standalone');

      // SW registration happens after load; poll until it registers.
      await expect
        .poll(async () =>
          page.evaluate(
            async () =>
              (await navigator.serviceWorker.getRegistrations()).length,
          ),
        )
        .toBeGreaterThan(0);
    } finally {
      await context.close();
    }
  },
);
