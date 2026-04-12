import { describe, expect, test } from 'vitest';

import {
  compareIncomeNextDue,
  CreateIncomeSchema,
  EditIncomeSchema,
  IncomeSchema,
  RenewIncomesSchema,
  ToggleExcludeIncomesSchema,
} from '@/data';
import { Frequency, RenewalMode } from '@/lib';

describe('income schemas and comparator', () => {
  const base = {
    description: 'Salary',
    nextDue: '2026-05-15',
    endDate: null,
    frequency: Frequency.Weeks,
    frequencyCount: 2,
    amount: 1500,
    note: null,
  };

  test('parses valid income payloads', () => {
    expect(
      CreateIncomeSchema.parse({ ...base, accountRowId: 'acc-1' }),
    ).toBeTruthy();

    expect(
      EditIncomeSchema.parse({
        ...base,
        etag: 1n,
        excludeFromCalcs: false,
        accountRowId: 'acc-1',
      }),
    ).toBeTruthy();

    expect(
      IncomeSchema.parse({
        ...base,
        rowId: 'inc-1',
        etag: 2n,
        excludeFromCalcs: false,
        account: { rowId: 'acc-1', description: 'Main' },
      }),
    ).toBeTruthy();

    expect(
      RenewIncomesSchema.parse({
        rowIds: ['inc-1'],
        mode: RenewalMode.Overdue,
        asOfDate: '2026-04-12',
      }),
    ).toBeTruthy();

    expect(
      ToggleExcludeIncomesSchema.parse({ rowIds: ['inc-1'] }),
    ).toBeTruthy();
  });

  test('rejects invalid income payloads', () => {
    expect(() =>
      CreateIncomeSchema.parse({ ...base, frequencyCount: '2' }),
    ).toThrow();
    expect(() =>
      EditIncomeSchema.parse({ ...base, accountRowId: 'acc-1' }),
    ).toThrow();
    expect(() => IncomeSchema.parse({ ...base, rowId: 'inc-1' })).toThrow();
    expect(() =>
      RenewIncomesSchema.parse({ rowIds: [], mode: 'Invalid' }),
    ).toThrow();
  });

  test('compareIncomeNextDue sorts by due date then description', () => {
    const dueSoon = {
      nextDue: '2026-04-01',
      description: 'B',
    } as Parameters<typeof compareIncomeNextDue>[0];
    const dueLater = {
      nextDue: '2026-04-02',
      description: 'A',
    } as Parameters<typeof compareIncomeNextDue>[1];

    expect(compareIncomeNextDue(dueSoon, dueLater)).toBeLessThan(0);

    const sameDayA = {
      nextDue: '2026-04-01',
      description: 'Alpha',
    } as Parameters<typeof compareIncomeNextDue>[0];
    const sameDayB = {
      nextDue: '2026-04-01',
      description: 'beta',
    } as Parameters<typeof compareIncomeNextDue>[1];

    expect(compareIncomeNextDue(sameDayA, sameDayB)).toBeLessThan(0);
  });
});
