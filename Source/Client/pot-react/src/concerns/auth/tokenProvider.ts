import { createAuthTokenProvider } from './authTokenProvider';

/**
 * Global token provider singleton.
 * Created once and shared across the application.
 * Used by axios interceptors and TokenContext for token management.
 */
export const tokenProvider = createAuthTokenProvider();
