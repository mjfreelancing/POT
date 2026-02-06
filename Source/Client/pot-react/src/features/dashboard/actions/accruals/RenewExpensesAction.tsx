import { useQueryClient } from '@tanstack/react-query';
import { CheckCircle } from 'lucide-react';
import { CreditCard } from 'lucide-react';
import { useEffect } from 'react';
import { toast } from 'sonner';

import { useApiRenewExpenses } from '@/api/hooks';
import { ActionCard } from '@/components/cards';
import { SuccessToast } from '@/components/feedback/toast';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import { useAccrualsContext } from '@/features/dashboard/contexts/AccrualsContext';
import { renewExpenses } from '@/features/expenses/bulkActions/renew';

function RenewExpensesAction() {
  const {
    expenseRenewals,
    isLoading,
    error: accrualsError,
    invalidate: invalidateAccrualsStatus,
  } = useAccrualsContext();

  const { error, setError } = useErrorContext();
  const queryClient = useQueryClient();
  const renewExpensesMutation = useApiRenewExpenses();

  // Only set error if we don't already have one and there's an accruals error
  useEffect(() => {
    if (accrualsError !== null && accrualsError !== undefined) {
      if (error === null) {
        setError(accrualsError);
      }
    }
  }, [accrualsError, error, setError]);

  async function performExpenseRenewals() {
    if (expenseRenewals.length > 0) {
      logger.info(
        'RenewExpensesAction',
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

  // This method will never be called if there is an existing error since hasData will be false
  async function handleBulkAction() {
    // Need to renew Expenses and Incomes before accruing accounts
    if (!(await performExpenseRenewals())) {
      return;
    }

    invalidateAccrualsStatus();

    await queryClient.invalidateQueries({ queryKey: ['projections'] });

    toast(
      () => (
        <SuccessToast
          icon={CheckCircle}
          title="Expense Renewal Complete"
          description="All expense renewals have successfully processed"
        />
      ),
      { duration: 5000 },
    );
  }

  const hasData = expenseRenewals.length > 0;

  return (
    <ActionCard
      title="Renew Expenses"
      icon={<CreditCard className="text-information" />}
      onClick={hasData ? handleBulkAction : undefined}
      enabled={hasData && !isLoading}
      hint={[
        'Renews all expenses that were due before today, updates their due dates based on the original recurrence pattern, and marking the associated account accruals as dirty.',
        '',
        "Expenses marked as 'excluded from calculations' will not be renewed.",
      ].join('\n')}
    />
  );
}

export default RenewExpensesAction;
