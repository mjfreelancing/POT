import { useQueryClient } from '@tanstack/react-query';
import { CheckCircle } from 'lucide-react';
import { Coins } from 'lucide-react';
import { useEffect } from 'react';
import { toast } from 'sonner';

import { useApiRenewIncomes } from '@/api/hooks';
import { ActionCard } from '@/components/cards';
import { SuccessToast } from '@/components/feedback/toast';
import { useErrorContext } from '@/contexts';
import { useAccrualsContext } from '@/features/dashboard/contexts/AccrualsContext';
import { renewIncomes } from '@/features/incomes/bulkActions/renew';
import { logger } from '@/lib/logging';

function RenewIncomesAction() {
  const {
    incomeRenewals,
    isLoading,
    error: accrualsError,
    invalidate: invalidateAccrualsStatus,
  } = useAccrualsContext();

  const { setError } = useErrorContext();
  const queryClient = useQueryClient();
  const renewIncomesMutation = useApiRenewIncomes();

  // Reactively handle errors from the context, or clear any previous error
  useEffect(() => {
    setError(accrualsError);
  }, [accrualsError, setError]);

  async function performIncomeRenewals() {
    if (incomeRenewals.length > 0) {
      logger.info(
        'RenewIncomesAction',
        `Renewing ${incomeRenewals.length} incomes`,
      );

      const incomeResult = await renewIncomes(
        incomeRenewals,
        renewIncomesMutation,
        queryClient,
      );

      if (!incomeResult.success) {
        setError(incomeResult.error);
        return false;
      }
    }

    return true;
  }

  // This method will never be called if there is an existing error since hasData will be false
  async function handleBulkAction() {
    if (!(await performIncomeRenewals())) {
      return;
    }

    await queryClient.invalidateQueries({ queryKey: ['projections'] });

    invalidateAccrualsStatus();

    toast(
      () => (
        <SuccessToast
          icon={CheckCircle}
          title="Income Renewal Complete"
          description="All income renewals have successfully processed"
        />
      ),
      { duration: 5000 },
    );
  }

  const hasData = incomeRenewals.length > 0;

  return (
    <ActionCard
      title="Renew Incomes"
      description="Renew all overdue incomes"
      isLoading={isLoading}
      icon={<Coins className="text-information" />}
      onClick={hasData ? handleBulkAction : undefined}
      enabled={hasData}
      hint={[
        'Renews all incomes that were due before today and updates their due dates based on the original recurrence pattern.',
        '',
        "Incomes marked as 'excluded from calculations' will not be renewed.",
      ].join('\n')}
    />
  );
}

export default RenewIncomesAction;
