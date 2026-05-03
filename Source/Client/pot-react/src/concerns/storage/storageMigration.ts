/**
 * TEMPORARY: Legacy storage key migration helpers.
 *
 * This module maps legacy flat storage keys (e.g., `pot-dashboard`) to their
 * new per-user, per-environment scoped equivalents (e.g., `pot:dev:user:{id}:dashboard`).
 *
 * Migration logic:
 * - If the legacy key exists and the new key does not, the data is moved to the new key.
 * - If both keys exist, the legacy key is removed and the new key's data is kept.
 * - If the legacy key does not exist, no action is taken.
 *
 * Removal plan: Delete this file and all call-sites once the legacy flat keys have
 * been fully replaced across all active user sessions (Step 4.3).
 */

type MigrationEntry = {
  oldKey: string;
  newKey: string;
  storage: Storage;
};

function migrateStorageEntry({ oldKey, newKey, storage }: MigrationEntry): void {
  const oldValue = storage.getItem(oldKey);

  if (oldValue === null) {
    return;
  }

  const newValue = storage.getItem(newKey);

  if (newValue !== null) {
    // New key already populated — clean up the legacy key only.
    storage.removeItem(oldKey);
    return;
  }

  // Move data from the legacy key to the new scoped key.
  storage.setItem(newKey, oldValue);
  storage.removeItem(oldKey);
}

function migrateStorageEntries(entries: MigrationEntry[]): void {
  for (const entry of entries) {
    migrateStorageEntry(entry);
  }
}

export { migrateStorageEntries };
export type { MigrationEntry };
