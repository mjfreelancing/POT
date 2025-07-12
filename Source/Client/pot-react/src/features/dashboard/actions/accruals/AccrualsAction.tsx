import { useQueryClient } from '@tanstack/react-query';
import { BarChart3 } from 'lucide-react';
import { CheckCircle } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import {
  useApiAccrueExpenses,
  useApiGetAllAccounts,
  useApiGetAllExpenses,
  useApiGetAllIncomes,
  useApiRenewExpenses,
  useApiRenewIncomes,
} from '@/api/hooks';
import { ActionCard } from '@/components/cards';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { accrueAllExpenses } from '@/features/accounts/utils/bulkActions';
import { renewAllExpenses } from '@/features/expenses/utils/bulkActions';
import { renewAllIncomes } from '@/features/incomes/utils/bulkActions';
import type { DisplayError } from '@/lib';

function AccrualsAction() {
  const queryClient = useQueryClient();
  const { data: accountsResult, isLoading: accountsLoading } =
    useApiGetAllAccounts();
  const { data: expensesResult, isLoading: expensesLoading } =
    useApiGetAllExpenses();
  const { data: incomesResult, isLoading: incomesLoading } =
    useApiGetAllIncomes();
  const renewExpensesMutation = useApiRenewExpenses();
  const renewIncomesMutation = useApiRenewIncomes();
  const accrueExpensesMutation = useApiAccrueExpenses();
  const [error, setError] = useState<DisplayError | null>(null);

  const isLoading = accountsLoading || expensesLoading || incomesLoading;

  async function handleBulkAction() {
    setError(null);

    const accounts = accountsResult?.success ? accountsResult.value : [];
    const expenses = expensesResult?.success
      ? expensesResult.value.results
      : [];
    const incomes = incomesResult?.success ? incomesResult.value.results : [];

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
      <div className="flex items-start">
        <CheckCircle className="text-green-600 mr-6 w-16 h-16" />
        <div>
          <div className="font-semibold">Accruals Complete</div>
          <div className="mt-2 text-sm text-muted-foreground">
            All renewals and accruals have successfully processed
          </div>
        </div>
      </div>,
      { duration: 5000 },
    );
  }

  return (
    <>
      <ActionCard
        title="Renew & Accrue All"
        description="Renew all incomes and expenses, and accrue all expenses for each account"
        icon={<BarChart3 className="text-sky-500" />}
        onClick={handleBulkAction}
      />

      <LoadingMessage isLoading={isLoading} />

      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </>
  );
}

export default AccrualsAction;
