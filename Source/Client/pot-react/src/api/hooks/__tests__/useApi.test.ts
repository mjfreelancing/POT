import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';
import axios, { InternalAxiosRequestConfig } from 'axios';
import React, { act } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import {
  ConflictError,
  NetworkError,
  NotFoundError,
  UnexpectedError,
  ValidationError,
} from '@/api/errors/apiErrors';
import { FailResultBase } from '@/lib/result/failResultBase';
import { FailResult, SuccessResult } from '@/lib/result/result';

import { useDelete, useGet, usePost, usePut } from '../useApi';

// Mock axios instead of using vitest-mock-axios
vi.mock('axios');

// Mock the apiHelpers module
vi.mock('../apiHelpers', () => ({
  addCorrelationId: (config: InternalAxiosRequestConfig) => {
    config.headers = config.headers || {};
    config.headers['X-Correlation-ID'] = 'test-correlation-id';
    return config;
  },
  getNetworkError: () => new NetworkError('Network error (MOCKED)'),
}));

// Create a wrapper for the QueryClientProvider
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  // Replace the wrapper function with a more explicit approach
  const WrapperComponent = ({ children }: { children: React.ReactNode }) => {
    return React.createElement(
      QueryClientProvider,
      { client: queryClient },
      children,
    );
  };

  return WrapperComponent;
};

const expectSuccessResult = <T>(
  result: SuccessResult<T> | FailResult<FailResultBase>,
  expected: T,
) => {
  expect(result).toBeInstanceOf(SuccessResult);
  expect(result.success).toBe(true);
  if (result.success) {
    // type narrowed
    expect(result.value).toEqual(expected);
  }
};

const expectFailResult = <E extends FailResultBase>(
  result: SuccessResult<unknown> | FailResult<E>,
  errorType: new (description: string) => E,
) => {
  expect(result).toBeInstanceOf(FailResult);
  expect(result.success).toBe(false);
  if (!result.success) {
    // type narrowed
    expect(result.error).toBeInstanceOf(errorType);
  }
};

