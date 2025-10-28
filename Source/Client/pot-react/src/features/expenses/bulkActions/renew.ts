import type { QueryClient } from '@tanstack/react-query';

import type { useApiRenewExpenses } from '@/api/hooks/useExpenses';
import { type BulkActionResult, invalidateCache, todayIsoFormat } from '@/lib';

async function renewExpenses(
  expenseRowIds: string[],
  renewExpensesMutation: ReturnType<typeof useApiRenewExpenses>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const result = await renewExpensesMutation.mutateAsync({
    data: { rowIds: expenseRowIds, untilDate: todayIsoFormat() },
  });

  if (result.success) {
    invalidateCache(queryClient, ['expenses']);
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

export { renewExpenses };
