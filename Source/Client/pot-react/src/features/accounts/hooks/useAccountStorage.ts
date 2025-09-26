import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import { DisplayError } from '@/lib';

type AccountStorageData = {
  filterDescription: string | null;
};

const ACCOUNT_STORAGE_KEY = 'pot-accounts';

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
  const { getProperty, setProperty } =
    useLocalStorageManager<AccountStorageData>(ACCOUNT_STORAGE_KEY, onError);

  return {
    getAccountData: () => ({
      filterDescription: getProperty('filterDescription'),
    }),

    setAccountData: (data: Partial<AccountStorageData>) => {
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
