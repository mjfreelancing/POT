import axios, { AxiosError, AxiosResponse } from 'axios';

import { FailResult } from '@/lib';

import { addCorrelationId, getNetworkError } from '../apiHelpers';
import type { ApiErrorResponse } from '../errors/apiErrorResponse';
import {
  getAuthenticationMessage,
  getConflictMessage,
  getErrorTitle,
  getNotFoundMessage,
  getValidationMessage,
} from '../errors/apiErrorResponse';
import {
  AuthenticationError,
  ConflictError,
  NotFoundError,
  UnexpectedError,
  ValidationError,
} from '../errors/apiErrors';

type ApiError =
  | AuthenticationError
  | ConflictError
  | NotFoundError
  | UnexpectedError
  | ValidationError;

const responseSuccessHandler = (response: AxiosResponse): AxiosResponse => {
  console.log(
    `API Response [${response.config.headers['X-Correlation-ID']}]`,
    response.data,
  );

  return response;
};

const responseErrorHandler = async (error: AxiosError) => {
  // Ignore cancelled requests
  if (error.code === AxiosError.ERR_CANCELED) {
    return Promise.reject(error);
  }

  if (error.response) {
    const { status, data, config } = error.response;
    const apiError = data as ApiErrorResponse;

    console.error(
      `API Error [${config.headers['X-Correlation-ID']}]: ${status}`,
      apiError,
    );

    let errorResult: ApiError;

    switch (status) {
      case 404:
        errorResult = new NotFoundError(getNotFoundMessage(apiError));
        break;

      case 409:
        errorResult = new ConflictError(getConflictMessage(apiError));
        break;

      case 422:
        errorResult = new ValidationError(getValidationMessage(apiError));
        break;

      // Let auth interceptor handle 401s, but wrap any unhandled auth errors
      case 401:
        if (error.response?.data instanceof AuthenticationError) {
          errorResult = error.response.data;
        } else {
          errorResult = new AuthenticationError(
            getAuthenticationMessage(apiError),
          );
        }
        break;

      case 500:
      default:
        errorResult = new UnexpectedError(getErrorTitle(apiError));
        break;
    }

    return Promise.reject(new FailResult(errorResult));
  }

  if (error.isAxiosError) {
    console.error('API Error', error);
  }

  return Promise.reject(new FailResult(getNetworkError(error)));
};

/**
 * Setup axios interceptors
 */
const setupInterceptors = () => {
  // Add request interceptor to inject correlation ID
  const correlationInterceptorId =
    axios.interceptors.request.use(addCorrelationId);

  // Add response interceptors
  const responseInterceptorId = axios.interceptors.response.use(
    responseSuccessHandler,
    responseErrorHandler,
  );

  // Return the interceptor IDs so they can be later removed if needed
  return {
    correlationInterceptorId,
    responseInterceptorId,
  };
};

/**
 * Setup default axios configuration
 */
const setupAxiosDefaults = () => {
  axios.defaults.baseURL = import.meta.env.VITE_API_BASE_URL;
  axios.defaults.timeout = import.meta.env.VITE_API_TIMEOUT_MS;
};

export {
  responseErrorHandler,
  responseSuccessHandler,
  setupAxiosDefaults,
  setupInterceptors,
};
