import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { ThemeProvider, useTheme } from '@/components/theme/ThemeProvider';

function ThemeProbe() {
  const { theme, setTheme } = useTheme();

  return (
    <div>
      <span data-testid="theme-value">{theme}</span>
      <button type="button" onClick={() => setTheme('dark')}>
        Set dark
      </button>
    </div>
  );
}

describe('ThemeProvider', () => {
  beforeEach(() => {
    localStorage.clear();

    vi.stubGlobal(
      'matchMedia',
      vi.fn().mockImplementation(() => ({
        matches: false,
        media: '',
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    );
  });

  test('purges legacy pot-ui-theme key and reads from provided storageKey', () => {
    localStorage.setItem('pot-ui-theme', 'dark');
    localStorage.setItem('pot:dev:user:abc:theme', 'light');

    render(
      <ThemeProvider storageKey="pot:dev:user:abc:theme">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(localStorage.getItem('pot-ui-theme')).toBeNull();
    expect(screen.getByTestId('theme-value')).toHaveTextContent('light');
  });

  test('supports a global pre-login theme key', () => {
    localStorage.setItem('pot:dev:theme', 'dark');

    render(
      <ThemeProvider storageKey="pot:dev:theme">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(screen.getByTestId('theme-value')).toHaveTextContent('dark');
  });

  test('setTheme persists to provided storageKey', async () => {
    const user = userEvent.setup();

    render(
      <ThemeProvider storageKey="pot:dev:user:abc:theme">
        <ThemeProbe />
      </ThemeProvider>,
    );

    await user.click(screen.getByRole('button', { name: 'Set dark' }));

    expect(localStorage.getItem('pot:dev:user:abc:theme')).toBe('dark');
    expect(screen.getByTestId('theme-value')).toHaveTextContent('dark');
  });

  test('does not purge the env-scoped global theme key', () => {
    localStorage.setItem('pot-ui-theme', 'dark');
    localStorage.setItem('pot:dev:theme', 'light');

    render(
      <ThemeProvider storageKey="pot:dev:theme">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(localStorage.getItem('pot-ui-theme')).toBeNull();
    expect(localStorage.getItem('pot:dev:theme')).toBe('light');
    expect(screen.getByTestId('theme-value')).toHaveTextContent('light');
  });

  test('re-reads theme when storageKey changes without remounting', () => {
    localStorage.setItem('pot:dev:theme', 'dark');
    localStorage.setItem('pot:dev:user:abc:theme', 'light');

    const { rerender } = render(
      <ThemeProvider storageKey="pot:dev:theme">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(screen.getByTestId('theme-value')).toHaveTextContent('dark');

    rerender(
      <ThemeProvider storageKey="pot:dev:user:abc:theme">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(screen.getByTestId('theme-value')).toHaveTextContent('light');
  });

  test('uses defaultTheme when storageKey is null', () => {
    render(
      <ThemeProvider storageKey={null} defaultTheme="dark">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(screen.getByTestId('theme-value')).toHaveTextContent('dark');
  });

  test('setTheme updates state but does not persist when storageKey is null', async () => {
    const user = userEvent.setup();

    render(
      <ThemeProvider storageKey={null} defaultTheme="light">
        <ThemeProbe />
      </ThemeProvider>,
    );

    await user.click(screen.getByRole('button', { name: 'Set dark' }));

    expect(screen.getByTestId('theme-value')).toHaveTextContent('dark');
    expect(localStorage.length).toBe(0);
  });
});
