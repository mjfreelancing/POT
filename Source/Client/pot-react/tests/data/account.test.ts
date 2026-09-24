import { describe, expect, test } from 'vitest';

import {
  AccountSchema,
  compareAccountDescription,
  CreateAccountSchema,
  EditAccountSchema,
} from '@/data';

describe('account schemas and comparator', () => {
  test('parses valid account payloads', () => {
    const base = {
      description: 'Main account',
      balance: 100,
      reserved: 20,
    };

    expect(CreateAccountSchema.parse(base)).toEqual(base);

    expect(EditAccountSchema.parse({ ...base, etag: 1n }).etag).toBe(1n);

    const parsed = AccountSchema.parse({
      ...base,
      rowId: 'account-1',
      etag: 2n,
      totalExpenseAccrued: 5,
      dailyExpenseAccrual: 1,
      stableExpenseAccrual: 1,
      available: 80,
      linkedExpenses: 2,
      linkedIncomes: 1,
    });

    expect(parsed.rowId).toBe('account-1');
  });

  test('rejects invalid account payloads', () => {
    expect(() => CreateAccountSchema.parse({})).toThrow();
    expect(() => EditAccountSchema.parse({ etag: 1n })).toThrow();
    expect(() => AccountSchema.parse({ rowId: 'x', etag: 1n })).toThrow();
  });

  test('compareAccountDescription sorts by description (case-insensitive)', () => {
    const holidaySavings = {
      description: 'Holiday savings',
    } as Parameters<typeof compareAccountDescription>[0];

    const mainAccount = {
      description: 'Main account',
    } as Parameters<typeof compareAccountDescription>[1];

    expect(compareAccountDescription(holidaySavings, mainAccount)).toBeLessThan(
      0,
    );

    const sameDescriptionDifferentCase = {
      description: 'main account',
    } as Parameters<typeof compareAccountDescription>[0];

    expect(
      compareAccountDescription(mainAccount, sameDescriptionDifferentCase),
    ).toBe(0);
  });
});
