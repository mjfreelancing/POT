import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useProjectionStorage from '@/features/projections/hooks/useProjectionStorage';

import useLocalStorage from '@/hooks/useLocalStorage';

vi.mock('@/hooks/useLocalStorage', () => ({
  default: vi.fn(),
}));

describe('useProjectionStorage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('composes useLocalStorage with projection key and defaults', () => {
    const localStorageAdapter = {
      getItem: vi.fn(),
      setItem: vi.fn(),
    };

    vi.mocked(useLocalStorage).mockReturnValue(
      localStorageAdapter as unknown as ReturnType<typeof useLocalStorage>,
    );

    const onError = vi.fn();
    const { result } = renderHook(() => useProjectionStorage(onError));

    expect(useLocalStorage).toHaveBeenCalledWith({
      key: 'pot-projections',
      defaultValue: {
        metric: 'balance',
        period: 6,
        hiddenSeries: [],
      },
      onError,
    });

    expect(result.current.getProjectionStorageData).toBe(
      localStorageAdapter.getItem,
    );
    expect(result.current.setProjectionStorageData).toBe(
      localStorageAdapter.setItem,
    );
  });

  test('removeStorageStartDate removes startDate and persists remaining values', () => {
    const getItem = vi.fn().mockReturnValue({
      startDate: '2026-04-01',
      metric: 'available',
      period: 3,
      hiddenSeries: ['account-2'],
    });
    const setItem = vi.fn();

    vi.mocked(useLocalStorage).mockReturnValue({
      getItem,
      setItem,
    } as unknown as ReturnType<typeof useLocalStorage>);

    const { result } = renderHook(() => useProjectionStorage());

    result.current.removeStorageStartDate();

    expect(setItem).toHaveBeenCalledWith({
      metric: 'available',
      period: 3,
      hiddenSeries: ['account-2'],
    });
  });

  test('removeStorageStartDate does nothing when startDate is absent', () => {
    const getItem = vi.fn().mockReturnValue({
      metric: 'balance',
      period: 6,
      hiddenSeries: [],
    });
    const setItem = vi.fn();

    vi.mocked(useLocalStorage).mockReturnValue({
      getItem,
      setItem,
    } as unknown as ReturnType<typeof useLocalStorage>);

    const { result } = renderHook(() => useProjectionStorage());

    result.current.removeStorageStartDate();

    expect(setItem).not.toHaveBeenCalled();
  });
});
