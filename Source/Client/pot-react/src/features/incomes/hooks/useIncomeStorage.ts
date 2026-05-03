import {
  buildEnvScopedKey,
  buildUserScopedKey,
  purgeLegacyStorageKeys,
} from '@/concerns/storage';
import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import type { DisplayError } from '@/lib';
import useUserStore from '@/stores/useUserStore';

type IncomeStorageData = {
  selectedAccountId: string | null;
  filterDescription: string | null;
};

const incomeStorageDefaults: IncomeStorageData = {
  selectedAccountId: null,
  filterDescription: null,
};

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing income storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for income data
 */
function useIncomeStorage(onError?: StorageErrorHandler) {
  const userId = useUserStore(store => store.userInfo?.rowId);
  const storageKey = userId
    ? buildUserScopedKey({ userId, feature: 'incomes' })
    : buildEnvScopedKey('unauthenticated:incomes');

  // TEMPORARY: remove the legacy flat key now that the user is known and a scoped key is active.
  if (userId) {
    purgeLegacyStorageKeys([{ key: 'pot-incomes', storage: localStorage }]);
  }

  const { getProperty, setProperty } =
    useLocalStorageManager<IncomeStorageData>(
      storageKey,
      onError,
      incomeStorageDefaults,
      sessionStorage,
    );

  return {
    getIncomeData: () => {
      if (!userId) {
        return incomeStorageDefaults;
      }

      return {
        selectedAccountId: getProperty('selectedAccountId'),
        filterDescription: getProperty('filterDescription'),
      };
    },

    setIncomeData: (data: Partial<IncomeStorageData>) => {
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

export default useIncomeStorage;
export { incomeStorageDefaults };
export type { IncomeStorageData, StorageErrorHandler };
