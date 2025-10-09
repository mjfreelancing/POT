import { useQueryClient } from '@tanstack/react-query';
import { BarChart3 } from 'lucide-react';
import { CheckCircle } from 'lucide-react';
import { useEffect } from 'react';
import { toast } from 'sonner';

import {
  useApiAccrueAccountExpenses,
  useApiRenewExpenses,
  useApiRenewIncomes,
} from '@/api/hooks';
import { ActionCard } from '@/components/cards';
import { SuccessToast } from '@/components/feedback/toast';
import { useErrorContext } from '@/contexts';
import { accrueAllAccountExpenses } from '@/features/accounts/utils/bulkActions';
import { useAccrualsContext } from '@/features/dashboard/contexts/AccrualsContext';
import { renewExpenses } from '@/features/expenses/bulkActions/renew';
import { renewIncomes } from '@/features/incomes/bulkActions/renew';
import { logger } from '@/lib/logging';

function RenewAccrueAllAction() {
  const {
    expenseRenewals,
    incomeRenewals,
    accountAccruals,
    isLoading,
    error: accrualsError,
    invalidate: invalidateAccrualsStatus,
  } = useAccrualsContext();

  const { setError } = useErrorContext();
  const queryClient = useQueryClient();
  const renewExpensesMutation = useApiRenewExpenses();
  const renewIncomesMutation = useApiRenewIncomes();
  const accrueAccountExpensesMutation = useApiAccrueAccountExpenses();

  // Reactively handle errors from the context, or clear any previous error
  useEffect(() => {
    setError(accrualsError);
  }, [accrualsError, setError]);

  async function performExpenseRenewals() {
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

  async function performIncomeRenewals() {
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

  async function performAccountAccruals() {
    if (accountAccruals.length > 0) {
      logger.info(
        'RenewAccrueAllAction',
        `Accruing ${accountAccruals.length} accounts`,
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

  // This method will never be called if there is an existing error since hasData will be false
  async function handleBulkAction() {
    // Need to renew Expenses and Incomes before accruing accounts
    if (!(await performExpenseRenewals())) {
      return;
    }

    if (!(await performIncomeRenewals())) {
      return;
    }

    if (!(await performAccountAccruals())) {
      return;
    }

    invalidateAccrualsStatus();

    await queryClient.invalidateQueries({ queryKey: ['projections'] });

    toast(
      () => (
        <SuccessToast
          icon={CheckCircle}
          title="Renew / Accruals Complete"
          description="All renewals and accruals have successfully processed"
        />
      ),
      { duration: 5000 },
    );
  }

  // Only enable if there's something:
  // * expense renewals (will require accruals once renewed) OR
  // * income renewals (does not affect accruals) AND accruals
  const hasData =
    expenseRenewals.length > 0 ||
    (incomeRenewals.length > 0 && accountAccruals.length > 0);

  return (
    <ActionCard
      title="Renew & Accrue All"
      description="Renew all overdue incomes and expenses, and accrue accounts"
      isLoading={isLoading}
      icon={<BarChart3 className="text-information" />}
      onClick={hasData ? handleBulkAction : undefined}
      enabled={hasData}
      hint={[
        "Renews all expenses and incomes that were due before today, updates their due dates based on the original recurrence pattern, and re-aggregates the expense's account accruals.",
        '',
        "All expenses and incomes marked as 'excluded from calculations' will not be renewed or accrued.",
      ].join('\n')}
    />
  );
}

export default RenewAccrueAllAction;
