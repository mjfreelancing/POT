import { beforeEach, describe, expect, test, vi } from 'vitest';

import { AuthenticationError } from '@/api/errors/apiErrors';
import { FailResult, SuccessResult } from '@/lib';

import { refreshAccessToken } from '@/api/services/authService';
import logoutManager from '@/concerns/auth/logoutManager';
import { tokenProvider } from '@/concerns/auth/tokenProvider';

vi.mock('@/api/services/authService', () => ({
  refreshAccessToken: vi.fn(),
}));

vi.mock('@/concerns/auth/logoutManager', () => ({
  default: {
    logout: vi.fn(),
  },
}));

describe('tokenProvider singleton', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    tokenProvider.setAccessToken(undefined);
  });

  test('maintains in-memory token set via setAccessToken', () => {
    tokenProvider.setAccessToken('singleton-token');

    expect(tokenProvider.getAccessToken()).toBe('singleton-token');
  });

  test('refreshTokens updates singleton token when refresh succeeds', async () => {
    vi.mocked(refreshAccessToken).mockResolvedValue(
      new SuccessResult({ accessToken: 'refreshed-singleton-token' }),
    );

    const accessToken = await tokenProvider.refreshTokens();

    expect(accessToken).toBe('refreshed-singleton-token');
    expect(tokenProvider.getAccessToken()).toBe('refreshed-singleton-token');
  });

  test('refreshTokens rejects with AuthenticationError when refresh fails', async () => {
    vi.mocked(refreshAccessToken).mockResolvedValue(
      new FailResult(new AuthenticationError('Token refresh failed')),
    );

    await expect(tokenProvider.refreshTokens()).rejects.toBeInstanceOf(
      AuthenticationError,
    );
  });

  test('clearTokens clears singleton token and logs out', () => {
    tokenProvider.setAccessToken('singleton-token');

    tokenProvider.clearTokens();

    expect(tokenProvider.getAccessToken()).toBeUndefined();
    expect(logoutManager.logout).toHaveBeenCalledTimes(1);
  });
});
