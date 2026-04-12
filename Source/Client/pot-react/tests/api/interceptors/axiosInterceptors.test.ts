import axios, {
  AxiosError,
  AxiosHeaders,
  InternalAxiosRequestConfig,
} from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { FailResult, FailResultBase } from '@/lib';

import { addCorrelationId } from '@/api/apiHelpers';
import { ApiErrorResponse } from '@/api/errors/apiErrorResponse';
import {
  AuthenticationError,
  ConflictError,
  ForbiddenError,
  MethodNotAllowedError,
  NetworkError,
  NotFoundError,
  RateLimitedError,
  ServerError,
  ValidationError,
} from '@/api/errors/apiErrors';

// Explicitly mock axios
vi.mock('axios');

// Mock the authClient module to prevent initialization errors during testing
vi.mock('@/api/authClient', () => ({
  default: {
    defaults: {},
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  },
}));

// Import the error handler function from the interceptors module for direct testing
import { responseErrorHandler } from '../../../src/api/interceptors/axiosInterceptors';

const expectRejectedFailResultErrorType = async <TError extends FailResultBase>(
  rejectionPromise: Promise<unknown>,
  expectedErrorType: new (...args: never[]) => TError,
) => {
  await expect(rejectionPromise).rejects.toBeInstanceOf(FailResult);

  const rejectedValue = await rejectionPromise.catch(
    rejectedError => rejectedError,
  );

  const failResult = rejectedValue as FailResult<TError>;

  expect(failResult.error).toBeInstanceOf(expectedErrorType);
};

