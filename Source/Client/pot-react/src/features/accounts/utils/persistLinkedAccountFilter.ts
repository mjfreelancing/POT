import { buildEnvScopedKey, buildUserScopedKey } from '@/concerns/storage';

type LinkedFilterFeature = 'expenses' | 'incomes';

type PersistLinkedAccountFilterOptions = {
  userId?: string | null;
  feature: LinkedFilterFeature;
  accountId: string;
};

/**
 * Writes a linked-account selection to the feature's session storage key before navigation.
 *
 * Badge clicks are direct user intent to open a filtered list for a specific account.
 * Persisting that account ID ahead of route changes keeps sidebar feature links and list
 * pages aligned with the same account context.
 *
 * This helper updates only `selectedAccountId` and preserves any other stored fields
 * (for example, text filters) that may exist in the same storage record.
 */
function persistLinkedAccountFilter({
  userId,
  feature,
  accountId,
}: PersistLinkedAccountFilterOptions): void {
  const storageKey = userId
    ? buildUserScopedKey({ userId, feature })
    : buildEnvScopedKey(`unauthenticated:${feature}`);

  let existingData: {
    selectedAccountId?: string | null;
    filterDescription?: string | null;
  } = {};

  try {
    const existingValue = sessionStorage.getItem(storageKey);

    if (existingValue) {
      existingData = JSON.parse(existingValue);
    }
  } catch {
    existingData = {};
  }

  // Preserve other properties (like filterDescription) and only update the account filter
  sessionStorage.setItem(
    storageKey,
    JSON.stringify({
      ...existingData,
      selectedAccountId: accountId,
    }),
  );
}

export default persistLinkedAccountFilter;
export type { LinkedFilterFeature, PersistLinkedAccountFilterOptions };
