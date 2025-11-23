import { useApiExport } from '@/api/hooks/useExports';
import { logger } from '@/concerns';
import type { FailResultBase, Result } from '@/lib';
import { SuccessResult } from '@/lib';

import { downloadBlob, extractFilename } from '../utils/fileUtils';

type ExportResult = {
  filename: string;
};

function useExport() {
  const apiExport = useApiExport();

  async function exportData(): Promise<Result<ExportResult, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Export', 'Export started');
    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiExport.mutateAsync({
        signal: controller.signal,
      });

      if (result.success) {
        const filename = extractFilename(result.value.headers);
        logger.info('Export', 'Export successful', filename);
        // const size = result.value.blob.size;

        // Wait for the file to be actually saved by the user
        await downloadBlob(result.value.blob, filename);

        return new SuccessResult<ExportResult>({
          filename,
        });
      }

      logger.error('Export', 'Export failed', result.error);
      return result;
    } catch (error) {
      logger.error('Export', 'Export error', error);
      throw error;
    } finally {
      controller.abort();
    }
  }

  return {
    exportData,
    isPending: apiExport.isPending,
  };
}

export { useExport };
export type { ExportResult };
