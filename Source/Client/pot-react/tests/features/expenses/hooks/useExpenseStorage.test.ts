import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useExpenseStorage from '@/features/expenses/hooks/useExpenseStorage';
import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import useUserStore from '@/stores/useUserStore';

vi.mock('@/hooks/useLocalStorageManager', () => ({
  default: vi.fn(),
}));

vi.mock('@/stores/useUserStore', () => ({
  default: vi.fn(),
}));

function mockUserStore(userId: string | undefined) {
  vi.mocked(useUserStore).mockImplementation(selector =>
    selector({
      userInfo: userId ? { rowId: userId } : null,
    } as Parameters<typeof selector>[0]),
  );
}

describe('useExpenseStorage', () => {
  beforeEach(() => {
    sessionStorage.clear();
    localStorage.clear();
    mockUserStore('user-1');
    vi.clearAllMocks();
  });

  test('wires scoped key and sessionStorage backend into useLocalStorageManager', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const onError = vi.fn();
    renderHook(() => useExpenseStorage(onError));

    expect(useLocalStorageManager).toHaveBeenCalledWith(
      'pot:dev:user:user-1:expenses',
      onError,
      { selectedAccountId: null, filterDescription: null },
      sessionStorage,
    );
  });

  test('purges legacy expenses key from localStorage when user is authenticated', () => {
    localStorage.setItem(
      'pot-expenses',
      JSON.stringify({ filterDescription: 'legacy' }),
    );

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    renderHook(() => useExpenseStorage());

    expect(localStorage.getItem('pot-expenses')).toBeNull();
  });

  test('setExpenseData updates selected account and normalizes empty filter', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useExpenseStorage());

    result.current.setExpenseData({
      selectedAccountId: 'a-1',
      filterDescription: '',
    });

    expect(setProperty).toHaveBeenNthCalledWith(1, 'selectedAccountId', 'a-1');
    expect(setProperty).toHaveBeenNthCalledWith(2, 'filterDescription', null);
  });

  test('setExpenseData is a no-op when userId is unavailable', () => {
    mockUserStore(undefined);

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useExpenseStorage());

    result.current.setExpenseData({
      selectedAccountId: 'a-1',
      filterDescription: 'rent',
    });

    expect(setProperty).not.toHaveBeenCalled();
  });
});
