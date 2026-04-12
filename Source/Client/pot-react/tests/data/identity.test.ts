import { describe, expect, test } from 'vitest';

import { EtagSchema, IdentitySchema, RowIdSchema } from '@/data';

describe('identity schemas', () => {
  test('parses valid rowId and etag payloads', () => {
    expect(RowIdSchema.parse({ rowId: 'row-1' })).toEqual({ rowId: 'row-1' });
    expect(EtagSchema.parse({ etag: 1n })).toEqual({ etag: 1n });
    expect(IdentitySchema.parse({ rowId: 'row-1', etag: 2n })).toEqual({
      rowId: 'row-1',
      etag: 2n,
    });
  });

  test('rejects invalid identity payloads', () => {
    expect(() => RowIdSchema.parse({ rowId: 123 })).toThrow();
    expect(() => EtagSchema.parse({ etag: 2 })).toThrow();
    expect(() => IdentitySchema.parse({ rowId: 'row-1' })).toThrow();
  });
});
