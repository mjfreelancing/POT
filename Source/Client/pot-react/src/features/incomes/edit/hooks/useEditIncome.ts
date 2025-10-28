import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateIncome } from '@/api/hooks';
import type { EditIncome, Identity } from '@/data';
import type { FailResultBase, Result } from '@/lib';
import { useCacheInvalidation } from '@/lib';

// Hook to handle editing an existing income entry
function useEditIncome() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const apiUpdateIncome = useApiUpdateIncome();

  async function editIncome(
    id: string,
    income: EditIncome,
  ): Promise<Result<Identity, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiUpdateIncome.mutateAsync({
        id,
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
  }

  return { editIncome };
}

export default useEditIncome;
