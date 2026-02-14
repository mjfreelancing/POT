import type { QueryClient } from '@tanstack/react-query';

import type { useApiRenewIncomes } from '@/api/hooks';
import { invalidateCache } from '@/concerns';
import {
  type BulkActionResult,
  type RenewalModeType,
  todayIsoFormat,
} from '@/lib';

async function renewIncomes(
  incomesRowIds: string[],
  mode: RenewalModeType,
  renewIncomesMutation: ReturnType<typeof useApiRenewIncomes>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const result = await renewIncomesMutation.mutateAsync({
    data: {
      rowIds: incomesRowIds,
      mode,
      asOfDate: todayIsoFormat(),
    },
  });

  if (result.success) {
    invalidateCache(queryClient, ['incomes']);
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

export { renewIncomes };
