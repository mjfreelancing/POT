/// <reference types="vite/client" />
/// <reference types="vite-plugin-pwa/client" />

// Identity of the running build, baked in by vite.config.ts. It is compared with the deployed
// build reported by /version.json to detect that a newer client has been released.
// Undefined outside a build that sets it (for example Vite dev mode).
type ImportMetaEnv = {
  readonly VITE_CLIENT_BUILD_ID?: string;
};
