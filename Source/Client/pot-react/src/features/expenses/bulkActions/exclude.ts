import type { QueryClient } from '@tanstack/react-query';

import type { useApiToggleExcludeExpenses } from '@/api/hooks/useExpenses';
import type { Expense } from '@/data';
import type { BulkActionResult } from '@/lib';

async function toggleExcludeExpenses(
  expenses: Expense[],
  excludeExpensesMutation: ReturnType<typeof useApiToggleExcludeExpenses>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const rowIds = expenses.map(expense => expense.rowId);
  const result = await excludeExpensesMutation.mutateAsync({
    data: { rowIds },
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

export { toggleExcludeExpenses };
