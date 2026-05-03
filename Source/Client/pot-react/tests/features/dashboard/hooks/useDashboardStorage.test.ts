import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useDashboardStorage from '@/features/dashboard/hooks/useDashboardStorage';
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

describe('useDashboardStorage', () => {
  beforeEach(() => {
    localStorage.clear();
    mockUserStore('user-1');
    vi.clearAllMocks();
  });

  test('wires scoped dashboard key and defaults into useLocalStorageManager', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const onError = vi.fn();
    renderHook(() => useDashboardStorage(onError));

    expect(useLocalStorageManager).toHaveBeenCalledWith(
      'pot:dev:user:user-1:dashboard',
      onError,
      {
        quickActionsOpen: true,
        accountsOpen: true,
        incomesOpen: true,
        expensesOpen: true,
        expensesPeriod: 30,
        incomesPeriod: 30,
      },
    );
  });

  test('purges legacy dashboard key when user is authenticated', () => {
    localStorage.setItem(
      'pot-dashboard',
      JSON.stringify({ accountsOpen: false }),
    );

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    renderHook(() => useDashboardStorage());

    expect(localStorage.getItem('pot-dashboard')).toBeNull();
  });

  test('getDashboardData returns fallback values when storage properties are null', () => {
    const getProperty = vi.fn().mockReturnValue(null);
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useDashboardStorage());

    expect(result.current.getDashboardData()).toEqual({
      quickActionsOpen: true,
      accountsOpen: true,
      incomesOpen: true,
      expensesOpen: true,
      expensesPeriod: 30,
      incomesPeriod: 30,
    });
  });

  test('setDashboardData updates only provided properties', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useDashboardStorage());

    result.current.setDashboardData({
      accountsOpen: false,
      expensesPeriod: 14,
    });

    expect(setProperty).toHaveBeenCalledTimes(2);
    expect(setProperty).toHaveBeenNthCalledWith(1, 'accountsOpen', false);
    expect(setProperty).toHaveBeenNthCalledWith(2, 'expensesPeriod', 14);
  });

  test('setDashboardData is a no-op when userId is unavailable', () => {
    mockUserStore(undefined);

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useDashboardStorage());

    result.current.setDashboardData({
      accountsOpen: false,
      expensesPeriod: 14,
    });

    expect(setProperty).not.toHaveBeenCalled();
  });
});
