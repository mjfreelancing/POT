import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vite';

// https://vite.dev/config/
export default defineConfig({
  // Refer to: https://ui.shadcn.com/docs/installation/vite
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      // requires:
      // npm install -D @types/node
      '@': path.resolve(__dirname, './src'),
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
  },
});
