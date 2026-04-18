import { act, renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import useScreenDimensions from '@/hooks/useScreenDimensions';

describe('useScreenDimensions', () => {
  const originalInnerWidth = window.innerWidth;
  const originalInnerHeight = window.innerHeight;

  beforeEach(() => {
    Object.defineProperty(window, 'innerWidth', {
      configurable: true,
      writable: true,
      value: 1200,
    });

    Object.defineProperty(window, 'innerHeight', {
      configurable: true,
      writable: true,
      value: 800,
    });
  });

  afterEach(() => {
    Object.defineProperty(window, 'innerWidth', {
      configurable: true,
      writable: true,
      value: originalInnerWidth,
    });

    Object.defineProperty(window, 'innerHeight', {
      configurable: true,
      writable: true,
      value: originalInnerHeight,
    });
  });

  test('returns current viewport dimensions', () => {
    const { result } = renderHook(() => useScreenDimensions());

    expect(result.current).toEqual({
      width: 1200,
      height: 800,
    });
  });

  test('updates dimensions on resize events', () => {
    const { result } = renderHook(() => useScreenDimensions());

    act(() => {
      window.innerWidth = 640;
      window.innerHeight = 360;
      window.dispatchEvent(new Event('resize'));
    });

    expect(result.current).toEqual({
      width: 640,
      height: 360,
    });
  });

  test('registers and unregisters resize listener', () => {
    const addEventListenerSpy = vi.spyOn(window, 'addEventListener');
    const removeEventListenerSpy = vi.spyOn(window, 'removeEventListener');

    const { unmount } = renderHook(() => useScreenDimensions());

    expect(addEventListenerSpy).toHaveBeenCalledWith(
      'resize',
      expect.any(Function),
    );

    const resizeHandler = addEventListenerSpy.mock.calls.find(
      ([eventName]) => eventName === 'resize',
    )?.[1];

    unmount();

    expect(removeEventListenerSpy).toHaveBeenCalledWith(
      'resize',
      resizeHandler,
    );
  });
});
