import React, {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

import { useApiAccrualsStatus, useApiGetAllAccounts } from '@/api/hooks';
import { EMPTY_ACCOUNT_ARRAY } from '@/data';
import { DisplayError, EMPTY_STRING_ARRAY } from '@/lib/types';

type AccrualsContextProps = {
  accounts: typeof EMPTY_ACCOUNT_ARRAY;
  expenseRenewals: typeof EMPTY_STRING_ARRAY;
  incomeRenewals: typeof EMPTY_STRING_ARRAY;
  accountAccruals: typeof EMPTY_STRING_ARRAY;
  isLoading: boolean;
  hasData: boolean;
  error: DisplayError | null;
};

const AccrualsContext = createContext<AccrualsContextProps | undefined>(
  undefined,
);

const useAccrualsContext = (): AccrualsContextProps => {
  const context = useContext(AccrualsContext);

  if (!context) {
    throw new Error(
      'useAccrualsContext must be used within an AccrualsProvider',
    );
  }

  return context;
};

const AccrualsProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [error, setError] = useState<DisplayError | null>(null);

  const { data: accountsData, isLoading: accountsIsLoading } =
    useApiGetAllAccounts();

  const accounts = useMemo(
    () => (accountsData?.success ? accountsData.value : EMPTY_ACCOUNT_ARRAY),
    [accountsData],
  );

  const { data: accrualsStatusData, isLoading: accrualsStatusIsLoading } =
    useApiAccrualsStatus({ accountRowIds: accounts.map(a => a.rowId) });

  const { expenseRenewals, incomeRenewals, accountAccruals } = useMemo(
    () => ({
      expenseRenewals: accrualsStatusData?.success
        ? accrualsStatusData.value.expenseRenewalsRequired
        : EMPTY_STRING_ARRAY,

      incomeRenewals: accrualsStatusData?.success
        ? accrualsStatusData.value.incomeRenewalsRequired
        : EMPTY_STRING_ARRAY,

      accountAccruals: accrualsStatusData?.success
        ? accrualsStatusData.value.accountAccrualsRequired
        : EMPTY_STRING_ARRAY,
    }),
    [accrualsStatusData],
  );

  useEffect(() => {
    // Only set error if we don't already have one to prevent infinite loops
    if (error === null) {
      // Check results in order of importance
      if (accountsData?.success === false) {
        setError({
          title: accountsData.error.code,
          description: accountsData.error.description,
        });
      } else if (accrualsStatusData?.success === false) {
        setError({
          title: accrualsStatusData.error.code,
          description: accrualsStatusData.error.description,
        });
      }
    }
  }, [accountsData, accrualsStatusData, error]);

  const isLoading = accountsIsLoading || accrualsStatusIsLoading;

  const hasData =
    error === null &&
    (expenseRenewals.length > 0 ||
      incomeRenewals.length > 0 ||
      accountAccruals.length > 0);

  return (
    <AccrualsContext.Provider
      value={{
        accounts,
        expenseRenewals,
        incomeRenewals,
        accountAccruals,
        isLoading,
        hasData,
        error,
      }}
    >
      {children}
    </AccrualsContext.Provider>
  );
};

export type { AccrualsContextProps };
export { AccrualsProvider, useAccrualsContext };
