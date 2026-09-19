import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { ErrorProvider } from '@/contexts';
import type { Expense } from '@/data';
import ExpenseMobileCard from '@/features/expenses/components/ExpenseMobileCard';
import { usePermissions } from '@/hooks';
import { formatDate, formatMoneyValue, todayIsoFormat } from '@/lib';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createExpense } from '../../../shared/factories/expenseFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock('@/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/hooks')>();

  return {
    ...actual,
    usePermissions: vi.fn(),
  };
});

function renderCard(expense: Expense) {
  const queryClient = createQueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <ErrorProvider>
        <MemoryRouter>
          <ExpenseMobileCard expense={expense} />
        </MemoryRouter>
      </ErrorProvider>
    </QueryClientProvider>,
  );
}

describe('ExpenseMobileCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );
  });

  test('renders the description, due date, amount, and account', () => {
    const expense = createExpense({
      description: 'Rent',
      nextDue: '2020-01-15',
      amount: 900,
      account: { rowId: 'account-1', description: 'Bills Account' },
    });

    renderCard(expense);

    expect(screen.getByText('Rent')).toBeInTheDocument();

    expect(screen.getByText('Due:')).toBeInTheDocument();
    expect(screen.getByText(formatDate('2020-01-15'))).toBeInTheDocument();

    expect(screen.getByText('Amount:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(900))).toBeInTheDocument();

    expect(screen.getByText('Bills Account')).toBeInTheDocument();
  });

  test('shows Overdue for a past due date', () => {
    renderCard(createExpense({ nextDue: '2020-01-15' }));

    expect(screen.getByText('Overdue')).toBeInTheDocument();
  });

  test('shows Due Today when the expense is due today', () => {
    renderCard(createExpense({ nextDue: todayIsoFormat() }));

    expect(screen.getByText('Due Today')).toBeInTheDocument();
  });

  test('shows the days remaining for a future due date', () => {
    renderCard(createExpense({ nextDue: '2035-06-30' }));

    expect(screen.getByText(/^\(\d+ days\)$/)).toBeInTheDocument();
  });

  test('marks the card as excluded from calculations', () => {
    renderCard(
      createExpense({ nextDue: '2020-01-15', excludeFromCalcs: true }),
    );

    expect(
      screen.getByLabelText('Excluded from calculations'),
    ).toBeInTheDocument();

    expect(screen.queryByText('Overdue')).not.toBeInTheDocument();
  });
});
