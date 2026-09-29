import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiRenewExpenses, useApiRenewIncomes } from '@/api/hooks';
import { ErrorProvider } from '@/contexts';
import QuickActions from '@/features/dashboard/components/QuickActions';
import { usePermissions } from '@/hooks';

import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock(
  '@/features/dashboard/contexts/AccrualsContext',
  async importOriginal => {
    const actual =
      await importOriginal<
        typeof import('@/features/dashboard/contexts/AccrualsContext')
      >();

    return {
      ...actual,
      AccrualsProvider: ({ children }: { children: ReactNode }) => (
        <>{children}</>
      ),
      useAccrualsContext: () => ({
        expenseRenewals: [],
        incomeRenewals: [],
        isLoading: false,
        error: null,
        invalidate: vi.fn(),
      }),
    };
  },
);

vi.mock('@/api/hooks', async importOriginal => {
  const actual = await importOriginal<typeof import('@/api/hooks')>();

  return {
    ...actual,
    useApiRenewExpenses: vi.fn(),
    useApiRenewIncomes: vi.fn(),
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

function renderQuickActions() {
  return render(
    <QueryClientProvider client={createQueryClient()}>
      <ErrorProvider>
        <QuickActions isOpen={true} onOpenChange={vi.fn()} />
      </ErrorProvider>
    </QueryClientProvider>,
  );
}

describe('QuickActions', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAll: true, hasAny: true }),
    );

    vi.mocked(useApiRenewExpenses).mockReturnValue({
      mutateAsync: vi.fn(),
    } as unknown as ReturnType<typeof useApiRenewExpenses>);

    vi.mocked(useApiRenewIncomes).mockReturnValue({
      mutateAsync: vi.fn(),
    } as unknown as ReturnType<typeof useApiRenewIncomes>);
  });

  // Accrual is derived on read now, so the section offers the two renewals and nothing to accrue.
  test('renders the two renewal tiles', () => {
    renderQuickActions();

    const tiles = screen.getAllByRole('heading', { level: 1 });

    expect(tiles.map(tile => tile.textContent)).toEqual([
      'Renew Expenses',
      'Renew Incomes',
    ]);
  });
});
