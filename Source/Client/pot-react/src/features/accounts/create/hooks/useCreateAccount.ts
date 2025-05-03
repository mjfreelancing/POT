import { useQueryClient } from '@tanstack/react-query';

import { useApiCreateAccount } from '@/api/accounts/hooks/useAccounts';
import { CreateAccount } from '@/data/accounts/account';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result } from '@/lib/result/result';

export const useCreateAccount = () => {
  const queryClient = useQueryClient();
  const apiCreateAccount = useApiCreateAccount();

  const createAccount = async (
    account: CreateAccount,
  ): Promise<Result<void, FailResultBase>> => {
    const controller = new AbortController();

    try {
      const result = await apiCreateAccount.mutateAsync({
        data: account,
        signal: controller.signal,
      });

      if (result.success) {
        queryClient.invalidateQueries({ queryKey: ['accounts'] });
      }

      return result;
    } finally {
      controller.abort();
    }
  };

  return { createAccount };
};
