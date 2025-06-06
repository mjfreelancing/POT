import { useQueryClient } from '@tanstack/react-query';

import { useApiCreateIncome } from '@/api/incomes/hooks/useIncomes';
import { Identity } from '@/data/identity';
import { CreateIncome } from '@/data/incomes/income';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result } from '@/lib/result/result';

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
      }

      return result;
    } finally {
      controller.abort();
    }
  };

  return { createIncome };
}

export default useCreateIncome;