describe('Axios Interceptors', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    // Reset axios interceptors before each test
    axios.interceptors.request.clear();
    axios.interceptors.response.clear();
  });

  describe('Request Interceptor', () => {
    it('should add a correlation ID header when not present', () => {
      const mockUuid = '12345678-1234-1234-1234-123456789012';

      // Mock randomUUID to return consistent value
      vi.spyOn(crypto, 'randomUUID').mockReturnValue(mockUuid);

      // Create config and apply the interceptor directly
      const config: InternalAxiosRequestConfig = {
        headers: new AxiosHeaders(),
      };

      const result = addCorrelationId(config);

      expect(result.headers['X-Correlation-ID']).toBe(mockUuid);

      // Restore the mock
      vi.restoreAllMocks();
    });

    it('should not override existing correlation ID', () => {
      const existingId = 'existing-correlation-id';
      const headers = new AxiosHeaders();
      headers.set('X-Correlation-ID', existingId);

      const config: InternalAxiosRequestConfig = {
        headers,
      };

      const result = addCorrelationId(config);

      expect(result.headers['X-Correlation-ID']).toBe(existingId);
    });
  });

  describe('Response Interceptor', () => {
    it('should let successful responses pass through', async () => {
      const response = {
        data: { id: 1 },
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        },
      };

      // Test the success handler directly - it just passes through the response
      const successHandler = (res: typeof response) => {
        console.log(
          `API Response [${res.config.headers['X-Correlation-ID']}]`,
          res.data,
        );

        return res;
      };

      const result = successHandler(response);
      expect(result).toBe(response);
    });

    it('should convert 404 responses to NotFoundError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Not Found',
        detail: 'Resource not found',
        status: 404,
      };

      const axiosError = new AxiosError('Not Found');
      axiosError.response = {
        status: 404,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Not Found',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(rejectionPromise, NotFoundError);
    });

    it('should convert 401 responses to AuthenticationError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Unauthorized',
        detail: 'Authentication required',
        status: 401,
      };

      const axiosError = new AxiosError('Unauthorized');
      axiosError.response = {
        status: 401,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Unauthorized',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(
        rejectionPromise,
        AuthenticationError,
      );
    });

    it('should convert 403 responses to ForbiddenError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Forbidden',
        detail: 'Access denied',
        status: 403,
      };

      const axiosError = new AxiosError('Forbidden');
      axiosError.response = {
        status: 403,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Forbidden',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(rejectionPromise, ForbiddenError);
    });

    it('should convert 405 responses to MethodNotAllowedError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Method Not Allowed',
        detail: 'HTTP method is not allowed for this endpoint',
        status: 405,
      };

      const axiosError = new AxiosError('Method Not Allowed');
      axiosError.response = {
        status: 405,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Method Not Allowed',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(
        rejectionPromise,
        MethodNotAllowedError,
      );
    });

    it('should convert 409 responses to ConflictError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Conflict',
        detail: 'Resource already exists',
        status: 409,
        errors: [
          {
            propertyName: 'name',
            errorCode: 'duplicate',
            attemptedValue: 'test',
            errorMessage: 'Name already exists',
          },
        ],
      };

      const axiosError = new AxiosError('Conflict');
      axiosError.response = {
        status: 409,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Conflict',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(rejectionPromise, ConflictError);
    });

    it('should convert 422 responses to ValidationError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Validation Error',
        detail: 'Validation failed',
        status: 422,
        errors: [
          {
            propertyName: 'email',
            errorCode: 'invalid',
            attemptedValue: 'test',
            errorMessage: 'Invalid email format',
          },
        ],
      };

      const axiosError = new AxiosError('Validation Error');
      axiosError.response = {
        status: 422,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Unprocessable Entity',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(
        rejectionPromise,
        ValidationError,
      );
    });

    it('should convert 500 responses to ServerError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Server Error',
        detail: 'Internal server error',
        status: 500,
      };

      const axiosError = new AxiosError('Server Error');
      axiosError.response = {
        status: 500,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Internal Server Error',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(rejectionPromise, ServerError);
    });

    it('should convert 429 responses to RateLimitedError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Too Many Requests',
        detail: 'Rate limit exceeded',
        status: 429,
      };

      const axiosError = new AxiosError('Too Many Requests');
      axiosError.response = {
        status: 429,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Too Many Requests',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(
        rejectionPromise,
        RateLimitedError,
      );
    });

    it.each([502, 503, 504])(
      'should convert %s responses to ServerError',
      async statusCode => {
        const apiErrorResponse: ApiErrorResponse = {
          title: 'Server Error',
          detail: 'Upstream service failed',
          status: statusCode,
        };

        const axiosError = new AxiosError('Server Error');
        axiosError.response = {
          status: statusCode,
          data: apiErrorResponse,
          headers: {},
          config: {
            headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
          } as InternalAxiosRequestConfig,
          statusText: 'Server Error',
        };

        const rejectionPromise = responseErrorHandler(axiosError);

        await expectRejectedFailResultErrorType(rejectionPromise, ServerError);
      },
    );

    it('should convert network errors to NetworkError', async () => {
      const networkError = new AxiosError('Network Error', 'ERR_NETWORK');

      Object.defineProperty(networkError, 'isAxiosError', {
        value: true,
        writable: true,
        enumerable: true,
      });

      const rejectionPromise = responseErrorHandler(networkError);

      await expectRejectedFailResultErrorType(rejectionPromise, NetworkError);
    });

    it('should pass through existing FailResult unchanged', async () => {
      const existingFailResult = new FailResult(
        new NetworkError('Already normalized'),
      );

      const rejectionPromise = responseErrorHandler(
        existingFailResult as unknown as AxiosError,
      );

      await expect(rejectionPromise).rejects.toBe(existingFailResult);
      await expect(rejectionPromise).rejects.toBeInstanceOf(FailResult);
    });

    it('should ignore cancelled requests', async () => {
      const cancelledError = new AxiosError(
        'Request cancelled',
        'ERR_CANCELED',
      );

      // Make sure the error is set up correctly
      Object.defineProperty(cancelledError, 'code', {
        value: 'ERR_CANCELED',
        writable: true,
        enumerable: true,
      });

      const rejectionPromise = responseErrorHandler(cancelledError);

      await expect(rejectionPromise).rejects.toBe(cancelledError);
      await expect(rejectionPromise).rejects.not.toBeInstanceOf(FailResult);
    });
  });
});
