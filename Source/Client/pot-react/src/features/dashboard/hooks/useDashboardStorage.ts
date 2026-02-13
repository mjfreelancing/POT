import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import type { DisplayError } from '@/lib';

const DASHBOARD_STORAGE_KEY = 'pot-dashboard';

type DashboardStorageData = {
  quickActionsOpen: boolean | null;
  accountsOpen: boolean | null;
  incomesOpen: boolean | null;
  expensesOpen: boolean | null;
};

const dashboardStorageDefaults: DashboardStorageData = {
  quickActionsOpen: true,
  accountsOpen: true,
  incomesOpen: true,
  expensesOpen: true,
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
    },
  };
}

export default useDashboardStorage;
export { dashboardStorageDefaults };
export type { DashboardStorageData, StorageErrorHandler };
