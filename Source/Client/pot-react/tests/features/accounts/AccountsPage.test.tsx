import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiGetAllAccounts } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import type { Account } from '@/data';
import { WithPermission } from '@/features/auth/components';
import AccountsPage from '@/features/accounts/AccountsPage';
import useAccountStorage from '@/features/accounts/hooks/useAccountStorage';
import { useIsMobile } from '@/hooks/use-mobile';
import { SuccessResult } from '@/lib';

import { createAccountWithIdentity } from '../../shared/factories/accountFactory';

const navigateMock = vi.fn();

vi.mock('react-router', async importOriginal => {
  const actual = await importOriginal<typeof import('react-router')>();

  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

vi.mock('@/api/hooks', () => ({
  useApiGetAllAccounts: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/features/accounts/hooks/useAccountStorage', () => ({
  default: vi.fn(),
}));

vi.mock('@/hooks/use-mobile', () => ({
  useIsMobile: vi.fn(),
}));

vi.mock('@/features/auth/components', () => ({
  WithPermission: vi.fn(),
}));

vi.mock('@/features/accounts/components', () => ({
  AccountsHeader: () => <h1>Accounts</h1>,
  AccountsTable: ({ accounts }: { accounts: Account[] }) => (
    <div>{`table:${accounts.map(account => account.description).join('|')}`}</div>
  ),
  AccountCardGrid: ({ accounts }: { accounts: Account[] }) => (
    <div>{`cards:${accounts.map(account => account.description).join('|')}`}</div>
  ),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

function renderAccountsPage() {
  return render(
    <MemoryRouter>
      <AccountsPage />
    </MemoryRouter>,
  );
}

describe('AccountsPage', () => {
  const getAccountDataMock = vi.fn();
  const setAccountDataMock = vi.fn();
  const setErrorMock = vi.fn();

  const accounts = [
    createAccountWithIdentity('account-1', 'Main account'),
    createAccountWithIdentity('account-2', 'Holiday savings'),
  ];

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useApiGetAllAccounts).mockReturnValue({
      data: new SuccessResult(accounts),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetAllAccounts>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    getAccountDataMock.mockReturnValue({
      filterDescription: null,
    });

    vi.mocked(useAccountStorage).mockReturnValue({
      getAccountData: getAccountDataMock,
      setAccountData: setAccountDataMock,
    });

    vi.mocked(useIsMobile).mockReturnValue(false);

    vi.mocked(WithPermission).mockImplementation(({ children }) => (
      <>{children}</>
    ));
  });

  test('renders accounts header and desktop accounts table when data exists', () => {
    renderAccountsPage();

    expect(screen.getByText('Accounts')).toBeInTheDocument();
    expect(
      screen.getByText('table:Main account|Holiday savings'),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('cards:Main account|Holiday savings'),
    ).not.toBeInTheDocument();
  });

  test('applies stored description filter on mount', () => {
    getAccountDataMock.mockReturnValue({
      filterDescription: 'holiday',
    });

    renderAccountsPage();

    expect(screen.getByLabelText('Search accounts by description')).toHaveValue(
      'holiday',
    );
    expect(screen.getByText('table:Holiday savings')).toBeInTheDocument();
    expect(
      screen.queryByText('table:Main account|Holiday savings'),
    ).not.toBeInTheDocument();
  });

  test('trims search text and persists updated filter', async () => {
    renderAccountsPage();

    await userEvent.clear(
      screen.getByLabelText('Search accounts by description'),
    );
    await userEvent.type(
      screen.getByLabelText('Search accounts by description'),
      '  main  ',
    );

    expect(setAccountDataMock).toHaveBeenLastCalledWith({
      filterDescription: 'main',
    });

    expect(screen.getByText('table:Main account')).toBeInTheDocument();
  });

  test('shows no-accounts empty state and navigates to create from primary action', async () => {
    vi.mocked(useApiGetAllAccounts).mockReturnValue({
      data: new SuccessResult([]),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetAllAccounts>);

    renderAccountsPage();

    expect(screen.getByText('No accounts yet')).toBeInTheDocument();

    await userEvent.click(
      screen.getByRole('button', { name: 'Add first account' }),
    );

    expect(navigateMock).toHaveBeenCalledWith('create');
  });

  test('shows no-matching state and clears search when requested', async () => {
    getAccountDataMock.mockReturnValue({
      filterDescription: 'missing-account',
    });

    renderAccountsPage();

    expect(screen.getByText('No matching accounts')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Clear search' }));

    expect(setAccountDataMock).toHaveBeenCalledWith({ filterDescription: '' });
    expect(
      screen.getByText('table:Main account|Holiday savings'),
    ).toBeInTheDocument();
  });

  test('renders mobile account cards on mobile viewport', () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    renderAccountsPage();

    expect(
      screen.getByText('cards:Main account|Holiday savings'),
    ).toBeInTheDocument();
    expect(
      screen.queryByText('table:Main account|Holiday savings'),
    ).not.toBeInTheDocument();
  });

  test('renders and dismisses page error sheet from error context', async () => {
    vi.mocked(useErrorContext).mockReturnValue({
      error: {
        title: 'Accounts Error',
        description: 'Could not load accounts',
      },
      setError: setErrorMock,
    });

    renderAccountsPage();

    expect(screen.getByText('Accounts Error')).toBeInTheDocument();
    expect(screen.getByText('Could not load accounts')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });
});
