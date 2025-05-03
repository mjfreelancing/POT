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

// // Response Interceptor
// axios.interceptors.response.use(
//   res => res,
//   (error: AxiosError) => {
//     if (error.response) {
//       const { data, status } = error.response;
//       switch (status) {
//         case 400:
//           console.error(data);
//           break;
//         case 401:
//           console.error('Unauthorized');
//           break;
//         case 404:
//           console.error('Not Found');
//           break;
//         case 500:
//           console.error('Server Error');
//           break;
//       }
//     }
//     return Promise.reject(error);
//   },
// );

const responseData = <TResponse>(
  response: AxiosResponse<TResponse>,
): TResponse => {
  return response.data;
};

const getApiError = (error: AxiosError): FailResultBase => {
  if (error.response) {
    const { status, data } = error.response;
    const apiError = data as ApiErrorResponse;

    console.error(`API Error: ${status} ${apiError.detail}`, apiError);

    switch (status) {
      case 409:
        return new ConflictError(getConflictMessage(apiError));

      case 422:
        return new ValidationError(getValidationMessage(apiError));

      default:
        return new UnexpectedError(getErrorMessage(apiError));
    }
  }

  return new UnexpectedError(error.message);
};

const getFailResult = (error: unknown): FailResult<FailResultBase> => {
  if (error instanceof AxiosError) {
    return new FailResult(getApiError(error));
  }

  return new FailResult(new UnexpectedError('An unexpected error occurred'));
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
        return getFailResult(error);
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
        return getFailResult(error);
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
