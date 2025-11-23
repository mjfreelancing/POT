import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateExpense } from '@/api/hooks';
import { useCacheInvalidation } from '@/concerns';
import type { EditExpense, Identity } from '@/data';
import type { FailResultBase, Result } from '@/lib';

// Hook to handle editing an existing expense entry
function useEditExpense() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const apiUpdateExpense = useApiUpdateExpense();

  async function editExpense(
    id: string,
    expense: EditExpense,
  ): Promise<Result<Identity, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiUpdateExpense.mutateAsync({
        id,
        data: expense,
        signal: controller.signal,
      });

      if (result.success) {
        invalidateCache(['expenses']);
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { editExpense };
}

export default useEditExpense;
