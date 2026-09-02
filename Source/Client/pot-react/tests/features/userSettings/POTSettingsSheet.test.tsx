import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useErrorContext } from '@/contexts';
import { AccountSettingsSheet } from '@/features/userSettings/POTSettingsSheet';
import { usePermissions } from '@/hooks';

vi.mock('@/hooks', () => ({
  usePermissions: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

describe('POTSettingsSheet', () => {
  const setErrorMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue({
      hasPermission: () => true,
      hasAnyPermission: () => true,
    } as unknown as ReturnType<typeof usePermissions>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });
  });

  test('renders a single error sheet from the shared error context and dismisses it', async () => {
    vi.mocked(useErrorContext).mockReturnValue({
      error: {
        title: 'Settings Error',
        description: 'Could not save settings',
      },
      setError: setErrorMock,
    });

    render(<AccountSettingsSheet open onClose={vi.fn()} />);

    // getByText throws if more than one ErrorSheet instance matches, so a
    // single match locks in the container-level single-render behavior.
    expect(screen.getByText('Settings Error')).toBeInTheDocument();
    expect(screen.getByText('Could not save settings')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });

  test('renders no error sheet when there is no shared error', () => {
    render(<AccountSettingsSheet open onClose={vi.fn()} />);

    expect(
      screen.queryByRole('button', { name: 'Dismiss' }),
    ).not.toBeInTheDocument();
  });
});
