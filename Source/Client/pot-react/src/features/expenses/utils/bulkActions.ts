import { QueryClient } from '@tanstack/react-query';

import { useApiRenewExpenses } from '@/api/hooks/useExpenses';
import { Expense } from '@/data';
import { BulkActionResult, todayIsoFormat } from '@/lib';

export async function renewAllExpenses(
  expenses: Expense[],
  renewExpensesMutation: ReturnType<typeof useApiRenewExpenses>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const rowIds = expenses.map(expense => expense.rowId);
  const result = await renewExpensesMutation.mutateAsync({
    data: { rowIds, untilDate: todayIsoFormat() },
  });

  if (result.success) {
    await queryClient.invalidateQueries({ queryKey: ['expenses'] });
    return { success: true };
  }

  return {
    success: false,
    error: {
      title: result.error.code,
      description: result.error.description,
    },
  };
}
