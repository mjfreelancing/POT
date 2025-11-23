import type { ReactNode } from 'react';
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

import { refreshAccessToken } from '@/api/services/authService';
import { logger, tokenProvider } from '@/concerns';
import type { DisplayError } from '@/lib';

// Base layer of auth system - handles only token storage and basic auth state.
// This separation allows other parts of the app to access tokens without creating
// circular dependencies with the full auth system.
// Note: Access token stored ONLY in memory - refreshToken lives in HTTP-only cookie,
// meaning token is lost on page refresh, but auto-refresh restores authentication.
type AccessTokenContextType = {
  accessToken: string | undefined;
  isAuthenticated: boolean;
  isInitialized: boolean;
  setAccessToken: (token: string | undefined) => void;
  error?: DisplayError | null;
};

const AccessTokenContext = createContext<AccessTokenContextType | undefined>(
  undefined,
);

function AccessTokenProvider({ children }: { children: ReactNode }) {
  // Store access token ONLY in memory (React state) - not in localStorage
  const [accessToken, setAccessTokenState] = useState<string | undefined>(
    undefined,
  );
  const [error] = useState<DisplayError | null>(null);

  // Track initialization state for auth recovery on page load
  const [isInitialized, setIsInitialized] = useState(false);
  const hasInitialized = useRef(false);

  // Handle setting access token (only in memory)
  const setAccessToken = useCallback((token: string | undefined) => {
    setAccessTokenState(token);

    // Sync with global token provider for axios interceptors
    tokenProvider.setAccessToken(token);
  }, []);

  // Auto-refresh on mount if no access token but refresh cookie might exist
  useEffect(() => {
    // Only run once on mount - using ref guard to prevent re-runs when dependencies change
    // This ensures we only attempt token refresh on initial load, not when tokens update
    if (hasInitialized.current) {
      return;
    }

    hasInitialized.current = true;

    const initializeAuth = async () => {
      logger.info('AccessTokenContext', 'Initializing authentication');

      // If we have access token in memory, we're good
      if (accessToken) {
        logger.info('AccessTokenContext', 'Access token found in memory');

        setIsInitialized(true);
        return;
      }

      // No access token - try to refresh from HTTP-only cookie
      logger.info(
        'AccessTokenContext',
        'No access token in memory, attempting refresh from cookie',
      );

      try {
        const result = await refreshAccessToken();

        if (result.success) {
          logger.info(
            'AccessTokenContext',
            'Successfully refreshed token from cookie',
          );

          setAccessToken(result.value.accessToken);
        } else {
          logger.info('AccessTokenContext', 'No valid refresh cookie found');
        }
      } catch (error) {
        logger.error(
          'AccessTokenContext',
          'Failed to refresh token from cookie',
          error,
        );
      }

      setIsInitialized(true);
    };

    initializeAuth();
  }, [accessToken, setAccessToken]);

  // isAuthenticated: true if accessToken exists
  const isAuthenticated = Boolean(accessToken);

  // Memoize context value
  const value = useMemo(
    () => ({
      accessToken,
      isAuthenticated,
      isInitialized,
      setAccessToken,
      error,
    }),
    [accessToken, isAuthenticated, isInitialized, setAccessToken, error],
  );

  return (
    <AccessTokenContext.Provider value={value}>
      {children}
    </AccessTokenContext.Provider>
  );
}

function useAccessToken() {
  const context = useContext(AccessTokenContext);

  if (!context) {
    throw new Error(
      'useAccessToken must be used within an AccessTokenProvider',
    );
  }

  return context;
}

export type { AccessTokenContextType };
export { AccessTokenProvider, useAccessToken };
