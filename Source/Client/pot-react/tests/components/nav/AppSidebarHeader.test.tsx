import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import { describe, expect, test, vi } from 'vitest';

import { AppSidebarHeader } from '@/components/nav/AppSidebarHeader';

vi.mock('@/components/ui/sidebar', () => ({
  SidebarHeader: ({
    children,
    className,
  }: {
    children: React.ReactNode;
    className?: string;
  }) => <header className={className}>{children}</header>,
}));

describe('AppSidebarHeader', () => {
  test('renders brand text and home link', () => {
    render(
      <MemoryRouter>
        <AppSidebarHeader />
      </MemoryRouter>,
    );

    expect(
      screen.getByRole('link', {
        name: 'Navigate to Pay On Time homepage',
      }),
    ).toHaveAttribute('href', '/');

    expect(screen.getByText('Pay On Time')).toBeInTheDocument();
    expect(screen.getByText('Financial Management')).toBeInTheDocument();
  });
});
