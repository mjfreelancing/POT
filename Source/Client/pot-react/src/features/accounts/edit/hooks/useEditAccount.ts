import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateAccount } from '@/api/accounts/hooks/useAccounts';
import { EditAccount } from '@/data/accounts/account';

export const useEditAccount = () => {
  const queryClient = useQueryClient();
  const apiUpdateAccount = useApiUpdateAccount();

  const editAccount = async (account: EditAccount) => {
    const controller = new AbortController();

    try {
      await apiUpdateAccount.mutateAsync(
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

  return { editAccount };
};
