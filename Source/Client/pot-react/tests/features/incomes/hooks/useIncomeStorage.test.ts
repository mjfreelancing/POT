import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useIncomeStorage from '@/features/incomes/hooks/useIncomeStorage';
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

describe('useIncomeStorage', () => {
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
    renderHook(() => useIncomeStorage(onError));

    expect(useLocalStorageManager).toHaveBeenCalledWith(
      'pot:dev:user:user-1:incomes',
      onError,
      { selectedAccountId: null, filterDescription: null },
      sessionStorage,
    );
  });

  test('purges legacy incomes key from localStorage when user is authenticated', () => {
    localStorage.setItem(
      'pot-incomes',
      JSON.stringify({ filterDescription: 'legacy' }),
    );

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    renderHook(() => useIncomeStorage());

    expect(localStorage.getItem('pot-incomes')).toBeNull();
  });

  test('setIncomeData updates selected account and normalizes empty filter', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useIncomeStorage());

    result.current.setIncomeData({
      selectedAccountId: 'a-1',
      filterDescription: '',
    });

    expect(setProperty).toHaveBeenNthCalledWith(1, 'selectedAccountId', 'a-1');
    expect(setProperty).toHaveBeenNthCalledWith(2, 'filterDescription', null);
  });

  test('setIncomeData is a no-op when userId is unavailable', () => {
    mockUserStore(undefined);

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useIncomeStorage());

    result.current.setIncomeData({
      selectedAccountId: 'a-1',
      filterDescription: 'salary',
    });

    expect(setProperty).not.toHaveBeenCalled();
  });
});
