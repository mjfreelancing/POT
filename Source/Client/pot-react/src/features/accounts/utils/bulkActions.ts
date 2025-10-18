import type { QueryClient } from '@tanstack/react-query';

import type { useApiAccrueAccountExpenses } from '@/api/hooks';
import type { BulkActionResult } from '@/lib';

async function accrueAllAccountExpenses(
  accountRowIds: string[],
  accrueExpensesMutation: ReturnType<typeof useApiAccrueAccountExpenses>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const result = await accrueExpensesMutation.mutateAsync({
    data: { rowIds: accountRowIds },
  });

  if (result.success) {
    await queryClient.invalidateQueries({ queryKey: ['accounts'] });
    await queryClient.invalidateQueries({ queryKey: ['expenses'] });
    await queryClient.invalidateQueries({ queryKey: ['projections'] });
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

export { accrueAllAccountExpenses };
export type { BulkActionResult };
