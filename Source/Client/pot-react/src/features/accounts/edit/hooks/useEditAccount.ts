import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateAccount } from '@/api/accounts/hooks/useAccounts';
import { EditAccount, Identity } from '@/data';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result } from '@/lib/result/result';

function useEditAccount() {
  const queryClient = useQueryClient();
  const apiUpdateAccount = useApiUpdateAccount();

  async function editAccount(
    account: EditAccount,
  ): Promise<Result<Identity, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiUpdateAccount.mutateAsync({
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
  }

  return { editAccount };
}

export default useEditAccount;
