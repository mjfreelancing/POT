import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import AccountCard from '@/features/dashboard/components/AccountCard';
import { formatMoneyValue } from '@/lib';

import { createAccount } from '../../../shared/factories/accountFactory';

describe('AccountCard', () => {
  test('renders the account name, BSB/number, balance, available, and daily need', () => {
    const account = createAccount({
      description: 'Main account',
      bsb: '112-879',
      number: '431685312',
      balance: 1703,
      available: 679.55,
      stableExpenseAccrual: 12.55,
    });

    render(<AccountCard account={account} />);

    expect(
      screen.getByRole('heading', { level: 3, name: 'Main account' }),
    ).toBeInTheDocument();

    expect(screen.getByText('BSB: 112-879')).toBeInTheDocument();
    expect(screen.getByText('Acc: 431685312')).toBeInTheDocument();

    expect(screen.getByText('Balance:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(1703))).toBeInTheDocument();

    expect(screen.getByText('Available:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(679.55))).toBeInTheDocument();

    expect(screen.getByText('Daily Need:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(12.55))).toBeInTheDocument();
  });

  test('shows the healthy badge when available funds are at least 10% of the balance', () => {
    const account = createAccount({ balance: 100, available: 50 });

    render(<AccountCard account={account} />);

    expect(screen.getByText('Healthy')).toBeInTheDocument();
  });

  test('shows the low badge when available funds are below 10% of the balance', () => {
    const account = createAccount({ balance: 100, available: 5 });

    render(<AccountCard account={account} />);

    expect(screen.getByText('Low')).toBeInTheDocument();
  });

  test('shows the overdrawn badge when available funds are negative', () => {
    const account = createAccount({ balance: 100, available: -1 });

    render(<AccountCard account={account} />);

    expect(screen.getByText('Overdrawn')).toBeInTheDocument();
  });
});
