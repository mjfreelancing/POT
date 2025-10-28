import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateAccount } from '@/api/hooks';
import type { EditAccount, Identity } from '@/data';
import type { FailResultBase, Result } from '@/lib';
import { logger, useCacheInvalidation } from '@/lib';

function useEditAccount() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const apiUpdateAccount = useApiUpdateAccount();

  async function editAccount(
    id: string,
    account: EditAccount,
  ): Promise<Result<Identity, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Accounts', 'Edit account started', account);
    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiUpdateAccount.mutateAsync({
        id,
        data: account,
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Accounts', 'Edit account successful', result.value);
        invalidateCache(['accounts']);
      } else {
        logger.error('Accounts', 'Edit account failed', result.error);
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { editAccount };
}

export default useEditAccount;
