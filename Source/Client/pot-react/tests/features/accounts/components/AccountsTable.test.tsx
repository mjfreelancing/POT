import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { TooltipProvider } from '@/components/ui/tooltip';
import { ErrorProvider } from '@/contexts';
import type { Account } from '@/data';
import AccountsTable from '@/features/accounts/components/AccountsTable';
import useDeleteAccount from '@/features/accounts/delete/hooks/useDeleteAccount';
import { usePermissions } from '@/hooks';

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

function renderTable(accounts: Account[]) {
  return render(
    <ErrorProvider>
      <MemoryRouter>
        <TooltipProvider>
          <AccountsTable accounts={accounts} />
        </TooltipProvider>
      </MemoryRouter>
    </ErrorProvider>,
  );
}

describe('AccountsTable', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );

    vi.mocked(useDeleteAccount).mockReturnValue({
      deleteAccount: vi.fn(),
    } as unknown as ReturnType<typeof useDeleteAccount>);
  });

  // The obligation columns and their order are a settled requirement: what the account holds, then the rate,
  // then the commitments left to right — accrual in progress, past-due debt, headline total.
  test('renders the columns in their settled order', () => {
    renderTable([createAccount({ description: 'Main account' })]);

    const headers = screen
      .getAllByRole('columnheader')
      .map(header => header.textContent);

    expect(headers).toEqual([
      'Description',
      'Balance',
      'Reserved',
      'Available',
      'Daily Need',
      'Accrued',
      'Arrears',
      'Committed',
      '',
    ]);
  });

  test('renders the arrears and committed figures for an account', () => {
    const account = createAccount({
      description: 'Main account',
      balance: 1000,
      reserved: 100,
      totalExpenseAccrued: 300,
      totalArrears: 140,
      totalCommitted: 440,
      stableExpenseAccrual: 12.55,
      available: 560,
    });

    renderTable([account]);

    expect(screen.getByText('$140.00')).toBeInTheDocument();
    expect(screen.getByText('$440.00')).toBeInTheDocument();
  });

  // The table has no bulk actions, so it must not offer row selection at all: no selection
  // checkboxes and no bulk actions bar.
  test('renders no row selection or bulk actions', () => {
    renderTable([createAccount({ description: 'Main account' })]);

    expect(screen.queryByLabelText('Select all')).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/^Select row/)).not.toBeInTheDocument();
    expect(screen.queryByText('No items selected')).not.toBeInTheDocument();
  });

  // Hint wording is content, so this asserts the mechanism instead: hovering a heading opens exactly
  // one tooltip, and it is the tooltip that heading describes.
  test('shows a tooltip for the hovered heading', async () => {
    const user = userEvent.setup();

    renderTable([createAccount({ description: 'Main account' })]);

    const heading = screen.getByRole('button', { name: /committed/i });

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await user.hover(heading);

    const tooltip = await screen.findByRole('tooltip');

    expect(screen.getAllByRole('tooltip')).toHaveLength(1);
    expect(heading).toHaveAttribute('aria-describedby', tooltip.id);
  });
});
