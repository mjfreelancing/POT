import { act, renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import { useIsShortViewport } from '@/hooks/use-short-viewport';

type MatchMediaListener = (event: MediaQueryListEvent) => void;

describe('useIsShortViewport', () => {
  const originalInnerHeight = window.innerHeight;
  const originalMatchMedia = window.matchMedia;

  let changeListeners: MatchMediaListener[] = [];

  beforeEach(() => {
    changeListeners = [];

    Object.defineProperty(window, 'innerHeight', {
      configurable: true,
      writable: true,
      value: 800,
    });

    window.matchMedia = vi.fn().mockImplementation(
      (query: string) =>
        ({
          matches: window.innerHeight < 560,
          media: query,
          onchange: null,
          addListener: (listener: MatchMediaListener) => {
            changeListeners.push(listener);
          },
          removeListener: (listener: MatchMediaListener) => {
            changeListeners = changeListeners.filter(
              existingListener => existingListener !== listener,
            );
          },
          addEventListener: (
            _eventName: string,
            listener: MatchMediaListener,
          ) => {
            changeListeners.push(listener);
          },
          removeEventListener: (
            _eventName: string,
            listener: MatchMediaListener,
          ) => {
            changeListeners = changeListeners.filter(
              existingListener => existingListener !== listener,
            );
          },
          dispatchEvent: () => false,
        }) as MediaQueryList,
    );
  });

  afterEach(() => {
    Object.defineProperty(window, 'innerHeight', {
      configurable: true,
      writable: true,
      value: originalInnerHeight,
    });

    window.matchMedia = originalMatchMedia;
  });

  test('returns false for a tall viewport', () => {
    const { result } = renderHook(() => useIsShortViewport());

    expect(result.current).toBe(false);
  });

  test('returns true for a short landscape viewport', () => {
    window.innerHeight = 390;

    const { result } = renderHook(() => useIsShortViewport());

    expect(result.current).toBe(true);
  });

  test('returns false at the boundary height of 560px', () => {
    window.innerHeight = 560;

    const { result } = renderHook(() => useIsShortViewport());

    expect(result.current).toBe(false);
  });

  test('updates value when media query change listener fires', () => {
    const { result } = renderHook(() => useIsShortViewport());

    expect(result.current).toBe(false);

    act(() => {
      window.innerHeight = 430;
      changeListeners.forEach(listener =>
        listener({ matches: true } as MediaQueryListEvent),
      );
    });

    expect(result.current).toBe(true);
  });

  test('registers and unregisters media query listener', () => {
    const { unmount } = renderHook(() => useIsShortViewport());

    expect(window.matchMedia).toHaveBeenCalledWith('(max-height: 559px)');
    expect(changeListeners.length).toBe(1);

    unmount();

    expect(changeListeners.length).toBe(0);
  });
});
