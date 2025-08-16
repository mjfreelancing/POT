import { useQueryClient } from '@tanstack/react-query';
import { BarChart3 } from 'lucide-react';
import { CheckCircle } from 'lucide-react';
import { toast } from 'sonner';

import {
  useApiAccrueExpenses,
  useApiRenewExpenses,
  useApiRenewIncomes,
} from '@/api/hooks';
import { ActionCard } from '@/components/cards';
import { SuccessToast } from '@/components/feedback/toast';
import { accrueAllExpenses } from '@/features/accounts/utils/bulkActions';
import { useDashboardContext } from '@/features/dashboard/context';
import { renewAllExpenses } from '@/features/expenses/utils/bulkActions';
import { renewAllIncomes } from '@/features/incomes/utils/bulkActions';

function AccrualsAction() {
  const {
    accounts,
    expenses,
    incomes,
    accountsIsLoading,
    expensesIsLoading,
    incomesIsLoading,
    setError, // propagated back to the DashboardPage
  } = useDashboardContext();

  const queryClient = useQueryClient();
  const renewExpensesMutation = useApiRenewExpenses();
  const renewIncomesMutation = useApiRenewIncomes();
  const accrueExpensesMutation = useApiAccrueExpenses();

  const isLoading = accountsIsLoading || expensesIsLoading || incomesIsLoading;

  async function handleBulkAction() {
    setError(null);

    // Need to renew Expenses and Incomes before accruing accounts

    const expenseResult = await renewAllExpenses(
      expenses,
      renewExpensesMutation,
      queryClient,
    );

    if (!expenseResult.success) {
      setError(expenseResult.error ?? null);
      return;
    }

    const incomeResult = await renewAllIncomes(
      incomes,
      renewIncomesMutation,
      queryClient,
    );

    if (!incomeResult.success) {
      setError(incomeResult.error ?? null);
      return;
    }

    const accrueResult = await accrueAllExpenses(
      accounts,
      accrueExpensesMutation,
      queryClient,
    );

    if (!accrueResult.success) {
      setError(accrueResult.error ?? null);
      return;
    }

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
        title="Renew & Accrue All"
        description="Renew all incomes and expenses, and accrue all accounts"
        isLoading={isLoading}
        icon={<BarChart3 className="text-information" />}
        onClick={handleBulkAction}
      />
    </>
  );
}

export default AccrualsAction;
