import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiRenewIncomes, useApiToggleExcludeIncomes } from '@/api/hooks';
import { TooltipProvider } from '@/components/ui/tooltip';
import { ErrorProvider, useErrorContext } from '@/contexts';
import type { Income } from '@/data';
import IncomesTable from '@/features/incomes/components/IncomesTable';
import useDeleteIncome from '@/features/incomes/delete/hooks/useDeleteIncome';
import { usePermissions } from '@/hooks';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createIncome } from '../../../shared/factories/incomeFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock('@/api/hooks/useIncomes', async importOriginal => {
  const actual =
    await importOriginal<typeof import('@/api/hooks/useIncomes')>();

  return {
    ...actual,
    useApiRenewIncomes: vi.fn(),
    useApiToggleExcludeIncomes: vi.fn(),
  };
});

vi.mock('@/features/incomes/delete/hooks/useDeleteIncome', () => ({
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

function renderTable(filteredIncomes: Income[]) {
  return render(
    <QueryClientProvider client={createQueryClient()}>
      <ErrorProvider>
        <MemoryRouter>
          <TooltipProvider>
            <IncomesTable filteredIncomes={filteredIncomes} />
          </TooltipProvider>
        </MemoryRouter>
      </ErrorProvider>
    </QueryClientProvider>,
  );
}

describe('IncomesTable', () => {
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

    vi.mocked(useApiRenewIncomes).mockReturnValue({
      mutateAsync: vi.fn(),
    } as unknown as ReturnType<typeof useApiRenewIncomes>);

    vi.mocked(useApiToggleExcludeIncomes).mockReturnValue({
      mutateAsync: vi.fn(),
    } as unknown as ReturnType<typeof useApiToggleExcludeIncomes>);

    vi.mocked(useDeleteIncome).mockReturnValue({
      deleteIncome: vi.fn(),
    } as unknown as ReturnType<typeof useDeleteIncome>);
  });

  test('shows a tooltip for the hovered heading', async () => {
    const user = userEvent.setup();

    renderTable([createIncome({ description: 'Salary' })]);

    const heading = screen.getByRole('button', { name: /amount/i });

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await user.hover(heading);

    const tooltip = await screen.findByRole('tooltip');

    expect(screen.getAllByRole('tooltip')).toHaveLength(1);
    expect(heading).toHaveAttribute('aria-describedby', tooltip.id);
  });
});
