import { fireEvent, render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import { describe, expect, test, vi } from 'vitest';

import { AppSidebarMenus } from '@/components/nav/AppSidebarMenus';

vi.mock('@/components/ui/sidebar', () => {
  return {
    SidebarContent: ({ children }: { children?: React.ReactNode }) => (
      <div data-testid="sidebar-content">{children}</div>
    ),
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
    }: {
      children: React.ReactNode;
      asChild?: boolean;
      onClick?: () => void;
      isActive?: boolean;
      tooltip?: string;
      className?: string;
    }) => {
      if (asChild) {
        return <div>{children}</div>;
      }

      return (
        <button type="button" onClick={onClick}>
          {children}
        </button>
      );
    },
    useSidebar: vi.fn(),
  };
});

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/features/maintenance/export', () => ({
  ExportModal: ({ isOpen }: { isOpen: boolean; onClose: () => void }) =>
    isOpen ? (
      <div data-testid="export-modal-open">Export modal open</div>
    ) : null,
}));

vi.mock('@/features/maintenance/import', () => ({
  ImportModal: ({ isOpen }: { isOpen: boolean; onClose: () => void }) =>
    isOpen ? (
      <div data-testid="import-modal-open">Import modal open</div>
    ) : null,
}));

vi.mock('@/hooks/usePermissions', () => ({
  default: () => ({
    hasAnyPermission: () => true,
  }),
}));

import { useSidebar } from '@/components/ui/sidebar';
import { useAuthContext } from '@/features/auth/contexts';

describe('AppSidebarMenus', () => {
  test('renders only empty sidebar content when not authenticated', () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: false,
    } as never);

    vi.mocked(useSidebar).mockReturnValue({
      isMobile: false,
      setOpenMobile: vi.fn(),
    } as never);

    render(
      <MemoryRouter>
        <AppSidebarMenus />
      </MemoryRouter>,
    );

    expect(screen.getByTestId('sidebar-content')).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'Dashboard' }),
    ).not.toBeInTheDocument();
  });

  test('builds all menu groups and hides maintenance actions on mobile', () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: true,
    } as never);

    vi.mocked(useSidebar).mockReturnValue({
      isMobile: true,
      setOpenMobile: vi.fn(),
    } as never);

    render(
      <MemoryRouter>
        <AppSidebarMenus />
      </MemoryRouter>,
    );

    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Export...' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Import...' }),
    ).not.toBeInTheDocument();
  });

  test('opens export and import modals from maintenance action callbacks', () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: true,
    } as never);

    vi.mocked(useSidebar).mockReturnValue({
      isMobile: false,
      setOpenMobile: vi.fn(),
    } as never);

    render(
      <MemoryRouter>
        <AppSidebarMenus />
      </MemoryRouter>,
    );

    expect(screen.queryByTestId('export-modal-open')).not.toBeInTheDocument();
    expect(screen.queryByTestId('import-modal-open')).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Export...' }));
    expect(screen.getByTestId('export-modal-open')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Import...' }));
    expect(screen.getByTestId('import-modal-open')).toBeInTheDocument();
  });
});
