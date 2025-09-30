import { useQueryClient } from '@tanstack/react-query';
import { BarChart3 } from 'lucide-react';
import { CheckCircle } from 'lucide-react';
import { useEffect, useMemo } from 'react';
import { toast } from 'sonner';

import {
  useApiAccrualsStatus,
  useApiAccrueAccountExpenses,
  useApiGetAllAccounts,
  useApiRenewExpenses,
  useApiRenewIncomes,
} from '@/api/hooks';
import { ActionCard } from '@/components/cards';
import { SuccessToast } from '@/components/feedback/toast';
import { useErrorContext } from '@/contexts';
import { EMPTY_ACCOUNT_ARRAY } from '@/data';
import { accrueAllAccountExpenses } from '@/features/accounts/utils/bulkActions';
import { renewExpenses } from '@/features/expenses/bulkActions/renew';
import { renewIncomes } from '@/features/incomes/bulkActions/renew';
import { EMPTY_STRING_ARRAY } from '@/lib';
import { logger } from '@/lib/logging';

function RenewAccrueAllAction() {
  const { error, setError } = useErrorContext();
  const queryClient = useQueryClient();
  const renewExpensesMutation = useApiRenewExpenses();
  const renewIncomesMutation = useApiRenewIncomes();
  const accrueAccountExpensesMutation = useApiAccrueAccountExpenses();

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
      // Expenses that need to be renewed
      expenseRenewals: accrualsStatusData?.success
        ? accrualsStatusData.value.expenseRenewalsRequired
        : EMPTY_STRING_ARRAY,

      // Incomes that need to be renewed
      incomeRenewals: accrualsStatusData?.success
        ? accrualsStatusData.value.incomeRenewalsRequired
        : EMPTY_STRING_ARRAY,

      // Accounts that need to be accrued
      accountAccruals: accrualsStatusData?.success
        ? accrualsStatusData.value.accountAccrualsRequired
        : EMPTY_STRING_ARRAY,
    }),
    [accrualsStatusData],
  );

  const isLoading = accountsIsLoading || accrualsStatusIsLoading;
  const hasData =
    expenseRenewals.length > 0 ||
    incomeRenewals.length > 0 ||
    accountAccruals.length > 0;

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

  async function handleExpenseRenewals() {
    if (expenseRenewals.length > 0) {
      logger.info(
        'RenewAccrueAllAction',
        `Renewing ${expenseRenewals.length} expenses`,
      );

      const expenseResult = await renewExpenses(
        expenseRenewals,
        renewExpensesMutation,
        queryClient,
      );

      if (!expenseResult.success) {
        setError(expenseResult.error ?? null);
        return false;
      }
    }
    return true;
  }

  async function handleIncomeRenewals() {
    if (incomeRenewals.length > 0) {
      logger.info(
        'RenewAccrueAllAction',
        `Renewing ${incomeRenewals.length} incomes`,
      );

      const incomeResult = await renewIncomes(
        incomeRenewals,
        renewIncomesMutation,
        queryClient,
      );

      if (!incomeResult.success) {
        setError(incomeResult.error ?? null);
        return false;
      }
    }
    return true;
  }

  async function handleAccountAccruals() {
    if (accountAccruals.length > 0) {
      logger.info(
        'RenewAccrueAllAction',
        `Accruing ${accounts.length} accounts`,
      );

      const accrueResult = await accrueAllAccountExpenses(
        accountAccruals,
        accrueAccountExpensesMutation,
        queryClient,
      );

      if (!accrueResult.success) {
        setError(accrueResult.error ?? null);
        return false;
      }
    }
    return true;
  }

  async function handleBulkAction() {
    setError(null);

    // Need to renew Expenses and Incomes before accruing accounts
    if (!(await handleExpenseRenewals())) {
      return;
    }

    if (!(await handleIncomeRenewals())) {
      return;
    }

    if (!(await handleAccountAccruals())) {
      return;
    }

    await queryClient.invalidateQueries({ queryKey: ['projections'] });

    toast(
      () => (
        <SuccessToast
          icon={CheckCircle}
          title="Accruals Complete"
          description="All renewals and accruals have successfully processed"
        />
      ),
      { duration: 5000 },
    );
  }

  return (
    <>
      <ActionCard
        enabled={hasData}
        isLoading={isLoading}
        title="Renew & Accrue All"
        description="Renew all incomes and expenses, and accrue all accounts"
        icon={<BarChart3 className="text-information" />}
        onClick={handleBulkAction}
      />
    </>
  );
}

export default RenewAccrueAllAction;
