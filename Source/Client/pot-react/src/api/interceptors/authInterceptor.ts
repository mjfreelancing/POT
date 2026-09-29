import type { AxiosError, InternalAxiosRequestConfig } from 'axios';
import axios from 'axios';

import { logger } from '@/concerns';
import { FailResult } from '@/lib';

import { AuthenticationError } from '../errors/apiErrors';
import type { TokenProvider } from '../types/auth';

type QueuedRequest = {
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
};

/**
 * Request interceptor to add the Authorization header with JWT token
 */
function createAuthRequestHandler(tokenProvider: TokenProvider) {
  return (config: InternalAxiosRequestConfig) => {
    const token = tokenProvider.getAccessToken();

    if (token) {
      config.headers.set('Authorization', `Bearer ${token}`);
    }

    return config;
  };
}

/**
 * Handle 401 errors with token refresh.
 *
 * This interceptor handles reactive token refresh when API calls receive 401 responses.
 * It works in conjunction with the proactive refresh timer in accessTokenRefreshTimer.ts.
 *
 * ERROR HANDLING:
 * - 401 on /auth/* endpoints: Do not attempt refresh (prevents infinite loops)
 * - 401 on other endpoints: Attempt to refresh and retry the original request
 * - Refresh success: Process queued requests and retry
 * - Refresh failure: Clear queue, logout user, propagate normalized error
 *
 * NOTES:
 * - Only one refresh attempt happens at a time (isRefreshing flag)
 * - Concurrent 401s are queued and retried after refresh completes
 * - Refresh state (in-flight flag and queue) is scoped to each handler instance
 * - Errors are normalized to FailResult<AuthenticationError> for consistent handling
 * - This catches cases where proactive refresh failed or didn't trigger in time
 */
function createAuthErrorHandler(tokenProvider: TokenProvider) {
  // Track token refresh state and queue for this handler instance
  let isRefreshing = false;
  let failedQueue: QueuedRequest[] = [];

  // Process queued requests with new token
  const processQueue = (token: string) => {
    failedQueue.forEach(request => request.resolve(token));
    failedQueue = [];
  };

  // Clear queue on refresh failure
  const clearQueue = (error: unknown) => {
    failedQueue.forEach(request => request.reject(error));
    failedQueue = [];
  };

  return async (error: AxiosError) => {
    const { response, config } = error;

    if (!response || !config) {
      return Promise.reject(error);
    }

    if (response.status === 401) {
      // If the request is to an /auth/* endpoint, do not attempt token refresh
      // This prevents infinite loops when the refresh endpoint itself returns 401
      if (config.url && config.url.includes('/auth/')) {
        return Promise.reject(error);
      }

      if (!isRefreshing) {
        isRefreshing = true;

        try {
          // Try to refresh the token
          const accessToken = await tokenProvider.refreshTokens();

          // Update the failed request's Authorization header
          config.headers.set('Authorization', `Bearer ${accessToken}`);

          // Process any queued requests
          processQueue(accessToken);

          // Retry the original request
          return axios(config);
        } catch (refreshError) {
          logger.error('API', 'Token refresh failed', refreshError);

          // Normalize the error to FailResult for consistent handling
          const normalizedError =
            refreshError instanceof FailResult
              ? refreshError
              : new FailResult(new AuthenticationError('Token refresh failed'));

          // Clear the queue with the normalized error
          clearQueue(normalizedError);

          // Logout the user since refresh token is invalid/expired
          tokenProvider.clearTokens();

          return Promise.reject(normalizedError);
        } finally {
          isRefreshing = false;
        }
      } else {
        // If currently refreshing, add the request to the queue
        return new Promise((resolve, reject) => {
          failedQueue.push({
            resolve: (token: string) => {
              config.headers.set('Authorization', `Bearer ${token}`);
              resolve(axios(config));
            },
            reject: (err: unknown) => {
              reject(err);
            },
          });
        });
      }
    }

    // Let other error handlers deal with non-401 errors
    return Promise.reject(error);
  };
}

/**
 * Setup auth interceptors. These must be set up before general interceptors to handle token refresh.
 */
function setupAuthInterceptors(tokenProvider: TokenProvider) {
  const requestInterceptorId = axios.interceptors.request.use(
    createAuthRequestHandler(tokenProvider),
  );
  const responseInterceptorId = axios.interceptors.response.use(
    response => response,
    createAuthErrorHandler(tokenProvider),
  );

  return {
    requestInterceptorId,
    responseInterceptorId,
  };
}

export {
  createAuthErrorHandler,
  createAuthRequestHandler,
  setupAuthInterceptors,
};
