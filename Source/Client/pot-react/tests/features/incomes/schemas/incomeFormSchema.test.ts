import { describe, expect, test } from 'vitest';

import { incomeFormSchema } from '@/features/incomes/schemas/incomeFormSchema';
import { Frequency } from '@/lib';

import { flattenIssues } from '../../../shared/schemaAssertions';

describe('incomeFormSchema', () => {
  const base = {
    excludeFromCalcs: false,
    description: 'Salary',
    nextDue: '2026-05-01',
    endDate: '2026-06-01',
    frequency: Frequency.Months,
    frequencyCount: 1,
    amount: 1000,
    note: '',
    accountRowId: 'acc-1',
  };

  test('accepts a valid income and normalizes an empty note to null', () => {
    const result = incomeFormSchema.safeParse(base);

    expect(result.success).toBe(true);

    const output = result.success ? result.data : undefined;
    expect(output?.note).toBeNull();
  });

  test('requires a description', () => {
    const result = incomeFormSchema.safeParse({
      ...base,
      description: '',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'description',
      message: 'Description is required',
    });
  });

  test('rejects a negative amount', () => {
    const result = incomeFormSchema.safeParse({ ...base, amount: -1 });

    expect(flattenIssues(result)).toContainEqual({
      path: 'amount',
      message: 'Value must be 0 or greater',
    });
  });

  test('requires an amount', () => {
    const result = incomeFormSchema.safeParse({
      excludeFromCalcs: false,
      description: 'Salary',
      nextDue: '2026-05-01',
      endDate: '2026-06-01',
      frequency: Frequency.Months,
      frequencyCount: 1,
      note: '',
      accountRowId: 'acc-1',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'amount',
      message: 'This field is required',
    });
  });

  test('requires a recurring frequency count of at least 1', () => {
    const result = incomeFormSchema.safeParse({
      ...base,
      frequencyCount: 0,
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'frequencyCount',
      message: 'Must be greater than zero',
    });
  });

  test('requires nextDue not to be after endDate', () => {
    const result = incomeFormSchema.safeParse({
      ...base,
      endDate: '2026-04-01',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'endDate',
      message: 'Cannot be earlier than the next due date',
    });
  });
});
