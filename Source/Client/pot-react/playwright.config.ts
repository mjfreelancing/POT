import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.test.ts',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  // Local retries: 1 is a safety net for the shared-stack matrix. The per-test
  // load reductions (Release API, reduced-login auth fixture, bounded workers)
  // removed most flakes, but a transient cold-browser contention blip can still
  // trip one load-sensitive test; a single retry (like CI's 2) catches it
  // without re-running the whole matrix. Video/trace record on the retry, so
  // diagnostics are preserved.
  retries: process.env.CI ? 2 : 1,
  // Bound local parallelism. With an unlimited worker count, ~16 browsers
  // simultaneously hit the ONE shared Vite dev server, the ONE dotnet API, and
  // the ONE Testcontainers Postgres — starving slow tests (login page not
  // rendering within 60s, auth-fixture logins timing out, CRUD lifecycles
  // exceeding the per-test timeout).
  //
  // 4 workers kept the original suite green, but adding the interaction
  // suites (bulk actions, quick actions, filters — each a full API-setup + UI
  // lifecycle) pushed the shared stack past capacity: the full 4-project matrix
  // began failing intermittently on whatever load-sensitive test was unlucky
  // (expensesCrud waitForResponse timeout, seedData landing on the login page,
  // loginFlow's Login button staying disabled, mobile ERR_ABORTED navigations).
  // Raising the timeout made it worse (heavy tests run longer → higher
  // sustained load → other tests starve), and 3 workers still flaked. 2 workers
  // is the tightest bound that reliably fits the shared stack; the local matrix
  // takes ~6-7 min as a result. CI pins to 1 (no contention, retries on top).
  workers: process.env.CI ? 1 : 2,
  reporter: 'html',
  // Per-test timeout. The Playwright default (30s) is too tight for this
  // shared-stack matrix: each test does a fresh login (bcrypt verify) and the
  // fixture-managed CRUD suites run a long create -> edit -> delete lifecycle.
  // Under 4-project parallel load against one API + Postgres these can exceed
  // 30s (observed as "Test timeout of 30000ms exceeded" flakes on the CRUD
  // lifecycles and the auth page-fixture setup). 60s gives room without
  // masking genuine hangs.
  timeout: 60_000,
  globalSetup: './playwright.globalSetup.ts',
  // globalTeardown cleans up the runtime state file written by globalSetup.
  // It does NOT explicitly stop the Testcontainers container — on Docker Desktop for Windows,
  // calling `docker stop` during teardown leaves the port in TIME_WAIT, which prevents
  // Docker from rebinding 55432 on the next run. Instead, the Testcontainers Ryuk reaper
  // stops the container after the process exits (reliable for normal and interrupted runs).
  // For runs where Ryuk also cannot fire (hard kill), globalSetup detects the stale
  // container from the runtime state file and removes it before starting a new one.
  //
  // NOTE: both globalSetup and globalTeardown point at the same module. Playwright calls
  // the `default` export as setup and the named `globalTeardown` export as teardown.
  // Support for a named globalTeardown export in a shared file requires Playwright >= 1.44
  // (repo pins ^1.59.1). Do not downgrade below 1.44 without splitting teardown into its own module.
  globalTeardown: './playwright.globalSetup.ts',
  expect: {
    // Cold-browser first loads (Edge channel, mobile matrix) can exceed the 5s
    // default while lazy page chunks mount; give web-first assertions room.
    timeout: 10000,
  },
  use: {
    baseURL: 'http://127.0.0.1:5175',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    // 'retain-on-failure' records video for EVERY test on all 4 projects
    // (Edge + WebKit capture is expensive CPU on the shared local stack).
    // Recording only on retry cuts that cost; screenshots still capture
    // failures, and CI (retries 2) still gets video for flake diagnosis.
    video: 'on-first-retry',
  },
  webServer: [
    {
      // Server stdout/stderr is redirected to ../../Server/e2e-server.log (overwritten each run).
      // stdout/stderr are set to 'ignore' to prevent Playwright capturing the redirected output.
      //
      // Startup contract (see also G2): Playwright launches this webServer BEFORE globalSetup
      // creates the Testcontainers Postgres container, so the API starts pointing at a DB that
      // does not exist yet (port 55432). It survives via Npgsql's connection retry — the HTTP
      // listener opens before the DB connection is established. globalSetup then starts the
      // container, runs migrations, seeds, and polls /_health/ready to confirm the DB connection
      // before any test runs. Do not remove that readiness barrier.
      // Run the API in Release (-c Release) so the shared stack pays Debug-mode
      // latency for the whole matrix. dotnet run builds Release on first use and
      // reuses it when up to date.
      command:
        'dotnet run --no-launch-profile -c Release --project Pot.AspNetCore/Pot.AspNetCore.csproj -- --urls http://127.0.0.1:5242 --Cors:AllowedOrigins=http://127.0.0.1:5175,http://localhost:5175 --Jwt:Issuer=pot-e2e --Jwt:Audience=pot-e2e-client --Jwt:SecretKey=F4B58D0C1Z2B3E4D5F6A7B8C9D0E1F2A3B4C536E7F8A194A0EF29B0C1D2E3F4A5B6C7D8E9F0A1B2CWD4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C --Database:Host=localhost --Database:Port=55432 --Database:Name=pot_e2e --Database:Username=test_user --Database:Password=test_pass --Database:SSLMode=disable --RateLimiting:Anonymous:PermitLimit=200 --RateLimiting:Anonymous:WindowSeconds=10 --RateLimiting:Authenticated:PermitLimit=500 --RateLimiting:Authenticated:WindowSeconds=30 --Authentication:Cookie:SecureOnly=false --PlatformAdmin:UserIds=58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5 > e2e-server.log 2>&1',
      cwd: '../../Server',
      port: 5242,
      reuseExistingServer: false,
      // Release build on first run can take longer than Debug; give it headroom.
      timeout: 180000,
      stdout: 'ignore',
      stderr: 'ignore',
    },
    // The Vite dev server is used for the local matrix. Serving a BUILT client
    // (vite preview) was trialled to remove dev-server HMR/transform load, but
    // it regressed the matrix (the PWA service worker installs on every fresh
    // per-test context, adding per-test overhead, and the errorBoundary test
    // aborts the dev-source lazy chunk URL). The dev server stays; load is
    // managed via bounded workers + the reduced-login auth fixture instead.
    {
      command: 'npm run dev -- --host 127.0.0.1 --strictPort',
      port: 5175,
      reuseExistingServer: false,
      timeout: 120000,
      // Disables Vite HMR for E2E (see vite.config.ts) so the dev server can
      // never full-reload a page mid-test — removes the load-induced reload
      // flake (userSettings sheet wiped to /dashboard, loginFlow form to /login).
      env: { E2E: '1' },
    },
  ],
  projects: [
    {
      name: 'chromium',
      use: {
        browserName: 'chromium',
      },
    },
    // userSettings.test.ts is CHROMIUM-ONLY. Its tests mutate shared canonical
    // identities (e2e_admin display name, e2e_pwchange password) in the ONE
    // shared Testcontainers DB. Running the file on another project concurrently
    // collides (etag races; the password change bumps TokenVersion and 401s the
    // other project's session) — see the file's header comment. Excluding it
    // here keeps the parallel matrix green (an in-body skip cannot stop the
    // auth fixture from running, which is where the collision bites).
    {
      name: 'edge',
      testIgnore: ['**/settings/userSettings.test.ts'],
      use: {
        browserName: 'chromium',
        channel: 'msedge',
      },
    },
    {
      name: 'mobile-chrome',
      testIgnore: [
        // userSettings.test.ts is chromium-only (mutates shared canonical
        // identities — see the file header). filters.test.ts is desktop-only
        // (mobile uses the card grid, not the table filter layout) and its tests
        // immediately skip on
        // mobile, but each still runs the auth fixture (a PBKDF2 login) first —
        // excluding the file here removes 8 wasted logins per matrix run.
        // shortLandscapeScroll.test.ts overrides the viewport to a short
        // landscape size (desktop layout), so it is excluded from the portrait
        // phone projects.
        '**/settings/userSettings.test.ts',
        '**/filters/filters.test.ts',
        '**/rendering/shortLandscapeScroll.test.ts',
      ],
      use: {
        ...devices['Pixel 7'],
      },
    },
    {
      name: 'mobile-safari',
      testIgnore: [
        '**/settings/userSettings.test.ts',
        '**/filters/filters.test.ts',
        '**/rendering/shortLandscapeScroll.test.ts',
      ],
      use: {
        ...devices['iPhone 14'],
      },
    },
  ],
});
