import { useApiImport, ImportApiResult } from '@/api/hooks/useImports';
import { FailResultBase, Result, SuccessResult } from '@/lib';

type ImportResult = {
  imported: number;
};

function useImport() {
  const apiImport = useApiImport();

  async function importData(
    file: File,
  ): Promise<Result<ImportResult, FailResultBase>> {
    const controller = new AbortController();

    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiImport.mutateAsync({
        file,
        signal: controller.signal,
      });

      if (result.success) {
        return new SuccessResult<ImportResult>({
          ...result.value,
        });
      }

      return result;
    } finally {
      controller.abort();
    }
  }

  return {
    importData,
    isLoading: apiImport.isPending,
  };
}

export { useImport };
export type { ImportResult };
