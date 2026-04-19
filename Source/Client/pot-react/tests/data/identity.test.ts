import { describe, expect, test } from 'vitest';

import { EtagSchema, IdentitySchema, RowIdSchema } from '@/data';

describe('identity schemas', () => {
  test('parses valid rowId payload', () => {
    expect(RowIdSchema.parse({ rowId: 'row-1' })).toEqual({ rowId: 'row-1' });
  });

  test('parses valid etag payload', () => {
    expect(EtagSchema.parse({ etag: 1n })).toEqual({ etag: 1n });
  });

  test('parses valid identity payload', () => {
    expect(IdentitySchema.parse({ rowId: 'row-1', etag: 2n })).toEqual({
      rowId: 'row-1',
      etag: 2n,
    });
  });

  test('rejects invalid rowId payload', () => {
    expect(() => RowIdSchema.parse({ rowId: 123 })).toThrow();
  });

  test('rejects invalid etag payload', () => {
    expect(() => EtagSchema.parse({ etag: 2 })).toThrow();
  });

  test('rejects invalid identity payload', () => {
    expect(() => IdentitySchema.parse({ rowId: 'row-1' })).toThrow();
  });
});
