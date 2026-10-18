import { QueryClientProvider } from '@tanstack/react-query';
import { renderHook } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { BadRequestError } from '@/api/errors/apiErrors';
import { useApiDeleteUser } from '@/api/hooks';
import { useCacheInvalidation } from '@/concerns';
import useDeleteUser from '@/features/users/delete/hooks/useDeleteUser';
import { FailResult, SuccessResult } from '@/lib';

import { createQueryClient } from '../../../../shared/react-query/queryHookWrapper';

const invalidateCacheMock = vi.fn();
const mutateAsyncMock = vi.fn();

vi.mock('@/api/hooks', () => ({
  useApiDeleteUser: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
  useCacheInvalidation: vi.fn(),
}));

function createWrapper() {
  const queryClient = createQueryClient();

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

describe('useDeleteUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useCacheInvalidation).mockReturnValue(invalidateCacheMock);

    vi.mocked(useApiDeleteUser).mockReturnValue({
      mutateAsync: mutateAsyncMock,
    } as unknown as ReturnType<typeof useApiDeleteUser>);
  });

  test('sends the password for the selected user', async () => {
    mutateAsyncMock.mockResolvedValue(new SuccessResult(undefined));

    const { result } = renderHook(() => useDeleteUser('user-123'), {
      wrapper: createWrapper(),
    });

    await result.current.deleteUser('secret');

    expect(useApiDeleteUser).toHaveBeenCalledWith('user-123');

    const [mutationData] = mutateAsyncMock.mock.calls[0] as [
      { data: { password: string }; signal: AbortSignal },
    ];

    expect(mutationData.data).toEqual({ password: 'secret' });
    expect(mutationData.signal).toBeInstanceOf(AbortSignal);
  });

  test('invalidates the user list after a successful delete', async () => {
    mutateAsyncMock.mockResolvedValue(new SuccessResult(undefined));

    const { result } = renderHook(() => useDeleteUser('user-123'), {
      wrapper: createWrapper(),
    });

    const deleteResult = await result.current.deleteUser('secret');

    expect(deleteResult.success).toBe(true);
    expect(invalidateCacheMock).toHaveBeenCalledWith(['users']);
  });

  test('returns the failure without invalidating the user list', async () => {
    const failure = new FailResult(
      new BadRequestError('The password is incorrect.'),
    );

    mutateAsyncMock.mockResolvedValue(failure);

    const { result } = renderHook(() => useDeleteUser('user-123'), {
      wrapper: createWrapper(),
    });

    const deleteResult = await result.current.deleteUser('wrong');

    expect(deleteResult).toBe(failure);
    expect(invalidateCacheMock).not.toHaveBeenCalled();
  });
});
