import { useQueryClient } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import {
  useResendInvitation,
  useUpdateUserStatus,
  useUsers,
} from '@/api/hooks';
import { useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { SiteUser } from '@/data/siteUser';
import { useAuthContext } from '@/features/auth/contexts';
import UsersPage from '@/features/users/UsersPage';
import { useIsMobile, usePermissions } from '@/hooks';
import { FailResult, SuccessResult } from '@/lib';

import { createPermissionsApi } from '../../shared/auth/permissionsTestHelpers';

const navigateMock = vi.fn();
const invalidateCacheMock = vi.fn();
const toastMock = vi.fn();

vi.mock('react-router', async importOriginal => {
  const actual = await importOriginal<typeof import('react-router')>();

  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

vi.mock('@tanstack/react-query', async importOriginal => {
  const actual = await importOriginal<typeof import('@tanstack/react-query')>();

  return {
    ...actual,
    useQueryClient: vi.fn(),
  };
});

vi.mock('sonner', () => ({
  toast: vi.fn(),
}));

vi.mock('@/api/hooks', () => ({
  useUsers: vi.fn(),
  useUpdateUserStatus: vi.fn(),
  useResendInvitation: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
  useCacheInvalidation: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/hooks', () => ({
  useIsMobile: vi.fn(),
  usePermissions: vi.fn(),
}));

vi.mock('@/components/layout', () => ({
  PageHeader: ({ title }: { title: string }) => <h1>{title}</h1>,
  Toolbar: ({ children }: { children: ReactNode }) => <div>{children}</div>,
}));

vi.mock('@/features/auth/components', () => ({
  WithPermission: ({ children }: { children: ReactNode }) => <>{children}</>,
}));

vi.mock('@/features/users/components', () => ({
  UsersTable: ({
    users,
    onChangeRole,
  }: {
    users: SiteUser[];
    onChangeRole: (user: SiteUser) => void;
  }) => (
    <div>
      <div>{`table:${users.map(user => user.username).join('|')}`}</div>
      <button type="button" onClick={() => onChangeRole(users[0])}>
        Change role for first user
      </button>
    </div>
  ),
  UserCardGrid: ({
    users,
    onChangeRole,
    onToggleStatus,
    onResendInvitation,
    currentUsername,
  }: {
    users: SiteUser[];
    onChangeRole: (user: SiteUser) => void;
    onToggleStatus?: (user: SiteUser) => Promise<void> | void;
    onResendInvitation?: (user: SiteUser) => Promise<void> | void;
    currentUsername?: string;
  }) => (
    <div>
      <div>{`cards:${users.map(user => user.username).join('|')}`}</div>
      <div>{`current:${currentUsername ?? 'none'}`}</div>
      <button type="button" onClick={() => onChangeRole(users[0])}>
        Mobile change role
      </button>
      <button type="button" onClick={() => onToggleStatus?.(users[0])}>
        Toggle first status
      </button>
      <button type="button" onClick={() => onResendInvitation?.(users[0])}>
        Resend first invitation
      </button>
    </div>
  ),
  UserRoleDialog: ({
    user,
    isOpen,
  }: {
    user: SiteUser | null;
    isOpen: boolean;
  }) => (isOpen ? <div>{`role-dialog:${user?.username}`}</div> : null),
}));

vi.mock('@/components/feedback/toast', () => ({
  SuccessToast: ({
    title,
    description,
  }: {
    title: string;
    description: string;
  }) => <div>{`${title}:${description}`}</div>,
}));

function renderUsersPage() {
  return render(
    <MemoryRouter>
      <UsersPage />
    </MemoryRouter>,
  );
}

describe('UsersPage', () => {
  const setErrorMock = vi.fn();
  const updateStatusMutateAsyncMock = vi.fn();
  const resendInvitationMutateAsyncMock = vi.fn();

  const users: SiteUser[] = [
    {
      rowId: '11111111-1111-1111-1111-111111111111',
      etag: 1n,
      username: 'maria',
      displayName: 'Maria Carter',
      email: 'maria@example.com',
      roles: ['Admin'],
      status: 'Enabled',
      lastLoggedInUtc: null,
    },
    {
      rowId: '22222222-2222-2222-2222-222222222222',
      etag: 2n,
      username: 'alex',
      displayName: 'Alex Pine',
      email: 'alex@example.com',
      roles: ['Viewer'],
      status: 'Pending',
      lastLoggedInUtc: null,
    },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();

    vi.mocked(useUsers).mockReturnValue({
      data: new SuccessResult(users),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useUsers>);

    updateStatusMutateAsyncMock.mockResolvedValue(new SuccessResult(undefined));
    resendInvitationMutateAsyncMock.mockResolvedValue(
      new SuccessResult(undefined),
    );

    vi.mocked(useUpdateUserStatus).mockReturnValue({
      mutateAsync: updateStatusMutateAsyncMock,
    } as unknown as ReturnType<typeof useUpdateUserStatus>);

    vi.mocked(useResendInvitation).mockReturnValue({
      mutateAsync: resendInvitationMutateAsyncMock,
    } as unknown as ReturnType<typeof useResendInvitation>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    vi.mocked(useAuthContext).mockReturnValue({
      userInfo: {
        username: 'maria',
      },
    } as ReturnType<typeof useAuthContext>);

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAny: true }),
    );
    vi.mocked(useIsMobile).mockReturnValue(false);

    vi.mocked(useQueryClient).mockReturnValue(
      {} as ReturnType<typeof useQueryClient>,
    );
    vi.mocked(useCacheInvalidation).mockReturnValue(invalidateCacheMock);

    const sonnerModule = await import('sonner');
    vi.mocked(sonnerModule.toast).mockImplementation(toastMock);
  });

  test('renders desktop users table with user count', () => {
    renderUsersPage();

    expect(screen.getByRole('heading', { name: 'Users' })).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('table:maria|alex')).toBeInTheDocument();
    expect(screen.queryByText('cards:maria|alex')).not.toBeInTheDocument();
  });

  test('navigates to invite route when invite button is clicked', async () => {
    renderUsersPage();

    await userEvent.click(
      screen.getByRole('button', { name: 'Invite a new user' }),
    );

    expect(navigateMock).toHaveBeenCalledWith('invite');
  });

  test('opens role dialog when user role change is requested from table', async () => {
    renderUsersPage();

    await userEvent.click(
      screen.getByRole('button', { name: 'Change role for first user' }),
    );

    expect(screen.getByText('role-dialog:maria')).toBeInTheDocument();
  });

  test('renders mobile card grid and passes current username to child grid', () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    renderUsersPage();

    expect(screen.getByText('cards:maria|alex')).toBeInTheDocument();
    expect(screen.queryByText('table:maria|alex')).not.toBeInTheDocument();
    expect(screen.getByText('current:maria')).toBeInTheDocument();
  });

  test('toggles user status, invalidates users cache, and shows success toast', async () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    renderUsersPage();

    await userEvent.click(
      screen.getByRole('button', { name: 'Toggle first status' }),
    );

    expect(updateStatusMutateAsyncMock).toHaveBeenCalledWith({
      id: '11111111-1111-1111-1111-111111111111',
      data: {
        etag: 1n,
        status: 'Disabled',
      },
    });

    expect(invalidateCacheMock).toHaveBeenCalledWith(['users']);
    expect(toastMock).toHaveBeenCalledTimes(1);
  });

  test('resends invitation and shows success toast', async () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    renderUsersPage();

    await userEvent.click(
      screen.getByRole('button', { name: 'Resend first invitation' }),
    );

    expect(resendInvitationMutateAsyncMock).toHaveBeenCalledWith({
      id: '11111111-1111-1111-1111-111111111111',
    });

    expect(toastMock).toHaveBeenCalledTimes(1);
  });

  test('maps mutation failure to page error context for status updates', async () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    updateStatusMutateAsyncMock.mockResolvedValueOnce(
      new FailResult(new UnexpectedError('Status update failed')),
    );

    renderUsersPage();

    await userEvent.click(
      screen.getByRole('button', { name: 'Toggle first status' }),
    );

    expect(setErrorMock).toHaveBeenCalledWith({
      title: 'Unexpected Error',
      description: 'Status update failed',
    });
  });

  test('renders and dismisses page error sheet from error context', async () => {
    vi.mocked(useErrorContext).mockReturnValue({
      error: {
        title: 'Users Error',
        description: 'Could not load users',
      },
      setError: setErrorMock,
    });

    renderUsersPage();

    // getByText throws if more than one ErrorSheet instance matches, so a
    // single match locks in the page-level single-render behavior.
    expect(screen.getByText('Users Error')).toBeInTheDocument();
    expect(screen.getByText('Could not load users')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });

  test('shows permission denied message when user cannot view users', () => {
    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAny: false }),
    );

    renderUsersPage();

    expect(
      screen.getByText('You do not have permission to view users.'),
    ).toBeInTheDocument();
  });
});
