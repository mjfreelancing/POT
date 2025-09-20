import { useMutation, useQuery } from '@tanstack/react-query';
import axios, { AxiosResponse } from 'axios';

import { FailResult, FailResultBase, Result, SuccessResult } from '@/lib';

type MutationData<TData> = {
  data: TData;
  signal?: AbortSignal;
};

type DeleteMutationData = {
  signal?: AbortSignal;
};

type MutationOperation<TResponse> = () => Promise<AxiosResponse<TResponse>>;

const performOperation = async <TResponse>(
  operation: MutationOperation<TResponse>,
): Promise<Result<TResponse, FailResultBase>> => {
  try {
    const response = await operation();
    return new SuccessResult(response.data);
  } catch (error) {
    return error as FailResult<FailResultBase>;
  }
};

const useDelete = <TResponse>(url: string) => {
  return useMutation({
    mutationFn: async ({
      signal,
    }: DeleteMutationData): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() => axios.delete<TResponse>(url, { signal }));
    },
  });
};

import type { UseQueryOptions } from '@tanstack/react-query';

// Options type for useGet that excludes queryKey and queryFn since they are
// handled internally by the hook. This ensures type safety while allowing
// consumers to customize other React Query options like caching and refetching.
type GetOptions<TResponse> = Omit<
  UseQueryOptions<
    Result<TResponse, FailResultBase>,
    unknown,
    Result<TResponse, FailResultBase>,
    string[]
  >,
  'queryKey' | 'queryFn'
>;

const useGet = <TResponse>(
  url: string,
  queryKey: string[],
  options: GetOptions<TResponse> = {},
) => {
  return useQuery({
    ...options,
    queryKey,
    queryFn: async ({ signal }): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() => axios.get<TResponse>(url, { signal }));
    },
    placeholderData: prev => prev,
  });
};

const usePost = <TResponse, TData>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: MutationData<TData>): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() =>
        axios.post<TResponse>(url, data, { signal }),
      );
    },
  });
};

const usePut = <TResponse, TData>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: MutationData<TData>): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() =>
        axios.put<TResponse>(url, data, { signal }),
      );
    },
  });
};

export { useDelete, useGet, usePost, usePut };
