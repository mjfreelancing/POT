import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useChangePassword from '@/api/hooks/useChangePassword';
import { SuccessResult } from '@/lib';

import { usePut } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  usePut: vi.fn(),
}));

describe('useChangePassword hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('composes change-password endpoint via usePut', () => {
    const mutationResult = {
      mutate: vi.fn(),
      mutateAsync: vi.fn(),
      isPending: false,
      data: new SuccessResult(null),
    };

    vi.mocked(usePut).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePut>,
    );

    const { result } = renderHook(() => useChangePassword());

    expect(usePut).toHaveBeenCalledWith('/me/change-password');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('forwards changePassword fields into mutateAsync payload', async () => {
    const mutateAsync = vi.fn().mockResolvedValue(new SuccessResult(null));

    const mutationResult = {
      mutate: vi.fn(),
      mutateAsync,
      isPending: false,
      data: undefined,
    };

    vi.mocked(usePut).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePut>,
    );

    const { result } = renderHook(() => useChangePassword());

    await result.current.changePassword({
      currentPassword: 'old-password',
      newPassword: 'new-password',
    });

    expect(mutateAsync).toHaveBeenCalledWith({
      data: {
        currentPassword: 'old-password',
        newPassword: 'new-password',
      },
    });
  });
});
