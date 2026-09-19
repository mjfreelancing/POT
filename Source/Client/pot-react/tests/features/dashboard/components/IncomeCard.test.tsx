import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { addDays } from 'date-fns';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { ErrorProvider } from '@/contexts';
import type { Income } from '@/data';
import IncomeCard from '@/features/dashboard/components/IncomeCard';
import { usePermissions } from '@/hooks';
import {
  dateIsoFormat,
  formatDate,
  formatMoneyValue,
  localToday,
  todayIsoFormat,
} from '@/lib';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createIncome } from '../../../shared/factories/incomeFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock('@/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/hooks')>();

  return {
    ...actual,
    usePermissions: vi.fn(),
  };
});

function renderCard(income: Income) {
  const queryClient = createQueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <ErrorProvider>
        <IncomeCard income={income} />
      </ErrorProvider>
    </QueryClientProvider>,
  );
}

describe('IncomeCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );
  });

  test('renders the description, due date, amount, and account', () => {
    const income = createIncome({
      description: 'Salary',
      nextDue: '2035-06-30',
      amount: 2500,
      account: { rowId: 'account-1', description: 'Everyday Account' },
    });

    renderCard(income);

    expect(screen.getByText('Salary')).toBeInTheDocument();

    expect(screen.getByText('Due:')).toBeInTheDocument();
    expect(screen.getByText(formatDate('2035-06-30'))).toBeInTheDocument();

    expect(screen.getByText('Amount:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(2500))).toBeInTheDocument();

    expect(screen.getByText('Everyday Account')).toBeInTheDocument();
  });

  test('omits the account row when the income has no account', () => {
    const income = createIncome({
      description: 'Salary',
      nextDue: '2035-06-30',
      account: undefined,
    });

    renderCard(income);

    expect(screen.getByText('Salary')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Income actions' }),
    ).not.toBeInTheDocument();
  });

  test('surfaces the overdue state when the due date has passed', () => {
    renderCard(createIncome({ nextDue: '2020-01-15' }));

    // The status badge and the due-date hint both report the overdue state.
    expect(screen.getAllByText('Overdue')).toHaveLength(2);
  });

  test('surfaces the due today state when the income is due today', () => {
    renderCard(createIncome({ nextDue: todayIsoFormat() }));

    expect(screen.getAllByText('Due Today')).toHaveLength(2);
  });

  test('shows Due Soon when the income is due within a week', () => {
    const nextDue = dateIsoFormat(addDays(localToday(), 3));

    renderCard(createIncome({ nextDue }));

    expect(screen.getByText('Due Soon')).toBeInTheDocument();
  });

  test('shows the days remaining and no status badge for a later due date', () => {
    renderCard(createIncome({ nextDue: '2035-06-30' }));

    expect(screen.getByText(/^\(\d+ days\)$/)).toBeInTheDocument();
    expect(screen.queryByText('Due Soon')).not.toBeInTheDocument();
  });
});
