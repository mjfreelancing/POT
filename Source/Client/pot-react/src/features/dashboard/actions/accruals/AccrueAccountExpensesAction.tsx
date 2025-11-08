import { useQueryClient } from '@tanstack/react-query';
import { CheckCircle } from 'lucide-react';
import { Wallet } from 'lucide-react';
import { useEffect } from 'react';
import { toast } from 'sonner';

import { useApiAccrueAccountExpenses } from '@/api/hooks';
import { ActionCard } from '@/components/cards';
import { SuccessToast } from '@/components/feedback/toast';
import { useErrorContext } from '@/contexts';
import { accrueAllAccountExpenses } from '@/features/accounts/utils/bulkActions';
import { useAccrualsContext } from '@/features/dashboard/contexts/AccrualsContext';
import { logger } from '@/lib/logging';

function AccrueAccountExpensesAction() {
  const {
    accountAccruals,
    isLoading,
    error: accrualsError,
    invalidate: invalidateAccrualsStatus,
  } = useAccrualsContext();

  const { error, setError } = useErrorContext();
  const queryClient = useQueryClient();
  const accrueAccountExpensesMutation = useApiAccrueAccountExpenses();

  // Only set error if we don't already have one and there's an accruals error
  useEffect(() => {
    if (accrualsError !== null && accrualsError !== undefined) {
      if (error === null) {
        setError(accrualsError);
      }
    }
  }, [accrualsError, error, setError]);

  async function performAccountAccruals() {
    if (accountAccruals.length > 0) {
      logger.info(
        'AccrueAccountExpensesAction',
        `Accruing ${accountAccruals.length} accounts`,
      );

      const accrueResult = await accrueAllAccountExpenses(
        accountAccruals,
        accrueAccountExpensesMutation,
        queryClient,
      );

      if (!accrueResult.success) {
        setError(accrueResult.error);
        return false;
      }
    }

    return true;
  }

  // This method will never be called if there is an existing error since hasData will be false
  async function handleBulkAction() {
    if (!(await performAccountAccruals())) {
      return;
    }

    await queryClient.invalidateQueries({ queryKey: ['projections'] });

    invalidateAccrualsStatus();

    toast(
      () => (
        <SuccessToast
          icon={CheckCircle}
          title="Account Accruals Complete"
          description="All account accruals have successfully processed"
        />
      ),
      { duration: 5000 },
    );
  }

  const hasData = accountAccruals.length > 0;

  return (
    <ActionCard
      title="Accrue Account Expenses"
      description="Accrue all account expenses"
      isLoading={isLoading}
      icon={<Wallet className="text-information" />}
      onClick={hasData ? handleBulkAction : undefined}
      enabled={hasData}
      hint="Recalculates the expense accruals (aggregated to their associated account) based on their current due dates, even if they are in the past."
    />
  );
}

export default AccrueAccountExpensesAction;
