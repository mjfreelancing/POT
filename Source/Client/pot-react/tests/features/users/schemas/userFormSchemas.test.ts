import { describe, expect, test } from 'vitest';

import {
  userInvitationFormSchema,
  userRoleUpdateFormSchema,
} from '@/features/users/schemas/userFormSchemas';

import { flattenIssues } from '../../../shared/schemaAssertions';

describe('userInvitationFormSchema', () => {
  test('accepts a valid invitation payload', () => {
    const result = userInvitationFormSchema.safeParse({
      username: 'newuser',
      email: 'newuser@example.com',
      roleId: 'role-1',
    });

    expect(result.success).toBe(true);
  });

  test('rejects a blank username', () => {
    const result = userInvitationFormSchema.safeParse({
      username: '',
      email: 'newuser@example.com',
      roleId: 'role-1',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'username',
      message: 'Username must be provided',
    });
  });

  test('rejects an invalid email', () => {
    const result = userInvitationFormSchema.safeParse({
      username: 'newuser',
      email: 'not-an-email',
      roleId: 'role-1',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'email',
      message: 'Please enter a valid email address',
    });
  });

  test('rejects an unselected role', () => {
    const result = userInvitationFormSchema.safeParse({
      username: 'newuser',
      email: 'newuser@example.com',
      roleId: '',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'roleId',
      message: 'Please select a role',
    });
  });
});

describe('userRoleUpdateFormSchema', () => {
  test('accepts a valid role update payload', () => {
    const result = userRoleUpdateFormSchema.safeParse({ roleId: 'role-1' });

    expect(result.success).toBe(true);
  });

  test('rejects an unselected role', () => {
    const result = userRoleUpdateFormSchema.safeParse({ roleId: '' });

    expect(flattenIssues(result)).toContainEqual({
      path: 'roleId',
      message: 'Please select a role',
    });
  });
});
