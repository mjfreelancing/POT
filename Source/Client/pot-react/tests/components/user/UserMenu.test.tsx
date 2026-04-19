import { fireEvent, render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UserMenu } from '@/components/user/UserMenu';

const { authState, logoutMock } = vi.hoisted(() => ({
  authState: {
    displayName: 'Alex User' as string | undefined,
  },
  logoutMock: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logoutManager: {
    logout: logoutMock,
  },
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: () => ({
    userInfo: authState.displayName
      ? { displayName: authState.displayName }
      : undefined,
  }),
}));

vi.mock('@/components/ui/dropdown-menu', () => ({
  DropdownMenu: ({ children }: { children: ReactNode }) => (
    <div>{children}</div>
  ),
  DropdownMenuTrigger: ({
    children,
  }: {
    children: ReactNode;
    asChild?: boolean;
  }) => <div>{children}</div>,
  DropdownMenuContent: ({ children }: { children: ReactNode }) => (
    <div>{children}</div>
  ),
  DropdownMenuItem: ({
    children,
    onClick,
  }: {
    children: ReactNode;
    onClick?: () => void;
  }) => (
    <button type="button" onClick={onClick}>
      {children}
    </button>
  ),
  DropdownMenuSeparator: () => <hr />,
}));

vi.mock('@/features/userSettings/UserSettingsSheet', () => ({
  AccountSettingsSheet: ({
    open,
    onClose,
    onOpenChange,
  }: {
    open: boolean;
    onClose: () => void;
    onOpenChange: (open: boolean) => void;
  }) => (
    <div
      data-testid="account-settings-sheet"
      data-open={open ? 'true' : 'false'}
    >
      <button type="button" onClick={onClose}>
        Sheet Close
      </button>
      <button type="button" onClick={() => onOpenChange(false)}>
        Sheet Open Change False
      </button>
    </div>
  ),
}));

describe('UserMenu', () => {
  beforeEach(() => {
    logoutMock.mockClear();
    authState.displayName = 'Alex User';
  });

  test('renders authenticated display name', () => {
    render(<UserMenu />);

    expect(
      screen.getByRole('button', { name: 'Alex User' }),
    ).toBeInTheDocument();
  });

  test('renders fallback label when display name is unavailable', () => {
    authState.displayName = undefined;

    render(<UserMenu />);

    expect(screen.getByRole('button', { name: 'User' })).toBeInTheDocument();
  });

  test('opens settings sheet from settings action and closes it from sheet close', () => {
    render(<UserMenu />);

    expect(screen.getByTestId('account-settings-sheet')).toHaveAttribute(
      'data-open',
      'false',
    );

    fireEvent.click(screen.getByRole('button', { name: 'Settings' }));

    expect(screen.getByTestId('account-settings-sheet')).toHaveAttribute(
      'data-open',
      'true',
    );

    fireEvent.click(screen.getByRole('button', { name: 'Sheet Close' }));

    expect(screen.getByTestId('account-settings-sheet')).toHaveAttribute(
      'data-open',
      'false',
    );
  });

  test('calls logout manager when log out action is clicked', () => {
    render(<UserMenu />);

    fireEvent.click(screen.getByRole('button', { name: 'Log Out' }));

    expect(logoutMock).toHaveBeenCalledTimes(1);
  });
});
