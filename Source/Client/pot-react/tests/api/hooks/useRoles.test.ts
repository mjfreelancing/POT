import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useRoles } from '@/api/hooks/useRoles';
import { SuccessResult } from '@/lib';

import { useGet } from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
}));

describe('useRoles hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('composes roles query endpoint and key', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult([
        {
          rowId: '11111111-1111-1111-1111-111111111111',
          etag: 1,
          name: 'Admin',
        },
      ]),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useRoles());

    expect(useGet).toHaveBeenCalledWith('/roles', ['roles']);
    expect(result.current.data).toBe(queryResult.data);
  });
});
