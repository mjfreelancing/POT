import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

import useLocalStorage from '@/hooks/useLocalStorage';
import { DisplayError } from '@/lib';

import logoutManager from './logoutManager';
import { AUTH_STORAGE_KEY, type AuthTokens } from './types';

// Type for the AuthContext value
type AuthContextType = {
  tokens: AuthTokens | undefined;
  isAuthenticated: boolean;
  login: (tokens: AuthTokens) => void;
  logout: () => void;
  error?: DisplayError;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function AuthProvider({ children }: { children: ReactNode }) {
  const [error, setError] = useState<DisplayError | undefined>(undefined);
  const { getItem, setItem, removeItem } = useLocalStorage<AuthTokens>({
    key: AUTH_STORAGE_KEY,
    onError: setError,
  });

  // Read tokens from localStorage on mount
  const [tokens, setTokens] = useState<AuthTokens | undefined>(() => getItem());

  // Login: store tokens
  const login = useCallback(
    (newTokens: AuthTokens) => {
      setItem(newTokens);
      setTokens(newTokens);
    },
    [setItem],
  );

  // Logout: remove tokens
  const logout = useCallback(() => {
    removeItem();
    setTokens(undefined);
  }, [removeItem]);

  // isAuthenticated: true if accessToken exists
  const isAuthenticated = Boolean(tokens && tokens.accessToken);

  // Register logout callback with the logout manager
  useEffect(() => {
    logoutManager.setLogoutCallback(logout);
  }, [logout]);

  // Memoize context value
  const value = useMemo(
    () => ({
      tokens,
      isAuthenticated,
      login,
      logout,
      error,
    }),
    [tokens, isAuthenticated, login, logout, error],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
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
