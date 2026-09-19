import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { toast } from 'sonner';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import { useUpdatePendingUserStatus } from '@/api/hooks';
import { useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { PendingApprovalUser } from '@/data/approvals';
import PendingApprovalMobileCard from '@/features/approvals/components/PendingApprovalMobileCard';
import { FailResult, SuccessResult } from '@/lib';

import { createPendingApprovalUser } from '../../../shared/factories/approvalFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock('@/api/hooks', () => ({
  useUpdatePendingUserStatus: vi.fn(),
}));

vi.mock('sonner', () => ({
  toast: vi.fn(),
}));

vi.mock('@/concerns', async importOriginal => {
  const actual = await importOriginal<typeof import('@/concerns')>();

  return {
    ...actual,
    logger: {
      info: vi.fn(),
      error: vi.fn(),
    },
    useCacheInvalidation: vi.fn(),
  };
});

vi.mock('@/contexts', async importOriginal => {
  const actual = await importOriginal<typeof import('@/contexts')>();

  return {
    ...actual,
    useErrorContext: vi.fn(),
  };
});

function renderCard(user: PendingApprovalUser) {
  const queryClient = createQueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <PendingApprovalMobileCard user={user} />
    </QueryClientProvider>,
  );
}

describe('PendingApprovalMobileCard', () => {
  const mutateAsyncMock = vi.fn();
  const invalidateCacheMock = vi.fn();
  const setErrorMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    vi.mocked(useCacheInvalidation).mockReturnValue(
      invalidateCacheMock as unknown as ReturnType<typeof useCacheInvalidation>,
    );

    vi.mocked(useUpdatePendingUserStatus).mockReturnValue({
      mutateAsync: mutateAsyncMock,
    } as unknown as ReturnType<typeof useUpdatePendingUserStatus>);
  });

  test('renders the username and email', () => {
    const pendingUser = createPendingApprovalUser({
      username: 'pending.user',
      email: 'pending.user@example.com',
    });

    renderCard(pendingUser);

    expect(screen.getByText('pending.user')).toBeInTheDocument();
    expect(screen.getByText('Email:')).toBeInTheDocument();
    expect(screen.getByText('pending.user@example.com')).toBeInTheDocument();
  });

  test('approves the user with the pending etag and invalidates the approvals cache', async () => {
    const pendingUser = createPendingApprovalUser({
      rowId: 'user-42',
      etag: 7n,
      username: 'gamma.pending',
    });

    mutateAsyncMock.mockResolvedValue(new SuccessResult(undefined));

    renderCard(pendingUser);

    await userEvent.click(screen.getByRole('button', { name: 'Approve' }));

    expect(mutateAsyncMock).toHaveBeenCalledWith({
      id: 'user-42',
      data: {
        etag: 7n,
        status: 'Approved',
      },
    });

    expect(invalidateCacheMock).toHaveBeenCalledWith(['pending-approvals']);
    expect(toast).toHaveBeenCalledTimes(1);
  });

  test('rejects the user with the pending etag and invalidates the approvals cache', async () => {
    const pendingUser = createPendingApprovalUser({
      rowId: 'user-43',
      etag: 8n,
      username: 'delta.pending',
    });

    mutateAsyncMock.mockResolvedValue(new SuccessResult(undefined));

    renderCard(pendingUser);

    await userEvent.click(screen.getByRole('button', { name: 'Reject' }));

    expect(mutateAsyncMock).toHaveBeenCalledWith({
      id: 'user-43',
      data: {
        etag: 8n,
        status: 'Rejected',
      },
    });

    expect(invalidateCacheMock).toHaveBeenCalledWith(['pending-approvals']);
  });

  test('surfaces the error sheet and skips cache invalidation when approval fails', async () => {
    const failure = new FailResult(new UnexpectedError('approve failed'));

    mutateAsyncMock.mockResolvedValue(failure);

    renderCard(createPendingApprovalUser());

    await userEvent.click(screen.getByRole('button', { name: 'Approve' }));

    expect(setErrorMock).toHaveBeenCalledWith({
      title: failure.error.code,
      description: failure.error.description,
    });

    expect(invalidateCacheMock).not.toHaveBeenCalled();
    expect(toast).not.toHaveBeenCalled();
  });
});
