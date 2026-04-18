import { act, renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import { useIsMobile } from '@/hooks/use-mobile';

type MatchMediaListener = (event: MediaQueryListEvent) => void;

describe('useIsMobile', () => {
  const originalInnerWidth = window.innerWidth;
  const originalMatchMedia = window.matchMedia;

  let changeListeners: MatchMediaListener[] = [];

  beforeEach(() => {
    changeListeners = [];

    Object.defineProperty(window, 'innerWidth', {
      configurable: true,
      writable: true,
      value: 1024,
    });

    window.matchMedia = vi.fn().mockImplementation((query: string) => {
      return {
        matches: window.innerWidth < 768,
        media: query,
        onchange: null,
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
      } as MediaQueryList;
    });
  });

  afterEach(() => {
    Object.defineProperty(window, 'innerWidth', {
      configurable: true,
      writable: true,
      value: originalInnerWidth,
    });

    window.matchMedia = originalMatchMedia;
  });

  test('returns false for desktop width', () => {
    const { result } = renderHook(() => useIsMobile());

    expect(result.current).toBe(false);
  });

  test('returns true for mobile width', () => {
    window.innerWidth = 767;

    const { result } = renderHook(() => useIsMobile());

    expect(result.current).toBe(true);
  });

  test('updates value when media query change listener fires', () => {
    const { result } = renderHook(() => useIsMobile());

    expect(result.current).toBe(false);

    act(() => {
      window.innerWidth = 600;
      changeListeners.forEach(listener =>
        listener({ matches: true } as MediaQueryListEvent),
      );
    });

    expect(result.current).toBe(true);
  });

  test('registers and unregisters media query listener', () => {
    const { unmount } = renderHook(() => useIsMobile());

    expect(window.matchMedia).toHaveBeenCalledWith('(max-width: 767px)');
    expect(changeListeners.length).toBe(1);

    unmount();

    expect(changeListeners.length).toBe(0);
  });
});
