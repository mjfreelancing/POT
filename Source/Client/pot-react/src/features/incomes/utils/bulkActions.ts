import { QueryClient } from '@tanstack/react-query';

import { useApiRenewIncomes } from '@/api/hooks';
import { Income } from '@/data';
import { BulkActionResult } from '@/lib';

export async function renewAllIncomes(
  incomes: Income[],
  renewIncomesMutation: ReturnType<typeof useApiRenewIncomes>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const rowIds = incomes.map(income => income.rowId);
  const result = await renewIncomesMutation.mutateAsync({ data: { rowIds } });

  if (result.success) {
    await queryClient.invalidateQueries({ queryKey: ['incomes'] });
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
