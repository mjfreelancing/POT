/**
 * This file implements Axios interceptors for handling API responses and errors.
 *
 * Key Implementation Notes:
 * 1. Error Flow:
 *    - HTTP errors first hit the responseErrorHandler
 *    - They are transformed into domain-specific errors (BadRequestError, ForbiddenError, etc.)
 *    - These are wrapped in a FailResult and rejected
 *    - Due to Axios Promise chains, the error handler may be called twice:
 *      a) First with the HTTP error
 *      b) Then with our wrapped FailResult
 *
 * 2. Error Processing:
 *    - Cancelled requests (ERR_CANCELED) are passed through unchanged
 *    - HTTP errors (error.response exists) are mapped to domain errors
 *    - Network errors (error.isAxiosError) get wrapped as NetworkError
 *    - FailResults are passed through unchanged to avoid double-wrapping
 *    - Unknown errors become UnexpectedError
 *
 * 3. Debug Considerations:
 *    - If you see an error processed twice, this is normal due to Promise chains
 *    - Check the error.response for HTTP errors
 *    - Check error.isAxiosError for network-level issues
 *    - Check instanceof FailResult for our domain errors
 *
 * 4. Common Issues:
 *    - "Unexpected Error" means an HTTP status code not explicitly handled by the switch
 *    - "Server Error" means 500/502/503/504:
 *      - 502/503/504 with no readable problem body (a proxy answered but the application did not):
 *        "Unable to connect to the server."
 *      - Any other 5xx, or one whose body carries a message: the server's detail/title
 *    - The body is read once, at this boundary: a proxy page, an empty body, JSON that is not
 *      problem shaped or a blob of either counts as no body at all, so no markup becomes a message
 *    - "Network Error" means no HTTP response at all (true connection failure, timeout, CORS)
 *    - Double-processing is normal and handled by the FailResult check
 *    - An empty apiError means the body could not be read - the fixed copy for the status applies
 *
 * 5. Future Considerations:
 *    - When adding new error types:
 *      a) Add the error class to apiErrors.ts
 *      b) Add the status code case here
 *      c) Add the message generator in apiErrorResponse.ts
 *    - Keep error transformation centralized in this interceptor
 *    - Maintain the pattern of wrapping all errors in FailResult
 */

import type { AxiosResponse } from 'axios';
import axios, { AxiosError } from 'axios';

import { logger } from '@/concerns';
import { FailResult } from '@/lib';

import { addCorrelationId, getNetworkError } from '../apiHelpers';
import type { ApiError } from '../errors';
import {
  AuthenticationError,
  BadRequestError,
  ConflictError,
  ForbiddenError,
  getAuthenticationMessage,
  getBadRequestMessage,
  getConflictMessage,
  getErrorTitle,
  getForbiddenMessage,
  getMethodNotAllowedMessage,
  getNotFoundMessage,
  getRateLimitedMessage,
  getServerErrorMessage,
  getUnreachableMessage,
  MethodNotAllowedError,
  NotFoundError,
  RateLimitedError,
  readApiErrorResponse,
  ServerError,
  UnexpectedError,
} from '../errors';

const responseSuccessHandler = (response: AxiosResponse): AxiosResponse => {
  const correlationId = response.config.headers?.['X-Correlation-ID'];

  logger.info(
    'API',
    `Response: [${response.status}] ${response.config.url} [Correlation-ID: ${correlationId}]`,
    response.data,
  );

  return response;
};

/**
 * Handles all response errors from Axios requests.
 * This is called in two scenarios:
 * 1. For HTTP errors (status codes 4xx/5xx)
 * 2. For previously transformed errors (FailResult instances)
 *
 * The error parameter could be:
 * - AxiosError with response (HTTP errors)
 * - AxiosError without response (network errors)
 * - FailResult (our transformed errors)
 * - Unknown error type (caught by final fallback)
 */
const responseErrorHandler = async (error: AxiosError) => {
  const correlationId = error.config?.headers?.['X-Correlation-ID'];

  // Handle cancelled requests (e.g., from React Query unmounts or cache invalidations)
  // These should pass through unchanged to avoid masking intentional cancellations
  if (error.code === AxiosError.ERR_CANCELED) {
    logger.info(
      'API',
      `Request cancelled [${error.config?.url}] [Correlation-ID: ${correlationId}]`,
    );

    return Promise.reject(error);
  }

  // If this is already a FailResult from a previous error transformation, return it directly
  // This prevents double-wrapping and allows proper error propagation from auth interceptor
  if (error instanceof FailResult) {
    logger.info(
      'API',
      `FailResult passthrough [${error.error.title}]`,
      error.error,
    );

    return Promise.reject(error);
  }

  // Handle HTTP errors with response data
  // These are mapped to specific domain errors based on status code
  // The body is read before the message is chosen: whatever axios handed back - a problem body, a
  // proxy page, a blob, nothing at all - is reduced once to the members that can be trusted
  if (error.response) {
    const { status, data } = error.response;
    const apiError = await readApiErrorResponse(data);

    logger.error(
      'API',
      `HTTP Error: [${status}] ${error.config?.url} [Correlation-ID: ${correlationId}]`,
      error,
    );

    let errorResult: ApiError;

    switch (status) {
      case 400:
        errorResult = new BadRequestError(getBadRequestMessage(apiError));
        break;

      case 401:
        errorResult = new AuthenticationError(
          getAuthenticationMessage(apiError),
        );
        break;

      case 403:
        errorResult = new ForbiddenError(getForbiddenMessage(apiError));
        break;

      case 404:
        errorResult = new NotFoundError(getNotFoundMessage(apiError));
        break;

      case 405:
        errorResult = new MethodNotAllowedError(
          getMethodNotAllowedMessage(apiError),
        );
        break;

      case 409:
        errorResult = new ConflictError(getConflictMessage(apiError));
        break;

      case 429:
        errorResult = new RateLimitedError(getRateLimitedMessage(apiError));
        break;

      case 500:
        errorResult = new ServerError(getServerErrorMessage(apiError));
        break;

      case 502:
      case 503:
      case 504:
        // Whatever answered, the request never reached the application.
        errorResult = new ServerError(getUnreachableMessage(apiError));
        break;

      default:
        // True unexpected errors - status codes we don't explicitly handle
        errorResult = new UnexpectedError(getErrorTitle(apiError));
        break;
    }

    const failResult = new FailResult(errorResult);
    return Promise.reject(failResult);
  }

  // Handle network-level errors (CORS, timeout, no connection, etc.)
  // These don't have response data but are marked as Axios errors
  if (error.isAxiosError) {
    logger.error(
      'API',
      `Network error: ${error.config?.url} [Correlation-ID: ${correlationId}]`,
      error,
    );

    return Promise.reject(new FailResult(getNetworkError(error)));
  }

  // Last resort - unexpected error
  logger.error('API', 'Unexpected error in responseErrorHandler', error);

  return Promise.reject(
    new FailResult(new UnexpectedError('An unexpected error occurred')),
  );
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
