import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

import { AuthenticationError } from '../errors/apiErrors';
import type { TokenProvider } from '../types/auth';

// Track token refresh state and queue
let isRefreshing = false;
let failedQueue: {
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}[] = [];

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

/**
 * Request interceptor to add the Authorization header with JWT token
 */
const addAuthHeader =
  (tokenProvider: TokenProvider) => (config: InternalAxiosRequestConfig) => {
    const token = tokenProvider.getAccessToken();

    if (token) {
      config.headers.set('Authorization', `Bearer ${token}`);
    }

    return config;
  };

/**
 * Handle 401 errors with token refresh
 */
const handleAuthError =
  (tokenProvider: TokenProvider) => async (error: AxiosError) => {
    const { response, config } = error;

    if (!response || !config) {
      return Promise.reject(error);
    }

    if (response.status === 401) {
      // If the request is to an /auth/* endpoint, do not attempt token refresh
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
          // Clear the queue with the refresh error
          clearQueue(refreshError);

          // Logout the user since refresh token is invalid/expired
          tokenProvider.clearTokens();

          // Make sure we propagate an AuthenticationError
          if (refreshError instanceof Error) {
            return Promise.reject(
              new AuthenticationError(refreshError.message),
            );
          }

          return Promise.reject(
            new AuthenticationError('Failed to refresh token'),
          );
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

/**
 * Setup auth interceptors. These must be set up before general interceptors to handle token refresh.
 */
const setupAuthInterceptors = (tokenProvider: TokenProvider) => {
  const requestInterceptorId = axios.interceptors.request.use(
    addAuthHeader(tokenProvider),
  );
  const responseInterceptorId = axios.interceptors.response.use(
    response => response,
    handleAuthError(tokenProvider),
  );

  return {
    requestInterceptorId,
    responseInterceptorId,
  };
};

export { setupAuthInterceptors };
