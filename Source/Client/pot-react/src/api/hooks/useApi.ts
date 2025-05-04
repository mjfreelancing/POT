import { useMutation, useQuery } from '@tanstack/react-query';
import axios, { AxiosError, AxiosResponse } from 'axios';

import { FailResultBase } from '@/lib/result/failResultBase';
import { FailResult, Result, SuccessResult } from '@/lib/result/result';

import {
  ApiErrorResponse,
  getConflictMessage,
  getErrorMessage,
  getValidationMessage,
} from '../errors/apiErrorResponse';
import {
  ConflictError,
  UnexpectedError,
  ValidationError,
} from '../errors/apiErrors';

type MutationData<TData> = {
  data: TData;
  signal?: AbortSignal;
};

type DeleteMutationData = {
  signal?: AbortSignal;
};

axios.defaults.baseURL = 'http://localhost:5241/api';
axios.defaults.timeout = 3000;

// axios.defaults.headers.common['Authorization'] = 'Bearer your-token';

// // Request Interceptor
// axios.interceptors.request.use((config: AxiosRequestConfig) => {
//   if (token) {
//     config.headers = config.headers || {}; // Ensure headers object exists
//     config.headers.Authorization = `Bearer ${token}`;
//   }
//   return config;
// });

axios.interceptors.response.use(
  response => response,
  (error: AxiosError) => {
    if (error.response) {
      const { status, data } = error.response;
      const apiError = data as ApiErrorResponse;

      console.error(`API Error: ${status} ${apiError.detail}`, apiError);

      let failResult: FailResultBase;

      switch (status) {
        case 409:
          failResult = new ConflictError(getConflictMessage(apiError));
          break;

        case 422:
          failResult = new ValidationError(getValidationMessage(apiError));
          break;

        case 500:
        default:
          failResult = new UnexpectedError(getErrorMessage(apiError));
          break;
      }

      return Promise.reject(new FailResult(failResult));
    }

    // Handle cases where no response was received from the server:
    // - Network errors (no internet, DNS failures, server unreachable)
    // - Request timeout (exceeding axios.defaults.timeout)
    // - Request cancellation (via AbortController)
    return Promise.reject(
      new FailResult(new UnexpectedError('An unexpected error occurred')),
    );
  },
);

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
