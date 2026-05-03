/**
 * TEMPORARY: Legacy storage key cleanup helpers.
 *
 * Removes legacy flat storage keys (e.g., `pot-dashboard`, `pot-ui-theme`) so that
 * stale data from before the per-user/per-environment scoping change does not persist
 * in the browser. Users will simply start fresh with the new scoped keys.
 *
 * Removal plan: Delete this file and all call-sites once enough time has passed for
 * legacy keys to have been purged across active user sessions (Step 4.3).
 */

type LegacyStorageKey = {
  key: string;
  storage: Storage;
};

function purgeLegacyStorageKeys(keys: LegacyStorageKey[]): void {
  for (const { key, storage } of keys) {
    storage.removeItem(key);
  }
}

export { purgeLegacyStorageKeys };
export type { LegacyStorageKey };
