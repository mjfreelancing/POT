import { useMutation, useQuery } from '@tanstack/react-query';
import axios, { AxiosResponse } from 'axios';

import { FailResult, FailResultBase, Result, SuccessResult } from '@/lib';

import {
  setupAxiosDefaults,
  setupInterceptors,
} from '../interceptors/axiosInterceptors';

type MutationData<TData> = {
  data: TData;
  signal?: AbortSignal;
};

type DeleteMutationData = {
  signal?: AbortSignal;
};

// Set up axios defaults and interceptors
setupAxiosDefaults();
setupInterceptors();

const performOperation = async <TResponse>(
  operation: () => Promise<AxiosResponse<TResponse>>,
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

const useGet = <TResponse>(url: string, queryKey: string[]) => {
  return useQuery({
    queryKey,
    queryFn: async ({ signal }): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() => axios.get<TResponse>(url, { signal }));
    },
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
