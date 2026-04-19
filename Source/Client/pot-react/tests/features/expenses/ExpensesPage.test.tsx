import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiGetAllAccounts, useApiGetAllExpenses } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import type { Account, Expense } from '@/data';
import { WithPermission } from '@/features/auth/components';
import ExpensesPage from '@/features/expenses/ExpensesPage';
import useExpenseStorage from '@/features/expenses/hooks/useExpenseStorage';
import { useAccountFilter } from '@/hooks';
import { useIsMobile } from '@/hooks/use-mobile';
import { SuccessResult } from '@/lib';

import { createAccountWithIdentity } from '../../shared/factories/accountFactory';
import { createExpense } from '../../shared/factories/expenseFactory';

const navigateMock = vi.fn();
const setSearchParamsMock = vi.fn();
let currentSearchParams = new URLSearchParams();

vi.mock('react-router', async importOriginal => {
  const actual = await importOriginal<typeof import('react-router')>();

  return {
    ...actual,
    useNavigate: () => navigateMock,
    useSearchParams: () => [currentSearchParams, setSearchParamsMock] as const,
  };
});

vi.mock('@/api/hooks', () => ({
  useApiGetAllAccounts: vi.fn(),
  useApiGetAllExpenses: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/features/expenses/hooks/useExpenseStorage', () => ({
  default: vi.fn(),
}));

vi.mock('@/hooks', () => ({
  useAccountFilter: vi.fn(),
}));

vi.mock('@/hooks/use-mobile', () => ({
  useIsMobile: vi.fn(),
}));

vi.mock('@/features/auth/components', () => ({
  WithPermission: vi.fn(),
}));

vi.mock('@/components/filters', async importOriginal => {
  const actual = await importOriginal<typeof import('@/components/filters')>();

  return {
    ...actual,
    AccountFilter: ({
      onAccountChange,
    }: {
      onAccountChange: (accountId: string | null) => void;
    }) => (
      <button type="button" onClick={() => onAccountChange('account-2')}>
        Set account filter
      </button>
    ),
  };
});

