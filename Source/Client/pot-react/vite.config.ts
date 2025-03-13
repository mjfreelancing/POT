import path from "path";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  // Refer to: https://ui.shadcn.com/docs/installation/vite
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      // requires:
      // npm install -D @types/node
      "@": path.resolve(__dirname, "./src"),
    },
  },
});
