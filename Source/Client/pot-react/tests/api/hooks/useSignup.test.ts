import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useCompleteSignup, useRequestSignup } from '@/api/hooks/useSignup';
import { SuccessResult } from '@/lib';

import { usePost } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  usePost: vi.fn(),
}));

describe('useSignup hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useRequestSignup composes send endpoint and forwards sendSignup payload', async () => {
    const mutateAsync = vi.fn().mockResolvedValue(
      new SuccessResult({
        status: 'Success' as const,
        message: 'Verification code sent',
        referenceCode: 'ref-123',
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

    const { result } = renderHook(() => useRequestSignup());

    expect(usePost).toHaveBeenCalledWith('/auth/signup/send');

    await result.current.sendSignup('new.user', 'new.user@example.com');

    expect(mutateAsync).toHaveBeenCalledWith({
      data: {
        username: 'new.user',
        email: 'new.user@example.com',
      },
    });
  });

  test('useCompleteSignup composes complete endpoint and forwards completeSignup payload', async () => {
    const mutateAsync = vi.fn().mockResolvedValue(
      new SuccessResult({
        status: 'Success' as const,
        message: 'Signup completed',
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

    const { result } = renderHook(() => useCompleteSignup());

    expect(usePost).toHaveBeenCalledWith('/auth/signup/complete');

    await result.current.completeSignup('new.user', 'ref-123', 'otp-456');

    expect(mutateAsync).toHaveBeenCalledWith({
      data: {
        username: 'new.user',
        referenceCode: 'ref-123',
        verificationCode: 'otp-456',
      },
    });
  });
});
