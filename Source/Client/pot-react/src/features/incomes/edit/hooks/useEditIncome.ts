import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateIncome } from '@/api/hooks';
import { EditIncome, Identity } from '@/data';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result } from '@/lib/result/result';

// Hook to handle editing an existing income entry
function useEditIncome() {
  const queryClient = useQueryClient();
  const apiUpdateIncome = useApiUpdateIncome();

  async function editIncome(
    income: EditIncome,
  ): Promise<Result<Identity, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiUpdateIncome.mutateAsync({
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
  }

  return { editIncome };
}

export default useEditIncome;
