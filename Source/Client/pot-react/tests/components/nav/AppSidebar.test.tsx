import { render, screen } from '@testing-library/react';
import React from 'react';
import { describe, expect, test, vi } from 'vitest';

import { AppSidebar } from '@/components/nav/AppSidebar';

vi.mock('@/components/ui/sidebar', () => ({
  Sidebar: ({
    children,
    collapsible,
  }: {
    children: React.ReactNode;
    collapsible?: string;
  }) => <div data-testid="sidebar" data-collapsible={collapsible}>{children}</div>,
  SidebarFooter: ({ children }: { children: React.ReactNode }) => <footer>{children}</footer>,
}));

vi.mock('@/components/theme', () => ({
  ThemeToggle: () => <div data-testid="theme-toggle">Theme Toggle</div>,
}));

vi.mock('@/components/nav/AppSidebarHeader', () => ({
  AppSidebarHeader: () => <div data-testid="app-sidebar-header">Header</div>,
}));

vi.mock('@/components/nav/AppSidebarMenus', () => ({
  AppSidebarMenus: () => <div data-testid="app-sidebar-menus">Menus</div>,
}));

describe('AppSidebar', () => {
  test('composes header menus and theme toggle inside sidebar shell', () => {
    render(<AppSidebar />);

    expect(screen.getByTestId('sidebar')).toHaveAttribute('data-collapsible', 'icon');
    expect(screen.getByTestId('app-sidebar-header')).toBeInTheDocument();
    expect(screen.getByTestId('app-sidebar-menus')).toBeInTheDocument();
    expect(screen.getByTestId('theme-toggle')).toBeInTheDocument();
  });
});
