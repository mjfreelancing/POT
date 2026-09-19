import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { ErrorProvider } from '@/contexts';
import type { Account } from '@/data';
import AccountMobileCard from '@/features/accounts/components/AccountMobileCard';
import useDeleteAccount from '@/features/accounts/delete/hooks/useDeleteAccount';
import { usePermissions } from '@/hooks';
import { formatMoneyValue } from '@/lib';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createAccount } from '../../../shared/factories/accountFactory';

vi.mock('@/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/hooks')>();

  return {
    ...actual,
    usePermissions: vi.fn(),
  };
});

vi.mock('@/features/accounts/delete/hooks/useDeleteAccount', () => ({
  default: vi.fn(),
}));

function renderCard(account: Account) {
  return render(
    <ErrorProvider>
      <MemoryRouter>
        <AccountMobileCard account={account} />
      </MemoryRouter>
    </ErrorProvider>,
  );
}

describe('AccountMobileCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );

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

    renderCard(account);

    expect(screen.getByText('Balance:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(1234.5))).toBeInTheDocument();

    expect(screen.getByText('Available:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(1000))).toBeInTheDocument();

    expect(screen.getByText('Daily Need:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(12.55))).toBeInTheDocument();
  });

  test('blocks the edit and delete actions when the user cannot manage accounts', async () => {
    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: false }),
    );

    renderCard(createAccount({ description: 'Main account' }));

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }));

    const editMenuItem = screen.getByRole('menuitem', { name: 'Edit' });
    const deleteMenuItem = screen.getByRole('menuitem', { name: 'Delete' });

    expect(editMenuItem).toHaveAttribute('aria-disabled', 'true');
    expect(editMenuItem).toHaveClass('pointer-events-none');
    expect(editMenuItem.closest('div.cursor-not-allowed')).not.toBeNull();

    expect(deleteMenuItem).toHaveAttribute('aria-disabled', 'true');
    expect(deleteMenuItem).toHaveClass('pointer-events-none');
    expect(deleteMenuItem.closest('div.cursor-not-allowed')).not.toBeNull();
  });
});
