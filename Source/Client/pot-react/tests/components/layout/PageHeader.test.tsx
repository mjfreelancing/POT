import { render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import { PageHeader } from '@/components/layout';

vi.mock('@/components/nav', () => ({
  AppSidebarTrigger: () => <button type="button">Mock Sidebar Trigger</button>,
}));

vi.mock('@/components/user/UserMenu', () => ({
  UserMenu: () => <div>Mock User Menu</div>,
}));

describe('PageHeader', () => {
  test('renders title, subtitle, sidebar trigger, and user menu by default', () => {
    render(<PageHeader title="Accounts" subtitle="Manage your accounts" />);

    expect(
      screen.getByRole('heading', { level: 1, name: 'Accounts' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Manage your accounts')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Mock Sidebar Trigger' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Mock User Menu')).toBeInTheDocument();
  });

  test('hides sidebar trigger when showSidebarTrigger is false', () => {
    render(
      <PageHeader
        title="Dashboard"
        subtitle="Overview"
        showSidebarTrigger={false}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Mock Sidebar Trigger' }),
    ).toBeNull();

    expect(
      screen.getByRole('heading', { level: 1, name: 'Dashboard' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Mock User Menu')).toBeInTheDocument();
  });

  test('does not render subtitle when it is not provided', () => {
    render(<PageHeader title="Projections" />);

    expect(
      screen.getByRole('heading', { level: 1, name: 'Projections' }),
    ).toBeInTheDocument();

    expect(screen.queryByText('Manage your accounts')).toBeNull();
  });
});
