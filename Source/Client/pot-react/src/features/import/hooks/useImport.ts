import { useQueryClient } from '@tanstack/react-query';

import { useApiImport } from '@/api/hooks/useImports';
import { FailResultBase, Result, SuccessResult } from '@/lib';

type ImportResult = {
  imported: number;
};

function useImport() {
  const queryClient = useQueryClient();
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
        await queryClient.invalidateQueries({ queryKey: ['accounts'] });
        await queryClient.invalidateQueries({ queryKey: ['incomes'] });
        await queryClient.invalidateQueries({ queryKey: ['expenses'] });

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
