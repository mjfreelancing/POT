import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  useInviteUser,
  useResendInvitation,
  useUpdateUserRole,
  useUpdateUserStatus,
  useUsers,
} from '@/api/hooks/useUsers';
import { SuccessResult } from '@/lib';

import {
  useGet,
  usePost,
  usePostWithIdNoData,
  usePutWithId,
} from '@/api/hooks/useApi';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
  usePost: vi.fn(),
  usePutWithId: vi.fn(),
  usePostWithIdNoData: vi.fn(),
}));

describe('useUsers hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useUsers composes users query endpoint and key', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult([
        {
          rowId: '11111111-1111-1111-1111-111111111111',
          etag: 0n,
          username: 'pending.user',
          displayName: 'Pending User',
          email: 'pending.user@example.com',
          roles: ['Member'],
          status: 'Pending' as const,
          lastLoggedInUtc: null,
        },
      ]),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useUsers());

    expect(useGet).toHaveBeenCalledWith('/users', ['users']);
    expect(result.current.data).toBe(queryResult.data);
  });

  test('useInviteUser composes invite endpoint via usePost', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({
        rowId: '22222222-2222-2222-2222-222222222222',
        etag: 1n,
      }),
    };

    vi.mocked(usePost).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePost>,
    );

    const { result } = renderHook(() => useInviteUser());

    expect(usePost).toHaveBeenCalledWith('/users/invite');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useUpdateUserRole composes role update endpoint via usePutWithId', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult({
        rowId: '33333333-3333-3333-3333-333333333333',
        etag: 2n,
      }),
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    renderHook(() => useUpdateUserRole());

    expect(usePutWithId).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePutWithId).mock.calls[0]?.[0];
    expect(endpointBuilder?.('user-123')).toBe('/users/user-123/roles');
  });

  test('useUpdateUserStatus composes status update endpoint via usePutWithId', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    const { result } = renderHook(() => useUpdateUserStatus());

    expect(usePutWithId).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePutWithId).mock.calls[0]?.[0];
    expect(endpointBuilder?.('user-456')).toBe('/users/user-456/status');
    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useResendInvitation composes resend endpoint via usePostWithIdNoData', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePostWithIdNoData).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePostWithIdNoData>,
    );

    const { result } = renderHook(() => useResendInvitation());

    expect(usePostWithIdNoData).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePostWithIdNoData).mock.calls[0]?.[0];
    expect(endpointBuilder?.('user-789')).toBe('/users/user-789/resend-invite');
    expect(result.current.data).toBe(mutationResult.data);
  });
});
