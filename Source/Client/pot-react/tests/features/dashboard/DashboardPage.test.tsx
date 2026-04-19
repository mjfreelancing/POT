import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import DashboardPage from '@/features/dashboard/DashboardPage';

import type { ReactNode } from 'react';

import { useErrorContext } from '@/contexts';
import useDashboardStorage from '@/features/dashboard/hooks/useDashboardStorage';

type MockSectionProps = {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
};

function createMockSection(name: string) {
  return function MockSection({ isOpen, onOpenChange }: MockSectionProps) {
    return (
      <div>
        <div>{`${name} section is ${isOpen ? 'open' : 'closed'}`}</div>
        <button
          onClick={() => onOpenChange(!isOpen)}
        >{`toggle ${name}`}</button>
      </div>
    );
  };
}

vi.mock('@/features/dashboard/components', async importOriginal => {
  const actual =
    await importOriginal<typeof import('@/features/dashboard/components')>();

  return {
    ...actual,
    DashboardHeader: () => <h1>Financial Dashboard</h1>,
    QuickActions: createMockSection('quick-actions'),
    AccountsOverview: createMockSection('accounts'),
    ExpensesOverview: createMockSection('expenses'),
    IncomesOverview: createMockSection('incomes'),
  };
});

vi.mock('@/features/auth/components', () => ({
  PermissionGuard: ({ children }: { children: ReactNode }) => <>{children}</>,
}));

vi.mock('@/features/dashboard/hooks/useDashboardStorage', () => ({
  default: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

describe('DashboardPage', () => {
  const getDashboardDataMock = vi.fn();
  const setDashboardDataMock = vi.fn();
  const setErrorMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    getDashboardDataMock.mockReturnValue({
      quickActionsOpen: true,
      accountsOpen: false,
      incomesOpen: true,
      expensesOpen: false,
      expensesPeriod: 30,
      incomesPeriod: 30,
    });

    vi.mocked(useDashboardStorage).mockReturnValue({
      getDashboardData: getDashboardDataMock,
      setDashboardData: setDashboardDataMock,
    });

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });
  });

  test('renders dashboard header and initializes section state from storage', () => {
    render(<DashboardPage />);

    expect(screen.getByText('Financial Dashboard')).toBeInTheDocument();

    expect(
      screen.getByText('quick-actions section is open'),
    ).toBeInTheDocument();
    expect(screen.getByText('accounts section is closed')).toBeInTheDocument();
    expect(screen.getByText('incomes section is open')).toBeInTheDocument();
    expect(screen.getByText('expenses section is closed')).toBeInTheDocument();

    expect(getDashboardDataMock).toHaveBeenCalledTimes(1);
  });

  test('updates quick actions state and persists when toggled', async () => {
    render(<DashboardPage />);

    await userEvent.click(
      screen.getByRole('button', { name: 'toggle quick-actions' }),
    );

    expect(setDashboardDataMock).toHaveBeenCalledWith({
      quickActionsOpen: false,
    });

    expect(
      screen.getByText('quick-actions section is closed'),
    ).toBeInTheDocument();
  });

  test('updates accounts state and persists when toggled', async () => {
    render(<DashboardPage />);

    await userEvent.click(
      screen.getByRole('button', { name: 'toggle accounts' }),
    );

    expect(setDashboardDataMock).toHaveBeenCalledWith({ accountsOpen: true });
    expect(screen.getByText('accounts section is open')).toBeInTheDocument();
  });

  test('updates expenses state and persists when toggled', async () => {
    render(<DashboardPage />);

    await userEvent.click(
      screen.getByRole('button', { name: 'toggle expenses' }),
    );

    expect(setDashboardDataMock).toHaveBeenCalledWith({ expensesOpen: true });
    expect(screen.getByText('expenses section is open')).toBeInTheDocument();
  });

  test('updates incomes state and persists when toggled', async () => {
    render(<DashboardPage />);

    await userEvent.click(
      screen.getByRole('button', { name: 'toggle incomes' }),
    );

    expect(setDashboardDataMock).toHaveBeenCalledWith({ incomesOpen: false });
    expect(screen.getByText('incomes section is closed')).toBeInTheDocument();
  });

  test('renders and dismisses error sheet using error context', async () => {
    vi.mocked(useErrorContext).mockReturnValue({
      error: {
        title: 'Dashboard Error',
        description: 'Something broke in dashboard',
      },
      setError: setErrorMock,
    });

    render(<DashboardPage />);

    expect(screen.getByText('Dashboard Error')).toBeInTheDocument();
    expect(
      screen.getByText('Something broke in dashboard'),
    ).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });
});
