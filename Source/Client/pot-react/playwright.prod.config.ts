import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.test.ts',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  // Bound local parallelism like the dev config: the shared stack (one API, one
  // Postgres, one preview server) plus the built client's per-context service-
  // worker install can't fit unlimited browsers.
  workers: process.env.CI ? 1 : 2,
  reporter: 'html',
  globalSetup: './playwright.globalSetup.ts',
  use: {
    baseURL: 'http://127.0.0.1:5175',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  webServer: [
    {
      // The API webServer must match playwright.config.ts EXACTLY (Release build,
      // the PlatformAdmin user grant, a log-file redirect instead of a pipe, and
      // a 180s start timeout). The full prodlike matrix exercises platform-admin
      // flows (approvals / user settings) and runs far more API traffic than the
      // subset, so Debug mode, a missing --PlatformAdmin:UserIds, or a piped
      // stdout that fills under load all break it.
      command:
        'dotnet run --no-launch-profile -c Release --project Pot.AspNetCore/Pot.AspNetCore.csproj -- --urls http://127.0.0.1:5242 --Cors:AllowedOrigins=http://127.0.0.1:5175,http://localhost:5175 --Jwt:Issuer=pot-e2e --Jwt:Audience=pot-e2e-client --Jwt:SecretKey=F4B58D0C1Z2B3E4D5F6A7B8C9D0E1F2A3B4C536E7F8A194A0EF29B0C1D2E3F4A5B6C7D8E9F0A1B2CWD4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C --Database:Host=localhost --Database:Port=55432 --Database:Name=pot_e2e --Database:Username=test_user --Database:Password=test_pass --Database:SSLMode=disable --RateLimiting:Anonymous:PermitLimit=200 --RateLimiting:Anonymous:WindowSeconds=10 --RateLimiting:Authenticated:PermitLimit=500 --RateLimiting:Authenticated:WindowSeconds=30 --Authentication:Cookie:SecureOnly=false --PlatformAdmin:UserIds=58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5 > e2e-server.log 2>&1',
      cwd: '../../Server',
      port: 5242,
      reuseExistingServer: false,
      timeout: 180000,
      stdout: 'ignore',
      stderr: 'ignore',
    },
    {
      command:
        'npm run build && npm run preview -- --host 127.0.0.1 --strictPort',
      port: 5175,
      reuseExistingServer: false,
      timeout: 180000,
    },
  ],
  projects: [
    {
      name: 'chromium',
      testIgnore: [
        // errorBoundary.test.ts blocks a DEV-SOURCE lazy chunk URL
        // (**/src/features/accounts/AccountsPage.tsx*); a built client serves
        // hashed chunk names, so the route never matches and the test fails.
        '**/feedback/errorBoundary.test.ts',
      ],
      use: {
        browserName: 'chromium',
      },
    },
    {
      name: 'edge',
      testIgnore: [
        '**/feedback/errorBoundary.test.ts',
        // userSettings.test.ts is chromium-only (mutates shared canonical
        // identities — see the file header + dev config rationale).
        '**/settings/userSettings.test.ts',
      ],
      use: {
        browserName: 'chromium',
        channel: 'msedge',
      },
    },
    {
      name: 'mobile-chrome',
      testIgnore: [
        '**/feedback/errorBoundary.test.ts',
        '**/settings/userSettings.test.ts',
        // filters.test.ts is desktop-only; on mobile it immediately skips but
        // still runs the auth fixture (8 wasted PBKDF2 logins per matrix run).
        '**/filters/filters.test.ts',
      ],
      use: {
        ...devices['Pixel 7'],
      },
    },
    {
      name: 'mobile-safari',
      testIgnore: [
        '**/feedback/errorBoundary.test.ts',
        '**/settings/userSettings.test.ts',
        '**/filters/filters.test.ts',
      ],
      use: {
        ...devices['iPhone 14'],
      },
    },
  ],
});
