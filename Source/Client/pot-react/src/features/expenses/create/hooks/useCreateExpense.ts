import { useQueryClient } from '@tanstack/react-query';

import { useApiCreateExpense } from '@/api/hooks';
import type { CreateExpense, Identity } from '@/data';
import type { FailResultBase, Result } from '@/lib';

function useCreateExpense() {
  const queryClient = useQueryClient();
  const apiCreateExpense = useApiCreateExpense();

  const createExpense = async (
    expense: CreateExpense,
  ): Promise<Result<Identity, FailResultBase>> => {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiCreateExpense.mutateAsync({
        data: expense,
        signal: controller.signal,
      });

      if (result.success) {
        queryClient.invalidateQueries({ queryKey: ['expenses'] });
      }

      return result;
    } finally {
      controller.abort();
    }
  };

  return { createExpense };
}

export default useCreateExpense;
