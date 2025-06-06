import { useQueryClient } from '@tanstack/react-query';

import { useApiDeleteIncome } from '@/api/incomes/hooks/useIncomes';

function useDeleteIncome(rowId: string) {
  const queryClient = useQueryClient();
  const apiDeleteIncome = useApiDeleteIncome(rowId);

  const deleteIncome = async () => {
    const controller = new AbortController();

    try {
      await apiDeleteIncome.mutateAsync(
        { signal: controller.signal },
        {
          onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['incomes'] });
          },
        },
      );
    } finally {
      controller.abort();
    }
  };

  return { deleteIncome };
}

export default useDeleteIncome;
