import { describe, expect, test } from 'vitest';

import { accountFormSchema } from '@/features/accounts/schemas/accountFormSchema';

import { flattenIssues } from '../../../shared/schemaAssertions';

describe('accountFormSchema', () => {
  const base = {
    bsb: '123-456',
    number: '001-2345',
    description: 'Main account',
    balance: 100,
    reserved: 50,
  };

  test('accepts a valid account payload', () => {
    const result = accountFormSchema.safeParse(base);

    expect(result.success).toBe(true);
  });

  test('requires an account number', () => {
    const result = accountFormSchema.safeParse({ ...base, number: '' });

    expect(flattenIssues(result)).toContainEqual({
      path: 'number',
      message: 'Account number is required',
    });
  });

  test('requires a description', () => {
    const result = accountFormSchema.safeParse({ ...base, description: '' });

    expect(flattenIssues(result)).toContainEqual({
      path: 'description',
      message: 'Description is required',
    });
  });

  test('rejects a BSB that is not in XXX-XXX format', () => {
    const result = accountFormSchema.safeParse({ ...base, bsb: '123456' });

    expect(flattenIssues(result)).toContainEqual({
      path: 'bsb',
      message: 'BSB must be in the format XXX-XXX',
    });
  });

  test('rejects a negative balance', () => {
    const result = accountFormSchema.safeParse({ ...base, balance: -1 });

    expect(flattenIssues(result)).toContainEqual({
      path: 'balance',
      message: 'Value must be 0 or greater',
    });
  });
});
