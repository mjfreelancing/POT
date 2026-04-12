import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

import { useApiAccrualsStatus, useApiGetAllAccounts } from '@/api/hooks';
import { logger } from '@/concerns';
import { useWindowFocus } from '@/hooks/useWindowFocus';
import { type DisplayError, EMPTY_STRING_ARRAY } from '@/lib';

type AccrualsContextProps = {
  expenseRenewals: typeof EMPTY_STRING_ARRAY;
  incomeRenewals: typeof EMPTY_STRING_ARRAY;
  accountAccruals: typeof EMPTY_STRING_ARRAY;
  isLoading: boolean;
  error: DisplayError | null;
  invalidate: () => void;
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

  const {
    data: accountsData,
    isLoading: accountsIsLoading,
    refetch: refetchAccounts,
  } = useApiGetAllAccounts();

  const accountRowIds = useMemo(
    () =>
      accountsData?.success
        ? accountsData.value.map(account => account.rowId)
        : EMPTY_STRING_ARRAY,
    [accountsData],
  );

  const {
    data: accrualsStatusData,
    isLoading: accrualsStatusIsLoading,
    refetch: refetchAccrualsStatus,
  } = useApiAccrualsStatus({ accountRowIds });

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
  }, [accountsData, accrualsStatusData, error, setError]);

  const isLoading = accountsIsLoading || accrualsStatusIsLoading;

  const invalidate = useCallback(() => {
    // Not really needed, but it's an edge case that another account could have been added by another user
    logger.info('AccrualsContext', 'Refreshing accounts');
    refetchAccounts();

    if (accountRowIds.length > 0) {
      logger.info('AccrualsContext', 'Refreshing accruals status');
      refetchAccrualsStatus();
    }
  }, [accountRowIds, refetchAccounts, refetchAccrualsStatus]);

  const refreshAccrualsOnly = useCallback(() => {
    if (accountRowIds.length > 0) {
      logger.info(
        'AccrualsContext',
        'Focus/Visibility refresh - accruals only',
      );
      refetchAccrualsStatus();
    }
  }, [accountRowIds, refetchAccrualsStatus]);

  // Refresh accruals when the window regains focus (which implies the page is visible)
  useWindowFocus(refreshAccrualsOnly);

  return (
    <AccrualsContext.Provider
      value={{
        expenseRenewals,
        incomeRenewals,
        accountAccruals,
        isLoading,
        error,
        invalidate,
      }}
    >
      {children}
    </AccrualsContext.Provider>
  );
};

export { AccrualsProvider, useAccrualsContext };
export type { AccrualsContextProps };
