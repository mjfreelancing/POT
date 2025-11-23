import { AuthenticationError } from '@/api/errors/apiErrors';
import { refreshAccessToken } from '@/api/services/authService';
import type { TokenProvider } from '@/api/types/auth';

import logoutManager from './logoutManager';

/**
 * Global token storage - tokens live only in memory
 */
let currentAccessToken: string | undefined;

const createAuthTokenProvider = (): TokenProvider => {
  const getAccessToken = () => {
    return currentAccessToken;
  };

  /**
   * Attempts to refresh the access token using HTTP-only cookie
   * @throws {AuthenticationError} If refresh token cookie is missing or the refresh attempt fails
   */
  const refreshUserTokens = async (): Promise<string> => {
    const result = await refreshAccessToken();

    if (!result.success) {
      throw new AuthenticationError('No refresh token available');
    }

    // Update the in-memory token
    currentAccessToken = result.value.accessToken;
    return result.value.accessToken;
  };

  const clearTokens = () => {
    currentAccessToken = undefined;
    logoutManager.logout();
  };

  const setAccessToken = (token: string | undefined) => {
    currentAccessToken = token;
  };

  return {
    getAccessToken,
    refreshTokens: refreshUserTokens,
    clearTokens,
    setAccessToken,
  };
};

export { createAuthTokenProvider };
export type { TokenProvider };
