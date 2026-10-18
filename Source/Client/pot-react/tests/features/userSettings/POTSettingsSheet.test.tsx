import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useErrorContext } from '@/contexts';
import { useAuthContext } from '@/features/auth/contexts';
import useClearSession from '@/features/auth/hooks/useClearSession';
import useDeleteCurrentSite from '@/features/userSettings/delete/hooks/useDeleteCurrentSite';
import useDeleteOwnAccount from '@/features/userSettings/delete/hooks/useDeleteOwnAccount';
import { AccountSettingsSheet } from '@/features/userSettings/POTSettingsSheet';
import { usePermissions } from '@/hooks';

vi.mock('@/hooks', () => ({
  usePermissions: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/features/auth/hooks/useClearSession', () => ({
  default: vi.fn(),
}));

vi.mock('@/features/userSettings/delete/hooks/useDeleteOwnAccount', () => ({
  default: vi.fn(),
}));

vi.mock('@/features/userSettings/delete/hooks/useDeleteCurrentSite', () => ({
  default: vi.fn(),
}));

function renderSheet() {
  return render(
    <MemoryRouter>
      <AccountSettingsSheet open onClose={vi.fn()} />
    </MemoryRouter>,
  );
}

describe('POTSettingsSheet', () => {
  const setErrorMock = vi.fn();
  const deleteCurrentSiteMock = vi.fn();

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

    vi.mocked(useAuthContext).mockReturnValue({
      userInfo: { username: 'maria', site: { name: 'Site A' } },
    } as ReturnType<typeof useAuthContext>);

    vi.mocked(useClearSession).mockReturnValue(vi.fn());
    vi.mocked(useDeleteOwnAccount).mockReturnValue({
      deleteOwnAccount: vi.fn(),
    });
    vi.mocked(useDeleteCurrentSite).mockReturnValue({
      deleteCurrentSite: deleteCurrentSiteMock,
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

    renderSheet();

    // getByText throws if more than one ErrorSheet instance matches, so a
    // single match locks in the container-level single-render behavior.
    expect(screen.getByText('Settings Error')).toBeInTheDocument();
    expect(screen.getByText('Could not save settings')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });

  test('renders no error sheet when there is no shared error', () => {
    renderSheet();

    expect(
      screen.queryByRole('button', { name: 'Dismiss' }),
    ).not.toBeInTheDocument();
  });

  test('offers the account deletion action in a collapsed danger zone below the settings sections', () => {
    renderSheet();

    expect(
      screen.getByRole('button', { name: /Danger Zone/ }),
    ).toHaveAttribute('aria-expanded', 'false');
  });

  test('offers the site deletion action to a site administrator', async () => {
    renderSheet();

    await userEvent.click(screen.getByRole('button', { name: /Danger Zone/ }));

    expect(
      screen.getByRole('button', { name: 'Delete my account' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Delete this site' }),
    ).toBeInTheDocument();
  });

  test('omits the site deletion action without the site management permission', async () => {
    vi.mocked(usePermissions).mockReturnValue({
      hasPermission: (permission: string) => permission !== 'site:manage',
      hasAnyPermission: () => true,
    } as unknown as ReturnType<typeof usePermissions>);

    renderSheet();

    await userEvent.click(screen.getByRole('button', { name: /Danger Zone/ }));

    expect(
      screen.getByRole('button', { name: 'Delete my account' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Delete this site' }),
    ).not.toBeInTheDocument();
  });

  test('does not prompt for unsaved changes when the danger zone is opened', async () => {
    renderSheet();

    await userEvent.click(screen.getByRole('button', { name: /Danger Zone/ }));
    await userEvent.click(
      screen.getByRole('button', { name: 'Delete my account' }),
    );

    expect(screen.queryByText('Unsaved Changes')).not.toBeInTheDocument();
    expect(screen.getByRole('alertdialog')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument();
  });

  test('enables the confirmation once the signed-in username and password are typed', async () => {
    renderSheet();

    await userEvent.click(screen.getByRole('button', { name: /Danger Zone/ }));
    await userEvent.click(
      screen.getByRole('button', { name: 'Delete my account' }),
    );

    const confirmButton = screen.getByRole('button', { name: 'Delete Account' });

    expect(confirmButton).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Username'), 'maria');

    expect(confirmButton).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Your password'), 'secret');

    expect(confirmButton).toBeEnabled();
  });
});
