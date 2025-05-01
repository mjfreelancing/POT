import { useQueryClient } from '@tanstack/react-query';
import { useDeleteAccount as useApiDeleteAccount } from '@/api/accounts/hooks/useAccounts';

export const useDeleteAccount = (rowId: string) => {
  const queryClient = useQueryClient();
  const apiDeleteAccount = useApiDeleteAccount(rowId);

  const deleteAccount = async () => {
    await apiDeleteAccount.mutateAsync(
      {},
      {
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: ['accounts'] });
        },
      },
    );
  };

  return { deleteAccount };
};
