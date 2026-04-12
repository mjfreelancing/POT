import { describe, expect, test } from 'vitest';

import {
  AccountSchema,
  compareAccountBsbNumber,
  CreateAccountSchema,
  EditAccountSchema,
} from '@/data';

describe('account schemas and comparator', () => {
  test('parses valid account payloads', () => {
    const base = {
      bsb: '123-456',
      number: '00112233',
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

  test('compareAccountBsbNumber sorts by bsb then number (case-insensitive)', () => {
    const accountA = {
      bsb: '111-111',
      number: '999',
    } as Parameters<typeof compareAccountBsbNumber>[0];
    const accountB = {
      bsb: '222-222',
      number: '000',
    } as Parameters<typeof compareAccountBsbNumber>[1];

    expect(compareAccountBsbNumber(accountA, accountB)).toBeLessThan(0);

    const accountC = {
      bsb: '123-456',
      number: 'A10',
    } as Parameters<typeof compareAccountBsbNumber>[0];
    const accountD = {
      bsb: '123-456',
      number: 'a20',
    } as Parameters<typeof compareAccountBsbNumber>[1];

    expect(compareAccountBsbNumber(accountC, accountD)).toBeLessThan(0);
  });
});
