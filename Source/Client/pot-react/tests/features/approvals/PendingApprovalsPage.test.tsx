import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import { useGetPendingApprovals } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import type { PendingApprovalUser } from '@/data/approvals';
import PendingApprovalsPage from '@/features/approvals/pages/PendingApprovalsPage';
import { useIsMobile, usePermissions } from '@/hooks';
import { FailResult, SuccessResult } from '@/lib';

import { createPermissionsApi } from '../../shared/auth/permissionsTestHelpers';
import { createPendingApprovalUser } from '../../shared/factories/approvalFactory';

vi.mock('@/api/hooks', () => ({
  useGetPendingApprovals: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/hooks', () => ({
  useIsMobile: vi.fn(),
  usePermissions: vi.fn(),
}));

vi.mock('@/components/layout', () => ({
  PageHeader: ({ title }: { title: string }) => <h1>{title}</h1>,
}));

vi.mock('@/features/approvals/components/PendingApprovalsTable', () => ({
  PendingApprovalsTable: ({ users }: { users: PendingApprovalUser[] }) => (
    <div>{`table:${users.map(user => user.username).join('|')}`}</div>
  ),
}));

vi.mock('@/features/approvals/components/PendingApprovalCardGrid', () => ({
  PendingApprovalCardGrid: ({ users }: { users: PendingApprovalUser[] }) => (
    <div>{`cards:${users.map(user => user.username).join('|')}`}</div>
  ),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

describe('PendingApprovalsPage', () => {
  const setErrorMock = vi.fn();

  const approvals = [
    createPendingApprovalUser({
      rowId: '11111111-1111-1111-1111-111111111111',
      username: 'alpha.pending',
    }),
    createPendingApprovalUser({
      rowId: '22222222-2222-2222-2222-222222222222',
      username: 'beta.pending',
    }),
  ];

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useGetPendingApprovals).mockReturnValue({
      data: new SuccessResult(approvals),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useGetPendingApprovals>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });

    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAny: true }),
    );
    vi.mocked(useIsMobile).mockReturnValue(false);
  });

  test('renders desktop pending approvals table when not mobile', () => {
    render(<PendingApprovalsPage />);

    expect(screen.getByText('Pending Approvals')).toBeInTheDocument();
    expect(
      screen.getByText('table:alpha.pending|beta.pending'),
    ).toBeInTheDocument();
    expect(
      screen.queryByText('cards:alpha.pending|beta.pending'),
    ).not.toBeInTheDocument();
  });

  test('renders mobile approvals card grid on mobile viewport', () => {
    vi.mocked(useIsMobile).mockReturnValue(true);

    render(<PendingApprovalsPage />);

    expect(
      screen.getByText('cards:alpha.pending|beta.pending'),
    ).toBeInTheDocument();
    expect(
      screen.queryByText('table:alpha.pending|beta.pending'),
    ).not.toBeInTheDocument();
  });

  test('shows permission denied message when user cannot manage platform approvals', () => {
    vi.mocked(usePermissions).mockReturnValue(
      createPermissionsApi({ hasAny: false }),
    );

    render(<PendingApprovalsPage />);

    expect(
      screen.getByText(
        'You do not have permission to manage platform approvals.',
      ),
    ).toBeInTheDocument();
  });

  test('shows loading overlay while approvals query is loading', () => {
    vi.mocked(useGetPendingApprovals).mockReturnValue({
      data: undefined,
      isLoading: true,
      isFetching: false,
    } as unknown as ReturnType<typeof useGetPendingApprovals>);

    render(<PendingApprovalsPage />);

    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  test('maps failed approvals response into page error context', () => {
    vi.mocked(useGetPendingApprovals).mockReturnValue({
      data: new FailResult(new UnexpectedError('Pending approvals failed')),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useGetPendingApprovals>);

    render(<PendingApprovalsPage />);

    expect(setErrorMock).toHaveBeenCalledWith({
      title: 'Unexpected Error',
      description: 'Pending approvals failed',
    });
  });

  test('renders and dismisses page error sheet from error context', async () => {
    vi.mocked(useErrorContext).mockReturnValue({
      error: {
        title: 'Approvals Error',
        description: 'Could not load approvals',
      },
      setError: setErrorMock,
    });

    render(<PendingApprovalsPage />);

    expect(screen.getByText('Approvals Error')).toBeInTheDocument();
    expect(screen.getByText('Could not load approvals')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(setErrorMock).toHaveBeenCalledWith(null);
  });
});
