import { useMutation, useQuery } from '@tanstack/react-query';
import axios, { AxiosError, AxiosResponse } from 'axios';

import { FailResultBase } from '@/lib/result/failResultBase';
import { FailResult, Result, SuccessResult } from '@/lib/result/result';

import {
  ApiErrorResponse,
  getConflictMessage,
  getErrorTitle,
  getNotFoundMessage,
  getValidationMessage,
} from '../errors/apiErrorResponse';
import {
  ConflictError,
  NetworkError,
  NotFoundError,
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

const getNetworkErrorMessage = (error: AxiosError): string => {
  switch (error.code) {
    case AxiosError.ECONNABORTED:
    case AxiosError.ETIMEDOUT:
      return 'Request timed out while waiting for the server';

    case AxiosError.ERR_NETWORK:
      return 'Unable to connect to the server';

    case AxiosError.ERR_BAD_REQUEST:
      return 'Invalid request';

    case AxiosError.ERR_BAD_RESPONSE:
      return 'Server returned an invalid response';

    case AxiosError.ERR_NOT_SUPPORT:
      return 'Operation not supported';

    case AxiosError.ERR_INVALID_URL:
      return 'Invalid server URL';

    case AxiosError.ERR_FR_TOO_MANY_REDIRECTS:
      return 'Too many redirects';

    default:
      return `Network error: ${error.message}`;
  }
};

const getNetworkError = (error: AxiosError): NetworkError => {
  const message = getNetworkErrorMessage(error);
  const code = error.code ?? 'UNKNOWN';

  return new NetworkError(`${message} (${code})`);
};

// Add request interceptor to inject correlation ID
axios.interceptors.request.use(config => {
  config.headers['X-Correlation-ID'] = crypto.randomUUID();

  console.log(
    `API ${config.method?.toUpperCase()} Request [${config.headers['X-Correlation-ID']}]`,
    config.url,
  );

  return config;
});

axios.interceptors.response.use(
  response => {
    console.log(
      `API Response [${response.config.headers['X-Correlation-ID']}]`,
      response.data,
    );
    return response;
  },
  (error: AxiosError) => {
    // Ignore cancelled requests
    if (error.code === AxiosError.ERR_CANCELED) {
      return Promise.reject(error);
    }

    if (error.response) {
      const { status, data, config } = error.response;
      const apiError = data as ApiErrorResponse;

      console.error(
        `API Error [${config.headers['X-Correlation-ID']}]: ${status} ${apiError.detail}`,
        apiError,
      );

      let failResult: FailResultBase;

      switch (status) {
        case 400:
          failResult = new NotFoundError(getNotFoundMessage(apiError));
          break;

        case 409:
          failResult = new ConflictError(getConflictMessage(apiError));
          break;

        case 422:
          failResult = new ValidationError(getValidationMessage(apiError));
          break;

        case 500:
        default:
          failResult = new UnexpectedError(getErrorTitle(apiError));
          break;
      }

      return Promise.reject(new FailResult(failResult));
    }

    if (error.isAxiosError) {
      console.error('API Error', error);
    }

    return Promise.reject(new FailResult(getNetworkError(error)));
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
