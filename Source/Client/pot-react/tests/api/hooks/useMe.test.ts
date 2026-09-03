import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useMe } from '@/api/hooks/useMe';
import { SuccessResult } from '@/lib';

import { useGet } from '@/api/hooks/useApi';
import { useAccessToken } from '@/features/auth/contexts';
import { createUser } from '../../shared/factories/userFactory';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
}));

vi.mock('@/features/auth/contexts', () => ({
  useAccessToken: vi.fn(),
}));

describe('useMe hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('composes me query with auth-specific options and enabled true when token exists', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult(createUser({ username: 'me.user' })),
    };

    vi.mocked(useAccessToken).mockReturnValue({
      accessToken: 'jwt-token',
      isAuthenticated: true,
      isInitialized: true,
      setAccessToken: vi.fn(),
    });

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useMe());

    expect(useGet).toHaveBeenCalledTimes(1);

    const call = vi.mocked(useGet).mock.calls[0];
    expect(call?.[0]).toBe('/me');
    expect(call?.[1]).toEqual(['me']);
    expect(call?.[2]).toMatchObject({
      refetchOnMount: true,
      refetchOnWindowFocus: false,
      retry: false,
      enabled: true,
    });

    const placeholderData = call?.[2]?.placeholderData as
      ((previous: unknown) => unknown) | undefined;

    expect(placeholderData?.(queryResult.data)).toBe(queryResult.data);

    expect(result.current.data).toBe(queryResult.data);
  });

  test('disables me query when access token is missing', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: false,
      data: undefined,
    };

    vi.mocked(useAccessToken).mockReturnValue({
      accessToken: undefined,
      isAuthenticated: false,
      isInitialized: true,
      setAccessToken: vi.fn(),
    });

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    renderHook(() => useMe());

    const call = vi.mocked(useGet).mock.calls[0];
    expect(call?.[2]?.enabled).toBe(false);
  });
});
