import { useQueryClient } from '@tanstack/react-query';

import { useApiDeleteAccount } from '@/api/hooks';
import { FailResultBase, Result } from '@/lib';

function useDeleteAccount(rowId: string) {
  const queryClient = useQueryClient();
  const apiDeleteAccount = useApiDeleteAccount(rowId);

  async function deleteAccount(): Promise<Result<void, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiDeleteAccount.mutateAsync({
        signal: controller.signal,
      });

      if (result.success) {
        queryClient.invalidateQueries({ queryKey: ['accounts'] });
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { deleteAccount };
}

export default useDeleteAccount;
