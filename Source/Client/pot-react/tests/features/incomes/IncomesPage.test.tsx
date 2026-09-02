import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiGetAllAccounts, useApiGetAllIncomes } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import type { Account, Income } from '@/data';
import { WithPermission } from '@/features/auth/components';
import IncomesPage from '@/features/incomes/IncomesPage';
import useIncomeStorage from '@/features/incomes/hooks/useIncomeStorage';
import { useAccountFilter, useIsMobile } from '@/hooks';
import { useIsShortViewport } from '@/hooks/use-short-viewport';
import { SuccessResult } from '@/lib';

import { createAccountWithIdentity } from '../../shared/factories/accountFactory';
import { createIncome } from '../../shared/factories/incomeFactory';

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
  useApiGetAllIncomes: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/features/incomes/hooks/useIncomeStorage', () => ({
  default: vi.fn(),
}));

vi.mock('@/hooks', () => ({
  useAccountFilter: vi.fn(),
  useIsMobile: vi.fn(),
}));

vi.mock('@/hooks/use-short-viewport', () => ({
  useIsShortViewport: vi.fn(),
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

vi.mock('@/features/incomes/components', () => ({
  IncomesHeader: () => <h1>Incomes</h1>,
  IncomesTable: ({ filteredIncomes }: { filteredIncomes: Income[] }) => (
    <div>{`table:${filteredIncomes.map(income => income.description).join('|')}`}</div>
  ),
  IncomeCardGrid: ({ incomes }: { incomes: Income[] }) => (
    <div>{`cards:${incomes.map(income => income.description).join('|')}`}</div>
  ),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

function renderIncomesPage() {
  return render(
    <MemoryRouter>
      <IncomesPage />
    </MemoryRouter>,
  );
}

describe('IncomesPage', () => {
  const getIncomeDataMock = vi.fn();
  const setIncomeDataMock = vi.fn();
  const setErrorMock = vi.fn();
  const setSelectedAccountIdMock = vi.fn();

  const accounts: Account[] = [
    createAccountWithIdentity('account-1', 'Main account'),
    createAccountWithIdentity('account-2', 'Holiday savings'),
  ];

  const incomes: Income[] = [
    createIncome({ rowId: 'income-1', description: 'Salary' }),
    createIncome({ rowId: 'income-2', description: 'Interest' }),
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    currentSearchParams = new URLSearchParams();

    vi.mocked(useApiGetAllIncomes).mockReturnValue({
      data: new SuccessResult(incomes),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetAllIncomes>);

    vi.mocked(useApiGetAllAccounts).mockReturnValue({
      data: new SuccessResult(accounts),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetAllAccounts>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    getIncomeDataMock.mockReturnValue({
      selectedAccountId: null,
      filterDescription: null,
    });

    vi.mocked(useIncomeStorage).mockReturnValue({
      getIncomeData: getIncomeDataMock,
      setIncomeData: setIncomeDataMock,
    });

    vi.mocked(useAccountFilter).mockReturnValue({
      accountsInItems: accounts,
      selectedAccountId: null,
      setSelectedAccountId: setSelectedAccountIdMock,
      filteredItems: incomes,
    });

    vi.mocked(useIsMobile).mockReturnValue(false);
    vi.mocked(useIsShortViewport).mockReturnValue(false);

    vi.mocked(WithPermission).mockImplementation(({ children }) => (
      <>{children}</>
    ));
  });

  test('renders incomes header and desktop table when data exists', () => {
    renderIncomesPage();

    expect(screen.getByText('Incomes')).toBeInTheDocument();
    expect(screen.getByText('table:Salary|Interest')).toBeInTheDocument();

    expect(screen.queryByText('cards:Salary|Interest')).not.toBeInTheDocument();
  });

  test('applies stored description filter on mount', () => {
    getIncomeDataMock.mockReturnValue({
      selectedAccountId: null,
      filterDescription: 'salary',
    });

    renderIncomesPage();

    expect(screen.getByLabelText('Search incomes by description')).toHaveValue(
      'salary',
    );

    expect(screen.getByText('table:Salary')).toBeInTheDocument();
    expect(screen.queryByText('table:Salary|Interest')).not.toBeInTheDocument();
  });

  test('trims search text and persists updated filter', async () => {
    renderIncomesPage();

    await userEvent.clear(
      screen.getByLabelText('Search incomes by description'),
    );
    await userEvent.type(
      screen.getByLabelText('Search incomes by description'),
      '  salary  ',
    );

    expect(setIncomeDataMock).toHaveBeenLastCalledWith({
      filterDescription: 'salary',
    });

    expect(screen.getByText('table:Salary')).toBeInTheDocument();
  });

  test('navigates to create income when add button is clicked', async () => {
    renderIncomesPage();

    await userEvent.click(
      screen.getByRole('button', { name: 'Add a new income' }),
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
      filteredItems: incomes,
    });

    renderIncomesPage();

    expect(
      screen.getByText('Create an account to get started'),
    ).toBeInTheDocument();

    await userEvent.click(
      screen.getByRole('button', { name: 'Create Account' }),
    );

    expect(navigateMock).toHaveBeenCalledWith('/accounts/create');
  });

  test('shows no-matching state and clears filters from primary action', async () => {
    getIncomeDataMock.mockReturnValue({
      selectedAccountId: null,
      filterDescription: 'missing',
    });

    renderIncomesPage();

    expect(screen.getByText('No matching incomes')).toBeInTheDocument();

    await userEvent.click(
      screen.getByRole('button', { name: 'Clear filters' }),
    );

    expect(setIncomeDataMock).toHaveBeenCalledWith({
      filterDescription: '',
      selectedAccountId: null,
    });

    expect(setSelectedAccountIdMock).toHaveBeenCalledWith(null);
  });

  test('renders mobile income cards on mobile viewport', () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    renderIncomesPage();

    expect(screen.getByText('cards:Salary|Interest')).toBeInTheDocument();
    expect(screen.queryByText('table:Salary|Interest')).not.toBeInTheDocument();
  });

  test('renders and dismisses page error sheet from error context', async () => {
    vi.mocked(useErrorContext).mockReturnValue({
      error: {
        title: 'Incomes Error',
        description: 'Could not load incomes',
      },
      setError: setErrorMock,
    });

    renderIncomesPage();

    expect(screen.getByText('Incomes Error')).toBeInTheDocument();
    expect(screen.getByText('Could not load incomes')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });
});
