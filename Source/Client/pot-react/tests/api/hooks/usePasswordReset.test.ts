import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  useRequestPasswordReset,
  useVerifyPasswordReset,
} from '@/api/hooks/usePasswordReset';
import { SuccessResult } from '@/lib';

import { usePost } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  usePost: vi.fn(),
}));

describe('usePasswordReset hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useRequestPasswordReset composes send endpoint and forwards sendPasswordReset payload', async () => {
    const mutateAsync = vi.fn().mockResolvedValue(
      new SuccessResult({
        referenceCode: 'ref-001',
      }),
    );

    const mutationResult = {
      mutate: vi.fn(),
      mutateAsync,
      isPending: false,
      data: undefined,
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useRequestPasswordReset());

    expect(usePost).toHaveBeenCalledWith('/auth/password-reset/send');

    await result.current.sendPasswordReset('reset.user');

    expect(mutateAsync).toHaveBeenCalledWith({
      data: {
        username: 'reset.user',
      },
    });
  });

  test('useVerifyPasswordReset composes verify endpoint and forwards verifyPasswordReset payload', async () => {
    const mutateAsync = vi.fn().mockResolvedValue(
      new SuccessResult({
        status: 'Success' as const,
        message: 'Verification succeeded',
      }),
    );

    const mutationResult = {
      mutate: vi.fn(),
      mutateAsync,
      isPending: false,
      data: undefined,
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useVerifyPasswordReset());

    expect(usePost).toHaveBeenCalledWith('/auth/password-reset/verify');

    await result.current.verifyPasswordReset(
      'reset.user',
      'ref-001',
      'otp-001',
    );

    expect(mutateAsync).toHaveBeenCalledWith({
      data: {
        username: 'reset.user',
        referenceCode: 'ref-001',
        verificationCode: 'otp-001',
      },
    });
  });
});
