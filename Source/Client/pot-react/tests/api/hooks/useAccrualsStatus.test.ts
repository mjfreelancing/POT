import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  useApiAccrualsStatus,
  useApiAccrueAccountExpenses,
} from '@/api/hooks/useAccrualsStatus';
import { SuccessResult } from '@/lib';

import { useGet, usePost } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
  usePost: vi.fn(),
}));

describe('useAccrualsStatus hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useApiAccrueAccountExpenses composes accrue-expenses mutation endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiAccrueAccountExpenses());

    expect(usePost).toHaveBeenCalledWith('/accruals/accrue-expenses');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiAccrualsStatus composes status query endpoint, key, and options', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult({
        expenseRenewalsRequired: [],
        incomeRenewalsRequired: [],
        accountAccrualsRequired: [],
      }),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const input = {
      accountRowIds: ['acc-1', 'acc-2'],
    };

    const { result } = renderHook(() => useApiAccrualsStatus(input));

    expect(useGet).toHaveBeenCalledWith(
      '/accruals/status?accountRowIds=acc-1,acc-2',
      ['accruals', 'acc-1', 'acc-2'],
      {
        enabled: true,
        staleTime: 0,
        gcTime: 0,
        refetchOnWindowFocus: true,
        refetchOnMount: true,
      },
    );

    expect(result.current.data).toBe(queryResult.data);
  });

  test('useApiAccrualsStatus disables query when no account ids are provided', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: false,
      data: undefined,
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    renderHook(() =>
      useApiAccrualsStatus({
        accountRowIds: [],
      }),
    );

    const call = vi.mocked(useGet).mock.calls[0];
    expect(call?.[0]).toBe('/accruals/status?accountRowIds=');
    expect(call?.[2]?.enabled).toBe(false);
  });
});
