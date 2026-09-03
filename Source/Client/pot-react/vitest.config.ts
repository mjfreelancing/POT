/// <reference types="vitest" />
import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./tests/setup.ts'],
    // Vitest 4 simplified its default excludes (only node_modules + .git), so
    // without explicit excludes the Playwright E2E files (e2e/**/*.test.ts) and
    // generated artifacts would be swept into `vitest run`. Keep the unit suite
    // scoped to ./tests (also enforced by `--dir tests` in scripts).
    exclude: [
      '**/node_modules/**',
      '**/.git/**',
      '**/e2e/**',
      '**/playwright-report/**',
      '**/test-results/**',
      '**/coverage/**',
      '**/dist/**',
    ],
    coverage: {
      provider: 'istanbul',
      // Vitest 4 removed coverage.all; explicit include keeps uncovered src
      // files in the report (was the previous coverage.all behaviour).
      include: ['src/**'],
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@tests': path.resolve(__dirname, './tests'),
      'virtual:pwa-register': path.resolve(
        __dirname,
        './tests/mocks/virtual-pwa-register.ts',
      ),
    },
  },
});
