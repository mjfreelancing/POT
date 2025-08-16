import { QueryClient } from '@tanstack/react-query';

import { useApiAccrueExpenses } from '@/api/hooks';
import { Account } from '@/data';
import { ActionResultFail, ActionResultSuccess } from '@/lib';

type BulkActionResult = ActionResultSuccess | ActionResultFail;

async function accrueAllExpenses(
  accounts: Account[],
  accrueExpensesMutation: ReturnType<typeof useApiAccrueExpenses>,
  queryClient: QueryClient,
): Promise<BulkActionResult> {
  const rowIds = accounts.map(account => account.rowId);
  const result = await accrueExpensesMutation.mutateAsync({ data: { rowIds } });

  if (result.success) {
    await queryClient.invalidateQueries({ queryKey: ['accounts'] });
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

export { accrueAllExpenses };
export type { BulkActionResult };
