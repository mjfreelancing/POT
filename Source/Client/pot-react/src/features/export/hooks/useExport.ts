import { useApiExport } from '@/api/hooks/useExports';
import { FailResultBase, Result, SuccessResult } from '@/lib';

import { downloadBlob, extractFilename } from '../utils/fileUtils';

type ExportResult = {
  filename: string;
};

function useExport() {
  const apiExport = useApiExport();

  async function exportData(): Promise<Result<ExportResult, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiExport.mutateAsync({
        signal: controller.signal,
      });

      if (result.success) {
        const filename = extractFilename(result.value.headers);
        // const size = result.value.blob.size;

        // Wait for the file to be actually saved by the user
        await downloadBlob(result.value.blob, filename);

        return new SuccessResult<ExportResult>({
          filename,
        });
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return {
    exportData,
    isLoading: apiExport.isPending,
  };
}

export { useExport };
export type { ExportResult };
