import { act, renderHook } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import useAccountFilter from '@/hooks/useAccountFilter';

import { createAccountWithIdentity } from '../shared/factories/accountFactory';

type TestItem = {
  id: string;
  account?: {
    rowId: string | number;
  } | null;
};

describe('useAccountFilter', () => {
  test('builds accountsInItems from used accounts sorted by description', () => {
    const accounts = [
      createAccountWithIdentity('a-2', 'Zeta', { etag: 1n }),
      createAccountWithIdentity('a-1', 'Alpha', { etag: 1n }),
      createAccountWithIdentity('a-3', 'Beta', { etag: 1n }),
    ];

    const items: TestItem[] = [
      { id: '1', account: { rowId: 'a-2' } },
      { id: '2', account: { rowId: 'a-1' } },
      { id: '3', account: { rowId: 'a-2' } },
    ];

    const onAccountChange = vi.fn();

    const { result } = renderHook(() =>
      useAccountFilter({
        accounts,
        items,
        selectedAccountId: null,
        onAccountChange,
      }),
    );

    expect(
      result.current.accountsInItems.map(account => account.rowId),
    ).toEqual(['a-1', 'a-2']);

    expect(result.current.filteredItems).toEqual(items);
  });

  test('includes Not Assigned virtual account when unassigned items exist', () => {
    const accounts = [createAccountWithIdentity('a-1', 'Alpha', { etag: 1n })];
    const items: TestItem[] = [
      { id: '1', account: { rowId: 'a-1' } },
      { id: '2', account: null },
      { id: '3' },
    ];

    const onAccountChange = vi.fn();

    const { result } = renderHook(() =>
      useAccountFilter({
        accounts,
        items,
        selectedAccountId: null,
        onAccountChange,
      }),
    );

    expect(result.current.accountsInItems[0].rowId).toBe('not-assigned');
    expect(result.current.accountsInItems[0].description).toBe('Not Assigned');
  });

  test('filters to selected account id when account is selected', () => {
    const accounts = [
      createAccountWithIdentity('a-1', 'Alpha', { etag: 1n }),
      createAccountWithIdentity('a-2', 'Beta', { etag: 1n }),
    ];
    const items: TestItem[] = [
      { id: '1', account: { rowId: 'a-1' } },
      { id: '2', account: { rowId: 'a-2' } },
      { id: '3', account: { rowId: 'a-1' } },
    ];

    const onAccountChange = vi.fn();

    const { result } = renderHook(() =>
      useAccountFilter({
        accounts,
        items,
        selectedAccountId: 'a-1',
        onAccountChange,
      }),
    );

    expect(result.current.filteredItems.map(item => item.id)).toEqual([
      '1',
      '3',
    ]);
  });

  test('filters to unassigned items when Not Assigned is selected', () => {
    const accounts = [createAccountWithIdentity('a-1', 'Alpha', { etag: 1n })];
    const items: TestItem[] = [
      { id: '1', account: { rowId: 'a-1' } },
      { id: '2', account: null },
      { id: '3' },
    ];

    const onAccountChange = vi.fn();

    const { result } = renderHook(() =>
      useAccountFilter({
        accounts,
        items,
        selectedAccountId: 'not-assigned',
        onAccountChange,
      }),
    );

    expect(result.current.filteredItems.map(item => item.id)).toEqual([
      '2',
      '3',
    ]);
  });

  test('clears selected account when it no longer exists in accountsInItems', () => {
    const accounts = [
      createAccountWithIdentity('a-1', 'Alpha', { etag: 1n }),
      createAccountWithIdentity('a-2', 'Beta', { etag: 1n }),
    ];
    const onAccountChange = vi.fn();

    const { rerender } = renderHook(
      ({ items, selectedAccountId }) =>
        useAccountFilter({
          accounts,
          items,
          selectedAccountId,
          onAccountChange,
        }),
      {
        initialProps: {
          items: [{ id: '1', account: { rowId: 'a-1' } }] as TestItem[],
          selectedAccountId: 'a-1' as string | null,
        },
      },
    );

    onAccountChange.mockClear();

    rerender({
      items: [{ id: '2', account: { rowId: 'a-2' } }],
      selectedAccountId: 'a-1',
    });

    expect(onAccountChange).toHaveBeenCalledWith(null);
  });

  test('does not clear selected account when accountsInItems is empty', () => {
    const onAccountChange = vi.fn();

    renderHook(() =>
      useAccountFilter({
        accounts: [],
        items: [] as TestItem[],
        selectedAccountId: 'a-1',
        onAccountChange,
      }),
    );

    expect(onAccountChange).not.toHaveBeenCalled();
  });

  test('setSelectedAccountId delegates to onAccountChange', () => {
    const onAccountChange = vi.fn();

    const { result } = renderHook(() =>
      useAccountFilter({
        accounts: [createAccountWithIdentity('a-1', 'Alpha', { etag: 1n })],
        items: [] as TestItem[],
        selectedAccountId: null,
        onAccountChange,
      }),
    );

    act(() => {
      result.current.setSelectedAccountId('a-1');
    });

    expect(onAccountChange).toHaveBeenCalledWith('a-1');
  });
});
