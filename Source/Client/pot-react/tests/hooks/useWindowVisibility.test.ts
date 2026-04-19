import { act, renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns';
import { useWindowVisibility } from '@/hooks/useWindowVisibility';

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

describe('useWindowVisibility', () => {
  let isHidden = false;

  beforeEach(() => {
    vi.clearAllMocks();
    isHidden = false;

    Object.defineProperty(document, 'hidden', {
      configurable: true,
      get: () => isHidden,
    });
  });

  test('returns true initially when document is visible', () => {
    const { result } = renderHook(() => useWindowVisibility());

    expect(result.current).toBe(true);
  });

  test('returns false initially when document is hidden', () => {
    isHidden = true;

    const { result } = renderHook(() => useWindowVisibility());

    expect(result.current).toBe(false);
  });

  test('updates state on visibility changes', () => {
    isHidden = true;

    const { result } = renderHook(() => useWindowVisibility());

    expect(result.current).toBe(false);

    act(() => {
      isHidden = false;
      document.dispatchEvent(new Event('visibilitychange'));
    });

    expect(result.current).toBe(true);

    act(() => {
      isHidden = true;
      document.dispatchEvent(new Event('visibilitychange'));
    });

    expect(result.current).toBe(false);
  });

  test('invokes callback and logs when window becomes visible', () => {
    isHidden = true;

    const onVisible = vi.fn();
    const { result } = renderHook(() => useWindowVisibility(onVisible));

    expect(result.current).toBe(false);

    act(() => {
      isHidden = false;
      document.dispatchEvent(new Event('visibilitychange'));
    });

    expect(result.current).toBe(true);
    expect(onVisible).toHaveBeenCalledTimes(1);
    expect(logger.info).toHaveBeenCalledWith(
      'useWindowVisibility',
      'Window became visible, triggering refresh',
    );
  });

  test('does not invoke callback when window becomes hidden', () => {
    isHidden = false;

    const onVisible = vi.fn();
    const { result } = renderHook(() => useWindowVisibility(onVisible));

    expect(result.current).toBe(true);

    act(() => {
      isHidden = true;
      document.dispatchEvent(new Event('visibilitychange'));
    });

    expect(result.current).toBe(false);
    expect(onVisible).not.toHaveBeenCalled();
  });

  test('registers and unregisters visibility change listener', () => {
    const addEventListenerSpy = vi.spyOn(document, 'addEventListener');
    const removeEventListenerSpy = vi.spyOn(document, 'removeEventListener');

    const { unmount } = renderHook(() => useWindowVisibility());

    const visibilityChangeHandler = addEventListenerSpy.mock.calls.find(
      ([eventName]) => eventName === 'visibilitychange',
    )?.[1];

    expect(visibilityChangeHandler).toBeTypeOf('function');

    unmount();

    expect(removeEventListenerSpy).toHaveBeenCalledWith(
      'visibilitychange',
      visibilityChangeHandler,
    );
  });
});
