import { act, render, screen } from '@testing-library/react';
import React from 'react';
import { describe, expect, test, vi } from 'vitest';

import { AppSidebarMenus } from '@/components/nav/AppSidebarMenus';

const menuGroupMock = vi.fn(
  ({
    group,
  }: {
    group: {
      label: string;
      items?: unknown[];
    };
  }) => {
    return <div data-testid={`menu-group-${group.label.toLowerCase()}`} />;
  },
);

vi.mock('@/components/ui/sidebar', () => ({
  SidebarContent: ({ children }: { children?: React.ReactNode }) => (
    <div data-testid="sidebar-content">{children}</div>
  ),
  useSidebar: vi.fn(),
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/features/maintenance/export', () => ({
  ExportModal: ({ isOpen }: { isOpen: boolean; onClose: () => void }) => (
    <div data-testid="export-modal">{String(isOpen)}</div>
  ),
}));

vi.mock('@/features/maintenance/import', () => ({
  ImportModal: ({ isOpen }: { isOpen: boolean; onClose: () => void }) => (
    <div data-testid="import-modal">{String(isOpen)}</div>
  ),
}));

vi.mock('@/components/nav/MenuGroup', () => ({
  default: (props: { group: { label: string } }) => menuGroupMock(props),
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
    } as never);

    render(<AppSidebarMenus />);

    expect(screen.getByTestId('sidebar-content')).toBeInTheDocument();
    expect(menuGroupMock).not.toHaveBeenCalled();
  });

  test('builds all menu groups and hides maintenance actions on mobile', () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: true,
    } as never);

    vi.mocked(useSidebar).mockReturnValue({
      isMobile: true,
    } as never);

    menuGroupMock.mockClear();

    render(<AppSidebarMenus />);

    expect(menuGroupMock).toHaveBeenCalledTimes(4);

    const maintenanceGroup = menuGroupMock.mock.calls[3][0].group as {
      label: string;
      items: unknown[];
    };

    expect(maintenanceGroup.label).toBe('Maintenance');
    expect(maintenanceGroup.items).toHaveLength(0);
  });

  test('opens export and import modals from maintenance action callbacks', () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: true,
    } as never);

    vi.mocked(useSidebar).mockReturnValue({
      isMobile: false,
    } as never);

    menuGroupMock.mockClear();

    render(<AppSidebarMenus />);

    const maintenanceGroup = menuGroupMock.mock.calls[3][0]
      .group as unknown as {
      items: Array<{
        label: string;
        onClick?: () => void;
      }>;
    };

    const exportAction = maintenanceGroup.items.find(
      item => item.label === 'Export...',
    );
    const importAction = maintenanceGroup.items.find(
      item => item.label === 'Import...',
    );

    expect(screen.getByTestId('export-modal')).toHaveTextContent('false');
    expect(screen.getByTestId('import-modal')).toHaveTextContent('false');

    act(() => {
      exportAction?.onClick?.();
    });

    expect(screen.getByTestId('export-modal')).toHaveTextContent('true');

    act(() => {
      importAction?.onClick?.();
    });

    expect(screen.getByTestId('import-modal')).toHaveTextContent('true');
  });
});
