import type { AuthTokens } from '@/api/types/auth';

/**
 * Manages the logout lifecycle
 */
type LogoutManager = {
  /**
   * Sets the callback to be invoked when logout is requested
   * @param callback The function to call when logout occurs
   */
  setLogoutCallback: (callback: () => void) => void;

  /**
   * Triggers the logout process by calling the registered callback
   */
  logout: () => void;
};

const AUTH_STORAGE_KEY = 'pot-auth' as const;

export type { AuthTokens, LogoutManager };
export { AUTH_STORAGE_KEY };
