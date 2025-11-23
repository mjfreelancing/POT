import type { LogoutManager } from '@/features/auth/types';

/**
 * Simple logout manager that holds a callback to be invoked when logout is requested.
 * This allows components like AuthContext to register their logout handler while
 * other parts of the app can trigger logout without direct dependency on AuthContext.
 */
const logoutManager: LogoutManager = (() => {
  let logoutCallback: (() => void) | undefined;

  return {
    setLogoutCallback: (callback: () => void) => {
      logoutCallback = callback;
    },
    logout: () => {
      logoutCallback?.();
    },
  };
})();

export default logoutManager;
