import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import {
  useApiCreateExpense,
  useApiDeleteExpense,
  useApiGetAllExpenses,
  useApiGetExpenseById,
  useApiRenewExpenses,
  useApiToggleExcludeExpenses,
  useApiUpdateExpense,
} from '@/api/hooks/useExpenses';
import type { Expense } from '@/data';
import { FailResult, SuccessResult } from '@/lib';
import { createExpense } from '../../shared/factories/expenseFactory';

import { useDelete, useGet, usePost, usePutWithId } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
  usePost: vi.fn(),
  usePutWithId: vi.fn(),
  useDelete: vi.fn(),
}));

describe('useExpenses hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useApiGetAllExpenses fetches expenses and returns sorted success data', () => {
    const unsortedExpenses: Expense[] = [
      createExpense({
        rowId: 'exp-3',
        nextDue: '2026-03-20',
        description: 'Zulu Expense',
      }),
      createExpense({
        rowId: 'exp-2',
        nextDue: '2026-02-10',
        description: 'Bravo Expense',
      }),
      createExpense({
        rowId: 'exp-1',
        nextDue: '2026-02-10',
        description: 'Alpha Expense',
      }),
    ];

    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult(unsortedExpenses),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useApiGetAllExpenses());

    expect(useGet).toHaveBeenCalledWith('/expenses', ['expenses']);
    expect(result.current.data).toBeInstanceOf(SuccessResult);

    if (result.current.data?.success) {
      expect(result.current.data.value.map(expense => expense.rowId)).toEqual([
        'exp-1',
        'exp-2',
        'exp-3',
      ]);
    }

    expect(unsortedExpenses.map(expense => expense.rowId)).toEqual([
      'exp-3',
      'exp-2',
      'exp-1',
    ]);
  });

  test('useApiGetAllExpenses passes through failure results unchanged', () => {
    const failureResult = new FailResult(new UnexpectedError('failed'));

    const queryResult = {
      isLoading: false,
      isSuccess: false,
      data: failureResult,
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useApiGetAllExpenses());

    expect(result.current.data).toBe(failureResult);
  });

  test('useApiGetExpenseById forwards forceFresh options to useGet', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult(createExpense({ rowId: 'exp-1' })),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    renderHook(() => useApiGetExpenseById('exp-1', { forceFresh: true }));

    expect(useGet).toHaveBeenCalledWith(
      '/expenses/exp-1',
      ['expenses', 'exp-1'],
      {
        staleTime: 0,
        refetchOnMount: 'always',
        usePreviousAsPlaceholder: false,
      },
    );
  });

  test('useApiCreateExpense composes usePost endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({ rowId: 'created-1' }),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiCreateExpense());

    expect(usePost).toHaveBeenCalledWith('/expenses');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiUpdateExpense composes id endpoint via usePutWithId', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({ rowId: 'updated-1' }),
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    renderHook(() => useApiUpdateExpense());

    expect(usePutWithId).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePutWithId).mock.calls[0]?.[0];
    expect(endpointBuilder?.('exp-123')).toBe('/expenses/exp-123');
  });

  test('useApiDeleteExpense composes delete endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(useDelete).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof useDelete>,
    );

    const { result } = renderHook(() => useApiDeleteExpense('exp-9'));

    expect(useDelete).toHaveBeenCalledWith('/expenses/exp-9');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiToggleExcludeExpenses composes toggle exclude endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiToggleExcludeExpenses());

    expect(usePost).toHaveBeenCalledWith('/expenses/toggleExclude');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiRenewExpenses composes renew endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiRenewExpenses());

    expect(usePost).toHaveBeenCalledWith('/expenses/renew');
    expect(result.current.data).toBe(mutationResult.data);
  });
});
