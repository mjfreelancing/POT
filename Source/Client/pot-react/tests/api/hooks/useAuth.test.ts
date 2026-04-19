import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useLogin, useLogout } from '@/api/hooks/useAuth';
import { SuccessResult } from '@/lib';

import { usePost } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  usePost: vi.fn(),
}));

describe('useAuth hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useLogin composes login endpoint via usePost', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({
        status: 'Success' as const,
        accessToken: 'jwt-token',
      }),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useLogin());

    expect(usePost).toHaveBeenCalledWith('/auth/login');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useLogout composes logout endpoint via usePost', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useLogout());

    expect(usePost).toHaveBeenCalledWith('/auth/logout');
    expect(result.current.data).toBe(mutationResult.data);
  });
});
