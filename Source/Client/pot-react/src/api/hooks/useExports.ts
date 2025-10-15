import { useMutation } from '@tanstack/react-query';
import type { AxiosResponse } from 'axios';
import axios from 'axios';

import type { FailResult, FailResultBase, Result } from '@/lib';
import { SuccessResult } from '@/lib';

type ExportApiResponse = {
  blob: Blob;
  headers: Record<string, string>;
};

/**
 * Custom export API hook
 *
 * Note: Cannot use the generic useGet() hook because:
 * 1. Requires responseType: 'blob' for file downloads
 * 2. Needs both response.data (blob) AND response.headers
 * 3. Uses useMutation (action trigger) instead of useQuery (data fetching)
 * 4. Generic performOperation() only returns response.data, not headers
 */
function useApiExport() {
  return useMutation({
    mutationFn: async ({
      signal,
    }: {
      signal?: AbortSignal;
    } = {}): Promise<Result<ExportApiResponse, FailResultBase>> => {
      try {
        const response: AxiosResponse<Blob> = await axios.get(
          '/maintenance/export',
          {
            responseType: 'blob',
            signal,
            headers: {
              'export-public-key': import.meta.env.VITE_EXPORT_PUBLIC_KEY,
            },
          },
        );

        return new SuccessResult({
          blob: response.data,
          headers: response.headers as Record<string, string>,
        });
      } catch (error) {
        return error as FailResult<FailResultBase>;
      }
    },
  });
}

export { useApiExport };
export type { ExportApiResponse };
