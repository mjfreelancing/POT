import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useResetErrorBoundary from '@/hooks/useResetErrorBoundary';
import { useErrorBoundary } from 'react-error-boundary';

vi.mock('react-error-boundary', () => ({
  useErrorBoundary: vi.fn(),
}));

describe('useResetErrorBoundary', () => {
  const resetBoundaryMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useErrorBoundary).mockReturnValue({
      error: undefined,
      resetBoundary: resetBoundaryMock,
      showBoundary: vi.fn(),
    });
  });

  test('returns resetBoundary from react-error-boundary', () => {
    const { result } = renderHook(() => useResetErrorBoundary());

    expect(result.current).toBe(resetBoundaryMock);
    expect(useErrorBoundary).toHaveBeenCalledTimes(1);
  });

  test('invokes resetBoundary when returned callback is called', () => {
    const { result } = renderHook(() => useResetErrorBoundary());

    result.current();

    expect(resetBoundaryMock).toHaveBeenCalledTimes(1);
  });
});
