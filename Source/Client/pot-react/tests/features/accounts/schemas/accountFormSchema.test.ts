import { describe, expect, test } from 'vitest';

import { accountFormSchema } from '@/features/accounts/schemas/accountFormSchema';

import { flattenIssues } from '../../../shared/schemaAssertions';

describe('accountFormSchema', () => {
  const base = {
    description: 'Main account',
    balance: 100,
    reserved: 50,
  };

  test('accepts a valid account payload', () => {
    const result = accountFormSchema.safeParse(base);

    expect(result.success).toBe(true);
  });

  test('requires a description', () => {
    const result = accountFormSchema.safeParse({ ...base, description: '' });

    expect(flattenIssues(result)).toContainEqual({
      path: 'description',
      message: 'Description is required',
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
