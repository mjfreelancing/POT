import {
  buildEnvScopedKey,
  buildUserScopedKey,
  purgeLegacyStorageKeys,
} from '@/concerns/storage';
import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import type { DisplayError } from '@/lib';
import useUserStore from '@/stores/useUserStore';

type AccountStorageData = {
  filterDescription: string | null;
};

const accountStorageDefaults: AccountStorageData = {
  filterDescription: null,
};

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing account storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for account data
 */
function useAccountStorage(onError?: StorageErrorHandler) {
  const userId = useUserStore(store => store.userInfo?.rowId);
  const storageKey = userId
    ? buildUserScopedKey({ userId, feature: 'accounts' })
    : buildEnvScopedKey('unauthenticated:accounts');

  // TEMPORARY: remove the legacy flat key now that the user is known and a scoped key is active.
  if (userId) {
    purgeLegacyStorageKeys([{ key: 'pot-accounts', storage: localStorage }]);
  }

  const { getProperty, setProperty } =
    useLocalStorageManager<AccountStorageData>(
      storageKey,
      onError,
      accountStorageDefaults,
      sessionStorage,
    );

  return {
    getAccountData: () => {
      if (!userId) {
        return accountStorageDefaults;
      }

      return {
        filterDescription: getProperty('filterDescription'),
      };
    },

    setAccountData: (data: Partial<AccountStorageData>) => {
      if (!userId) {
        return;
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

export default useAccountStorage;
export { accountStorageDefaults };
export type { AccountStorageData, StorageErrorHandler };
