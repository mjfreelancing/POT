import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { ErrorProvider } from '@/contexts';
import type { Income } from '@/data';
import IncomeMobileCard from '@/features/incomes/components/IncomeMobileCard';
import { usePermissions } from '@/hooks';
import { formatDate, formatMoneyValue, todayIsoFormat } from '@/lib';

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
        <MemoryRouter>
          <IncomeMobileCard income={income} />
        </MemoryRouter>
      </ErrorProvider>
    </QueryClientProvider>,
  );
}

describe('IncomeMobileCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );
  });

  test('renders the description, due date, amount, and account', () => {
    const income = createIncome({
      description: 'Salary',
      nextDue: '2020-01-15',
      amount: 2500,
      account: { rowId: 'account-1', description: 'Everyday Account' },
    });

    renderCard(income);

    expect(screen.getByText('Salary')).toBeInTheDocument();

    expect(screen.getByText('Due:')).toBeInTheDocument();
    expect(screen.getByText(formatDate('2020-01-15'))).toBeInTheDocument();

    expect(screen.getByText('Amount:')).toBeInTheDocument();
    expect(screen.getByText(formatMoneyValue(2500))).toBeInTheDocument();

    expect(screen.getByText('Everyday Account')).toBeInTheDocument();
  });

  test('renders the end date when the income has one', () => {
    const income = createIncome({
      nextDue: '2020-01-15',
      endDate: '2035-06-30',
    });

    renderCard(income);

    expect(screen.getByText('End Date:')).toBeInTheDocument();
    expect(screen.getByText(formatDate('2035-06-30'))).toBeInTheDocument();
  });

  test('omits the end date row when the income has no end date', () => {
    renderCard(createIncome({ nextDue: '2020-01-15', endDate: null }));

    expect(screen.queryByText('End Date:')).not.toBeInTheDocument();
  });

  test('shows Overdue for a past due date', () => {
    renderCard(createIncome({ nextDue: '2020-01-15' }));

    expect(screen.getByText('Overdue')).toBeInTheDocument();
  });

  test('shows Due Today when the income is due today', () => {
    renderCard(createIncome({ nextDue: todayIsoFormat() }));

    expect(screen.getByText('Due Today')).toBeInTheDocument();
  });

  test('shows the days remaining for a future due date', () => {
    renderCard(createIncome({ nextDue: '2035-06-30' }));

    expect(screen.getByText(/^\(\d+ days\)$/)).toBeInTheDocument();
  });

  test('marks the card as excluded from calculations', () => {
    renderCard(createIncome({ nextDue: '2020-01-15', excludeFromCalcs: true }));

    expect(
      screen.getByLabelText('Excluded from calculations'),
    ).toBeInTheDocument();

    expect(screen.queryByText('Overdue')).not.toBeInTheDocument();
  });
});
