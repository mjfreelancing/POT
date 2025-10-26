import { useQueryClient } from '@tanstack/react-query';

import { useApiCreateIncome } from '@/api/hooks';
import type { CreateIncome, Identity } from '@/data';
import type { FailResultBase, Result } from '@/lib';

function useCreateIncome() {
  const queryClient = useQueryClient();
  const apiCreateIncome = useApiCreateIncome();

  const createIncome = async (
    income: CreateIncome,
  ): Promise<Result<Identity, FailResultBase>> => {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiCreateIncome.mutateAsync({
        data: income,
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
  };

  return { createIncome };
}

export default useCreateIncome;
