import axios from 'axios';

import { setupAuthInterceptors } from './interceptors/authInterceptor';
import { setupInterceptors } from './interceptors/axiosInterceptors';
import type { TokenProvider } from './types/auth';

/**
 * Setup default axios configuration
 */
const setupAxiosDefaults = () => {
  axios.defaults.baseURL = import.meta.env.VITE_API_BASE_URL;
  axios.defaults.timeout = import.meta.env.VITE_API_TIMEOUT_MS;

  // Enable sending cookies with requests (required for HTTP-only refresh token cookie)
  axios.defaults.withCredentials = true;
};

/**
 * Setup all axios interceptors
 */
const setupAxiosInterceptors = (tokenProvider: TokenProvider) => {
  // Setup auth interceptors first
  const authInterceptors = setupAuthInterceptors(tokenProvider);

  // Setup general interceptors
  const generalInterceptors = setupInterceptors();

  return {
    ...authInterceptors,
    ...generalInterceptors,
  };
};

export { setupAxiosDefaults, setupAxiosInterceptors };
export type { TokenProvider };
