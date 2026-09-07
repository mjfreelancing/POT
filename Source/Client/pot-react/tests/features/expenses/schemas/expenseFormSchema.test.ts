import { describe, expect, test } from 'vitest';

import { expenseFormSchema } from '@/features/expenses/schemas/expenseFormSchema';
import { AccrualPolicy, Frequency } from '@/lib';

import { flattenIssues } from '../../../shared/schemaAssertions';

describe('expenseFormSchema', () => {
  const base = {
    excludeFromCalcs: false,
    description: 'Rent',
    nextDue: '2026-05-01',
    accrualPolicy: AccrualPolicy.Automatic,
    endDate: '2026-06-01',
    frequency: Frequency.Months,
    frequencyCount: 1,
    amount: 500,
    note: '',
    accountRowId: 'acc-1',
  };

  test('accepts a valid expense and normalizes an empty note to null', () => {
    const result = expenseFormSchema.safeParse(base);

    expect(result.success).toBe(true);

    const output = result.success ? result.data : undefined;
    expect(output?.note).toBeNull();
  });

  test('requires a description', () => {
    const result = expenseFormSchema.safeParse({
      ...base,
      description: '',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'description',
      message: 'Description is required',
    });
  });

  test('rejects a negative amount', () => {
    const result = expenseFormSchema.safeParse({ ...base, amount: -1 });

    expect(flattenIssues(result)).toContainEqual({
      path: 'amount',
      message: 'Value must be 0 or greater',
    });
  });

  test('requires an amount', () => {
    const result = expenseFormSchema.safeParse({
      excludeFromCalcs: false,
      description: 'Rent',
      nextDue: '2026-05-01',
      accrualPolicy: AccrualPolicy.Automatic,
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
    const result = expenseFormSchema.safeParse({
      ...base,
      frequencyCount: 0,
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'frequencyCount',
      message: 'Must be greater than zero',
    });
  });

  test('requires a one-time frequency count of 0', () => {
    const result = expenseFormSchema.safeParse({
      ...base,
      frequency: Frequency.OneTime,
      frequencyCount: 1,
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'frequencyCount',
      message: 'Must be zero when Frequency is One Time',
    });
  });

  test('requires accrualStart to be empty when the policy is None', () => {
    const result = expenseFormSchema.safeParse({
      ...base,
      accrualPolicy: AccrualPolicy.None,
      accrualStart: '2026-04-01',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'accrualStart',
      message: 'Must be empty when Accrual Policy is None',
    });
  });

  test('requires nextDue not to be after endDate', () => {
    const result = expenseFormSchema.safeParse({
      ...base,
      endDate: '2026-04-01',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'endDate',
      message: 'Cannot be earlier than the next due date',
    });
  });

  test('requires accrualStart not to be after endDate', () => {
    const result = expenseFormSchema.safeParse({
      ...base,
      accrualStart: '2026-07-01',
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'accrualStart',
      message: 'Cannot be after the end date',
    });
  });
});
