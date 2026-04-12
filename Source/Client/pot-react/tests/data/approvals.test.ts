import { describe, expect, test } from 'vitest';

import {
  approvalStatusSchema,
  pendingApprovalUserSchema,
  pendingUserStatusUpdateSchema,
} from '@/data/approvals';

describe('approval schemas', () => {
  test('parses valid approval payloads', () => {
    expect(approvalStatusSchema.parse('Approved')).toBe('Approved');

    expect(
      pendingApprovalUserSchema.parse({
        username: 'jane',
        email: 'jane@example.com',
        rowId: '550e8400-e29b-41d4-a716-446655440000',
        etag: 1n,
      }),
    ).toBeTruthy();

    expect(
      pendingUserStatusUpdateSchema.parse({
        etag: 2n,
        status: 'Rejected',
      }),
    ).toBeTruthy();
  });

  test('rejects invalid approval payloads', () => {
    expect(() => approvalStatusSchema.parse('Pending')).toThrow();

    expect(() =>
      pendingApprovalUserSchema.parse({ username: 'jane' }),
    ).toThrow();

    expect(() =>
      pendingUserStatusUpdateSchema.parse({ etag: 1n, status: 'Enabled' }),
    ).toThrow();
  });
});
