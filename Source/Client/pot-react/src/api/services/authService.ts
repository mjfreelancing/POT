import { authClient } from '@/api/authClient';
import { AuthenticationError } from '@/api/errors/apiErrors';
import { logger } from '@/concerns';
import { FailResult, type Result, SuccessResult } from '@/lib';

import type { AuthTokens } from '../types';

/**
 * Refresh access token using HTTP-only refresh token cookie.
 * The refresh token is automatically sent by the browser.
 * Note: No access token is sent during initial page load refresh.
 */
async function refreshAccessToken(): Promise<
  Result<{ accessToken: string }, AuthenticationError>
> {
  try {
    logger.info(
      'AuthService',
      'Attempting to refresh access token from cookie',
    );

    const response = await authClient.post<AuthTokens>(
      '/auth/refresh',
      {}, // No body needed - refresh token comes from cookie
    );

    logger.info('AuthService', 'Token refreshed successfully from cookie');

    return new SuccessResult({ accessToken: response.data.accessToken });
  } catch (error) {
    logger.error('AuthService', 'Failed to refresh token from cookie', error);

    return new FailResult(new AuthenticationError('Token refresh failed'));
  }
}

export { refreshAccessToken };
