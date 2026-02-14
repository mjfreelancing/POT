import type { QueryClient } from '@tanstack/react-query';

import type { useApiRenewExpenses } from '@/api/hooks/useExpenses';
import { invalidateCache } from '@/concerns';
import {
  type BulkActionResult,
  type RenewalModeType,
  todayIsoFormat,
} from '@/lib';

async function renewExpenses(
  expenseRowIds: string[],
  mode: RenewalModeType,
  renewExpensesMutation: ReturnType<typeof useApiRenewExpenses>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const result = await renewExpensesMutation.mutateAsync({
    data: {
      rowIds: expenseRowIds,
      mode,
      asOfDate: todayIsoFormat(),
    },
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
