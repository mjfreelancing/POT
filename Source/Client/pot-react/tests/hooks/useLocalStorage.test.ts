import { act, renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns';
import useLocalStorage from '@/hooks/useLocalStorage';

type TestPayload = {
  value: string;
};

vi.mock('@/concerns', () => ({
  logger: {
    error: vi.fn(),
  },
}));

describe('useLocalStorage', () => {
  beforeEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
  });

  test('sets and gets stored values', () => {
    const { result } = renderHook(() =>
      useLocalStorage<TestPayload>({
        key: 'pot-local-storage-test',
      }),
    );

    act(() => {
      result.current.setItem({ value: 'stored' });
    });

    const storedRaw = window.localStorage.getItem('pot-local-storage-test');

    expect(storedRaw).toBe('{"value":"stored"}');

    const loadedValue = result.current.getItem();

    expect(loadedValue).toEqual({ value: 'stored' });
  });

  test('returns default value when storage entry is missing', () => {
    const { result } = renderHook(() =>
      useLocalStorage<TestPayload>({
        key: 'pot-local-storage-missing',
        defaultValue: { value: 'default' },
      }),
    );

    const loadedValue = result.current.getItem();

    expect(loadedValue).toEqual({ value: 'default' });
  });

  test('removes stored value', () => {
    const { result } = renderHook(() =>
      useLocalStorage<TestPayload>({
        key: 'pot-local-storage-remove',
      }),
    );

    act(() => {
      result.current.setItem({ value: 'stored' });
    });

    expect(window.localStorage.getItem('pot-local-storage-remove')).toBe(
      '{"value":"stored"}',
    );

    act(() => {
      result.current.removeItem();
    });

    expect(window.localStorage.getItem('pot-local-storage-remove')).toBeNull();
  });

  test('returns undefined and reports error when stored JSON cannot be parsed', () => {
    window.localStorage.setItem('pot-local-storage-invalid-json', '{invalid-json');
    const onError = vi.fn();

    const { result } = renderHook(() =>
      useLocalStorage<TestPayload>({
        key: 'pot-local-storage-invalid-json',
        defaultValue: { value: 'default' },
        onError,
      }),
    );

    const loadedValue = result.current.getItem();

    expect(loadedValue).toBeUndefined();

    expect(logger.error).toHaveBeenCalledWith(
      'useLocalStorage',
      'Error getting item with key "pot-local-storage-invalid-json"',
      expect.any(Error),
    );

    expect(onError).toHaveBeenCalledWith(
      expect.objectContaining({
        title: 'Local Storage Error',
        description: expect.any(String),
      }),
    );
  });

  test('reports error when setItem fails', () => {
    const setItemSpy = vi
      .spyOn(Storage.prototype, 'setItem')
      .mockImplementation(() => {
        throw new Error('Quota exceeded');
      });

    const onError = vi.fn();
    const { result } = renderHook(() =>
      useLocalStorage<TestPayload>({
        key: 'pot-local-storage-set-fail',
        onError,
      }),
    );

    act(() => {
      result.current.setItem({ value: 'stored' });
    });

    expect(logger.error).toHaveBeenCalledWith(
      'useLocalStorage',
      'Error setting item with key "pot-local-storage-set-fail"',
      expect.any(Error),
    );

    expect(onError).toHaveBeenCalledWith({
      title: 'Local Storage Error',
      description: 'Quota exceeded',
    });

    setItemSpy.mockRestore();
  });

  test('reports error when removeItem fails', () => {
    const removeItemSpy = vi
      .spyOn(Storage.prototype, 'removeItem')
      .mockImplementation(() => {
        throw new Error('Storage unavailable');
      });

    const onError = vi.fn();
    const { result } = renderHook(() =>
      useLocalStorage<TestPayload>({
        key: 'pot-local-storage-remove-fail',
        onError,
      }),
    );

    act(() => {
      result.current.removeItem();
    });

    expect(logger.error).toHaveBeenCalledWith(
      'useLocalStorage',
      'Error removing item with key "pot-local-storage-remove-fail"',
      expect.any(Error),
    );

    expect(onError).toHaveBeenCalledWith({
      title: 'Local Storage Error',
      description: 'Storage unavailable',
    });

    removeItemSpy.mockRestore();
  });
});
