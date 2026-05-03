import { resolveStorageEnv } from './storageEnv';

/**
 * Whether storage should persist across sessions (localStorage) or
 * be cleared when the browser tab/session ends (sessionStorage).
 */
type StorageMode = 'persistent' | 'session';

type UserScopedKeyOptions = {
  userId: string;
  feature: string;
};

/**
 * Builds a storage key scoped to the current environment and a specific user.
 * Produces keys in the format: `pot:{env}:user:{userId}:{feature}`.
 *
 * Use this for any storage that should be isolated per user, so that switching
 * accounts on the same browser does not expose another user's state.
 */
function buildUserScopedKey({ userId, feature }: UserScopedKeyOptions): string {
  const env = resolveStorageEnv();

  return `pot:${env}:user:${userId}:${feature}`;
}

/**
 * Builds a storage key scoped to the current environment only (no user segment).
 * Produces keys in the format: `pot:{env}:{feature}`.
 *
 * Use this for application-level state that is not user-specific (e.g., UI theme).
 */
function buildEnvScopedKey(feature: string): string {
  const env = resolveStorageEnv();

  return `pot:${env}:${feature}`;
}

/**
 * Resolves the Web Storage backend for the given mode.
 * - 'persistent' → localStorage (survives browser restarts)
 * - 'session'    → sessionStorage (cleared when the tab/session ends)
 */
function resolveStorageBackend(mode: StorageMode): Storage {
  return mode === 'session' ? sessionStorage : localStorage;
}

export { buildEnvScopedKey, buildUserScopedKey, resolveStorageBackend };
export type { StorageMode, UserScopedKeyOptions };
