import {
  buildEnvScopedKey,
  buildUserScopedKey,
  purgeLegacyStorageKeys,
} from '@/concerns/storage';
import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import type { DisplayError } from '@/lib';
import useUserStore from '@/stores/useUserStore';

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

const dashboardResolvedDefaults = {
  quickActionsOpen: true,
  accountsOpen: true,
  incomesOpen: true,
  expensesOpen: true,
  expensesPeriod: 30 as PeriodDays,
  incomesPeriod: 30 as PeriodDays,
};

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing dashboard storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for dashboard section collapsed/open state
 */
function useDashboardStorage(onError?: StorageErrorHandler) {
  const userId = useUserStore(store => store.userInfo?.rowId);
  const storageKey = userId
    ? buildUserScopedKey({ userId, feature: 'dashboard' })
    : buildEnvScopedKey('unauthenticated:dashboard');

  // TEMPORARY: remove the legacy flat key now that the user is known and a scoped key is active.
  if (userId) {
    purgeLegacyStorageKeys([{ key: 'pot-dashboard', storage: localStorage }]);
  }

  const { getProperty, setProperty } =
    useLocalStorageManager<DashboardStorageData>(
      storageKey,
      onError,
      dashboardStorageDefaults,
    );

  return {
    getDashboardData: (): {
      quickActionsOpen: boolean;
      accountsOpen: boolean;
      incomesOpen: boolean;
      expensesOpen: boolean;
      expensesPeriod: PeriodDays;
      incomesPeriod: PeriodDays;
    } => {
      if (!userId) {
        return dashboardResolvedDefaults;
      }

      return {
        quickActionsOpen: (getProperty('quickActionsOpen') ??
          dashboardResolvedDefaults.quickActionsOpen) as boolean,
        accountsOpen: (getProperty('accountsOpen') ??
          dashboardResolvedDefaults.accountsOpen) as boolean,
        incomesOpen: (getProperty('incomesOpen') ??
          dashboardResolvedDefaults.incomesOpen) as boolean,
        expensesOpen: (getProperty('expensesOpen') ??
          dashboardResolvedDefaults.expensesOpen) as boolean,
        expensesPeriod: (getProperty('expensesPeriod') ??
          dashboardResolvedDefaults.expensesPeriod) as PeriodDays,
        incomesPeriod: (getProperty('incomesPeriod') ??
          dashboardResolvedDefaults.incomesPeriod) as PeriodDays,
      };
    },

    setDashboardData: (data: Partial<DashboardStorageData>) => {
      if (!userId) {
        return;
      }

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
