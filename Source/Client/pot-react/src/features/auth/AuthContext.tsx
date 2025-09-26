import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
} from 'react';

import { useMe } from '@/api/hooks/useMe';
import { User } from '@/data/user';
import { DisplayError } from '@/lib';
import { logger } from '@/lib/logging';

import { useUserStore } from '../../stores/useUserStore';
import logoutManager from './logoutManager';
import { TokenProvider, useTokens } from './TokenContext';
import {
  createTokenRefreshTimer,
  type TokenRefreshHandle,
} from './tokenRefreshTimer';
import type { AuthTokens } from './types';

// Feature layer of auth system - builds on TokenContext to provide:
// 1. Full authentication state with user info
// 2. Permission management
// 3. Automatic token refresh
// 4. Login/logout operations
type AuthContextType = {
  tokens: AuthTokens | undefined;
  userInfo: User | null;
  isAuthenticated: boolean;
  login: (tokens: AuthTokens) => void;
  logout: () => void;
  error?: DisplayError;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Split into separate component to ensure TokenProvider is mounted before
// attempting to use tokens or start the auth flow
function AuthProviderContent({ children }: { children: ReactNode }) {
  // Only fetch user info if we have tokens
  const { data: userInfo } = useMe();
  const userStore = useUserStore();
  const { tokens, setTokens } = useTokens();
  const prevUserInfoRef = useRef<typeof userInfo>(undefined);

  // Update permission store when userInfo changes
  useEffect(() => {
    // Only update if userInfo has actually changed and is successful
    if (userInfo?.success && userInfo !== prevUserInfoRef.current) {
      // Ensure we have valid user info before updating permissions
      const userDetails = userInfo.value;

      if (userDetails) {
        userStore.setUserInfo(userDetails);
        prevUserInfoRef.current = userInfo;
      } else {
        // If we don't have a valid user, clear permissions
        userStore.clearUserInfo();
      }
    }
  }, [userInfo, userStore]);

  // Login: store tokens

  const login = useCallback(
    (newTokens: AuthTokens) => {
      logger.info('Auth', 'User logged in');
      setTokens(newTokens);
    },
    [setTokens],
  );

  // Logout: remove tokens and clear permissions

  const logout = useCallback(() => {
    logger.info('Auth', 'User logged out');
    setTokens(undefined);
    userStore.clearUserInfo();
  }, [setTokens, userStore]);

  // Register logout callback with the logout manager
  useEffect(() => {
    logoutManager.setLogoutCallback(logout);
  }, [logout]);

  // Proactive token refresh system:
  // 1. Calculates optimal refresh time before token expires
  // 2. Ensures only one refresh timer runs at a time
  // 3. Cleans up on unmount or token changes
  const refreshTimerRef = useRef<TokenRefreshHandle | undefined>(undefined);

  // Setup refresh timer when tokens change
  // Critical: This must run after TokenContext is mounted and tokens are loaded
  useEffect(() => {
    if (tokens) {
      refreshTimerRef.current = createTokenRefreshTimer({
        currentTokens: tokens,
        onRefreshSuccess: login,
        onRefreshError: () => logout(),
      });

      refreshTimerRef.current.start();
    }

    return () => {
      refreshTimerRef.current?.stop();
    };
  }, [tokens, login, logout]);

  // Memoize context value
  const value = useMemo(
    () => ({
      tokens,
      userInfo: userStore.userInfo,
      isAuthenticated: Boolean(tokens?.accessToken),
      login,
      logout,
    }),
    [tokens, userStore, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

function AuthProvider({ children }: { children: ReactNode }) {
  return (
    <TokenProvider>
      <AuthProviderContent>{children}</AuthProviderContent>
    </TokenProvider>
  );
}

function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
}

export type { AuthContextType };
export { AuthProvider, useAuth };
