import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { describe, expect, test, vi } from 'vitest';

import MenuGroup, {
  type MenuGroupDefinition,
} from '@/components/nav/MenuGroup';

const locationMock = {
  pathname: '/dashboard',
};

const hasAnyPermissionMock = vi.fn();
const setOpenMobileMock = vi.fn();

vi.mock('react-router', () => ({
  Link: ({
    children,
    to,
    onClick,
    'aria-label': ariaLabel,
  }: {
    children: React.ReactNode;
    to: string;
    onClick?: () => void;
    'aria-label'?: string;
  }) => (
    <a
      href={to}
      onClick={event => {
        event.preventDefault();
        onClick?.();
      }}
      aria-label={ariaLabel}
    >
      {children}
    </a>
  ),
  useLocation: () => locationMock,
  matchPath: (matcher: { path: string; end: boolean }, currentPath: string) => {
    return matcher.end && matcher.path === currentPath ? { params: {} } : null;
  },
}));

vi.mock('@/components/ui/sidebar', () => ({
  SidebarGroup: ({ children }: { children: React.ReactNode }) => (
    <section>{children}</section>
  ),
  SidebarGroupLabel: ({ children }: { children: React.ReactNode }) => (
    <h3>{children}</h3>
  ),
  SidebarGroupContent: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  SidebarMenu: ({ children }: { children: React.ReactNode }) => (
    <ul>{children}</ul>
  ),
  SidebarMenuItem: ({ children }: { children: React.ReactNode }) => (
    <li>{children}</li>
  ),
  SidebarMenuButton: ({
    children,
    asChild,
    onClick,
    isActive,
  }: {
    children: React.ReactNode;
    asChild?: boolean;
    onClick?: () => void;
    isActive?: boolean;
    tooltip?: string;
    className?: string;
  }) => {
    if (asChild) {
      return <div data-active={String(!!isActive)}>{children}</div>;
    }

    return (
      <button type="button" data-active={String(!!isActive)} onClick={onClick}>
        {children}
      </button>
    );
  },
  useSidebar: () => ({
    isMobile: true,
    setOpenMobile: setOpenMobileMock,
  }),
}));

vi.mock('@/hooks/usePermissions', () => ({
  default: () => ({
    hasAnyPermission: hasAnyPermissionMock,
  }),
}));

const TestIcon = () => <span data-testid="icon" />;

describe('MenuGroup', () => {
  test('does not render when all items are permission-filtered', () => {
    hasAnyPermissionMock.mockReturnValue(false);

    const group: MenuGroupDefinition = {
      label: 'Manage',
      items: [
        {
          type: 'href',
          label: 'Users',
          href: '/users',
          icon: TestIcon,
          permissions: ['user:view'],
        },
      ],
    };

    const { container } = render(<MenuGroup group={group} />);

    expect(container.firstChild).toBeNull();
  });

  test('renders visible href and onClick items', () => {
    hasAnyPermissionMock.mockReturnValue(true);

    const group: MenuGroupDefinition = {
      label: 'Main',
      items: [
        {
          type: 'href',
          label: 'Dashboard',
          href: '/dashboard',
          icon: TestIcon,
        },
        {
          type: 'onClick',
          label: 'Export',
          icon: TestIcon,
          onClick: vi.fn(),
        },
      ],
    };

    render(<MenuGroup group={group} />);

    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Export' })).toBeInTheDocument();
  });

  test('closes mobile sidebar when href item is clicked', async () => {
    const user = userEvent.setup();
    hasAnyPermissionMock.mockReturnValue(true);
    setOpenMobileMock.mockClear();

    const group: MenuGroupDefinition = {
      label: 'Main',
      items: [
        {
          type: 'href',
          label: 'Dashboard',
          href: '/dashboard',
          icon: TestIcon,
        },
      ],
    };

    render(<MenuGroup group={group} />);

    await user.click(screen.getByRole('link', { name: 'Dashboard' }));

    expect(setOpenMobileMock).toHaveBeenCalledWith(false);
    expect(setOpenMobileMock).toHaveBeenCalledTimes(1);
  });

  test('runs onClick action when onClick item is clicked', async () => {
    const user = userEvent.setup();
    hasAnyPermissionMock.mockReturnValue(true);
    setOpenMobileMock.mockClear();

    const onClick = vi.fn();

    const group: MenuGroupDefinition = {
      label: 'Main',
      items: [
        {
          type: 'onClick',
          label: 'Export',
          icon: TestIcon,
          onClick,
        },
      ],
    };

    render(<MenuGroup group={group} />);

    await user.click(screen.getByRole('button', { name: 'Export' }));

    expect(onClick).toHaveBeenCalledTimes(1);
  });

  test('closes mobile sidebar when onClick item is clicked', async () => {
    const user = userEvent.setup();
    hasAnyPermissionMock.mockReturnValue(true);
    setOpenMobileMock.mockClear();

    const group: MenuGroupDefinition = {
      label: 'Main',
      items: [
        {
          type: 'onClick',
          label: 'Export',
          icon: TestIcon,
          onClick: vi.fn(),
        },
      ],
    };

    render(<MenuGroup group={group} />);

    await user.click(screen.getByRole('button', { name: 'Export' }));

    expect(setOpenMobileMock).toHaveBeenCalledWith(false);
    expect(setOpenMobileMock).toHaveBeenCalledTimes(1);
  });
});
