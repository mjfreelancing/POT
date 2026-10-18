import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useDelete } from '@/api/hooks/useApi';
import { useApiDeleteUser } from '@/api/hooks/useUser';
import { SuccessResult } from '@/lib';

vi.mock('@/api/hooks/useApi', () => ({
  useDelete: vi.fn(),
}));

describe('useApiDeleteUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('targets the user endpoint with useDelete', () => {
    const mutationResult = {
      mutate: vi.fn(),
      mutateAsync: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(useDelete).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof useDelete>,
    );

    const { result } = renderHook(() => useApiDeleteUser('user-123'));

    expect(useDelete).toHaveBeenCalledWith('/users/user-123');
    expect(result.current.data).toBe(mutationResult.data);
    expect(result.current.mutateAsync).toBe(mutationResult.mutateAsync);
  });
});
