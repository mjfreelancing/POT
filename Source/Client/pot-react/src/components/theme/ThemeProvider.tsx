import { createContext, useContext, useEffect, useState } from 'react';

import { purgeLegacyStorageKeys } from '@/concerns/storage';

const LEGACY_THEME_STORAGE_KEY = 'pot-ui-theme';

type Theme = 'dark' | 'light' | 'system';

type ThemeProviderProps = {
  children: React.ReactNode;
  defaultTheme?: Theme;
  storageKey?: string | null;
};

type ThemeProviderState = {
  theme: Theme;
  setTheme: (theme: Theme) => void;
};

const initialState: ThemeProviderState = {
  theme: 'system',
  setTheme: () => null,
};

const ThemeProviderContext = createContext<ThemeProviderState>(initialState);

// Reads the persisted theme for the given storage key, removing legacy keys so
// stale data does not persist under old keys. Shared by the initial state and
// the storage-key-change effect so both stay in sync.
function readThemeFromStorage(
  storageKey: string | null | undefined,
  defaultTheme: Theme,
): Theme {
  // TEMPORARY: remove legacy theme keys so stale data does not persist under old keys.
  purgeLegacyStorageKeys([
    { key: LEGACY_THEME_STORAGE_KEY, storage: localStorage },
  ]);

  if (!storageKey) {
    return defaultTheme;
  }

  return (localStorage.getItem(storageKey) as Theme) || defaultTheme;
}

function ThemeProvider({
  children,
  defaultTheme = 'system',
  storageKey = 'app-ui-theme',
  ...props
}: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(() =>
    readThemeFromStorage(storageKey, defaultTheme),
  );

  // Re-read the theme when the storage key (or default) changes — e.g. when the
  // signed-in user's identity resolves after mount and the key becomes
  // user-scoped. Previously App.tsx forced this re-read via a key={userId} on
  // the provider (remounting the whole app subtree incl. the error boundary);
  // later it moved to an effect, which the react-hooks rules discourage. Doing
  // it as a render-time state adjustment (documented "adjusting state when a
  // prop changes" pattern) avoids both the remount and the effect.
  const [themeSource, setThemeSource] = useState({ storageKey, defaultTheme });

  if (
    themeSource.storageKey !== storageKey ||
    themeSource.defaultTheme !== defaultTheme
  ) {
    setThemeSource({ storageKey, defaultTheme });
    setTheme(readThemeFromStorage(storageKey, defaultTheme));
  }

  useEffect(() => {
    const root = window.document.documentElement;

    root.classList.remove('light', 'dark');

    if (theme === 'system') {
      const systemTheme = window.matchMedia('(prefers-color-scheme: dark)')
        .matches
        ? 'dark'
        : 'light';

      root.classList.add(systemTheme);
      return;
    }

    root.classList.add(theme);
  }, [theme]);

  const value = {
    theme,
    setTheme: (theme: Theme) => {
      if (storageKey) {
        localStorage.setItem(storageKey, theme);
      }

      setTheme(theme);
    },
  };

  return (
    <ThemeProviderContext.Provider {...props} value={value}>
      {children}
    </ThemeProviderContext.Provider>
  );
}

const useTheme = () => {
  const context = useContext(ThemeProviderContext);

  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }

  return context;
};

export { ThemeProvider, useTheme };
export type { ThemeProviderProps };
