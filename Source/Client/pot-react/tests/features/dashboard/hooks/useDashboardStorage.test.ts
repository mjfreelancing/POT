import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useDashboardStorage from '@/features/dashboard/hooks/useDashboardStorage';

import useLocalStorageManager from '@/hooks/useLocalStorageManager';

vi.mock('@/hooks/useLocalStorageManager', () => ({
  default: vi.fn(),
}));

describe('useDashboardStorage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('wires dashboard key and defaults into useLocalStorageManager', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const onError = vi.fn();
    renderHook(() => useDashboardStorage(onError));

    expect(useLocalStorageManager).toHaveBeenCalledWith(
      'pot-dashboard',
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
});
