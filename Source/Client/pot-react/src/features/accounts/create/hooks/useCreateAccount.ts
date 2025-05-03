import { useQueryClient } from '@tanstack/react-query';
import { useApiCreateAccount } from '@/api/accounts/hooks/useAccounts';
import { CreateAccount } from '@/data/accounts/account';

export const useCreateAccount = () => {
  const queryClient = useQueryClient();
  const apiCreateAccount = useApiCreateAccount();

  const createAccount = async (account: CreateAccount) => {
    const controller = new AbortController();

    try {
      await apiCreateAccount.mutateAsync(
        {
          data: account,
          signal: controller.signal,
        },
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

  return { createAccount };
};
