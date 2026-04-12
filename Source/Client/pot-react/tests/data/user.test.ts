import { describe, expect, test } from 'vitest';

import {
  compareUserDisplayName,
  CreateUserSchema,
  EditUserSchema,
  UserSchema,
} from '@/data';

describe('user schemas and comparator', () => {
  test('parses valid user payloads', () => {
    expect(
      CreateUserSchema.parse({
        displayName: 'Jane Doe',
        email: 'jane@example.com',
        username: 'jane',
      }),
    ).toBeTruthy();

    expect(
      EditUserSchema.parse({
        displayName: 'Jane Doe',
        email: 'jane@example.com',
        etag: 1n,
      }),
    ).toBeTruthy();

    expect(
      UserSchema.parse({
        rowId: 'user-1',
        etag: 2n,
        username: 'jane',
        displayName: 'Jane Doe',
        email: 'jane@example.com',
        permissions: ['users.read'],
        site: {
          rowId: 'site-1',
          etag: 3n,
          name: 'HQ',
          description: 'Main site',
        },
      }),
    ).toBeTruthy();
  });

  test('rejects invalid user payloads', () => {
    expect(() => CreateUserSchema.parse({ displayName: 'Jane' })).toThrow();
    expect(() =>
      EditUserSchema.parse({ displayName: 'Jane', email: 'x' }),
    ).toThrow();
    expect(() => UserSchema.parse({ rowId: 'user-1', etag: 1n })).toThrow();
  });

  test('compareUserDisplayName sorts by display name', () => {
    const userA = { displayName: 'Alice' } as Parameters<
      typeof compareUserDisplayName
    >[0];
    const userB = { displayName: 'Bob' } as Parameters<
      typeof compareUserDisplayName
    >[1];

    expect(compareUserDisplayName(userA, userB)).toBeLessThan(0);
  });
});
