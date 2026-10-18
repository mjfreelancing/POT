import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { ConflictError } from '@/api/errors/apiErrors';
import { useResendInvitation, useUpdateUserStatus } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import { useAuthContext } from '@/features/auth/contexts';
import UserActions from '@/features/users/components/UserActions';
import { usePermissions } from '@/hooks';
import { FailResult } from '@/lib';

import {
  createPermissionsApi,
  type PermissionsApi,
} from '../../../shared/auth/permissionsTestHelpers';
import { createSiteUser } from '../../../shared/factories/siteUserFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

const setErrorMock = vi.fn();
const resendInvitationMutateAsyncMock = vi.fn();
const updateStatusMutateAsyncMock = vi.fn();

vi.mock('@/api/hooks', () => ({
  useResendInvitation: vi.fn(),
  useUpdateUserStatus: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
  useCacheInvalidation: vi.fn(() => vi.fn()),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/hooks', () => ({
  usePermissions: vi.fn(),
}));

function createPermittedApi(): PermissionsApi {
  const permissionsApi = createPermissionsApi();

  permissionsApi.hasPermission.mockReturnValue(true);

  return permissionsApi;
}

function renderUserActions(children: ReactNode) {
  const queryClient = createQueryClient();

  return render(
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>,
  );
}

describe('UserActions', () => {
  const onChangeRoleMock = vi.fn();
  const onDeleteUserMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(createPermittedApi());

    vi.mocked(useAuthContext).mockReturnValue({
      userInfo: { username: 'manager' },
    } as ReturnType<typeof useAuthContext>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    vi.mocked(useResendInvitation).mockReturnValue({
      mutateAsync: resendInvitationMutateAsyncMock,
    } as unknown as ReturnType<typeof useResendInvitation>);

    vi.mocked(useUpdateUserStatus).mockReturnValue({
      mutateAsync: updateStatusMutateAsyncMock,
    } as unknown as ReturnType<typeof useUpdateUserStatus>);
  });

  test('offers Delete User as the last action with destructive styling', async () => {
    const user = createSiteUser();

    renderUserActions(
      <UserActions
        user={user}
        onChangeRole={onChangeRoleMock}
        onDeleteUser={onDeleteUserMock}
      />,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));

    const menuItems = screen.getAllByRole('menuitem');
    const deleteItem = screen.getByRole('menuitem', { name: 'Delete User…' });

    expect(menuItems[menuItems.length - 1]).toBe(deleteItem);
    expect(deleteItem).toHaveClass('text-destructive-high-contrast');
  });

  test('reports the user to delete', async () => {
    const user = createSiteUser();

    renderUserActions(
      <UserActions
        user={user}
        onChangeRole={onChangeRoleMock}
        onDeleteUser={onDeleteUserMock}
      />,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Delete User…' }),
    );

    expect(onDeleteUserMock).toHaveBeenCalledWith(user);
  });

  test('omits Delete User when no handler is supplied', async () => {
    const user = createSiteUser();

    renderUserActions(
      <UserActions user={user} onChangeRole={onChangeRoleMock} />,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));

    expect(
      screen.queryByRole('menuitem', { name: 'Delete User…' }),
    ).not.toBeInTheDocument();
  });

  test('hides the action menu for the current user', () => {
    const user = createSiteUser({ username: 'manager' });

    renderUserActions(
      <UserActions
        user={user}
        onChangeRole={onChangeRoleMock}
        onDeleteUser={onDeleteUserMock}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Open menu' }),
    ).not.toBeInTheDocument();
  });

  test('hides the action menu without the user management permission', () => {
    vi.mocked(usePermissions).mockReturnValue(createPermissionsApi());

    const user = createSiteUser();

    renderUserActions(
      <UserActions
        user={user}
        onChangeRole={onChangeRoleMock}
        onDeleteUser={onDeleteUserMock}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Open menu' }),
    ).not.toBeInTheDocument();
  });

  test('reports a failed status change to the error context', async () => {
    const failure = new FailResult(
      new ConflictError('The user was changed by someone else.'),
    );

    updateStatusMutateAsyncMock.mockResolvedValue(failure);

    const user = createSiteUser();

    renderUserActions(
      <UserActions user={user} onChangeRole={onChangeRoleMock} />,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Disable User' }),
    );

    expect(setErrorMock).toHaveBeenCalledWith({
      title: failure.error.code,
      description: failure.error.description,
    });
  });
});
