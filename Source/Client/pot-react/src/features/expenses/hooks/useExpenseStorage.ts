import useLocalStorage from '@/hooks/useLocalStorage';
import { DisplayError } from '@/lib';

type ExpenseStorageData = {
  selectedAccountId: string | null;
};

// Default - no default filter
const expenseStorageDefaults: ExpenseStorageData = {
  selectedAccountId: null,
};

const EXPENSE_STORAGE_KEY = 'pot-expenses';

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing expense storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for expense data
 */
function useExpenseStorage(onError?: StorageErrorHandler) {
  const { getItem, setItem } = useLocalStorage<ExpenseStorageData>({
    key: EXPENSE_STORAGE_KEY,
    initialValue: expenseStorageDefaults,
    onError: onError,
  });

  return {
    getExpenseData: getItem,
    setExpenseData: setItem,
  };
}

export default useExpenseStorage;
export { expenseStorageDefaults };
export type { ExpenseStorageData, StorageErrorHandler };
