import { useQueryClient } from '@tanstack/react-query';

import { useApiImport } from '@/api/hooks/useImports';
import { logger, useCacheInvalidation } from '@/concerns';
import type { FailResultBase, Result } from '@/lib';
import { SuccessResult } from '@/lib';

type ImportResult = {
  imported: number;
};

function useImport() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const apiImport = useApiImport();

  async function importData(
    file: File,
  ): Promise<Result<ImportResult, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Import', 'Import started', file.name);
    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiImport.mutateAsync({
        file,
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Import', 'Import successful', {
          imported: result.value.imported,
        });
        invalidateCache(['expenses', 'incomes', 'accounts']);

        return new SuccessResult<ImportResult>({
          ...result.value,
        });
      }

      logger.error('Import', 'Import failed', result.error);
      return result;
    } catch (error) {
      logger.error('Import', 'Import error', error);
      throw error;
    } finally {
      controller.abort();
    }
  }

  return {
    importData,
    isPending: apiImport.isPending,
  };
}

export { useImport };
export type { ImportResult };
