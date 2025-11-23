import axios from 'axios';

import { addCorrelationId } from './apiHelpers';
import {
  responseErrorHandler,
  responseSuccessHandler,
} from './interceptors/axiosInterceptors';

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
authClient.defaults.withCredentials = true; // Required to send HTTP-only cookies with requests

// Add request/response interceptors for consistent correlation ID and error handling
authClient.interceptors.request.use(addCorrelationId);
authClient.interceptors.response.use(
  responseSuccessHandler,
  responseErrorHandler,
);

export { authClient };
