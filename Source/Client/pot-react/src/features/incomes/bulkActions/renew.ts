import { QueryClient } from '@tanstack/react-query';

import { useApiRenewIncomes } from '@/api/hooks';
import { Income } from '@/data';
import { BulkActionResult, todayIsoFormat } from '@/lib';

async function renewIncomes(
  incomes: Income[],
  renewIncomesMutation: ReturnType<typeof useApiRenewIncomes>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const rowIds = incomes.map(income => income.rowId);
  const result = await renewIncomesMutation.mutateAsync({
    data: { rowIds, untilDate: todayIsoFormat() },
  });

  if (result.success) {
    await queryClient.invalidateQueries({ queryKey: ['incomes'] });
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

export { renewIncomes };
