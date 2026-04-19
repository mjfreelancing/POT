import { act, renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns';
import { useWindowFocus } from '@/hooks/useWindowFocus';

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

describe('useWindowFocus', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('returns the current document focus state initially', () => {
    vi.spyOn(document, 'hasFocus').mockReturnValue(false);

    const { result } = renderHook(() => useWindowFocus());

    expect(result.current).toBe(false);
  });

  test('updates focus state on blur and focus events', () => {
    vi.spyOn(document, 'hasFocus').mockReturnValue(true);

    const { result } = renderHook(() => useWindowFocus());

    expect(result.current).toBe(true);

    act(() => {
      window.dispatchEvent(new Event('blur'));
    });

    expect(result.current).toBe(false);

    act(() => {
      window.dispatchEvent(new Event('focus'));
    });

    expect(result.current).toBe(true);
  });

  test('invokes callback and logs when window gains focus', () => {
    vi.spyOn(document, 'hasFocus').mockReturnValue(true);

    const onFocus = vi.fn();

    renderHook(() => useWindowFocus(onFocus));

    act(() => {
      window.dispatchEvent(new Event('focus'));
    });

    expect(onFocus).toHaveBeenCalledTimes(1);
    expect(logger.info).toHaveBeenCalledWith(
      'useWindowFocus',
      'Window focused, triggering refresh',
    );
  });

  test('does not invoke callback when refetchOnWindowFocus is disabled', () => {
    vi.spyOn(document, 'hasFocus').mockReturnValue(false);

    const onFocus = vi.fn();

    renderHook(() =>
      useWindowFocus(onFocus, {
        refetchOnWindowFocus: false,
      }),
    );

    act(() => {
      window.dispatchEvent(new Event('focus'));
    });

    expect(onFocus).not.toHaveBeenCalled();
  });

  test('registers and unregisters focus listeners', () => {
    vi.spyOn(document, 'hasFocus').mockReturnValue(true);

    const addEventListenerSpy = vi.spyOn(window, 'addEventListener');
    const removeEventListenerSpy = vi.spyOn(window, 'removeEventListener');

    const { unmount } = renderHook(() => useWindowFocus());

    const focusHandler = addEventListenerSpy.mock.calls.find(
      ([eventName]) => eventName === 'focus',
    )?.[1];

    const blurHandler = addEventListenerSpy.mock.calls.find(
      ([eventName]) => eventName === 'blur',
    )?.[1];

    expect(focusHandler).toBeTypeOf('function');
    expect(blurHandler).toBeTypeOf('function');

    unmount();

    expect(removeEventListenerSpy).toHaveBeenCalledWith('focus', focusHandler);
    expect(removeEventListenerSpy).toHaveBeenCalledWith('blur', blurHandler);
  });
});
