import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import { DisplayError } from '@/lib';

const INCOME_STORAGE_KEY = 'pot-incomes';

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
  const { getProperty, setProperty } =
    useLocalStorageManager<IncomeStorageData>(INCOME_STORAGE_KEY, onError);

  return {
    getIncomeData: () => ({
      selectedAccountId: getProperty('selectedAccountId'),
      filterDescription: getProperty('filterDescription'),
    }),

    setIncomeData: (data: Partial<IncomeStorageData>) => {
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
