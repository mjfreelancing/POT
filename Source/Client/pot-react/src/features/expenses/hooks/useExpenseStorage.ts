import {
  buildEnvScopedKey,
  buildUserScopedKey,
  purgeLegacyStorageKeys,
} from '@/concerns/storage';
import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import type { DisplayError } from '@/lib';
import useUserStore from '@/stores/useUserStore';

type ExpenseStorageData = {
  selectedAccountId: string | null;
  filterDescription: string | null;
};

const expenseStorageDefaults: ExpenseStorageData = {
  selectedAccountId: null,
  filterDescription: null,
};

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing expense storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for expense data
 */
function useExpenseStorage(onError?: StorageErrorHandler) {
  const userId = useUserStore(store => store.userInfo?.rowId);
  const storageKey = userId
    ? buildUserScopedKey({ userId, feature: 'expenses' })
    : buildEnvScopedKey('unauthenticated:expenses');

  // TEMPORARY: remove the legacy flat key now that the user is known and a scoped key is active.
  if (userId) {
    purgeLegacyStorageKeys([{ key: 'pot-expenses', storage: localStorage }]);
  }

  const { getProperty, setProperty } =
    useLocalStorageManager<ExpenseStorageData>(
      storageKey,
      onError,
      expenseStorageDefaults,
      sessionStorage,
    );

  return {
    getExpenseData: () => {
      if (!userId) {
        return expenseStorageDefaults;
      }

      return {
        selectedAccountId: getProperty('selectedAccountId'),
        filterDescription: getProperty('filterDescription'),
      };
    },

    setExpenseData: (data: Partial<ExpenseStorageData>) => {
      if (!userId) {
        return;
      }

      if (data.selectedAccountId !== undefined) {
        setProperty('selectedAccountId', data.selectedAccountId);
      }

      if (data.filterDescription !== undefined) {
        setProperty(
          'filterDescription',
          data.filterDescription === '' ? null : data.filterDescription,
        );
      }
    },
  };
}

export default useExpenseStorage;
export { expenseStorageDefaults };
export type { ExpenseStorageData, StorageErrorHandler };
