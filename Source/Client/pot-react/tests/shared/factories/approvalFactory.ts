import type {
  PendingApprovalUser,
  PendingUserStatusUpdate,
} from '@/data/approvals';

function createPendingApprovalUser(
  overrides: Partial<PendingApprovalUser> = {},
): PendingApprovalUser {
  return {
    rowId: '11111111-1111-1111-1111-111111111111',
    etag: 0,
    username: 'pending.user',
    email: 'pending.user@example.com',
    ...overrides,
  };
}

function createPendingUserStatusUpdate(
  overrides: Partial<PendingUserStatusUpdate> = {},
): PendingUserStatusUpdate {
  return {
    etag: 0,
    status: 'Approved',
    ...overrides,
  };
}

export { createPendingApprovalUser, createPendingUserStatusUpdate };

