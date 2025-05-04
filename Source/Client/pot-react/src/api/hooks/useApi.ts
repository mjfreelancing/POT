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

// Response Interceptor
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

const responseData = <TResponse>(
  response: AxiosResponse<TResponse>,
): TResponse => {
  return response.data;
};

export const useGet = <TResponse>(url: string, queryKey: string[]) => {
  return useQuery({
    queryKey,
    queryFn: async ({ signal }) => {
      return axios.get<TResponse>(url, { signal }).then(responseData);
    },
  });
};

export const usePost = <TResponse, TData>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: {
      data: TData;
      signal?: AbortSignal;
    }): Promise<Result<TResponse, FailResultBase>> => {
      try {
        const response = await axios.post<TResponse>(url, data, { signal });
        return new SuccessResult(response.data);
      } catch (error) {
        return error as FailResult<FailResultBase>;
      }
    },
  });
};

export const usePut = <TResponse, TData>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: {
      data: TData;
      signal?: AbortSignal;
    }): Promise<Result<TResponse, FailResultBase>> => {
      try {
        const response = await axios.put<TResponse>(url, data, { signal });
        return new SuccessResult(response.data);
      } catch (error) {
        return error as FailResult<FailResultBase>;
      }
    },
  });
};

export const useDelete = <TResponse>(url: string) => {
  return useMutation({
    mutationFn: async ({ signal }: { signal?: AbortSignal }) => {
      return axios.delete<TResponse>(url, { signal }).then(responseData);
    },
  });
};
