import type { AxiosResponse } from 'axios';
import axios from 'axios';

import { addCorrelationId } from './apiHelpers';
import { AuthenticationError } from './errors/apiErrors';
import {
  responseErrorHandler,
  responseSuccessHandler,
} from './interceptors/axiosInterceptors';
import type { AuthTokens, RefreshTokenRequest } from './types/auth';

/**
 * We use a separate axios instance for auth operations instead of usePost/useRefreshToken hooks because:
 * 1. Token refresh needs to work outside of React components (in interceptors)
 * 2. Using hooks would create a circular dependency since the token refresh
 *    would try to use the same interceptors that triggered it
 * 3. Token refresh logic should be independent of React's component lifecycle
 */
const authClient = axios.create();

// Configure the auth client
authClient.defaults.baseURL = import.meta.env.VITE_API_BASE_URL;

// Add request/response interceptors for consistent correlation ID and error handling
authClient.interceptors.request.use(addCorrelationId);
authClient.interceptors.response.use(
  responseSuccessHandler,
  responseErrorHandler,
);

/**
 * Refresh the access token using a refresh token
 */
async function refreshTokens(
  request: RefreshTokenRequest,
  expiredToken: string,
): Promise<AuthTokens> {
  try {
    const response: AxiosResponse<AuthTokens> = await authClient.post(
      '/auth/refresh',
      request,
      {
        headers: {
          Authorization: `Bearer ${expiredToken}`,
        },
      },
    );

    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error) && error.response) {
      const message = error.response.data?.message ?? error.message;
      throw new AuthenticationError(`Failed to refresh token: ${message}`);
    }

    throw new AuthenticationError(
      `Failed to refresh token: ${error instanceof Error ? error.message : 'Unknown error'}`,
    );
  }
}

export { authClient, refreshTokens };
