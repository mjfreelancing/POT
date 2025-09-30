import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useMemo,
  useState,
} from 'react';

import { useErrorContext } from '@/contexts';
import useLocalStorage from '@/hooks/useLocalStorage';
import { DisplayError } from '@/lib';

import { AUTH_STORAGE_KEY, type AuthTokens } from './types';

// Base layer of auth system - handles only token storage and basic auth state
// This separation allows other parts of the app to access tokens without creating
// circular dependencies with the full auth system
type TokenContextType = {
  tokens: AuthTokens | undefined;
  isAuthenticated: boolean;
  setTokens: (tokens: AuthTokens | undefined) => void;
  error?: DisplayError | null;
};

const TokenContext = createContext<TokenContextType | undefined>(undefined);

function TokenProvider({ children }: { children: ReactNode }) {
  const { error, setError } = useErrorContext();
  const { getItem, setItem, removeItem } = useLocalStorage<AuthTokens>({
    key: AUTH_STORAGE_KEY,
    onError: setError,
  });

  // Read tokens from localStorage on mount
  const [tokens, setTokensState] = useState<AuthTokens | undefined>(() =>
    getItem(),
  );

  // Handle setting tokens
  const setTokens = useCallback(
    (newTokens: AuthTokens | undefined) => {
      if (newTokens) {
        setItem(newTokens);
      } else {
        removeItem();
      }
      setTokensState(newTokens);
    },
    [setItem, removeItem],
  );

  // isAuthenticated: true if accessToken exists
  const isAuthenticated = Boolean(tokens && tokens.accessToken);

  // Memoize context value
  const value = useMemo(
    () => ({
      tokens,
      isAuthenticated,
      setTokens,
      error,
    }),
    [tokens, isAuthenticated, setTokens, error],
  );

  return (
    <TokenContext.Provider value={value}>{children}</TokenContext.Provider>
  );
}

function useTokens() {
  const context = useContext(TokenContext);

  if (!context) {
    throw new Error('useTokens must be used within a TokenProvider');
  }

  return context;
}

export type { TokenContextType };
export { TokenProvider, useTokens };
