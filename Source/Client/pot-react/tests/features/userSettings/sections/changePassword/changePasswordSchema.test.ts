import { describe, expect, test } from 'vitest';

import { changePasswordSchema } from '@/features/userSettings/sections/changePassword/changePasswordSchema';

import { flattenIssues } from '../../../../shared/schemaAssertions';

describe('changePasswordSchema', () => {
  test('accepts a valid change-password payload', () => {
    const result = changePasswordSchema.safeParse({
      currentPassword: 'Oldpass1!',
      newPassword: 'Newpass1!',
      confirmPassword: 'Newpass1!',
    });

    expect(result.success).toBe(true);
  });

  test('rejects a new password without an uppercase letter', () => {
    const result = changePasswordSchema.safeParse({
      currentPassword: 'Oldpass1!',
      newPassword: 'newpass1!',
      confirmPassword: 'newpass1!',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'newPassword',
      message: 'Password must contain at least one uppercase letter',
    });
  });

  test('requires the confirmation to match the new password', () => {
    const result = changePasswordSchema.safeParse({
      currentPassword: 'Oldpass1!',
      newPassword: 'Newpass1!',
      confirmPassword: 'Newpass2!',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'confirmPassword',
      message: 'Passwords do not match',
    });
  });

  test('requires the new password to differ from the current password', () => {
    const result = changePasswordSchema.safeParse({
      currentPassword: 'Same1!',
      newPassword: 'Same1!',
      confirmPassword: 'Same1!',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'newPassword',
      message: 'New password must be different from current password',
    });
  });
});
