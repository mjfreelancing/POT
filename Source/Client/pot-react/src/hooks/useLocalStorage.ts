import { logger } from '@/concerns';
import { type DisplayError,getErrorMessage } from '@/lib';

type LocalStorageProps<T> = {
  key: string;
  defaultValue?: T;
  onError?: (error: DisplayError) => void;
};

function getDisplayError(error: unknown): DisplayError {
  return {
    title: 'Local Storage Error',
    description: getErrorMessage(error),
  };
}

/**
 * Type-safe localStorage hook with error handling.
 *
 * IMPORTANT: Callers must provide the FULL storage key including the 'pot-' prefix.
 * This hook does NOT automatically add the prefix.
 *
 * Why callers provide the full key:
 * ---------------------------------
 * Several parts of the application access localStorage directly without using this hook,
 * making centralized prefix management impractical:
 *
 * 1. authTokenProvider.ts - Created before React mounts, used by Axios interceptors
 *    - Must work outside React component lifecycle (no hooks allowed)
 *    - Direct localStorage access: localStorage.getItem('pot-auth')
 *
 * 2. Zustand persist middleware - Operates outside component lifecycle
 *    - Third-party middleware that directly accesses localStorage
 *    - Configuration: { name: 'pot-user' }
 *
 * 3. ThemeProvider.tsx - Simple implementation with direct localStorage access
 *    - Uses 'app-ui-theme' (different prefix convention)
 *
 * Attempting to centralize the prefix would create a split architecture where:
 * - Some code looks for 'pot-projections' (prefix in constant)
 * - Other code looks for 'projections' (prefix added by hook)
 * This would cause data access failures and confusion.
 *
 * By keeping the prefix in the caller's constant, all systems (hook-based and direct)
 * use the same key consistently.
 *
 * Example usage:
 * ```typescript
 * const PROJECTION_STORAGE_KEY = 'pot-projections'; // Full key with prefix
 * const { getItem, setItem } = useLocalStorage<ProjectionStorageData>({
 *   key: PROJECTION_STORAGE_KEY,
 *   defaultValue: projectionStorageDefaults,
 *   onError,
 * });
 * ```
 */
const useLocalStorage = <T = Record<string, unknown>>({
  key,
  defaultValue,
  onError,
}: LocalStorageProps<T>) => {
  const setItem = (value: T) => {
    try {
      window.localStorage.setItem(key, JSON.stringify(value));
    } catch (error) {
      logger.error(
        'useLocalStorage',
        `Error setting item with key "${key}"`,
        error,
      );

      onError?.(getDisplayError(error));
    }
  };

  const getItem = (): T | undefined => {
    try {
      const item = window.localStorage.getItem(key);

      return item ? JSON.parse(item) : defaultValue;
    } catch (error) {
      logger.error(
        'useLocalStorage',
        `Error getting item with key "${key}"`,
        error,
      );

      onError?.(getDisplayError(error));
      return undefined;
    }
  };

  const removeItem = () => {
    try {
      window.localStorage.removeItem(key);
    } catch (error) {
      logger.error(
        'useLocalStorage',
        `Error removing item with key "${key}"`,
        error,
      );

      onError?.(getDisplayError(error));
    }
  };

  return { setItem, getItem, removeItem };
};

export default useLocalStorage;
export type { LocalStorageProps };

