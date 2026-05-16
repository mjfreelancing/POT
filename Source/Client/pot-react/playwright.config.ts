import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.test.ts',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
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
      command:
        'dotnet run --no-launch-profile --project Pot.AspNetCore/Pot.AspNetCore.csproj -- --urls http://127.0.0.1:5242 --Cors:AllowedOrigins=http://127.0.0.1:5175,http://localhost:5175 --Jwt:Issuer=pot-e2e --Jwt:Audience=pot-e2e-client --Jwt:SecretKey=F4B58D0C1Z2B3E4D5F6A7B8C9D0E1F2A3B4C536E7F8A194A0EF29B0C1D2E3F4A5B6C7D8E9F0A1B2CWD4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C --Database:Host=localhost --Database:Port=55432 --Database:Name=pot_e2e --Database:Username=test_user --Database:Password=test_pass --Database:SSLMode=disable --RateLimiting:Anonymous:PermitLimit=200 --RateLimiting:Anonymous:WindowSeconds=10 --RateLimiting:Authenticated:PermitLimit=500 --RateLimiting:Authenticated:WindowSeconds=30 --Authentication:Cookie:SecureOnly=false',
      cwd: '../../Server',
      port: 5242,
      reuseExistingServer: false,
      timeout: 120000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
    {
      command: 'npm run dev -- --host 127.0.0.1 --strictPort',
      port: 5175,
      reuseExistingServer: false,
      timeout: 120000,
    },
  ],
  projects: [
    {
      name: 'chromium',
      use: {
        browserName: 'chromium',
      },
    },
    {
      name: 'edge',
      use: {
        browserName: 'chromium',
        channel: 'msedge',
      },
    },
    {
      name: 'mobile-chrome',
      use: {
        ...devices['Pixel 7'],
      },
    },
    {
      name: 'mobile-safari',
      use: {
        ...devices['iPhone 14'],
      },
    },
  ],
});
