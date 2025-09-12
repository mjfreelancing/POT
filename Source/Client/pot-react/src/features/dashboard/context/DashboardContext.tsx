import {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useState,
} from 'react';

import {
  useApiGetAllAccounts,
  useApiGetAllExpenses,
  useApiGetAllIncomes,
} from '@/api/hooks';
import { Account } from '@/data/account';
import { Expense } from '@/data/expense';
import { Income } from '@/data/income';
import type { DisplayError } from '@/lib';

type DashboardContextType = {
  accounts: Account[];
  expenses: Expense[];
  incomes: Income[];
  accountsIsLoading: boolean;
  expensesIsLoading: boolean;
  incomesIsLoading: boolean;
  error: DisplayError | null;
  setError: (error: DisplayError | null) => void;
};

const DashboardContext = createContext<DashboardContextType | undefined>(
  undefined,
);

type DashboardProviderProps = {
  children: ReactNode;
};

function DashboardProvider({ children }: DashboardProviderProps) {
  const [error, setError] = useState<DisplayError | null>(null);

  const { data: accountsResult, isLoading: accountsIsLoading } =
    useApiGetAllAccounts();

  const { data: expensesResult, isLoading: expensesIsLoading } =
    useApiGetAllExpenses();

  const { data: incomesResult, isLoading: incomesIsLoading } =
    useApiGetAllIncomes();

  useEffect(() => {
    // Only set error if we don't already have one to prevent infinite loops
    if (error === null) {
      // Check results in order of importance
      if (accountsResult?.success === false) {
        setError({
          title: accountsResult.error.code,
          description: accountsResult.error.description,
        });
      } else if (expensesResult?.success === false) {
        setError({
          title: expensesResult.error.code,
          description: expensesResult.error.description,
        });
      } else if (incomesResult?.success === false) {
        setError({
          title: incomesResult.error.code,
          description: incomesResult.error.description,
        });
      }
    }
  }, [accountsResult, expensesResult, incomesResult, error]);

  const accounts = accountsResult?.success ? accountsResult.value : [];
  const expenses = expensesResult?.success ? expensesResult.value.results : [];
  const incomes = incomesResult?.success ? incomesResult.value : [];

  const context: DashboardContextType = {
    accounts,
    expenses,
    incomes,
    accountsIsLoading,
    expensesIsLoading,
    incomesIsLoading,
    error,
    setError,
  };

  return (
    <DashboardContext.Provider value={context}>
      {children}
    </DashboardContext.Provider>
  );
}

function useDashboardContext(): DashboardContextType {
  const context = useContext(DashboardContext);

  if (context === undefined) {
    throw new Error(
      'useDashboardContext must be used within a DashboardProvider',
    );
  }

  return context;
}

export { DashboardProvider, useDashboardContext };
export type { DashboardContextType };
