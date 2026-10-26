import type { InternalAxiosRequestConfig } from 'axios';
import axios, { AxiosError, AxiosHeaders } from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { addCorrelationId } from '@/api/apiHelpers';
import type { ApiErrorResponse } from '@/api/errors/apiErrorResponse';
import {
  AuthenticationError,
  BadRequestError,
  ConflictError,
  ForbiddenError,
  MethodNotAllowedError,
  NetworkError,
  NotFoundError,
  RateLimitedError,
  ServerError,
  UnexpectedError,
} from '@/api/errors/apiErrors';
import type { FailResultBase } from '@/lib';
import { FailResult } from '@/lib';

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

const rejectToFailResult = async <TError extends FailResultBase>(
  rejectionPromise: Promise<unknown>,
): Promise<FailResult<TError>> => {
  await expect(rejectionPromise).rejects.toBeInstanceOf(FailResult);

  const rejectedValue = await rejectionPromise.catch(
    rejectedError => rejectedError,
  );

  return rejectedValue as FailResult<TError>;
};

const expectRejectedFailResultErrorType = async <TError extends FailResultBase>(
  rejectionPromise: Promise<unknown>,
  expectedErrorType: new (...args: never[]) => TError,
): Promise<FailResult<TError>> => {
  const failResult = await rejectToFailResult<TError>(rejectionPromise);

  expect(failResult.error).toBeInstanceOf(expectedErrorType);

  return failResult;
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

    it('should not surface the response body in a 404 message', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Not Found',
        detail: 'Resource 42 was not found',
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

      const failResult = await rejectToFailResult<NotFoundError>(
        responseErrorHandler(axiosError),
      );

      // Deliberate: a 404 must never disclose whether (or whose) row exists, so the client shows one
      // fixed message and discards the server's own detail. The cross-tenant refusals depend on that
      // silence. Do not feed apiError.detail (or errors[].errorMessage) into the 404 message.
      expect(failResult.error.description).toBe(
        'The requested resource was not found',
      );
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

    it('should use the server detail for a generic database conflict', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Conflict',
        detail: 'The request conflicts with the current state of the data.',
        status: 409,
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

      const failResult = await rejectToFailResult<ConflictError>(
        responseErrorHandler(axiosError),
      );

      expect(failResult.error.description).toBe(apiErrorResponse.detail);
    });

    it('should convert 400 responses to BadRequestError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'One or more validation errors occurred.',
        status: 400,
        errors: {
          email: ['Invalid email format'],
        },
      };

      const axiosError = new AxiosError('Validation Error');

      axiosError.response = {
        status: 400,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Bad Request',
      };

      const rejectionPromise = responseErrorHandler(axiosError);

      await expectRejectedFailResultErrorType(
        rejectionPromise,
        BadRequestError,
      );
    });

    it('should not treat 422 responses as validation errors', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Unprocessable Entity',
        detail: 'Validation failed',
        status: 422,
      };

      const axiosError = new AxiosError('Unprocessable Entity');

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
        UnexpectedError,
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
        AxiosError.ERR_CANCELED,
      );

      // Make sure the error is set up correctly
      Object.defineProperty(cancelledError, 'code', {
        value: AxiosError.ERR_CANCELED,
        writable: true,
        enumerable: true,
      });

      const rejectionPromise = responseErrorHandler(cancelledError);

      await expect(rejectionPromise).rejects.toBe(cancelledError);
      await expect(rejectionPromise).rejects.not.toBeInstanceOf(FailResult);
    });
  });

  describe('Error bodies the client cannot read', () => {
    // What a reverse proxy answers with when it cannot reach the application.
    const proxyPage =
      '<html><head><title>502 Bad Gateway</title></head><body><h1>502 Bad Gateway</h1><hr><center>nginx</center></body></html>';

    // Every shape a transport failure can hand back, each resolved to the message a caller shows.
    const unreadableBodies: [string, () => unknown][] = [
      ['an html page', () => proxyPage],
      ['an empty body', () => ''],
      ['a missing body', () => undefined],
      ['an html blob', () => new Blob([proxyPage], { type: 'text/html' })],
      ['json that is not problem shaped', () => ({ message: 'boom' })],
    ];

    const createAxiosError = (status: number, data: unknown): AxiosError => {
      const axiosError = new AxiosError(`HTTP ${status}`);

      axiosError.response = {
        status,
        data,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: String(status),
      };

      return axiosError;
    };

    const messageFor = async (
      status: number,
      data: unknown,
    ): Promise<string> => {
      const failResult = await rejectToFailResult(
        responseErrorHandler(createAxiosError(status, data)),
      );

      return failResult.error.description;
    };

    const expectMessageOnly = (message: string): void => {
      expect(message.trim()).not.toBe('');
      expect(message).not.toContain('<');
    };

    it.each(unreadableBodies)(
      'should show the validation fallback for a 400 carrying %s',
      async (_description, body) => {
        const message = await messageFor(400, body());

        expect(message).toBe('The request was not valid.');
        expectMessageOnly(message);
      },
    );

    it.each(unreadableBodies)(
      'should show the conflict fallback for a 409 carrying %s',
      async (_description, body) => {
        const message = await messageFor(409, body());

        expect(message).toBe('A conflict error occurred');
        expectMessageOnly(message);
      },
    );

    it.each(unreadableBodies)(
      'should show the server fallback for a 500 carrying %s',
      async (_description, body) => {
        const message = await messageFor(500, body());

        expect(message).toBe(
          'The server encountered an error. Please try again.',
        );
        expectMessageOnly(message);
      },
    );

    it.each(unreadableBodies)(
      'should read a 504 carrying %s as unreachable',
      async (_description, body) => {
        const message = await messageFor(504, body());

        expect(message).toBe('Unable to connect to the server.');
        expectMessageOnly(message);
      },
    );

    it.each([502, 503])(
      'should read a %s with no body as unreachable',
      async status => {
        const message = await messageFor(status, '');

        expect(message).toBe('Unable to connect to the server.');
      },
    );

    it('should read the validation messages a download brought back as a blob', async () => {
      const blob = new Blob(
        [
          JSON.stringify({
            status: 400,
            title: 'One or more validation errors occurred.',
            errors: { email: ['Invalid email format'] },
          }),
        ],
        { type: 'application/problem+json' },
      );

      const message = await messageFor(400, blob);

      expect(message).toBe('Invalid email format');
    });

    it('should read a stale-tag conflict that arrives as a blob', async () => {
      const blob = new Blob(
        [
          JSON.stringify({
            status: 409,
            type: 'urn:pot:problem:stale-entity-tag',
            detail:
              'The entity tag for the Account does not match the current record',
          }),
        ],
        { type: 'application/problem+json' },
      );

      const message = await messageFor(409, blob);

      expect(message).toBe(
        'A conflicting update has been performed by another user. Refresh and try again.',
      );
    });

    it.each([
      [401, 'Invalid username or password'],
      [403, 'You do not have permission to access this resource'],
      [404, 'The requested resource was not found'],
      [405, 'The requested method is not allowed for this resource'],
      [429, 'Too many requests. Please wait a moment and try again.'],
    ])(
      'should keep the fixed copy for a %s whatever the body is',
      async (status, expected) => {
        const message = await messageFor(status, proxyPage);

        expect(message).toBe(expected);
        expectMessageOnly(message);
      },
    );
  });
});
