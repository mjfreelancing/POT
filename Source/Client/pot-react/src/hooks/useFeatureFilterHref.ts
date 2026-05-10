import { buildUserScopedKey } from '@/concerns/storage';
import useUserStore from '@/stores/useUserStore';

type FeatureName = 'expenses' | 'incomes';

/**
 * Dynamic Sidebar Link Builder Hook
 * =================================
 * This hook encapsulates the logic for building dynamic sidebar links that reflect
 * the current filter state stored in sessionStorage. It is used by the sidebar navigation
 * to ensure that links carry the active account filter when navigating between features.
 *
 * Design Principle:
 * The sidebar is the ONLY component that reads from storage. The page itself never reads
 * storage at render time—it only reads from the URL (the render-time source of truth).
 * This hook provides the sidebar with a clean interface to build its hrefs without knowing
 * the internal storage key schema.
 *
 * Behavior:
 * - Reads the feature's sessionStorage entry using the same key-building utilities as the page
 * - Extracts selectedAccountId from storage
 * - If selectedAccountId is non-null → returns `/{feature}?accountId={selectedAccountId}`
 * - If selectedAccountId is null or missing → returns `/{feature}` (base route)
 * - No caching; recomputes on every render so sidebar always reflects current storage state
 *
 * Base-route behavior:
 * When a user navigates to a base route (e.g., /expenses without ?accountId param),
 * the page writes null to storage via the mount effect. On next render, this hook returns
 * the base route href, ensuring sidebar links also use the unfiltered route.
 *
 * @param feature - The feature name ('expenses' or 'incomes')
 * @returns The href string to use for the sidebar link
 */
function useFeatureFilterHref(feature: FeatureName): string {
  const userId = useUserStore(store => store.userInfo?.rowId);

  // If no user context, return base route (shouldn't happen in authenticated app)
  if (!userId) {
    return `/${feature}`;
  }

  // Build the storage key using the same utilities as the page's storage hook
  const storageKey = buildUserScopedKey({ userId, feature });

  // Read the stored filter state
  let selectedAccountId: string | null = null;
  try {
    const storedValue = sessionStorage.getItem(storageKey);
    if (storedValue) {
      const parsed = JSON.parse(storedValue);
      selectedAccountId = parsed.selectedAccountId ?? null;
    }
  } catch {
    // If storage parsing fails, fall back to base route
    selectedAccountId = null;
  }

  // Build href: if we have a valid selectedAccountId, append it as query param
  if (selectedAccountId && selectedAccountId.trim().length > 0) {
    return `/${feature}?accountId=${encodeURIComponent(selectedAccountId)}`;
  }

  // Otherwise, return base route (unfiltered)
  return `/${feature}`;
}

export { useFeatureFilterHref };
export type { FeatureName };

