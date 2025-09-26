import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import { DisplayError } from '@/lib';

const EXPENSE_STORAGE_KEY = 'pot-expenses';

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
  const { getProperty, setProperty } =
    useLocalStorageManager<ExpenseStorageData>(EXPENSE_STORAGE_KEY, onError);

  return {
    getExpenseData: () => ({
      selectedAccountId: getProperty('selectedAccountId'),
      filterDescription: getProperty('filterDescription'),
    }),

    setExpenseData: (data: Partial<ExpenseStorageData>) => {
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
