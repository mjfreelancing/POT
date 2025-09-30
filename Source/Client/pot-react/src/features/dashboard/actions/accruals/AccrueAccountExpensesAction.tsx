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
    refresh,
  } = useAccrualsContext();

  const { setError } = useErrorContext();
  const queryClient = useQueryClient();
  const accrueAccountExpensesMutation = useApiAccrueAccountExpenses();

  // Reactively handle errors from the context, or clear any previous error
  useEffect(() => {
    setError(accrualsError);
  }, [accrualsError, setError]);

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

    refresh();

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
    />
  );
}

export default AccrueAccountExpensesAction;
