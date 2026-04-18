import { describe, expect, test } from 'vitest';

import { createRowIdGetter } from '@/components/table/dataTableUtils';

type IdentityLike = {
  rowId: string | number;
};

describe('createRowIdGetter', () => {
  test('returns rowId as string for string row ids', () => {
    const getRowId = createRowIdGetter<IdentityLike & { rowId: string }>();

    const result = getRowId(
      {
        rowId: 'account-1',
      },
      0,
    );

    expect(result).toBe('account-1');
  });

  test('coerces numeric row ids to strings', () => {
    const getRowId = createRowIdGetter<IdentityLike & { rowId: number }>();

    const result = getRowId(
      {
        rowId: 42,
      },
      0,
    );

    expect(result).toBe('42');
  });
});
