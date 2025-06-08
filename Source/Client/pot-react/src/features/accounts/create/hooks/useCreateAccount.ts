import { useQueryClient } from '@tanstack/react-query';

import { useApiCreateAccount } from '@/api/hooks';
import { CreateAccount, Identity } from '@/data';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result } from '@/lib/result/result';

function useCreateAccount() {
  const queryClient = useQueryClient();
  const apiCreateAccount = useApiCreateAccount();

  const createAccount = async (
    account: CreateAccount,
  ): Promise<Result<Identity, FailResultBase>> => {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
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
}

export default useCreateAccount;
