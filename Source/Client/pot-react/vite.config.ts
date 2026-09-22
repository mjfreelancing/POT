import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig, type Plugin } from 'vite';
import { VitePWA } from 'vite-plugin-pwa';

// Identity of the deployed build, used by the in-app deployed-build check
// (see src/concerns/pwa/pwaVersionCheck.ts). A timestamp is used because the Docker and Azure
// build contexts do not include .git, and it changes on every build.
const clientBuildId = new Date().toISOString();

// Emits the deployed build identity as a static file so a running client can compare it with the
// build id baked into its own bundle. JSON is not covered by the workbox `globPatterns` below, so
// this file is never precached and every read comes from the network.
const clientBuildIdPlugin = (): Plugin => ({
  name: 'pot-client-build-id',
  apply: 'build',
  generateBundle() {
    this.emitFile({
      type: 'asset',
      fileName: 'version.json',
      source: `${JSON.stringify({ buildId: clientBuildId }, null, 2)}\n`,
    });
  },
});

// https://vite.dev/config/
export default defineConfig({
  define: {
    'import.meta.env.VITE_CLIENT_BUILD_ID': JSON.stringify(clientBuildId),
  },
  // Refer to: https://ui.shadcn.com/docs/installation/vite
  plugins: [
    react(),
    tailwindcss(),
    clientBuildIdPlugin(),
    VitePWA({
      registerType: 'prompt',
      injectRegister: 'auto',
      manifest: {
        name: 'POT - Pay On Time',
        short_name: 'POT',
        description: 'Pay On Time - Personal cashflow and projection tool',
        theme_color: '#2563eb',
        background_color: '#ffffff',
        display: 'standalone',
        start_url: '/',
        scope: '/',
        icons: [
          {
            src: 'pwa-64x64.png',
            sizes: '64x64',
            type: 'image/png',
          },
          {
            src: 'pwa-192x192.png',
            sizes: '192x192',
            type: 'image/png',
          },
          {
            src: 'pwa-512x512.png',
            sizes: '512x512',
            type: 'image/png',
          },
          {
            src: 'maskable-icon-512x512.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'maskable',
          },
        ],
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,svg,png,ico,woff,woff2}'],
        navigateFallback: '/index.html',
        navigateFallbackDenylist: [/^\/api/],
        runtimeCaching: [],
      },
    }),
  ],
  resolve: {
    alias: {
      // import.meta.dirname (Node >= 20.11) requires @types/node for typing
      '@': import.meta.dirname + '/src',
    },
  },
  server: {
    proxy: {
      '/api': {
        // Use 5241 for the server in Docker
        // Use 5242 for the server running locally
        target: 'http://localhost:5242',
        changeOrigin: true,
        secure: false,
      },
    },
    // E2E runs must not receive Vite HMR/full-reload messages: under full-matrix
    // parallel load the dev server can emit a full-reload with NO file changes
    // (runtime optimizeDeps re-discovery), force-reloading a page mid-test
    // (observed: the userSettings sheet wiped to /dashboard, the login form to
    // /login). With HMR off there is no websocket to deliver the reload, so the
    // page can never be reloaded behind a test. Playwright sets E2E=1 on the
    // Vite webServer; a normal `npm run dev` leaves HMR enabled.
    ...(process.env.E2E === '1' ? { hmr: false } : {}),
  },
  build: {
    rollupOptions: {
      output: {
        // Keep vendor dependencies in stable, explicit chunks to avoid a single oversized
        // bundle and improve long-term cache reuse when app code changes.
        //
        // If new heavy dependencies are introduced, place them in a matching chunk bucket
        // here rather than allowing them to fall into the default app chunk.
        manualChunks(id) {
          if (!id.includes('node_modules')) {
            return;
          }

          if (
            // Core React runtime and routing stack
            id.includes('/react-router/') ||
            id.includes('/react-dom/') ||
            id.includes('/react/')
          ) {
            return 'react-vendor';
          }

          if (
            // Query and table state management
            id.includes('/@tanstack/react-query/') ||
            id.includes('/@tanstack/react-table/')
          ) {
            return 'tanstack-vendor';
          }

          if (id.includes('/recharts/')) {
            // Charting runtime kept isolated due to size
            return 'charts-vendor';
          }

          if (id.includes('/@radix-ui/')) {
            // Radix UI primitives used across shared components
            return 'radix-vendor';
          }

          if (
            // Form handling and schema resolver integration
            id.includes('/react-hook-form/') ||
            id.includes('/@hookform/resolvers/')
          ) {
            return 'forms-vendor';
          }

          if (
            // Shared utility/runtime dependencies used by multiple features
            id.includes('/zod/') ||
            id.includes('/date-fns/') ||
            id.includes('/axios/') ||
            id.includes('/jwt-decode/')
          ) {
            return 'utils-vendor';
          }
        },
      },
    },
  },
});
