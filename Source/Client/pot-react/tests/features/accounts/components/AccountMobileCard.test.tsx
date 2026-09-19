import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useErrorContext } from '@/contexts';
import AccountMobileCard from '@/features/accounts/components/AccountMobileCard';
import useDeleteAccount from '@/features/accounts/delete/hooks/useDeleteAccount';
import { formatMoneyValue } from '@/lib';

import { createAccount } from '../../../shared/factories/accountFactory';

vi.mock('react-router', async importOriginal => {
  const actual = await importOriginal<typeof import('react-router')>();

  return {
    ...actual,
    useNavigate: () => vi.fn(),
  };
});

vi.mock('@/contexts', async importOriginal => {
  const actual = await importOriginal<typeof import('@/contexts')>();

  return {
    ...actual,
    useErrorContext: vi.fn(),
  };
});

vi.mock('@/features/auth/components', async importOriginal => {
  const actual =
    await importOriginal<typeof import('@/features/auth/components')>();

  return {
    ...actual,
    WithPermission: ({ children }: { children: ReactNode }) => (
      <>{children}</>
    ),
  };
});

vi.mock('@/features/accounts/delete/hooks/useDeleteAccount', () => ({
  default: vi.fn(),
}));

describe('AccountMobileCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: vi.fn(),
    });

    vi.mocked(useDeleteAccount).mockReturnValue({
      deleteAccount: vi.fn(),
    } as unknown as ReturnType<typeof useDeleteAccount>);
  });

  test('renders the balance, available, and daily need rows', () => {
    const account = createAccount({
      description: 'Main account',
      balance: 1234.5,
      available: 1000,
      stableExpenseAccrual: 12.55,
    });

    render(<AccountMobileCard account={account} />);

    expect(screen.getByText('Balance:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(1234.5))).toBeInTheDocument();

    expect(screen.getByText('Available:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(1000))).toBeInTheDocument();

    expect(screen.getByText('Daily Need:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(12.55))).toBeInTheDocument();
  });
});
