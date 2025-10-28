import { useQueryClient } from '@tanstack/react-query';

import { useApiCreateAccount } from '@/api/hooks';
import type { CreateAccount, Identity } from '@/data';
import type { FailResultBase, Result } from '@/lib';
import { logger, useCacheInvalidation } from '@/lib';

function useCreateAccount() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const apiCreateAccount = useApiCreateAccount();

  const createAccount = async (
    account: CreateAccount,
  ): Promise<Result<Identity, FailResultBase>> => {
    const controller = new AbortController();

    logger.info('Accounts', 'Create account started', account);
    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiCreateAccount.mutateAsync({
        data: account,
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Accounts', 'Create account successful', result.value);
        invalidateCache(['accounts']);
      } else {
        logger.error('Accounts', 'Create account failed', result.error);
      }

      return result;
    } finally {
      controller.abort();
    }
  };

  return { createAccount };
}

export default useCreateAccount;
