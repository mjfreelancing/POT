import { describe, expect, test } from 'vitest';

import { roleSchema, roleSelectionSchema } from '@/data/role';

describe('role schemas', () => {
  test('parses valid role payloads', () => {
    expect(
      roleSchema.parse({
        name: 'Admin',
        rowId: '550e8400-e29b-41d4-a716-446655440000',
        etag: 1,
      }),
    ).toBeTruthy();

    expect(
      roleSelectionSchema.parse({
        roleId: '550e8400-e29b-41d4-a716-446655440000',
      }),
    ).toBeTruthy();
  });

  test('rejects invalid role payloads', () => {
    expect(() =>
      roleSchema.parse({ name: 'Admin', rowId: 'not-a-uuid', etag: 1 }),
    ).toThrow();

    expect(() => roleSelectionSchema.parse({ roleId: 'abc' })).toThrow();
  });
});
