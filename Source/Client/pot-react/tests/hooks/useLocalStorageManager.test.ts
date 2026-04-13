import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useLocalStorage from '@/hooks/useLocalStorage';
import useLocalStorageManager from '@/hooks/useLocalStorageManager';

vi.mock('@/hooks/useLocalStorage', () => ({
  default: vi.fn(),
}));

type StorageData = {
  alpha?: string;
  beta?: number;
};

describe('useLocalStorageManager', () => {
  const getItemMock = vi.fn();
  const setItemMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useLocalStorage).mockReturnValue({
      getItem: getItemMock,
      setItem: setItemMock,
      removeItem: vi.fn(),
    });
  });

  test('wires key, defaultValue and onError into useLocalStorage', () => {
    const onError = vi.fn();
    const defaultValue: StorageData = { alpha: 'fallback' };

    renderHook(() =>
      useLocalStorageManager<StorageData>('pot-manager-key', onError, defaultValue),
    );

    expect(useLocalStorage).toHaveBeenCalledWith({
      key: 'pot-manager-key',
      defaultValue,
      onError,
    });
  });

  test('getProperty returns stored property value when present', () => {
    getItemMock.mockReturnValue({ alpha: 'stored', beta: 10 });

    const { result } = renderHook(() =>
      useLocalStorageManager<StorageData>('pot-manager-key'),
    );

    const value = result.current.getProperty('alpha');

    expect(value).toBe('stored');
  });

  test('getProperty returns null when storage data is missing', () => {
    getItemMock.mockReturnValue(undefined);

    const { result } = renderHook(() =>
      useLocalStorageManager<StorageData>('pot-manager-key'),
    );

    const value = result.current.getProperty('alpha');

    expect(value).toBeNull();
  });

  test('setProperty merges value into existing storage payload', () => {
    getItemMock.mockReturnValue({ alpha: 'stored', beta: 10 });

    const { result } = renderHook(() =>
      useLocalStorageManager<StorageData>('pot-manager-key'),
    );

    result.current.setProperty('beta', 99);

    expect(setItemMock).toHaveBeenCalledWith({ alpha: 'stored', beta: 99 });
  });

  test('setProperty uses default value when storage data is missing', () => {
    getItemMock.mockReturnValue(undefined);
    const defaultValue: StorageData = { alpha: 'fallback' };

    const { result } = renderHook(() =>
      useLocalStorageManager<StorageData>('pot-manager-key', undefined, defaultValue),
    );

    result.current.setProperty('beta', 5);

    expect(setItemMock).toHaveBeenCalledWith({ alpha: 'fallback', beta: 5 });
  });
});
