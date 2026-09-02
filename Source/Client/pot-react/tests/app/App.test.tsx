import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { type ReactNode, useEffect } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import App from '@/App';
import { logger } from '@/concerns';
import { useUserStore } from '@/stores';

let shouldThrowFromRoutes = false;
const themeProviderMounts = vi.fn();
const themeProviderUnmounts = vi.fn();

vi.mock('@/components/nav', () => ({
  AppSidebar: () => <div data-testid="app-sidebar">Sidebar</div>,
}));

vi.mock('@/components/ui/sidebar', () => ({
  SidebarProvider: ({ children }: { children: ReactNode }) => (
    <div data-testid="sidebar-provider">{children}</div>
  ),
}));

vi.mock('@/contexts', () => ({
  ErrorProvider: ({ children }: { children: ReactNode }) => (
    <div data-testid="error-provider">{children}</div>
  ),
}));

vi.mock('@/features/auth/contexts', () => ({
  AuthProvider: ({ children }: { children: ReactNode }) => (
    <div data-testid="auth-provider">{children}</div>
  ),
}));

vi.mock('@/components/theme', () => ({
  ThemeProvider: ({
    children,
    defaultTheme,
    storageKey,
  }: {
    children: ReactNode;
    defaultTheme: string;
    storageKey: string | null;
  }) => {
    useEffect(() => {
      themeProviderMounts(storageKey);

      return () => {
        themeProviderUnmounts(storageKey);
      };
    }, []);

    return (
      <div
        data-testid="theme-provider"
        data-default-theme={defaultTheme}
        data-storage-key={storageKey}
      >
        {children}
      </div>
    );
  },
}));

vi.mock('@/routes/AppRoutes', () => ({
  AppRoutes: () => {
    if (shouldThrowFromRoutes) {
      throw new Error('Route render failed');
    }

    return <div data-testid="app-routes">Routes content</div>;
  },
}));

vi.mock('@/components/ui/sonner', () => ({
  Toaster: ({ position }: { position: string }) => (
    <div data-testid="toaster">{position}</div>
  ),
}));

vi.mock('@/components/feedback', () => ({
  ErrorSheet: ({
    title,
    description,
    onDismiss,
  }: {
    title: string;
    description: string;
    onDismiss: () => void;
  }) => (
    <div data-testid="error-sheet">
      <p>{title}</p>
      <p>{description}</p>
      <button type="button" onClick={onDismiss}>
        Dismiss
      </button>
    </div>
  ),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('@/stores', () => ({
  useUserStore: vi.fn().mockReturnValue(null),
}));

describe('App', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    shouldThrowFromRoutes = false;
    themeProviderMounts.mockClear();
    themeProviderUnmounts.mockClear();
    vi.mocked(useUserStore).mockReturnValue(null);
  });

  test('wires global providers and app shell components', () => {
    render(<App />);

    expect(screen.getByTestId('error-provider')).toBeInTheDocument();
    expect(screen.getByTestId('auth-provider')).toBeInTheDocument();
    expect(screen.getByTestId('theme-provider')).toHaveAttribute(
      'data-default-theme',
      'system',
    );
    expect(screen.getByTestId('theme-provider')).toHaveAttribute(
      'data-storage-key',
      'pot:dev:theme',
    );
    expect(screen.getByTestId('sidebar-provider')).toBeInTheDocument();
    expect(screen.getByTestId('app-sidebar')).toBeInTheDocument();
    expect(screen.getByTestId('app-routes')).toBeInTheDocument();
    expect(screen.getByTestId('toaster')).toHaveTextContent('top-center');

    expect(logger.info).toHaveBeenCalledWith(
      'App',
      expect.stringContaining('Running mode:'),
    );
  });

  test('passes user-scoped theme storage key when user is authenticated', () => {
    vi.mocked(useUserStore).mockReturnValue('user-row-id-abc');

    render(<App />);

    expect(screen.getByTestId('theme-provider')).toHaveAttribute(
      'data-storage-key',
      'pot:dev:user:user-row-id-abc:theme',
    );
  });

  test('does not remount ThemeProvider when auth state changes — storage key is re-read reactively', () => {
    const { rerender } = render(<App />);

    expect(themeProviderMounts).toHaveBeenCalledWith('pot:dev:theme');

    vi.mocked(useUserStore).mockReturnValue('user-row-id-abc');
    rerender(<App />);

    // The provider stays mounted: ThemeProvider re-reads its theme from the
    // new storage key internally, so the app subtree is not torn down.
    expect(themeProviderUnmounts).not.toHaveBeenCalled();
    expect(themeProviderMounts).toHaveBeenCalledTimes(1);
    expect(screen.getByTestId('theme-provider')).toHaveAttribute(
      'data-storage-key',
      'pot:dev:user:user-row-id-abc:theme',
    );

    vi.mocked(useUserStore).mockReturnValue(null);
    rerender(<App />);

    expect(themeProviderUnmounts).not.toHaveBeenCalled();
    expect(themeProviderMounts).toHaveBeenCalledTimes(1);
    expect(screen.getByTestId('theme-provider')).toHaveAttribute(
      'data-storage-key',
      'pot:dev:theme',
    );
  });

  test('shows global error fallback and error sheet when route rendering throws', async () => {
    shouldThrowFromRoutes = true;

    render(<App />);

    const fallbackAlert = await screen.findByRole('alert');

    expect(fallbackAlert).toBeInTheDocument();
    expect(screen.getByText('Something went wrong !')).toBeInTheDocument();
    expect(
      within(fallbackAlert).getByText('Route render failed'),
    ).toBeInTheDocument();
    expect(screen.getByTestId('error-sheet')).toBeInTheDocument();
    expect(screen.getByText('Application Error')).toBeInTheDocument();

    await waitFor(() => {
      expect(logger.error).toHaveBeenCalledWith(
        'App',
        'Error boundary caught an error',
        expect.any(Error),
      );
    });
  });

  test('dismisses error sheet after an application error is surfaced', async () => {
    const user = userEvent.setup();
    shouldThrowFromRoutes = true;

    render(<App />);

    expect(await screen.findByTestId('error-sheet')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Dismiss' }));

    await waitFor(() => {
      expect(screen.queryByTestId('error-sheet')).not.toBeInTheDocument();
    });
  });
});
