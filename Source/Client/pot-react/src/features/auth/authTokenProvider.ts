import { refreshTokens } from '@/api/authClient';
import { AuthenticationError } from '@/api/errors/apiErrors';
import type { AuthTokens, TokenProvider } from '@/api/types/auth';

import logoutManager from './logoutManager';
import { AUTH_STORAGE_KEY } from './types';

const createAuthTokenProvider = (): TokenProvider => {
  const getAccessToken = () => {
    const authData = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!authData) {
      return undefined;
    }

    const tokens = JSON.parse(authData) as AuthTokens;
    return tokens.accessToken;
  };

  /**
   * Attempts to refresh the access token using the stored refresh token
   * @throws {AuthenticationError} If refresh token is missing or the refresh attempt fails
   */
  const refreshUserTokens = async (): Promise<string> => {
    const authData = localStorage.getItem(AUTH_STORAGE_KEY);

    if (!authData) {
      throw new AuthenticationError('No refresh token available');
    }

    const tokens = JSON.parse(authData) as AuthTokens;

    const newTokens = await refreshTokens(
      {
        refreshToken: tokens.refreshToken,
      },
      tokens.accessToken,
    );

    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(newTokens));

    return newTokens.accessToken;
  };

  const clearTokens = () => {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    logoutManager.logout();
  };

  return {
    getAccessToken,
    refreshTokens: refreshUserTokens,
    clearTokens,
  };
};

export { createAuthTokenProvider };
