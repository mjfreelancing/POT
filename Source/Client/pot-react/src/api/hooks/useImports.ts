import { useMutation } from '@tanstack/react-query';
import axios from 'axios';

import type { FailResult, FailResultBase, Result } from '@/lib';
import { SuccessResult } from '@/lib';

type ImportApiResult = {
  imported: number;
};

/**
 * Custom import API hook
 *
 * Note: Cannot use the generic usePost() hook because:
 * 1. Requires FormData instead of JSON payload
 * 2. Needs multipart/form-data Content-Type header
 * 3. File parameter type differs from generic TData pattern
 */
function useApiImport() {
  return useMutation({
    mutationFn: async ({
      file,
      signal,
    }: {
      file: File;
      signal?: AbortSignal;
    }): Promise<Result<ImportApiResult, FailResultBase>> => {
      try {
        const formData = new FormData();
        formData.append('file', file);

        const response = await axios.post<ImportApiResult>(
          '/maintenance/import',
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data',
              'export-public-key': import.meta.env.VITE_EXPORT_PUBLIC_KEY,
            },
            signal,
          },
        );

        return new SuccessResult(response.data);
      } catch (error) {
        return error as FailResult<FailResultBase>;
      }
    },
  });
}

export { useApiImport };
export type { ImportApiResult };
