import type { QueryClient } from '@tanstack/react-query';

import type { useApiToggleExcludeIncomes } from '@/api/hooks/useIncomes';
import type { Income } from '@/data';
import { invalidateCache, type BulkActionResult } from '@/lib';

async function toggleExcludeIncomes(
  incomes: Income[],
  excludeIncomesMutation: ReturnType<typeof useApiToggleExcludeIncomes>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const rowIds = incomes.map(income => income.rowId);
  const result = await excludeIncomesMutation.mutateAsync({
    data: { rowIds },
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

export { toggleExcludeIncomes };
