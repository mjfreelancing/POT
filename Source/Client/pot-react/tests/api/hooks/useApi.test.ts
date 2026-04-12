import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';
import axios from 'axios';
import React, { act } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import {
  ConflictError,
  NetworkError,
  NotFoundError,
  UnexpectedError,
  ValidationError,
} from '@/api/errors/apiErrors';
import { FailResult, FailResultBase, SuccessResult } from '@/lib';

import {
  useDelete,
  useGet,
  usePost,
  usePostWithId,
  usePostWithIdNoData,
  usePut,
  usePutWithId,
  usePutWithIdNoData,
} from '@/api/hooks';

// Mock axios and authClient to prevent initialization errors
vi.mock('axios');

vi.mock('@/api/authClient', () => ({
  authClient: {
    post: vi.fn(),
    defaults: { baseURL: '' },
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  },
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

const renderUseGetHook = <TResponse>(
  url: string,
  queryKey: string[],
  options?: { usePreviousAsPlaceholder?: boolean },
) => {
  return renderHook(() => useGet<TResponse>(url, queryKey, options), {
    wrapper: createWrapper(),
  });
};

const renderUsePostHook = <TResponse, TData>(url: string) => {
  return renderHook(() => usePost<TResponse, TData>(url), {
    wrapper: createWrapper(),
  });
};

const renderUsePutHook = <TResponse, TData>(url: string) => {
  return renderHook(() => usePut<TResponse, TData>(url), {
    wrapper: createWrapper(),
  });
};

const renderUseDeleteHook = <TResponse>(url: string) => {
  return renderHook(() => useDelete<TResponse>(url), {
    wrapper: createWrapper(),
  });
};

const renderUsePutWithIdHook = <TResponse, TData>(
  urlFn: (id: string) => string,
) => {
  return renderHook(() => usePutWithId<TResponse, TData>(urlFn), {
    wrapper: createWrapper(),
  });
};

const renderUsePutWithIdNoDataHook = <TResponse>(
  urlFn: (id: string) => string,
) => {
  return renderHook(() => usePutWithIdNoData<TResponse>(urlFn), {
    wrapper: createWrapper(),
  });
};

const renderUsePostWithIdHook = <TResponse, TData>(
  urlFn: (id: string) => string,
) => {
  return renderHook(() => usePostWithId<TResponse, TData>(urlFn), {
    wrapper: createWrapper(),
  });
};

const renderUsePostWithIdNoDataHook = <TResponse>(
  urlFn: (id: string) => string,
) => {
  return renderHook(() => usePostWithIdNoData<TResponse>(urlFn), {
    wrapper: createWrapper(),
  });
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

      const { result } = renderUseGetHook<typeof responseData>('/test', [
        'test',
      ]);

      expect(result.current.isLoading).toBe(true);

      // Wait for the query to finish
      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      // Verify axios.get was called correctly
      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());

      // Use the helper function instead of multiple assertions
      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle network errors', async () => {
      // Mock axios.get to reject with a FailResult directly
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new NetworkError('Network error (MOCKED)')),
      );

      const { result } = renderUseGetHook<unknown>('/test', ['test']);

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

      const { result } = renderUseGetHook<unknown>('/test', ['test']);

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

      const { result } = renderUseGetHook<unknown>('/test', ['test']);

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());
      expectFailResult(result.current.data!, ConflictError);
    });

    it('should handle 422 validation errors', async () => {
      // Create a FailResult with a ValidationError
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new ValidationError('Validation failed')),
      );

      const { result } = renderUseGetHook<unknown>('/test', ['test']);

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());
      expectFailResult(result.current.data!, ValidationError);
    });

    it('should handle 500 unexpected errors', async () => {
      // Create a FailResult with an UnexpectedError
      vi.mocked(axios.get).mockRejectedValueOnce(
        new FailResult(new UnexpectedError('Server error occurred')),
      );

      const { result } = renderUseGetHook<unknown>('/test', ['test']);

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.get).toHaveBeenCalledWith('/test', expect.anything());
      expectFailResult(result.current.data!, UnexpectedError);
    });

    it('should keep previous data as placeholder when usePreviousAsPlaceholder is true', async () => {
      const firstResponse = { id: 1, name: 'First Item' };
      const secondResponse = { id: 2, name: 'Second Item' };

      let resolveSecondRequest:
        | ((value: { data: typeof secondResponse }) => void)
        | undefined;

      const secondRequest = new Promise<{ data: typeof secondResponse }>(
        resolve => {
          resolveSecondRequest = resolve;
        },
      );

      vi.mocked(axios.get)
        .mockResolvedValueOnce({ data: firstResponse })
        .mockImplementationOnce(() => secondRequest);

      const { result, rerender } = renderHook(
        ({ url, queryKey, options }) =>
          useGet<typeof firstResponse | typeof secondResponse>(
            url,
            queryKey,
            options,
          ),
        {
          initialProps: {
            url: '/test/first',
            queryKey: ['test', 'first'],
            options: { usePreviousAsPlaceholder: true },
          },
          wrapper: createWrapper(),
        },
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expectSuccessResult(result.current.data!, firstResponse);

      rerender({
        url: '/test/second',
        queryKey: ['test', 'second'],
        options: { usePreviousAsPlaceholder: true },
      });

      expect(result.current.data).toBeDefined();
      expectSuccessResult(result.current.data!, firstResponse);

      resolveSecondRequest?.({ data: secondResponse });

      await waitFor(() => {
        if (result.current.data?.success) {
          expect(result.current.data.value).toEqual(secondResponse);
        }
      });
    });

    it('should clear data during refetch when usePreviousAsPlaceholder is false', async () => {
      const firstResponse = { id: 1, name: 'First Item' };
      const secondResponse = { id: 2, name: 'Second Item' };

      let resolveSecondRequest:
        | ((value: { data: typeof secondResponse }) => void)
        | undefined;

      const secondRequest = new Promise<{ data: typeof secondResponse }>(
        resolve => {
          resolveSecondRequest = resolve;
        },
      );

      vi.mocked(axios.get)
        .mockResolvedValueOnce({ data: firstResponse })
        .mockImplementationOnce(() => secondRequest);

      const { result, rerender } = renderHook(
        ({ url, queryKey, options }) =>
          useGet<typeof firstResponse | typeof secondResponse>(
            url,
            queryKey,
            options,
          ),
        {
          initialProps: {
            url: '/test/first',
            queryKey: ['test', 'first'],
            options: { usePreviousAsPlaceholder: false },
          },
          wrapper: createWrapper(),
        },
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expectSuccessResult(result.current.data!, firstResponse);

      rerender({
        url: '/test/second',
        queryKey: ['test', 'second'],
        options: { usePreviousAsPlaceholder: false },
      });

      expect(result.current.data).toBeUndefined();

      resolveSecondRequest?.({ data: secondResponse });

      await waitFor(() => {
        if (result.current.data?.success) {
          expect(result.current.data.value).toEqual(secondResponse);
        }
      });
    });
  });

  describe('usePost', () => {
    it('should send data and return successful response', async () => {
      const requestData = { name: 'New Item' };
      const responseData = { id: 1, name: 'New Item' };

      // Mock axios.post to resolve with our test data
      vi.mocked(axios.post).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUsePostHook<
        typeof responseData,
        typeof requestData
      >('/test');

      result.current.mutate({ data: requestData });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(axios.post).toHaveBeenCalledWith(
        '/test',
        requestData,
        expect.anything(),
      );

      expectSuccessResult(result.current.data!, responseData);
    });

    it('should forward timeoutMs to axios timeout option', async () => {
      const requestData = { name: 'Timed Request' };
      const responseData = { id: 2, name: 'Timed Request' };

      vi.mocked(axios.post).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUsePostHook<
        typeof responseData,
        typeof requestData
      >('/test');

      result.current.mutate({
        data: requestData,
        timeoutMs: 4321,
      });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.post).toHaveBeenCalledWith('/test', requestData, {
        signal: undefined,
        timeout: 4321,
      });

      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle network errors', async () => {
      // Mock axios.post to reject with a network error
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new NetworkError('Network error (MOCKED)')),
      );

      const { result } = renderUsePostHook<unknown, { name: string }>('/test');

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

      const { result } = renderUsePostHook<unknown, { name: string }>('/test');

      result.current.mutate({ data: { name: 'Test' } });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expectFailResult(result.current.data!, ValidationError);
    });

    it('should handle not found errors', async () => {
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new NotFoundError('Endpoint not found')),
      );

      const { result } = renderUsePostHook<unknown, { name: string }>('/test');

      result.current.mutate({ data: { name: 'Test' } });

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.post).toHaveBeenCalledWith(
        '/test',
        { name: 'Test' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, NotFoundError);
    });

    it('should handle conflict errors', async () => {
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new ConflictError('Resource conflict')),
      );

      const { result } = renderUsePostHook<unknown, { name: string }>('/test');

      result.current.mutate({ data: { name: 'Test' } });

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.post).toHaveBeenCalledWith(
        '/test',
        { name: 'Test' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, ConflictError);
    });

    it('should handle unexpected errors', async () => {
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new UnexpectedError('Unexpected server error')),
      );

      const { result } = renderUsePostHook<unknown, { name: string }>('/test');

      result.current.mutate({ data: { name: 'Test' } });

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.post).toHaveBeenCalledWith(
        '/test',
        { name: 'Test' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, UnexpectedError);
    });
  });

  describe('usePut', () => {
    it('should send data and return successful response', async () => {
      const requestData = { id: 1, name: 'Updated Item' };
      const responseData = { id: 1, name: 'Updated Item' };

      // Mock axios.put to resolve with our test data
      vi.mocked(axios.put).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUsePutHook<
        typeof responseData,
        typeof requestData
      >('/test/1');

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

      const { result } = renderUsePutHook<
        unknown,
        { id: number; name: string }
      >('/test/1');

      result.current.mutate({ data: { id: 1, name: 'Updated Item' } });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expectFailResult(result.current.data!, ConflictError);
    });

    it('should handle network errors', async () => {
      vi.mocked(axios.put).mockRejectedValueOnce(
        new FailResult(new NetworkError('Network error (MOCKED)')),
      );

      const { result } = renderUsePutHook<
        unknown,
        { id: number; name: string }
      >('/test/1');

      result.current.mutate({ data: { id: 1, name: 'Updated Item' } });

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.put).toHaveBeenCalledWith(
        '/test/1',
        { id: 1, name: 'Updated Item' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, NetworkError);
    });

    it('should handle not found errors', async () => {
      vi.mocked(axios.put).mockRejectedValueOnce(
        new FailResult(new NotFoundError('Resource not found')),
      );

      const { result } = renderUsePutHook<
        unknown,
        { id: number; name: string }
      >('/test/1');

      result.current.mutate({ data: { id: 1, name: 'Updated Item' } });

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.put).toHaveBeenCalledWith(
        '/test/1',
        { id: 1, name: 'Updated Item' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, NotFoundError);
    });

    it('should handle validation errors', async () => {
      vi.mocked(axios.put).mockRejectedValueOnce(
        new FailResult(new ValidationError('Invalid data')),
      );

      const { result } = renderUsePutHook<
        unknown,
        { id: number; name: string }
      >('/test/1');

      result.current.mutate({ data: { id: 1, name: 'Updated Item' } });

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.put).toHaveBeenCalledWith(
        '/test/1',
        { id: 1, name: 'Updated Item' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, ValidationError);
    });

    it('should handle unexpected errors', async () => {
      vi.mocked(axios.put).mockRejectedValueOnce(
        new FailResult(new UnexpectedError('Unexpected server error')),
      );

      const { result } = renderUsePutHook<
        unknown,
        { id: number; name: string }
      >('/test/1');

      result.current.mutate({ data: { id: 1, name: 'Updated Item' } });

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.put).toHaveBeenCalledWith(
        '/test/1',
        { id: 1, name: 'Updated Item' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, UnexpectedError);
    });
  });

  describe('useDelete', () => {
    it('should send delete request and return successful response', async () => {
      const responseData = { success: true };

      // Mock axios.delete to resolve with our test data
      vi.mocked(axios.delete).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUseDeleteHook<typeof responseData>('/test/1');

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

      const { result } = renderUseDeleteHook<unknown>('/test/999');

      result.current.mutate({});

      await waitFor(() => expect(result.current.data).toBeDefined());

      expectFailResult(result.current.data!, NotFoundError);
    });

    it('should handle network errors', async () => {
      vi.mocked(axios.delete).mockRejectedValueOnce(
        new FailResult(new NetworkError('Network error (MOCKED)')),
      );

      const { result } = renderUseDeleteHook<unknown>('/test/1');

      result.current.mutate({});

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.delete).toHaveBeenCalledWith('/test/1', expect.anything());
      expectFailResult(result.current.data!, NetworkError);
    });

    it('should handle conflict errors', async () => {
      vi.mocked(axios.delete).mockRejectedValueOnce(
        new FailResult(
          new ConflictError('Cannot delete resource due to conflict'),
        ),
      );

      const { result } = renderUseDeleteHook<unknown>('/test/1');

      result.current.mutate({});

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.delete).toHaveBeenCalledWith('/test/1', expect.anything());
      expectFailResult(result.current.data!, ConflictError);
    });

    it('should handle validation errors', async () => {
      // This is less common for DELETE but included for completeness
      vi.mocked(axios.delete).mockRejectedValueOnce(
        new FailResult(new ValidationError('Invalid request for delete')),
      );

      const { result } = renderUseDeleteHook<unknown>('/test/1');

      result.current.mutate({});

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.delete).toHaveBeenCalledWith('/test/1', expect.anything());
      expectFailResult(result.current.data!, ValidationError);
    });

    it('should handle unexpected errors', async () => {
      vi.mocked(axios.delete).mockRejectedValueOnce(
        new FailResult(new UnexpectedError('Unexpected server error')),
      );

      const { result } = renderUseDeleteHook<unknown>('/test/1');

      result.current.mutate({});

      await waitFor(() => expect(result.current.data).toBeDefined());
      expect(axios.delete).toHaveBeenCalledWith('/test/1', expect.anything());
      expectFailResult(result.current.data!, UnexpectedError);
    });
  });

  describe('usePutWithId', () => {
    it('should send put request using id-based URL and data', async () => {
      const requestData = { name: 'Updated Item' };
      const responseData = { id: 'abc-123', name: 'Updated Item' };

      vi.mocked(axios.put).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUsePutWithIdHook<
        typeof responseData,
        typeof requestData
      >(id => `/test/${id}`);

      result.current.mutate({ id: 'abc-123', data: requestData });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.put).toHaveBeenCalledWith(
        '/test/abc-123',
        requestData,
        expect.anything(),
      );
      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle errors from put-with-id requests', async () => {
      vi.mocked(axios.put).mockRejectedValueOnce(
        new FailResult(new NotFoundError('Resource not found')),
      );

      const { result } = renderUsePutWithIdHook<unknown, { name: string }>(
        id => `/test/${id}`,
      );

      result.current.mutate({ id: 'missing-id', data: { name: 'Test' } });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.put).toHaveBeenCalledWith(
        '/test/missing-id',
        { name: 'Test' },
        expect.anything(),
      );
      expectFailResult(result.current.data!, NotFoundError);
    });
  });

  describe('usePutWithIdNoData', () => {
    it('should send put request with undefined body', async () => {
      const responseData = { success: true };

      vi.mocked(axios.put).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUsePutWithIdNoDataHook<typeof responseData>(
        id => `/test/${id}/action`,
      );

      result.current.mutate({ id: 'abc-123' });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.put).toHaveBeenCalledWith(
        '/test/abc-123/action',
        undefined,
        expect.anything(),
      );
      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle errors from put-with-id-no-data requests', async () => {
      vi.mocked(axios.put).mockRejectedValueOnce(
        new FailResult(new ConflictError('Conflict during operation')),
      );

      const { result } = renderUsePutWithIdNoDataHook<unknown>(
        id => `/test/${id}/action`,
      );

      result.current.mutate({ id: 'conflict-id' });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.put).toHaveBeenCalledWith(
        '/test/conflict-id/action',
        undefined,
        expect.anything(),
      );
      expectFailResult(result.current.data!, ConflictError);
    });
  });

  describe('usePostWithId', () => {
    it('should send post request using id-based URL and data', async () => {
      const requestData = { enabled: true };
      const responseData = { id: 'abc-123', enabled: true };

      vi.mocked(axios.post).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUsePostWithIdHook<
        typeof responseData,
        typeof requestData
      >(id => `/test/${id}/toggle`);

      result.current.mutate({ id: 'abc-123', data: requestData });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.post).toHaveBeenCalledWith(
        '/test/abc-123/toggle',
        requestData,
        expect.anything(),
      );
      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle errors from post-with-id requests', async () => {
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new ValidationError('Invalid request')),
      );

      const { result } = renderUsePostWithIdHook<unknown, { enabled: boolean }>(
        id => `/test/${id}/toggle`,
      );

      result.current.mutate({ id: 'invalid-id', data: { enabled: true } });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.post).toHaveBeenCalledWith(
        '/test/invalid-id/toggle',
        { enabled: true },
        expect.anything(),
      );
      expectFailResult(result.current.data!, ValidationError);
    });
  });

  describe('usePostWithIdNoData', () => {
    it('should send post request with undefined body', async () => {
      const responseData = { accepted: true };

      vi.mocked(axios.post).mockResolvedValueOnce({ data: responseData });

      const { result } = renderUsePostWithIdNoDataHook<typeof responseData>(
        id => `/test/${id}/approve`,
      );

      result.current.mutate({ id: 'abc-123' });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.post).toHaveBeenCalledWith(
        '/test/abc-123/approve',
        undefined,
        expect.anything(),
      );
      expectSuccessResult(result.current.data!, responseData);
    });

    it('should handle errors from post-with-id-no-data requests', async () => {
      vi.mocked(axios.post).mockRejectedValueOnce(
        new FailResult(new NetworkError('Network unavailable')),
      );

      const { result } = renderUsePostWithIdNoDataHook<unknown>(
        id => `/test/${id}/approve`,
      );

      result.current.mutate({ id: 'offline-id' });

      await waitFor(() => expect(result.current.data).toBeDefined());

      expect(axios.post).toHaveBeenCalledWith(
        '/test/offline-id/approve',
        undefined,
        expect.anything(),
      );
      expectFailResult(result.current.data!, NetworkError);
    });
  });
});
