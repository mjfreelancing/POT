/**
 * Resolves the environment scope for storage keys.
 * Returns 'prod' in production builds; returns 'dev' in all other environments
 * (development, test, etc.).
 *
 * Used as a component of scoped storage keys to prevent key collisions when
 * the same browser profile is used across local development and production.
 */
function resolveStorageEnv(): string {
  return import.meta.env.MODE === 'production' ? 'prod' : 'dev';
}

export { resolveStorageEnv };
