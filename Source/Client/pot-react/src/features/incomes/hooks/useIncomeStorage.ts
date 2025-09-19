import useLocalStorage from '@/hooks/useLocalStorage';
import { DisplayError } from '@/lib';

type IncomeStorageData = {
  selectedAccountId: string | null;
};

// Default - no default filter
const incomeStorageDefaults: IncomeStorageData = {
  selectedAccountId: null,
};

const INCOME_STORAGE_KEY = 'pot-incomes';

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing income storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for income data
 */
function useIncomeStorage(onError?: StorageErrorHandler) {
  const { getItem, setItem } = useLocalStorage<IncomeStorageData>({
    key: INCOME_STORAGE_KEY,
    initialValue: incomeStorageDefaults,
    onError: onError,
  });

  return {
    getIncomeData: getItem,
    setIncomeData: setItem,
  };
}

export default useIncomeStorage;
export { incomeStorageDefaults };
export type { IncomeStorageData, StorageErrorHandler };
