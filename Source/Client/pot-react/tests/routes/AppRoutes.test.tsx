import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logoutManager } from '@/concerns';
import { useAuthContext } from '@/features/auth/contexts';
import { AppRoutes } from '@/routes/AppRoutes';

vi.mock('@/features/auth/LoginPage', () => ({
  default: () => <div data-testid="login-page">Login page</div>,
}));

vi.mock('@/features/dashboard/DashboardPage', () => ({
  default: () => <div data-testid="dashboard-page">Dashboard page</div>,
}));

vi.mock('@/features/projections/ProjectionsPage', () => ({
  default: () => <div data-testid="projections-page">Projections page</div>,
}));

vi.mock('@/features/accounts/AccountsPage', () => ({
  default: () => <div data-testid="accounts-page">Accounts page</div>,
}));

vi.mock('@/features/accounts/create/CreateAccountSheet', () => ({
  default: () => <div data-testid="create-account-sheet">Create account</div>,
}));

vi.mock('@/features/accounts/edit/EditAccountSheet', () => ({
  default: () => <div data-testid="edit-account-sheet">Edit account</div>,
}));

vi.mock('@/features/incomes/IncomesPage', () => ({
  default: () => <div data-testid="incomes-page">Incomes page</div>,
}));

vi.mock('@/features/incomes/create/CreateIncomeSheet', () => ({
  default: () => <div data-testid="create-income-sheet">Create income</div>,
}));

vi.mock('@/features/incomes/edit/EditIncomeSheet', () => ({
  default: () => <div data-testid="edit-income-sheet">Edit income</div>,
}));

vi.mock('@/features/expenses/ExpensesPage', () => ({
  default: () => <div data-testid="expenses-page">Expenses page</div>,
}));

vi.mock('@/features/expenses/create/CreateExpenseSheet', () => ({
  default: () => <div data-testid="create-expense-sheet">Create expense</div>,
}));

vi.mock('@/features/expenses/edit/EditExpenseSheet', () => ({
  default: () => <div data-testid="edit-expense-sheet">Edit expense</div>,
}));

vi.mock('@/features/users/UsersPage', () => ({
  default: () => <div data-testid="users-page">Users page</div>,
}));

vi.mock('@/features/users/components/InviteUserSheet', () => ({
  default: () => <div data-testid="invite-user-sheet">Invite user</div>,
}));

vi.mock('@/features/approvals/pages/PendingApprovalsPage', () => ({
  default: () => <div data-testid="pending-approvals-page">Approvals</div>,
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logoutManager: {
    logout: vi.fn(),
  },
}));

function renderAppRoutes(initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <AppRoutes />
    </MemoryRouter>,
  );
}

describe('AppRoutes', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: true,
      isInitialized: true,
    } as ReturnType<typeof useAuthContext>);
  });

  test('redirects unauthenticated users from protected routes to login', async () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: false,
      isInitialized: true,
    } as ReturnType<typeof useAuthContext>);

    renderAppRoutes('/dashboard');

    expect(await screen.findByTestId('login-page')).toBeInTheDocument();
    expect(screen.queryByTestId('dashboard-page')).not.toBeInTheDocument();
  });

  test('shows no protected content while auth initialization is in progress', () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: false,
      isInitialized: false,
    } as ReturnType<typeof useAuthContext>);

    const { container } = renderAppRoutes('/dashboard');

    expect(container).toBeEmptyDOMElement();
  });

  test('renders protected route content for authenticated users', async () => {
    renderAppRoutes('/dashboard');

    expect(await screen.findByTestId('dashboard-page')).toBeInTheDocument();
  });

  test('redirects unknown authenticated routes to dashboard', async () => {
    renderAppRoutes('/missing-route');

    expect(await screen.findByTestId('dashboard-page')).toBeInTheDocument();
  });

  test('triggers logout manager and redirects logout route to login', async () => {
    renderAppRoutes('/logout');

    expect(logoutManager.logout).toHaveBeenCalledTimes(1);
    expect(await screen.findByTestId('login-page')).toBeInTheDocument();
  });
});
