import { describe, expectTypeOf, test } from 'vitest';

import type {
  AccrualsStatus,
  AccrualsStatusInput,
  AccrueAccountExpensesInput,
} from '@/data';

describe('accruals contracts', () => {
  test('models accrue input shape', () => {
    const payload: AccrueAccountExpensesInput = {
      rowIds: ['account-1', 'account-2'],
    };

    expectTypeOf(payload.rowIds).toEqualTypeOf<string[]>();
  });

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
      accountAccrualsRequired: ['account-1'],
    };

    expectTypeOf(payload.expenseRenewalsRequired).toEqualTypeOf<string[]>();
    expectTypeOf(payload.incomeRenewalsRequired).toEqualTypeOf<string[]>();
    expectTypeOf(payload.accountAccrualsRequired).toEqualTypeOf<string[]>();
  });
});
