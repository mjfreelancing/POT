import { describe, expect, test } from 'vitest';

import {
  compareExpenseNextDue,
  CreateExpenseSchema,
  EditExpenseSchema,
  ExpenseSchema,
  RenewExpensesSchema,
  ToggleExcludeExpensesSchema,
} from '@/data';
import { AccrualPolicy, Frequency, RenewalMode } from '@/lib';

describe('expense schemas and comparator', () => {
  const base = {
    description: 'Rent',
    nextDue: '2026-05-01',
    accrualStart: null,
    accrualPolicy: AccrualPolicy.Automatic,
    endDate: null,
    frequency: Frequency.Months,
    frequencyCount: 1,
    amount: 500,
    note: null,
  };

  test('parses valid expense payloads', () => {
    expect(
      CreateExpenseSchema.parse({ ...base, accountRowId: 'acc-1' }),
    ).toBeTruthy();

    expect(
      EditExpenseSchema.parse({
        ...base,
        etag: 1n,
        excludeFromCalcs: false,
        accountRowId: 'acc-1',
      }),
    ).toBeTruthy();

    expect(
      ExpenseSchema.parse({
        ...base,
        rowId: 'exp-1',
        etag: 2n,
        excludeFromCalcs: false,
        account: { rowId: 'acc-1', description: 'Main' },
        accrued: 12.5,
      }),
    ).toBeTruthy();

    expect(
      RenewExpensesSchema.parse({
        rowIds: ['exp-1'],
        mode: RenewalMode.Future,
        asOfDate: '2026-04-12',
      }),
    ).toBeTruthy();

    expect(
      ToggleExcludeExpensesSchema.parse({ rowIds: ['exp-1'] }),
    ).toBeTruthy();
  });

  test('rejects invalid expense payloads', () => {
    expect(() =>
      CreateExpenseSchema.parse({ ...base, amount: '500' }),
    ).toThrow();
    expect(() =>
      EditExpenseSchema.parse({ ...base, accountRowId: 'acc-1' }),
    ).toThrow();
    expect(() => ExpenseSchema.parse({ ...base, rowId: 'exp-1' })).toThrow();
    expect(() =>
      RenewExpensesSchema.parse({ rowIds: [], mode: 'Invalid' }),
    ).toThrow();
  });

  test('compareExpenseNextDue sorts by due date then description', () => {
    const dueSoon = {
      nextDue: '2026-04-01',
      description: 'B',
    } as Parameters<typeof compareExpenseNextDue>[0];
    const dueLater = {
      nextDue: '2026-04-02',
      description: 'A',
    } as Parameters<typeof compareExpenseNextDue>[1];

    expect(compareExpenseNextDue(dueSoon, dueLater)).toBeLessThan(0);

    const sameDayA = {
      nextDue: '2026-04-01',
      description: 'Alpha',
    } as Parameters<typeof compareExpenseNextDue>[0];
    const sameDayB = {
      nextDue: '2026-04-01',
      description: 'beta',
    } as Parameters<typeof compareExpenseNextDue>[1];

    expect(compareExpenseNextDue(sameDayA, sameDayB)).toBeLessThan(0);
  });
});
