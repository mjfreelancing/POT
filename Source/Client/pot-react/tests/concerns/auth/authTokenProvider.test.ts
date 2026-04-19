import { beforeEach, describe, expect, test, vi } from 'vitest';

import { AuthenticationError } from '@/api/errors/apiErrors';
import { FailResult, SuccessResult } from '@/lib';

import { refreshAccessToken } from '@/api/services/authService';
import logoutManager from '@/concerns/auth/logoutManager';

vi.mock('@/api/services/authService', () => ({
  refreshAccessToken: vi.fn(),
}));

vi.mock('@/concerns/auth/logoutManager', () => ({
  default: {
    logout: vi.fn(),
  },
}));

async function createProvider() {
  vi.resetModules();

  const module = await import('@/concerns/auth/authTokenProvider');

  return module.createAuthTokenProvider();
}

describe('createAuthTokenProvider', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('stores and returns access token via setAccessToken/getAccessToken', async () => {
    const provider = await createProvider();

    provider.setAccessToken('manual-token');

    expect(provider.getAccessToken()).toBe('manual-token');
  });

  test('refreshTokens stores and returns new token when refresh succeeds', async () => {
    const provider = await createProvider();

    vi.mocked(refreshAccessToken).mockResolvedValue(
      new SuccessResult({ accessToken: 'new-access-token' }),
    );

    const accessToken = await provider.refreshTokens();

    expect(accessToken).toBe('new-access-token');
    expect(provider.getAccessToken()).toBe('new-access-token');
  });

  test('refreshTokens throws AuthenticationError when refresh fails', async () => {
    const provider = await createProvider();

    vi.mocked(refreshAccessToken).mockResolvedValue(
      new FailResult(new AuthenticationError('Token refresh failed')),
    );

    await expect(provider.refreshTokens()).rejects.toMatchObject({
      code: 'Authentication Error',
      description: 'No refresh token available',
      type: 'Api',
    });
    expect(provider.getAccessToken()).toBeUndefined();
  });

  test('clearTokens removes token and triggers logout', async () => {
    const provider = await createProvider();

    provider.setAccessToken('existing-token');

    provider.clearTokens();

    expect(provider.getAccessToken()).toBeUndefined();
    expect(logoutManager.logout).toHaveBeenCalledTimes(1);
  });
});
