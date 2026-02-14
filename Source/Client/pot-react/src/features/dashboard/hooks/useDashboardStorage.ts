import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import type { DisplayError } from '@/lib';

const DASHBOARD_STORAGE_KEY = 'pot-dashboard';

export type PeriodDays = 7 | 14 | 30;

type DashboardStorageData = {
  quickActionsOpen: boolean | null;
  accountsOpen: boolean | null;
  incomesOpen: boolean | null;
  expensesOpen: boolean | null;
  expensesPeriod: PeriodDays | null;
  incomesPeriod: PeriodDays | null;
};

const dashboardStorageDefaults: DashboardStorageData = {
  quickActionsOpen: true,
  accountsOpen: true,
  incomesOpen: true,
  expensesOpen: true,
  expensesPeriod: 30,
  incomesPeriod: 30,
};

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing dashboard storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for dashboard section collapsed/open state
 */
function useDashboardStorage(onError?: StorageErrorHandler) {
  const { getProperty, setProperty } =
    useLocalStorageManager<DashboardStorageData>(
      DASHBOARD_STORAGE_KEY,
      onError,
      dashboardStorageDefaults,
    );

  return {
    getDashboardData: () => ({
      quickActionsOpen: getProperty('quickActionsOpen') ?? true,
      accountsOpen: getProperty('accountsOpen') ?? true,
      incomesOpen: getProperty('incomesOpen') ?? true,
      expensesOpen: getProperty('expensesOpen') ?? true,
      expensesPeriod: getProperty('expensesPeriod') ?? 30,
      incomesPeriod: getProperty('incomesPeriod') ?? 30,
    }),

    setDashboardData: (data: Partial<DashboardStorageData>) => {
      if (data.quickActionsOpen !== undefined) {
        setProperty('quickActionsOpen', data.quickActionsOpen);
      }

      if (data.accountsOpen !== undefined) {
        setProperty('accountsOpen', data.accountsOpen);
      }

      if (data.incomesOpen !== undefined) {
        setProperty('incomesOpen', data.incomesOpen);
      }

      if (data.expensesOpen !== undefined) {
        setProperty('expensesOpen', data.expensesOpen);
      }

      if (data.expensesPeriod !== undefined) {
        setProperty('expensesPeriod', data.expensesPeriod);
      }

      if (data.incomesPeriod !== undefined) {
        setProperty('incomesPeriod', data.incomesPeriod);
      }
    },
  };
}

export default useDashboardStorage;
export { dashboardStorageDefaults };
export type { DashboardStorageData, StorageErrorHandler };
