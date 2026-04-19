import { describe, expect, test } from 'vitest';

import {
  siteUserSchema,
  userInvitationSchema,
  userRoleUpdateSchema,
  userStatusSchema,
  userStatusUpdateSchema,
} from '@/data/siteUser';

describe('site user schemas', () => {
  test('parses a valid site user payload', () => {
    expect(
      siteUserSchema.parse({
        username: 'jane',
        displayName: 'Jane Doe',
        email: 'jane@example.com',
        roles: ['Admin'],
        status: 'Enabled',
        lastLoggedInUtc: null,
        rowId: '550e8400-e29b-41d4-a716-446655440000',
        etag: 1n,
      }),
    ).toBeTruthy();
  });

  test('parses a valid user invitation payload', () => {
    expect(
      userInvitationSchema.parse({
        username: 'new-user',
        email: 'new@example.com',
        roleIds: ['550e8400-e29b-41d4-a716-446655440000'],
      }),
    ).toBeTruthy();
  });

  test('parses a valid user role update payload', () => {
    expect(userRoleUpdateSchema.parse({ etag: 2n, roleIds: [] })).toBeTruthy();
  });

  test('parses a valid user status update payload', () => {
    expect(
      userStatusUpdateSchema.parse({ etag: 2n, status: 'Disabled' }),
    ).toBeTruthy();
  });

  test('parses a valid user status value', () => {
    expect(userStatusSchema.parse('Approval')).toBe('Approval');
  });

  test('rejects invalid site user payloads', () => {
    expect(() => siteUserSchema.parse({ username: 'jane' })).toThrow();

    expect(() =>
      userInvitationSchema.parse({ username: 'x', email: 'bad', roleIds: [] }),
    ).toThrow();

    expect(() =>
      userRoleUpdateSchema.parse({ etag: 1n, roleIds: ['not-uuid'] }),
    ).toThrow();

    expect(() =>
      userStatusUpdateSchema.parse({ etag: 1n, status: 'Pending' }),
    ).toThrow();
  });
});
