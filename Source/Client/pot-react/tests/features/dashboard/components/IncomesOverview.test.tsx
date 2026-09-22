import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { addDays } from 'date-fns';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import { useApiGetAllIncomes } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import type { Income } from '@/data';
import IncomesOverview from '@/features/dashboard/components/IncomesOverview';
import type { PeriodDays } from '@/features/dashboard/hooks/useDashboardStorage';
import useDashboardStorage from '@/features/dashboard/hooks/useDashboardStorage';
import { usePermissions } from '@/hooks';
import { dateIsoFormat, FailResult, localToday, SuccessResult } from '@/lib';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createIncome } from '../../../shared/factories/incomeFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock('@/api/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/api/hooks')>();

  return {
    ...actual,
    useApiGetAllIncomes: vi.fn(),
  };
});

vi.mock('@/concerns', async importOriginal => {
  const actual = await importOriginal<typeof import('@/concerns')>();

  return {
    ...actual,
    logger: {
      info: vi.fn(),
      warn: vi.fn(),
      error: vi.fn(),
    },
  };
});

vi.mock('@/contexts', async importOriginal => {
  const actual = await importOriginal<typeof import('@/contexts')>();

  return {
    ...actual,
    useErrorContext: vi.fn(),
  };
});

vi.mock('@/features/dashboard/hooks/useDashboardStorage', () => ({
  default: vi.fn(),
}));

vi.mock('@/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/hooks')>();

  return {
    ...actual,
    usePermissions: vi.fn(),
  };
});

const defaultDashboardData = {
  quickActionsOpen: true,
  accountsOpen: true,
  incomesOpen: true,
  expensesOpen: true,
  expensesPeriod: 30 as PeriodDays,
  incomesPeriod: 30 as PeriodDays,
};

function createIncomesQuery(incomes: Income[], isLoading = false) {
  return {
    data: new SuccessResult(incomes),
    isLoading,
  } as unknown as ReturnType<typeof useApiGetAllIncomes>;
}

function renderOverview() {
  const queryClient = createQueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <IncomesOverview isOpen onOpenChange={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe('IncomesOverview', () => {
  const getDashboardDataMock = vi.fn();
  const setDashboardDataMock = vi.fn();
  const setErrorMock = vi.fn();

  function givenStoredPeriod(days: PeriodDays) {
    getDashboardDataMock.mockReturnValue({
      ...defaultDashboardData,
      incomesPeriod: days,
    });
  }

  beforeEach(() => {
    vi.clearAllMocks();

    givenStoredPeriod(30);

    vi.mocked(useDashboardStorage).mockReturnValue({
      getDashboardData: getDashboardDataMock,
      setDashboardData: setDashboardDataMock,
    });

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );
  });

  test('renders the section heading and the next due filter', () => {
    vi.mocked(useApiGetAllIncomes).mockReturnValue(createIncomesQuery([]));

    renderOverview();

    expect(
      screen.getByRole('heading', { name: 'Incomes Overview' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Next Due:')).toBeInTheDocument();
    expect(screen.getByRole('combobox')).toBeInTheDocument();
  });

  test('renders incomes due within the stored period and omits later incomes', () => {
    const dueSoon = createIncome({
      rowId: 'income-1',
      description: 'Due soon',
      nextDue: dateIsoFormat(addDays(localToday(), 3)),
    });

    const dueLater = createIncome({
      rowId: 'income-2',
      description: 'Due later',
      nextDue: dateIsoFormat(addDays(localToday(), 20)),
    });

    const dueMuchLater = createIncome({
      rowId: 'income-3',
      description: 'Due much later',
      nextDue: dateIsoFormat(addDays(localToday(), 45)),
    });

    vi.mocked(useApiGetAllIncomes).mockReturnValue(
      createIncomesQuery([dueSoon, dueLater, dueMuchLater]),
    );

    renderOverview();

    expect(screen.getByText('Due soon')).toBeInTheDocument();
    expect(screen.getByText('Due later')).toBeInTheDocument();
    expect(screen.queryByText('Due much later')).not.toBeInTheDocument();
  });

  test('excludes incomes marked as excluded from calculations', () => {
    const included = createIncome({
      rowId: 'income-1',
      description: 'Included income',
      nextDue: dateIsoFormat(addDays(localToday(), 3)),
    });

    const excluded = createIncome({
      rowId: 'income-2',
      description: 'Excluded income',
      nextDue: dateIsoFormat(addDays(localToday(), 3)),
      excludeFromCalcs: true,
    });

    vi.mocked(useApiGetAllIncomes).mockReturnValue(
      createIncomesQuery([included, excluded]),
    );

    renderOverview();

    expect(screen.getByText('Included income')).toBeInTheDocument();
    expect(screen.queryByText('Excluded income')).not.toBeInTheDocument();
  });

  test('filters incomes using the stored 7 day period', () => {
    givenStoredPeriod(7);

    const dueSoon = createIncome({
      rowId: 'income-1',
      description: 'Due in three days',
      nextDue: dateIsoFormat(addDays(localToday(), 3)),
    });

    const dueInTenDays = createIncome({
      rowId: 'income-2',
      description: 'Due in ten days',
      nextDue: dateIsoFormat(addDays(localToday(), 10)),
    });

    vi.mocked(useApiGetAllIncomes).mockReturnValue(
      createIncomesQuery([dueSoon, dueInTenDays]),
    );

    renderOverview();

    expect(screen.getByText('Due in three days')).toBeInTheDocument();
    expect(screen.queryByText('Due in ten days')).not.toBeInTheDocument();
  });

  test('persists the selected period and re-filters the incomes', async () => {
    const user = userEvent.setup();

    givenStoredPeriod(7);

    const dueSoon = createIncome({
      rowId: 'income-1',
      description: 'Due in three days',
      nextDue: dateIsoFormat(addDays(localToday(), 3)),
    });

    const dueLater = createIncome({
      rowId: 'income-2',
      description: 'Due in twenty days',
      nextDue: dateIsoFormat(addDays(localToday(), 20)),
    });

    vi.mocked(useApiGetAllIncomes).mockReturnValue(
      createIncomesQuery([dueSoon, dueLater]),
    );

    renderOverview();

    expect(screen.queryByText('Due in twenty days')).not.toBeInTheDocument();

    await user.click(screen.getByRole('combobox'));
    await user.click(
      await screen.findByRole('option', { name: 'Within 30 Days' }),
    );

    expect(setDashboardDataMock).toHaveBeenCalledWith({ incomesPeriod: 30 });
    expect(screen.getByText('Due in twenty days')).toBeInTheDocument();
  });

  test('shows card skeletons while the incomes are loading', () => {
    const dueSoon = createIncome({
      rowId: 'income-1',
      description: 'Due in three days',
      nextDue: dateIsoFormat(addDays(localToday(), 3)),
    });

    vi.mocked(useApiGetAllIncomes).mockReturnValue(
      createIncomesQuery([dueSoon], true),
    );

    const { container } = renderOverview();

    expect(container.querySelectorAll('[data-slot="skeleton"]')).toHaveLength(
      4,
    );

    expect(screen.queryByText('Due in three days')).not.toBeInTheDocument();
  });

  test('reports API failures through the error context', () => {
    const failure = new FailResult(new UnexpectedError('Incomes failed'));

    vi.mocked(useApiGetAllIncomes).mockReturnValue({
      data: failure,
      isLoading: false,
    } as unknown as ReturnType<typeof useApiGetAllIncomes>);

    renderOverview();

    expect(setErrorMock).toHaveBeenCalledWith({
      title: failure.error.code,
      description: failure.error.description,
    });
  });
});
