import { useQueryClient } from '@tanstack/react-query';

import { useApiDeleteAccount } from '@/api/accounts/hooks/useAccounts';

function useDeleteAccount(rowId: string) {
  const queryClient = useQueryClient();
  const apiDeleteAccount = useApiDeleteAccount(rowId);

  const deleteAccount = async () => {
    const controller = new AbortController();

    try {
      await apiDeleteAccount.mutateAsync(
        { signal: controller.signal },
        {
          onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['accounts'] });
          },
        },
      );
    } finally {
      controller.abort();
    }
  };

  return { deleteAccount };
}

export default useDeleteAccount;
