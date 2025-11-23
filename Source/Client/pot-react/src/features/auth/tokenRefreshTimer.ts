import { authClient } from '@/api/authClient';
import { AuthenticationError } from '@/api/errors/apiErrors';
import { logger } from '@/concerns';
import { FailResult } from '@/lib';
import { calculateRefreshTime } from '@/lib/jwt';

type TokenRefreshConfig = {
  /** Current access token */
  currentAccessToken: string;
  /** Called when refresh succeeds with new access token */
  onRefreshSuccess: (accessToken: string) => void;
  /** Called when refresh fails */
  onRefreshError: (error: unknown) => void;
};

type TokenRefreshHandle = {
  /** Start the refresh timer */
  start: () => void;
  /** Stop and cleanup the refresh timer */
  stop: () => void;
};

/**
 * Creates a token refresh timer that will proactively refresh the token before it expires
 */
function createTokenRefreshTimer({
  currentAccessToken,
  onRefreshSuccess,
  onRefreshError,
}: TokenRefreshConfig): TokenRefreshHandle {
  let timerId: number | undefined;

  const stopTimer = () => {
    if (timerId) {
      window.clearTimeout(timerId);
      timerId = undefined;
    }
  };

  const startTimer = () => {
    // Clear any existing timer first
    stopTimer();

    const refreshTimeMs = calculateRefreshTime(currentAccessToken);

    if (!refreshTimeMs) {
      return;
    }

    logger.info('Auth', 'Setting up refresh timer');

    timerId = window.setTimeout(async () => {
      logger.info('Auth', 'Refresh timer triggered');

      try {
        const response = await authClient.post<{ accessToken: string }>(
          '/auth/refresh',
          {}, // No body - refresh token comes from HTTP-only cookie
          {
            headers: {
              Authorization: `Bearer ${currentAccessToken}`,
            },
          },
        );

        logger.info('Auth', 'Token refreshed successfully');
        onRefreshSuccess(response.data.accessToken);
      } catch (error) {
        logger.error('Auth', 'Failed to refresh token', error);

        // Ensure error is wrapped in FailResult for consistent handling
        const normalizedError =
          error instanceof FailResult
            ? error
            : new FailResult(new AuthenticationError('Token refresh failed'));

        onRefreshError(normalizedError);
      }
    }, refreshTimeMs);
  };

  return {
    start: startTimer,
    stop: stopTimer,
  };
}

export type { TokenRefreshConfig, TokenRefreshHandle };
export { createTokenRefreshTimer };
