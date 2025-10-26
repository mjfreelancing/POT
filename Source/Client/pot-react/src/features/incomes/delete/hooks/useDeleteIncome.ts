import { useQueryClient } from '@tanstack/react-query';

import { useApiDeleteIncome } from '@/api/hooks';
import type { FailResultBase, Result } from '@/lib';

function useDeleteIncome(rowId: string) {
  const queryClient = useQueryClient();
  const apiDeleteIncome = useApiDeleteIncome(rowId);

  async function deleteIncome(): Promise<Result<void, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiDeleteIncome.mutateAsync({
        signal: controller.signal,
      });

      if (result.success) {
        queryClient.invalidateQueries({ queryKey: ['incomes'] });
        queryClient.invalidateQueries({ queryKey: ['accounts'] });
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { deleteIncome };
}

export default useDeleteIncome;
