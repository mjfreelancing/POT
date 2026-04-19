import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import { useApiGetProjection } from '@/api/hooks/useProjections';
import { FailResult, SuccessResult } from '@/lib';

import { useGet } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
}));

describe('useProjections hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('composes projections query endpoint and key from date range', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult({
        accounts: [],
        global: [],
      }),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() =>
      useApiGetProjection('2026-04-01', '2026-04-30'),
    );

    expect(useGet).toHaveBeenCalledWith(
      '/projections?startDate=2026-04-01&endDate=2026-04-30',
      ['projections', '2026-04-01', '2026-04-30'],
    );

    expect(result.current.data).toBeInstanceOf(SuccessResult);

    if (result.current.data?.success) {
      expect(result.current.data.value).toEqual({
        accounts: [],
        global: [],
      });
    }
  });

  test('passes through failure results unchanged', () => {
    const failure = new FailResult(new UnexpectedError('projection failed'));

    const queryResult = {
      isLoading: false,
      isSuccess: false,
      data: failure,
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() =>
      useApiGetProjection('2026-04-01', '2026-04-30'),
    );

    expect(result.current.data).toBe(failure);
  });
});
