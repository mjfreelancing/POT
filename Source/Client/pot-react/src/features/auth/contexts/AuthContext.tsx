import { useQueryClient } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
} from 'react';

import { useMe } from '@/api/hooks';
import { useLogout } from '@/api/hooks/useAuth';
import { logger, logoutManager } from '@/concerns';
import type { User } from '@/data/user';
import type { DisplayError } from '@/lib';
import { useUserStore } from '@/stores';

import type { TokenRefreshHandle } from '../tokenRefreshTimer';
import { createTokenRefreshTimer } from '../tokenRefreshTimer';
import { AccessTokenProvider, useAccessToken } from './AccessTokenContext';

// Feature layer of auth system - builds on AccessTokenContext to provide:
// 1. Full authentication state with user info
// 2. Permission management
// 3. Automatic token refresh
// 4. Login/logout operations
type AuthContextType = {
  accessToken: string | undefined;
  userInfo: User | null;
  isAuthenticated: boolean;
  isInitialized: boolean;
  login: (accessToken: string) => void;
  logout: () => void;
  error?: DisplayError;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Split into separate component to ensure AccessTokenProvider is mounted before
// attempting to use tokens or start the auth flow
function AuthProviderContent({ children }: { children: ReactNode }) {
  // Only fetch user info if we have access token
  const { data: userInfo } = useMe();
  const userStore = useUserStore();
  const { accessToken, setAccessToken, isInitialized } = useAccessToken();
  const logoutMutation = useLogout();
  const queryClient = useQueryClient();
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

  // Login: store access token
  const login = useCallback(
    (token: string) => {
      logger.info('Auth', 'User logged in');

      setAccessToken(token);
    },
    [setAccessToken],
  );

  // Logout: remove tokens and clear permissions
  const logout = useCallback(async () => {
    // log out from the server first since we need the auth token
    try {
      await logoutMutation.mutateAsync({});

      logger.info('Auth', 'Logged out from the server');
    } catch (error) {
      logger.error('Auth', 'Error while logging out from the server', error);
    }

    logger.info('Auth', 'User logged out');

    setAccessToken(undefined);
    userStore.clearUserInfo();

    // Clear all React Query cache to prevent data leakage between users
    queryClient.clear();
  }, [logoutMutation, queryClient, setAccessToken, userStore]);

  // Register logout callback with the logout manager
  useEffect(() => {
    logoutManager.setLogoutCallback(logout);
  }, [logout]);

  // isAuthenticated: true if access token exists
  const isAuthenticated = Boolean(accessToken);

  // Proactive token refresh system:
  // 1. Calculates optimal refresh time before token expires
  // 2. Ensures only one refresh timer runs at a time
  // 3. Cleans up on unmount or token changes
  const refreshTimerRef = useRef<TokenRefreshHandle | undefined>(undefined);

  // Setup refresh timer when access token changes
  // Critical: This must run after AccessTokenContext is mounted and token is loaded
  useEffect(() => {
    if (accessToken) {
      refreshTimerRef.current = createTokenRefreshTimer({
        currentAccessToken: accessToken,
        onRefreshSuccess: login,
        onRefreshError: () => logout(),
      });

      refreshTimerRef.current.start();
    }

    return () => {
      refreshTimerRef.current?.stop();
    };
  }, [accessToken, login, logout]);

  // Memoize context value
  const value = useMemo(
    () => ({
      accessToken,
      userInfo: userStore.userInfo,
      isAuthenticated,
      isInitialized,
      login,
      logout,
    }),
    [
      accessToken,
      userStore.userInfo,
      isAuthenticated,
      isInitialized,
      login,
      logout,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

function AuthProvider({ children }: { children: ReactNode }) {
  return (
    <AccessTokenProvider>
      <AuthProviderContent>{children}</AuthProviderContent>
    </AccessTokenProvider>
  );
}

function useAuthContext() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
}

export { AuthProvider };
export type { AuthContextType };
export default useAuthContext;