describe('useApi hooks', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('useGet', () => {
    it('should return successful response data', async () => {
      const responseData = { id: 1, name: 'Test Item' };

      // Mock axios.get to resolve with our test data
      vi.mocked(axios.get).mockResolvedValueOnce({ data: responseData });

      const { result } = renderHook(
        () => useGet<typeof responseData>('/test', ['test']),
        { wrapper: createWrapper() },
      );

      expect(result.current.isLoading).toBe(true);

      // Wait for the query to finish
      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      // Verify axios.get was called correctly
      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());

      // Use the helper function instead of multiple assertions
      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle network errors', async () => {
      // Create a network error
      // const networkError = new AxiosError('Network Error', 'ERR_NETWORK');

      // Mock axios.get to reject with a FailResult directly
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new NetworkError('Network error (MOCKED)')),
      );

      const { result } = renderHook(() => useGet<unknown>('/test', ['test']), {
        wrapper: createWrapper(),
      });

      // Wait for the query to finish and data to be populated
      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());

      // Use the helper function instead of multiple assertions
      expectFailResult(result.current.data!, NetworkError);
    });

    it('should handle 400 not found errors', async () => {
      // Create a FailResult directly (same as the interceptor would create)
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new NotFoundError('Resource not found')),
      );

      const { result } = renderHook(() => useGet<unknown>('/test', ['test']), {
        wrapper: createWrapper(),
      });

      // Wait for the query to finish and data to be populated
      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());

      expectFailResult(result.current.data!, NotFoundError);
    });

    it('should handle 409 conflict errors', async () => {
      // Create a FailResult with a ConflictError
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new ConflictError('A conflict occurred')),
      );

      const { result } = renderHook(() => useGet<unknown>('/test', ['test']), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());
      expectFailResult(result.current.data!, ConflictError);
    });

    it('should handle 422 validation errors', async () => {
      // Create a FailResult with a ValidationError
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new ValidationError('Validation failed')),
      );

      const { result } = renderHook(() => useGet<unknown>('/test', ['test']), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());
      expectFailResult(result.current.data!, ValidationError);
    });

    it('should handle 500 unexpected errors', async () => {
      // Create a FailResult with an UnexpectedError
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new UnexpectedError('Server error occurred')),
      );

      const { result } = renderHook(() => useGet<unknown>('/test', ['test']), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());
      expectFailResult(result.current.data!, UnexpectedError);
    });
  });

  describe('usePost', () => {
    it('should send data and return successful response', async () => {
      const requestData = { name: 'New Item' };
      const responseData = { id: 1, name: 'New Item' };

      // Mock axios.post to resolve with our test data
      vi.mocked(axios.post).mockResolvedValueOnce({ data: responseData });

      const { result } = renderHook(
        () => usePost<typeof responseData, typeof requestData>('/test'),
        { wrapper: createWrapper() },
      );

      result.current.mutate({ data: requestData });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(axios.post).toHaveBeenCalledWith(
        '/test',
        requestData,
        expect.anything(),
      );

      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle network errors', async () => {
      // Mock axios.post to reject with a network error
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new NetworkError('Network error (MOCKED)')),
      );

      const { result } = renderHook(
        () => usePost<unknown, { name: string }>('/test'),
        { wrapper: createWrapper() },
      );

      result.current.mutate({ data: { name: 'Test' } });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.post).toHaveBeenCalledWith(
        '/test',
        { name: 'Test' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, NetworkError);
    });

    it('should handle validation errors', async () => {
      // Mock axios.post to reject with a validation error
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new ValidationError('Invalid data')),
      );

      const { result } = renderHook(
        () => usePost<unknown, { name: string }>('/test'),
        { wrapper: createWrapper() },
      );

      result.current.mutate({ data: { name: 'Test' } });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expectFailResult(result.current.data!, ValidationError);
    });
  });

  describe('usePut', () => {
    it('should send data and return successful response', async () => {
      const requestData = { id: 1, name: 'Updated Item' };
      const responseData = { id: 1, name: 'Updated Item' };

      // Mock axios.put to resolve with our test data
      vi.mocked(axios.put).mockResolvedValueOnce({ data: responseData });

      const { result } = renderHook(
        () => usePut<typeof responseData, typeof requestData>('/test/1'),
        { wrapper: createWrapper() },
      );

      // Trigger the mutation
      await act(async () => {
        result.current.mutate({ data: requestData });
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(axios.put).toHaveBeenCalledWith(
        '/test/1',
        requestData,
        expect.anything(),
      );

      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle conflict errors', async () => {
      // Mock axios.put to reject with a conflict error
      vi.mocked(axios.put).mockRejectedValueOnce(
        new FailResult(new ConflictError('Resource already exists')),
      );

      const { result } = renderHook(
        () => usePut<unknown, { id: number; name: string }>('/test/1'),
        { wrapper: createWrapper() },
      );

      result.current.mutate({ data: { id: 1, name: 'Updated Item' } });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expectFailResult(result.current.data!, ConflictError);
    });
  });

  describe('useDelete', () => {
    it('should send delete request and return successful response', async () => {
      const responseData = { success: true };

      // Mock axios.delete to resolve with our test data
      vi.mocked(axios.delete).mockResolvedValueOnce({ data: responseData });

      const { result } = renderHook(
        () => useDelete<typeof responseData>('/test/1'),
        { wrapper: createWrapper() },
      );

      await act(async () => {
        result.current.mutate({});
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(axios.delete).toHaveBeenCalledWith('/test/1', expect.anything());
      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle not found errors', async () => {
      // Mock axios.delete to reject with a not found error
      vi.mocked(axios.delete).mockRejectedValueOnce(
        new FailResult(new NotFoundError('Resource not found')),
      );

      const { result } = renderHook(() => useDelete<unknown>('/test/999'), {
        wrapper: createWrapper(),
      });

      result.current.mutate({});

      await waitFor(() => expect(result.current.data).toBeDefined());

      expectFailResult(result.current.data!, NotFoundError);
    });
  });
});
