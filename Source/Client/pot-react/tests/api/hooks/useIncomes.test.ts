import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import {
  useApiCreateIncome,
  useApiDeleteIncome,
  useApiGetAllIncomes,
  useApiGetIncomeById,
  useApiRenewIncomes,
  useApiToggleExcludeIncomes,
  useApiUpdateIncome,
} from '@/api/hooks/useIncomes';
import type { Income } from '@/data';
import { FailResult, SuccessResult } from '@/lib';

import { useDelete, useGet, usePost, usePutWithId } from '@/api/hooks/useApi';
import { createIncome } from '../../shared/factories/incomeFactory';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
  usePost: vi.fn(),
  usePutWithId: vi.fn(),
  useDelete: vi.fn(),
}));

describe('useIncomes hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useApiGetAllIncomes fetches incomes and returns sorted success data', () => {
    const unsortedIncomes: Income[] = [
      createIncome({
        rowId: 'inc-3',
        nextDue: '2026-03-20',
        description: 'Zulu Income',
      }),
      createIncome({
        rowId: 'inc-2',
        nextDue: '2026-02-10',
        description: 'Bravo Income',
      }),
      createIncome({
        rowId: 'inc-1',
        nextDue: '2026-02-10',
        description: 'Alpha Income',
      }),
    ];

    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult(unsortedIncomes),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useApiGetAllIncomes());

    expect(useGet).toHaveBeenCalledWith('/incomes', ['incomes']);
    expect(result.current.data).toBeInstanceOf(SuccessResult);

    if (result.current.data?.success) {
      expect(result.current.data.value.map(income => income.rowId)).toEqual([
        'inc-1',
        'inc-2',
        'inc-3',
      ]);
    }

    expect(unsortedIncomes.map(income => income.rowId)).toEqual([
      'inc-3',
      'inc-2',
      'inc-1',
    ]);
  });

  test('useApiGetAllIncomes passes through failure results unchanged', () => {
    const failureResult = new FailResult(new UnexpectedError('failed'));
    const queryResult = {
      isLoading: false,
      isSuccess: false,
      data: failureResult,
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useApiGetAllIncomes());

    expect(result.current.data).toBe(failureResult);
  });

  test('useApiGetIncomeById forwards forceFresh options to useGet', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult(createIncome({ rowId: 'inc-1' })),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    renderHook(() => useApiGetIncomeById('inc-1', { forceFresh: true }));

    expect(useGet).toHaveBeenCalledWith(
      '/incomes/inc-1',
      ['incomes', 'inc-1'],
      {
        staleTime: 0,
        refetchOnMount: 'always',
        usePreviousAsPlaceholder: false,
      },
    );
  });

  test('useApiCreateIncome composes usePost endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({ rowId: 'created-1' }),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiCreateIncome());

    expect(usePost).toHaveBeenCalledWith('/incomes');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiUpdateIncome composes id endpoint via usePutWithId', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({ rowId: 'updated-1' }),
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    renderHook(() => useApiUpdateIncome());

    expect(usePutWithId).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePutWithId).mock.calls[0]?.[0];
    expect(endpointBuilder?.('inc-123')).toBe('/incomes/inc-123');
  });

  test('useApiDeleteIncome composes delete endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(useDelete).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof useDelete>,
    );

    const { result } = renderHook(() => useApiDeleteIncome('inc-9'));

    expect(useDelete).toHaveBeenCalledWith('/incomes/inc-9');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiToggleExcludeIncomes composes toggle exclude endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiToggleExcludeIncomes());

    expect(usePost).toHaveBeenCalledWith('/incomes/toggleExclude');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiRenewIncomes composes renew endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiRenewIncomes());

    expect(usePost).toHaveBeenCalledWith('/incomes/renew');
    expect(result.current.data).toBe(mutationResult.data);
  });
});
