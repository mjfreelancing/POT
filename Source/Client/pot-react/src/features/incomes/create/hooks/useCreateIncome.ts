import { useQueryClient } from '@tanstack/react-query';

import { useApiCreateIncome } from '@/api/hooks';
import { useCacheInvalidation } from '@/concerns';
import type { CreateIncome, Identity } from '@/data';
import type { FailResultBase, Result } from '@/lib';

function useCreateIncome() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
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
        invalidateCache(['incomes']);
      }

      return result;
    } finally {
      controller.abort();
    }
  };

  return { createIncome };
}

export default useCreateIncome;
