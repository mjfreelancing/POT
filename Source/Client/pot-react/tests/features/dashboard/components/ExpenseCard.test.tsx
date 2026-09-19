import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { addDays } from 'date-fns';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { ErrorProvider } from '@/contexts';
import type { Expense } from '@/data';
import ExpenseCard from '@/features/dashboard/components/ExpenseCard';
import { usePermissions } from '@/hooks';
import {
  dateIsoFormat,
  formatDate,
  formatMoneyValue,
  localToday,
  todayIsoFormat,
} from '@/lib';

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
        <ExpenseCard expense={expense} />
      </ErrorProvider>
    </QueryClientProvider>,
  );
}

describe('ExpenseCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );
  });

  test('renders the description, due date, amount, and account', () => {
    const expense = createExpense({
      description: 'Rent',
      nextDue: '2035-06-30',
      amount: 900,
      account: { rowId: 'account-1', description: 'Bills Account' },
    });

    renderCard(expense);

    expect(screen.getByText('Rent')).toBeInTheDocument();

    expect(screen.getByText('Due:')).toBeInTheDocument();
    expect(screen.getByText(formatDate('2035-06-30'))).toBeInTheDocument();

    expect(screen.getByText('Amount:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(900))).toBeInTheDocument();

    expect(screen.getByText('Bills Account')).toBeInTheDocument();
  });

  test('omits the account row when the expense has no account', () => {
    const expense = createExpense({
      description: 'Rent',
      nextDue: '2035-06-30',
      account: undefined,
    });

    renderCard(expense);

    expect(screen.getByText('Rent')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Expense actions' }),
    ).not.toBeInTheDocument();
  });

  test('surfaces the overdue state when the due date has passed', () => {
    renderCard(createExpense({ nextDue: '2020-01-15' }));

    // The status badge and the due-date hint both report the overdue state.
    expect(screen.getAllByText('Overdue')).toHaveLength(2);
  });

  test('surfaces the due today state when the expense is due today', () => {
    renderCard(createExpense({ nextDue: todayIsoFormat() }));

    expect(screen.getAllByText('Due Today')).toHaveLength(2);
  });

  test('shows Due Soon when the expense is due within a week', () => {
    const nextDue = dateIsoFormat(addDays(localToday(), 3));

    renderCard(createExpense({ nextDue }));

    expect(screen.getByText('Due Soon')).toBeInTheDocument();
  });

  test('shows the days remaining and no status badge for a later due date', () => {
    renderCard(createExpense({ nextDue: '2035-06-30' }));

    expect(screen.getByText(/^\(\d+ days\)$/)).toBeInTheDocument();
    expect(screen.queryByText('Due Soon')).not.toBeInTheDocument();
  });
});
