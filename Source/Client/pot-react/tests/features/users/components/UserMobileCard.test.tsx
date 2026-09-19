import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import UserMobileCard from '@/features/users/components/UserMobileCard';
import { usePermissions } from '@/hooks';
import { formatDateTime } from '@/lib';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createSiteUser } from '../../../shared/factories/siteUserFactory';

vi.mock('@/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/hooks')>();

  return {
    ...actual,
    usePermissions: vi.fn(),
  };
});

describe('UserMobileCard', () => {
  const onChangeRoleMock = vi.fn();
  const onToggleStatusMock = vi.fn();
  const onResendInvitationMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );
  });

  test('renders the display name, username, email, role, and status', () => {
    const user = createSiteUser({
      displayName: 'Maria Carter',
      username: 'maria',
      email: 'maria@example.com',
      roles: ['Admin'],
      status: 'Enabled',
      lastLoggedInUtc: null,
    });

    render(<UserMobileCard user={user} onChangeRole={onChangeRoleMock} />);

    expect(screen.getByText('Maria Carter')).toBeInTheDocument();
    expect(screen.getByText('maria')).toBeInTheDocument();

    expect(screen.getByText('Email:')).toBeInTheDocument();
    expect(screen.getByText('maria@example.com')).toBeInTheDocument();

    expect(screen.getByText('Admin')).toBeInTheDocument();
    expect(screen.getByText('Enabled')).toBeInTheDocument();
    expect(screen.getByText('Never logged in')).toBeInTheDocument();
  });

  test('renders the formatted last login when the user has logged in', () => {
    const user = createSiteUser({ lastLoggedInUtc: '2026-03-04T05:06:07Z' });

    render(<UserMobileCard user={user} onChangeRole={onChangeRoleMock} />);

    const expectedLastLogin = `Last login: ${formatDateTime('2026-03-04T05:06:07Z')}`;

    expect(screen.getByText(expectedLastLogin)).toBeInTheDocument();
  });

  test('hides the action menu for the current user', () => {
    const user = createSiteUser();

    render(
      <UserMobileCard
        user={user}
        onChangeRole={onChangeRoleMock}
        isCurrentUser
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Open menu' }),
    ).not.toBeInTheDocument();
  });

  test('invokes onChangeRole from the action menu', async () => {
    const user = createSiteUser();

    render(<UserMobileCard user={user} onChangeRole={onChangeRoleMock} />);

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Change Role' }),
    );

    expect(onChangeRoleMock).toHaveBeenCalledWith(user);
  });

  test('offers Disable User for an enabled user', async () => {
    const user = createSiteUser({ status: 'Enabled' });

    render(
      <UserMobileCard
        user={user}
        onChangeRole={onChangeRoleMock}
        onToggleStatus={onToggleStatusMock}
      />,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Disable User' }),
    );

    expect(onToggleStatusMock).toHaveBeenCalledWith(user);
  });

  test('offers Resend Invitation for a pending user', async () => {
    const user = createSiteUser({ status: 'Pending' });

    render(
      <UserMobileCard
        user={user}
        onChangeRole={onChangeRoleMock}
        onResendInvitation={onResendInvitationMock}
      />,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Resend Invitation' }),
    );

    expect(onResendInvitationMock).toHaveBeenCalledWith(user);
  });
});
