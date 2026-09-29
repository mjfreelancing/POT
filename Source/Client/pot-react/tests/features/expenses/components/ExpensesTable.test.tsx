import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  useApiRenewExpenses,
  useApiToggleExcludeExpenses,
} from '@/api/hooks/useExpenses';
import { TooltipProvider } from '@/components/ui/tooltip';
import { ErrorProvider, useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import ExpensesTable from '@/features/expenses/components/ExpensesTable';
import useDeleteExpense from '@/features/expenses/delete/hooks/useDeleteExpense';
import { usePermissions } from '@/hooks';
import { AccrualPolicy } from '@/lib';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createExpense } from '../../../shared/factories/expenseFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock('@/api/hooks/useExpenses', async importOriginal => {
  const actual =
    await importOriginal<typeof import('@/api/hooks/useExpenses')>();

  return {
    ...actual,
    useApiRenewExpenses: vi.fn(),
    useApiToggleExcludeExpenses: vi.fn(),
  };
});

vi.mock('@/features/expenses/delete/hooks/useDeleteExpense', () => ({
  default: vi.fn(),
}));

vi.mock('@/contexts', async importOriginal => {
  const actual = await importOriginal<typeof import('@/contexts')>();

  return {
    ...actual,
    useErrorContext: vi.fn(),
  };
});

vi.mock('@/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/hooks')>();

  return {
    ...actual,
    usePermissions: vi.fn(),
  };
});

vi.mock('sonner', () => ({
  toast: vi.fn(),
}));

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

function renderTable(filteredExpenses: Expense[]) {
  return render(
    <QueryClientProvider client={createQueryClient()}>
      <ErrorProvider>
        <MemoryRouter>
          <TooltipProvider>
            <ExpensesTable filteredExpenses={filteredExpenses} />
          </TooltipProvider>
        </MemoryRouter>
      </ErrorProvider>
    </QueryClientProvider>,
  );
}

describe('ExpensesTable', () => {
  const setErrorMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true }),
    );

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    vi.mocked(useApiRenewExpenses).mockReturnValue({
      mutateAsync: vi.fn(),
    } as unknown as ReturnType<typeof useApiRenewExpenses>);

    vi.mocked(useApiToggleExcludeExpenses).mockReturnValue({
      mutateAsync: vi.fn(),
    } as unknown as ReturnType<typeof useApiToggleExcludeExpenses>);

    vi.mocked(useDeleteExpense).mockReturnValue({
      deleteExpense: vi.fn(),
    } as unknown as ReturnType<typeof useDeleteExpense>);
  });

  test('renders the accrued and arrears figures for an expense', () => {
    const expense = createExpense({
      description: 'Rent',
      amount: 500,
      accrualPolicy: AccrualPolicy.Automatic,
      accrued: 35,
      arrears: 140,
    });

    renderTable([expense]);

    expect(screen.getByText('Accrued')).toBeInTheDocument();
    expect(screen.getByText('$35.00')).toBeInTheDocument();

    expect(screen.getByText('Arrears')).toBeInTheDocument();
    expect(screen.getByText('$140.00')).toBeInTheDocument();
  });

  // Arrears follows the schedule rather than the accrual policy, so a non-accruing expense still shows it even
  // though its accrued cell is left empty.
  test('renders arrears for a non-accruing expense', () => {
    const expense = createExpense({
      description: 'Insurance',
      amount: 500,
      accrualPolicy: AccrualPolicy.None,
      accrued: 0,
      arrears: 140,
    });

    renderTable([expense]);

    expect(screen.getByText('$140.00')).toBeInTheDocument();
    expect(screen.queryByText('$0.00')).not.toBeInTheDocument();
  });

  // Accrual Start is not sortable, so its heading renders without the sort button. Hovering it must still
  // open exactly one tooltip, and it must be the tooltip that heading describes.
  test('shows a tooltip for the hovered heading', async () => {
    const user = userEvent.setup();

    renderTable([createExpense({ description: 'Rent' })]);

    const heading = screen.getByText('Accrual Start');

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await user.hover(heading);

    const tooltip = await screen.findByRole('tooltip');

    expect(screen.getAllByRole('tooltip')).toHaveLength(1);
    expect(heading).toHaveAttribute('aria-describedby', tooltip.id);
  });
});
