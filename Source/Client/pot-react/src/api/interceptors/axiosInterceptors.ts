import axios, { AxiosError, AxiosResponse } from 'axios';

import { FailResult } from '@/lib';

import { addCorrelationId, getNetworkError } from '../apiHelpers';
import {
  ApiErrorResponse,
  getConflictMessage,
  getErrorTitle,
  getNotFoundMessage,
  getValidationMessage,
} from '../errors/apiErrorResponse';
import {
  ConflictError,
  NotFoundError,
  UnexpectedError,
  ValidationError,
} from '../errors/apiErrors';

/**
 * Response success handler that logs the response
 */
export const responseSuccessHandler = (
  response: AxiosResponse,
): AxiosResponse => {
  console.log(
    `API Response [${response.config.headers['X-Correlation-ID']}]`,
    response.data,
  );
  return response;
};

/**
 * Response error handler that converts errors to domain-specific errors
 */
export const responseErrorHandler = (error: AxiosError) => {
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

    let failResult;

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
};

/**
 * Setup axios interceptors
 */
export const setupInterceptors = () => {
  // Add request interceptor to inject correlation ID
  const requestInterceptorId = axios.interceptors.request.use(addCorrelationId);

  // Add response interceptors
  const responseInterceptorId = axios.interceptors.response.use(
    responseSuccessHandler,
    responseErrorHandler,
  );

  // Return the interceptor IDs so they can be later removed if needed
  return {
    requestInterceptorId,
    responseInterceptorId,
  };
};

/**
 * Setup default axios configuration
 */
export const setupAxiosDefaults = () => {
  axios.defaults.baseURL = 'http://localhost:5241/api';
  axios.defaults.timeout = 10000;
  // Add any other default settings here
};
