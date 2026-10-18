import { QueryClientProvider } from '@tanstack/react-query';
import { renderHook } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { BadRequestError } from '@/api/errors/apiErrors';
import { useApiDeleteSite } from '@/api/hooks';
import useDeleteCurrentSite from '@/features/userSettings/delete/hooks/useDeleteCurrentSite';
import { FailResult, SuccessResult } from '@/lib';

import { createQueryClient } from '../../../../shared/react-query/queryHookWrapper';

const mutateAsyncMock = vi.fn();

vi.mock('@/api/hooks', () => ({
  useApiDeleteSite: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

function createWrapper() {
  const queryClient = createQueryClient();

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

const deleteInput = {
  siteName: 'Site A',
  username: 'maria',
  password: 'secret',
};

describe('useDeleteCurrentSite', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useApiDeleteSite).mockReturnValue({
      mutateAsync: mutateAsyncMock,
    } as unknown as ReturnType<typeof useApiDeleteSite>);
  });

  test('sends the confirmation values to the site endpoint', async () => {
    mutateAsyncMock.mockResolvedValue(new SuccessResult(undefined));

    const { result } = renderHook(() => useDeleteCurrentSite(), {
      wrapper: createWrapper(),
    });

    await result.current.deleteCurrentSite(deleteInput);

    const [mutationData] = mutateAsyncMock.mock.calls[0] as [
      { data: typeof deleteInput; signal: AbortSignal },
    ];

    expect(mutationData.data).toEqual(deleteInput);
    expect(mutationData.signal).toBeInstanceOf(AbortSignal);
  });

  test('returns the success result', async () => {
    mutateAsyncMock.mockResolvedValue(new SuccessResult(undefined));

    const { result } = renderHook(() => useDeleteCurrentSite(), {
      wrapper: createWrapper(),
    });

    const siteResult = await result.current.deleteCurrentSite(deleteInput);

    expect(siteResult.success).toBe(true);
  });

  test('returns the failure result unchanged', async () => {
    const failure = new FailResult(
      new BadRequestError('The password is incorrect.'),
    );

    mutateAsyncMock.mockResolvedValue(failure);

    const { result } = renderHook(() => useDeleteCurrentSite(), {
      wrapper: createWrapper(),
    });

    const siteResult = await result.current.deleteCurrentSite(deleteInput);

    expect(siteResult).toBe(failure);
  });
});
