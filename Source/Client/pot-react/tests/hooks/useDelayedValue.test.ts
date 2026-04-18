import { act, renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import useDelayedValue from '@/hooks/useDelayedValue';

describe('useDelayedValue', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.clearAllTimers();
    vi.useRealTimers();
  });

  test('returns initial value when condition is false', () => {
    const { result } = renderHook(() =>
      useDelayedValue({
        condition: false,
        delay: 500,
        initialValue: 'idle',
        delayedValue: 'loading',
      }),
    );

    expect(result.current).toBe('idle');
  });

  test('switches to delayed value after delay when condition is true', () => {
    const { result } = renderHook(() =>
      useDelayedValue({
        condition: true,
        delay: 500,
        initialValue: 'idle',
        delayedValue: 'loading',
      }),
    );

    expect(result.current).toBe('idle');

    act(() => {
      vi.advanceTimersByTime(499);
    });

    expect(result.current).toBe('idle');

    act(() => {
      vi.advanceTimersByTime(1);
    });

    expect(result.current).toBe('loading');
  });

  test('resets immediately to initial value when condition becomes false', () => {
    const { result, rerender } = renderHook(
      ({ condition }) =>
        useDelayedValue({
          condition,
          delay: 500,
          initialValue: 'idle',
          delayedValue: 'loading',
        }),
      {
        initialProps: { condition: true },
      },
    );

    act(() => {
      vi.advanceTimersByTime(500);
    });

    expect(result.current).toBe('loading');

    rerender({ condition: false });

    expect(result.current).toBe('idle');
  });

  test('clears pending timeout when condition toggles false before delay', () => {
    const { result, rerender } = renderHook(
      ({ condition }) =>
        useDelayedValue({
          condition,
          delay: 500,
          initialValue: 'idle',
          delayedValue: 'loading',
        }),
      {
        initialProps: { condition: true },
      },
    );

    act(() => {
      vi.advanceTimersByTime(200);
    });

    rerender({ condition: false });

    act(() => {
      vi.advanceTimersByTime(500);
    });

    expect(result.current).toBe('idle');
  });

  test('clears timeout on unmount', () => {
    const clearTimeoutSpy = vi.spyOn(window, 'clearTimeout');

    const { unmount } = renderHook(() =>
      useDelayedValue({
        condition: true,
        delay: 500,
        initialValue: 'idle',
        delayedValue: 'loading',
      }),
    );

    unmount();

    expect(clearTimeoutSpy).toHaveBeenCalled();
  });
});
