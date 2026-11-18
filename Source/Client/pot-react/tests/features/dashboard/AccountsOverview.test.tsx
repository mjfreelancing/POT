import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiGetAllAccounts } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import type { Account } from '@/data';
import AccountsOverview from '@/features/dashboard/components/AccountsOverview';
import { SuccessResult } from '@/lib';

import { createAccountWithIdentity } from '../../shared/factories/accountFactory';
vi.mock('@/api/hooks', () => ({
  useApiGetAllAccounts: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

function renderAccountsOverview(accounts: Account[]) {
  vi.mocked(useApiGetAllAccounts).mockReturnValue({
    data: new SuccessResult(accounts),
    isLoading: false,
    isFetching: false,
  } as unknown as ReturnType<typeof useApiGetAllAccounts>);

  return render(<AccountsOverview isOpen onOpenChange={vi.fn()} />);
}

// The accounts total is the sum the client computes itself, so it is the figure that can land a fraction of a cent
// away from what a person reading the cards would add up.
describe('AccountsOverview', () => {
  const setErrorMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });
  });

  test('renders a total that lands below zero as $0.00 rather than -$0.00', () => {
    renderAccountsOverview([
      createAccountWithIdentity('account-1', 'Everyday', {
        balance: 1,
        reserved: 0.5,
        stableExpenseAccrual: 0.25,
        available: 0.3,
      }),
      createAccountWithIdentity('account-2', 'Savings', {
        balance: 1,
        stableExpenseAccrual: 0.25,
        available: -0.1,
      }),
      createAccountWithIdentity('account-3', 'Visa', {
        balance: 1,
        stableExpenseAccrual: 0.25,
        available: -0.2,
      }),
    ]);

    // The other metrics carry distinct figures, so the only $0.00 on the row is the total available figure.
    expect(screen.getByText('$3.00')).toBeInTheDocument();
    expect(screen.getByText('$0.50')).toBeInTheDocument();

    expect(screen.getByText('Total Available')).toBeInTheDocument();
    expect(screen.getByText('$0.00')).toBeInTheDocument();
    expect(screen.queryByText('-$0.00')).not.toBeInTheDocument();
  });
});
