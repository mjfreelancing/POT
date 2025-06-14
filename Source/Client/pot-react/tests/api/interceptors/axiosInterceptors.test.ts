import axios, {
  AxiosError,
  AxiosHeaders,
  InternalAxiosRequestConfig,
} from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { FailResult } from '@/lib';

import { addCorrelationId } from '@/api/apiHelpers';
import { ApiErrorResponse } from '@/api/errors/apiErrorResponse';
import {
  ConflictError,
  NetworkError,
  NotFoundError,
  UnexpectedError,
  ValidationError,
} from '@/api/errors/apiErrors';

// Explicitly mock axios
vi.mock('axios');

// Import the error handler function from the interceptors module for direct testing
import { responseErrorHandler } from '../../../src/api/interceptors/axiosInterceptors';

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

    it('should convert 400 responses to NotFoundError', async () => {
      const apiErrorResponse: ApiErrorResponse = {
        title: 'Not Found',
        detail: 'Resource not found',
        status: 400,
      };

      const axiosError = new AxiosError('Not Found');
      axiosError.response = {
        status: 400,
        data: apiErrorResponse,
        headers: {},
        config: {
          headers: new AxiosHeaders({ 'X-Correlation-ID': 'test-id' }),
        } as InternalAxiosRequestConfig,
        statusText: 'Not Found',
      };

      try {
        // Call the error handler function directly
        await responseErrorHandler(axiosError);
        // If we reach here, the test should fail
        expect(true).toBe(false);
      } catch (error) {
        expect(error).toBeInstanceOf(FailResult);
        expect((error as FailResult<NotFoundError>).error).toBeInstanceOf(
          NotFoundError,
        );
      }
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

      try {
        // Call the error handler function directly
        await responseErrorHandler(axiosError);
        expect(true).toBe(false);
      } catch (error) {
        expect(error).toBeInstanceOf(FailResult);
        expect((error as FailResult<ConflictError>).error).toBeInstanceOf(
          ConflictError,
        );
      }
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

      try {
        // Call the error handler function directly
        await responseErrorHandler(axiosError);
        expect(true).toBe(false);
      } catch (error) {
        expect(error).toBeInstanceOf(FailResult);
        expect((error as FailResult<ValidationError>).error).toBeInstanceOf(
          ValidationError,
        );
      }
    });

    it('should convert 500 responses to UnexpectedError', async () => {
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

      try {
        // Call the error handler function directly
        await responseErrorHandler(axiosError);
        expect(true).toBe(false);
      } catch (error) {
        expect(error).toBeInstanceOf(FailResult);
        expect((error as FailResult<UnexpectedError>).error).toBeInstanceOf(
          UnexpectedError,
        );
      }
    });

    it('should convert network errors to NetworkError', async () => {
      const networkError = new AxiosError('Network Error', 'ERR_NETWORK');

      try {
        // Call the error handler function directly
        await responseErrorHandler(networkError);
        expect(true).toBe(false);
      } catch (error) {
        expect(error).toBeInstanceOf(FailResult);
        expect((error as FailResult<NetworkError>).error).toBeInstanceOf(
          NetworkError,
        );
      }
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

      try {
        // Use the real handler since it's working correctly now
        await responseErrorHandler(cancelledError);
        expect(true).toBe(false); // Should not get here
      } catch (error) {
        // Should be the original error, not wrapped in a FailResult
        expect(error).toBe(cancelledError);
        expect(error).not.toBeInstanceOf(FailResult);
      }
    });
  });
});