vi.mock('@/features/expenses/components', () => ({
  ExpensesHeader: () => <h1>Expenses</h1>,
  ExpensesTable: ({ filteredExpenses }: { filteredExpenses: Expense[] }) => (
    <div>{`table:${filteredExpenses.map(expense => expense.description).join('|')}`}</div>
  ),
  ExpenseCardGrid: ({ expenses }: { expenses: Expense[] }) => (
    <div>{`cards:${expenses.map(expense => expense.description).join('|')}`}</div>
  ),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

function renderExpensesPage() {
  return render(
    <MemoryRouter>
      <ExpensesPage />
    </MemoryRouter>,
  );
}

describe('ExpensesPage', () => {
  const getExpenseDataMock = vi.fn();
  const setExpenseDataMock = vi.fn();
  const setErrorMock = vi.fn();
  const setSelectedAccountIdMock = vi.fn();

  const accounts: Account[] = [
    createAccountWithIdentity('account-1', 'Main account'),
    createAccountWithIdentity('account-2', 'Holiday savings'),
  ];

  const expenses: Expense[] = [
    createExpense({ rowId: 'expense-1', description: 'Rent' }),
    createExpense({ rowId: 'expense-2', description: 'Insurance' }),
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    currentSearchParams = new URLSearchParams();

    vi.mocked(useApiGetAllExpenses).mockReturnValue({
      data: new SuccessResult(expenses),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetAllExpenses>);

    vi.mocked(useApiGetAllAccounts).mockReturnValue({
      data: new SuccessResult(accounts),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetAllAccounts>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    getExpenseDataMock.mockReturnValue({
      selectedAccountId: null,
      filterDescription: null,
    });

    vi.mocked(useExpenseStorage).mockReturnValue({
      getExpenseData: getExpenseDataMock,
      setExpenseData: setExpenseDataMock,
    });

    vi.mocked(useAccountFilter).mockReturnValue({
      accountsInItems: accounts,
      selectedAccountId: null,
      setSelectedAccountId: setSelectedAccountIdMock,
      filteredItems: expenses,
    });

    vi.mocked(useIsMobile).mockReturnValue(false);

    vi.mocked(WithPermission).mockImplementation(({ children }) => (
      <>{children}</>
    ));
  });

  test('renders expenses header and desktop table when data exists', () => {
    renderExpensesPage();

    expect(screen.getByText('Expenses')).toBeInTheDocument();
    expect(screen.getByText('table:Rent|Insurance')).toBeInTheDocument();

    expect(screen.queryByText('cards:Rent|Insurance')).not.toBeInTheDocument();
  });

  test('applies stored description filter on mount', () => {
    getExpenseDataMock.mockReturnValue({
      selectedAccountId: null,
      filterDescription: 'rent',
    });

    renderExpensesPage();

    expect(screen.getByLabelText('Search expenses by description')).toHaveValue(
      'rent',
    );

    expect(screen.getByText('table:Rent')).toBeInTheDocument();
    expect(screen.queryByText('table:Rent|Insurance')).not.toBeInTheDocument();
  });

  test('trims search text and persists updated filter', async () => {
    renderExpensesPage();

    await userEvent.clear(
      screen.getByLabelText('Search expenses by description'),
    );
    await userEvent.type(
      screen.getByLabelText('Search expenses by description'),
      '  rent  ',
    );

    expect(setExpenseDataMock).toHaveBeenLastCalledWith({
      filterDescription: 'rent',
    });

    expect(screen.getByText('table:Rent')).toBeInTheDocument();
  });

  test('navigates to create expense when add button is clicked', async () => {
    renderExpensesPage();

    await userEvent.click(
      screen.getByRole('button', { name: 'Add a new expense' }),
    );

    expect(navigateMock).toHaveBeenCalledWith('create');
  });

  test('shows no-accounts state and navigates to create account from action', async () => {
    vi.mocked(useApiGetAllAccounts).mockReturnValue({
      data: new SuccessResult([]),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetAllAccounts>);

    vi.mocked(useAccountFilter).mockReturnValue({
      accountsInItems: [],
      selectedAccountId: null,
      setSelectedAccountId: setSelectedAccountIdMock,
      filteredItems: expenses,
    });

    renderExpensesPage();

    expect(
      screen.getByText('Create an account to get started'),
    ).toBeInTheDocument();

    await userEvent.click(
      screen.getByRole('button', { name: 'Create Account' }),
    );

    expect(navigateMock).toHaveBeenCalledWith('/accounts/create');
  });

  test('shows no-matching state and clears filters from primary action', async () => {
    getExpenseDataMock.mockReturnValue({
      selectedAccountId: null,
      filterDescription: 'missing',
    });

    vi.mocked(useAccountFilter).mockReturnValue({
      accountsInItems: accounts,
      selectedAccountId: null,
      setSelectedAccountId: setSelectedAccountIdMock,
      filteredItems: expenses,
    });

    renderExpensesPage();

    expect(screen.getByText('No matching expenses')).toBeInTheDocument();

    await userEvent.click(
      screen.getByRole('button', { name: 'Clear filters' }),
    );

    expect(setExpenseDataMock).toHaveBeenCalledWith({
      filterDescription: '',
      selectedAccountId: null,
    });

    expect(setSelectedAccountIdMock).toHaveBeenCalledWith(null);
  });

  test('renders mobile expense cards on mobile viewport', () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    renderExpensesPage();

    expect(screen.getByText('cards:Rent|Insurance')).toBeInTheDocument();
    expect(screen.queryByText('table:Rent|Insurance')).not.toBeInTheDocument();
  });

  test('renders and dismisses page error sheet from error context', async () => {
    vi.mocked(useErrorContext).mockReturnValue({
      error: {
        title: 'Expenses Error',
        description: 'Could not load expenses',
      },
      setError: setErrorMock,
    });

    renderExpensesPage();

    expect(screen.getByText('Expenses Error')).toBeInTheDocument();
    expect(screen.getByText('Could not load expenses')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });
});
