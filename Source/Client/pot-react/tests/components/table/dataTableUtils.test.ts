import { describe, expect, test } from 'vitest';

import { createRowIdGetter } from '@/components/table/dataTableUtils';
import type { Identity } from '@/data';

type IdentityLike = {
  rowId: string | number;
};

describe('createRowIdGetter', () => {
  test('returns rowId as string for string row ids', () => {
    const getRowId = createRowIdGetter<
      IdentityLike & { rowId: string; etag: bigint }
    >();

    const result = getRowId(
      {
        rowId: 'account-1',
        etag: 0n,
      },
      0,
    );

    expect(result).toBe('account-1');
  });

  test('coerces numeric row ids to strings', () => {
    const getRowId = createRowIdGetter<Identity>();

    const result = getRowId(
      {
        rowId: 42,
        etag: 0n,
      } as unknown as Identity,
      0,
    );

    expect(result).toBe('42');
  });
});
