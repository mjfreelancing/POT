import { beforeEach, describe, expect, test, vi } from 'vitest';

import { AuthenticationError } from '@/api/errors/apiErrors';
import { refreshAccessToken } from '@/api/services/authService';
import { FailResult, SuccessResult } from '@/lib';

import { authClient } from '@/api/authClient';
import { logger } from '@/concerns';

vi.mock('@/api/authClient', () => ({
  authClient: {
    post: vi.fn(),
  },
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    warn: vi.fn(),
  },
}));

describe('refreshAccessToken', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('returns SuccessResult with access token when refresh succeeds', async () => {
    vi.mocked(authClient.post).mockResolvedValue({
      data: {
        accessToken: 'refreshed-token',
      },
    });

    const result = await refreshAccessToken();

    expect(authClient.post).toHaveBeenCalledWith('/auth/refresh', {});
    expect(logger.info).toHaveBeenCalledWith(
      'AuthService',
      'Attempting to refresh access token',
    );
    expect(logger.info).toHaveBeenCalledWith(
      'AuthService',
      'Token refreshed successfully',
    );
    expect(result).toEqual(
      new SuccessResult({ accessToken: 'refreshed-token' }),
    );
  });

  test('returns FailResult<AuthenticationError> when refresh fails', async () => {
    const apiError = new Error('request failed');

    vi.mocked(authClient.post).mockRejectedValue(apiError);

    const result = await refreshAccessToken();

    expect(result).toBeInstanceOf(FailResult);

    if (result.success) {
      throw new Error('Expected refreshAccessToken to fail');
    }

    expect(result.error).toBeInstanceOf(AuthenticationError);
    expect(result.error.description).toBe('Token refresh failed');
    expect(logger.warn).toHaveBeenCalledWith(
      'AuthService',
      'Failed to refresh token ',
      apiError,
    );
  });
});
