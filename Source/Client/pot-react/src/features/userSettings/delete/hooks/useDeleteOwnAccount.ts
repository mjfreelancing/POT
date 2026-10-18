import { useApiDeleteMe } from '@/api/hooks';
import { logger } from '@/concerns';
import type { DeleteOwnAccount } from '@/data/user';
import type { FailResultBase, Result } from '@/lib';

function useDeleteOwnAccount() {
  const apiDeleteMe = useApiDeleteMe();

  async function deleteOwnAccount(
    data: DeleteOwnAccount,
  ): Promise<Result<void, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Settings', 'Delete own account started');

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiDeleteMe.mutateAsync({
        data,
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Settings', 'Delete own account successful');
      } else {
        logger.error('Settings', 'Delete own account failed', result.error);
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { deleteOwnAccount };
}

export default useDeleteOwnAccount;
