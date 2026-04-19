import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  useGetPendingApprovals,
  useUpdatePendingUserStatus,
} from '@/api/hooks/useApprovals';
import { SuccessResult } from '@/lib';

import { useGet, usePutWithId } from '@/api/hooks/useApi';
import {
  createPendingApprovalUser,
  createPendingUserStatusUpdate,
} from '../../shared/factories/approvalFactory';

vi.mock('@/api/hooks/useApi', () => ({
  useGet: vi.fn(),
  usePutWithId: vi.fn(),
}));

describe('useApprovals hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('useGetPendingApprovals composes pending approvals query endpoint and key', () => {
    const queryResult = {
      isLoading: false,
      isSuccess: true,
      data: new SuccessResult([
        createPendingApprovalUser({ username: 'review.me' }),
      ]),
    };

    vi.mocked(useGet).mockReturnValue(
      queryResult as unknown as ReturnType<typeof useGet>,
    );

    const { result } = renderHook(() => useGetPendingApprovals());

    expect(useGet).toHaveBeenCalledWith('/approvals/pending', [
      'pending-approvals',
    ]);
    expect(result.current.data).toBe(queryResult.data);
  });

  test('useUpdatePendingUserStatus composes status update endpoint via usePutWithId', () => {
    const mutationResult = {
      mutate: vi.fn(),
      isPending: false,
      data: new SuccessResult(undefined),
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    const { result } = renderHook(() => useUpdatePendingUserStatus());

    expect(usePutWithId).toHaveBeenCalledTimes(1);

    const endpointBuilder = vi.mocked(usePutWithId).mock.calls[0]?.[0];
    expect(endpointBuilder?.('user-123')).toBe('/approvals/user-123/status');

    expect(result.current.data).toBe(mutationResult.data);
  });

  test('useUpdatePendingUserStatus forwards mutate variables shape for status updates', () => {
    const mutateMock = vi.fn();

    const mutationResult = {
      mutate: mutateMock,
      isPending: false,
      data: undefined,
    };

    vi.mocked(usePutWithId).mockReturnValue(
      mutationResult as unknown as ReturnType<typeof usePutWithId>,
    );

    const { result } = renderHook(() => useUpdatePendingUserStatus());

    const payload = createPendingUserStatusUpdate({ status: 'Rejected' });

    result.current.mutate({ id: 'user-789', data: payload });

    expect(mutateMock).toHaveBeenCalledWith({ id: 'user-789', data: payload });
  });
});
