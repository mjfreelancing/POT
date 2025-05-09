import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateAccount } from '@/api/accounts/hooks/useAccounts';
import { EditAccount } from '@/data/accounts/account';

export const useEditAccount = () => {
  const queryClient = useQueryClient();
  const apiUpdateAccount = useApiUpdateAccount();

  const editAccount = async (account: EditAccount) => {
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
  };

  return { editAccount };
};
