import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import {
  useApiCreateAccount,
  useApiDeleteAccount,
  useApiGetAccountById,
  useApiGetAllAccounts,
  useApiUpdateAccount,
} from '@/api/hooks/useAccounts';
import type { Account } from '@/data';
import { FailResult, SuccessResult } from '@/lib';

import { useDelete, useGet, usePost, usePutWithId } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
  usePost: vi.fn(),
  usePutWithId: vi.fn(),
  useDelete: vi.fn(),
}));

function createAccount(overrides: Partial<Account>): Account {
  return {
    rowId: 'acc-1',
    etag: 0n,
    bsb: '000-000',
    number: '000000',
    description: 'Default account',
    balance: 0,
    reserved: 0,
    totalExpenseAccrued: 0,
    dailyExpenseAccrual: 0,
    stableExpenseAccrual: 0,
    available: 0,
    linkedExpenses: 0,
    linkedIncomes: 0,
    ...overrides,
  };
}

describe('useAccounts hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useApiGetAllAccounts fetches accounts and returns sorted success data', () => {
    const unsortedAccounts: Account[] = [
      createAccount({ rowId: 'acc-3', bsb: '999-111', number: '222222' }),
      createAccount({ rowId: 'acc-2', bsb: '111-111', number: '999999' }),
      createAccount({ rowId: 'acc-1', bsb: '111-111', number: '000001' }),
    ];

    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult(unsortedAccounts),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useApiGetAllAccounts());

    expect(useGet).toHaveBeenCalledWith('/accounts', ['accounts']);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.data).toBeInstanceOf(SuccessResult);

    if (result.current.data?.success) {
      expect(result.current.data.value.map(account => account.rowId)).toEqual([
        'acc-1',
        'acc-2',
        'acc-3',
      ]);
    }

    // Verify the source query data was not mutated by sorting.
    expect(unsortedAccounts.map(account => account.rowId)).toEqual([
      'acc-3',
      'acc-2',
      'acc-1',
    ]);
  });

  test('useApiGetAllAccounts passes through failure results unchanged', () => {
    const failureResult = new FailResult(new UnexpectedError('failed'));
    const queryResult = {
      isLoading: false,
      isSuccess: false,
      data: failureResult,
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useApiGetAllAccounts());

    expect(result.current.data).toBe(failureResult);
  });

  test('useApiGetAccountById forwards forceFresh options to useGet', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult(createAccount({ rowId: 'acc-1' })),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    renderHook(() => useApiGetAccountById('acc-1', { forceFresh: true }));

    expect(useGet).toHaveBeenCalledWith(
      '/accounts/acc-1',
      ['accounts', 'acc-1'],
      {
        staleTime: 0,
        refetchOnMount: 'always',
        usePreviousAsPlaceholder: false,
      },
    );
  });

  test('useApiCreateAccount composes usePost endpoint and returns mutation data', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({ rowId: 'created-1' }),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useApiCreateAccount());

    expect(usePost).toHaveBeenCalledWith('/accounts');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useApiUpdateAccount composes id endpoint via usePutWithId', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({ rowId: 'updated-1' }),
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    renderHook(() => useApiUpdateAccount());

    expect(usePutWithId).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePutWithId).mock.calls[0]?.[0];
    expect(endpointBuilder?.('abc-123')).toBe('/accounts/abc-123');
  });

  test('useApiDeleteAccount composes account delete endpoint', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(useDelete).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof useDelete>,
    );

    const { result } = renderHook(() => useApiDeleteAccount('acc-9'));

    expect(useDelete).toHaveBeenCalledWith('/accounts/acc-9');
    expect(result.current.data).toBe(mutationResult.data);
  });
});
