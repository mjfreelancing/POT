import { useQueryClient } from '@tanstack/react-query';

import { useApiDeleteUser } from '@/api/hooks';
import { logger, useCacheInvalidation } from '@/concerns';
import type { FailResultBase, Result } from '@/lib';

function useDeleteUser(rowId: string) {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const apiDeleteUser = useApiDeleteUser(rowId);

  async function deleteUser(
    password: string,
  ): Promise<Result<void, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Users', 'Delete user started', rowId);

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiDeleteUser.mutateAsync({
        data: { password },
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Users', 'Delete user successful', rowId);
        invalidateCache(['users']);
      } else {
        logger.error('Users', 'Delete user failed', result.error);
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { deleteUser };
}

export default useDeleteUser;
