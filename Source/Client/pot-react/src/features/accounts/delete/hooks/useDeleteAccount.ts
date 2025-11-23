import { useQueryClient } from '@tanstack/react-query';

import { useApiDeleteAccount } from '@/api/hooks';
import { logger, useCacheInvalidation } from '@/concerns';
import type { FailResultBase, Result } from '@/lib';

function useDeleteAccount(rowId: string) {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const apiDeleteAccount = useApiDeleteAccount(rowId);

  async function deleteAccount(): Promise<Result<void, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Accounts', 'Delete account started', rowId);

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiDeleteAccount.mutateAsync({
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Accounts', 'Delete account successful', rowId);
        invalidateCache(['accounts']);
      } else {
        logger.error('Accounts', 'Delete account failed', result.error);
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { deleteAccount };
}

export default useDeleteAccount;
