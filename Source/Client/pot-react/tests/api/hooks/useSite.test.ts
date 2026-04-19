import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiUpdateSite } from '@/api/hooks/useSite';
import { SuccessResult } from '@/lib';

import { usePutWithId } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  usePutWithId: vi.fn(),
}));

describe('useSite hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useApiUpdateSite composes id endpoint via usePutWithId', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({
        rowId: '44444444-4444-4444-4444-444444444444',
        etag: 2n,
      }),
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    const { result } = renderHook(() => useApiUpdateSite());

    expect(usePutWithId).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePutWithId).mock.calls[0]?.[0];
    expect(endpointBuilder?.('site-123')).toBe('/sites/site-123');
    expect(result.current.data).toBe(mutationResult.data);
  });
});
