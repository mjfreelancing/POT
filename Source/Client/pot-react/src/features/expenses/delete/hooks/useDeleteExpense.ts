import { useQueryClient } from '@tanstack/react-query';

import { useApiDeleteExpense } from '@/api/hooks';
import { FailResultBase, Result } from '@/lib';

function useDeleteExpense(rowId: string) {
  const queryClient = useQueryClient();
  const apiDeleteExpense = useApiDeleteExpense(rowId);

  async function deleteExpense(): Promise<Result<void, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiDeleteExpense.mutateAsync({
        signal: controller.signal,
      });

      if (result.success) {
        queryClient.invalidateQueries({ queryKey: ['expenses'] });
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { deleteExpense };
}

export default useDeleteExpense;
