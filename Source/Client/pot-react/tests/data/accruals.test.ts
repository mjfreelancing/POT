import { describe, expectTypeOf, test } from 'vitest';

import type { AccrualsStatus, AccrualsStatusInput } from '@/data';

describe('accruals contracts', () => {
  test('models accrual status request input shape', () => {
    const payload: AccrualsStatusInput = {
      accountRowIds: ['account-1'],
    };

    expectTypeOf(payload.accountRowIds).toEqualTypeOf<string[]>();
  });

  test('models accrual status response shape', () => {
    const payload: AccrualsStatus = {
      expenseRenewalsRequired: ['expense-1'],
      incomeRenewalsRequired: ['income-1'],
    };

    expectTypeOf(payload.expenseRenewalsRequired).toEqualTypeOf<string[]>();
    expectTypeOf(payload.incomeRenewalsRequired).toEqualTypeOf<string[]>();
  });
});
