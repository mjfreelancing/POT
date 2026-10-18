import { useApiDeleteSite } from '@/api/hooks';
import { logger } from '@/concerns';
import type { DeleteSite } from '@/data/site';
import type { FailResultBase, Result } from '@/lib';

function useDeleteCurrentSite() {
  const apiDeleteSite = useApiDeleteSite();

  async function deleteCurrentSite(
    data: DeleteSite,
  ): Promise<Result<void, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Settings', 'Delete current site started');

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiDeleteSite.mutateAsync({
        data,
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Settings', 'Delete current site successful');
      } else {
        logger.error('Settings', 'Delete current site failed', result.error);
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return { deleteCurrentSite };
}

export default useDeleteCurrentSite;
