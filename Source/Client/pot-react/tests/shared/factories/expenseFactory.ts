import type { Expense } from '@/data';
import { AccrualPolicy, Frequency } from '@/lib';

function createExpense(overrides: Partial<Expense> = {}): Expense {
  return {
    rowId: 'expense-1',
    etag: 0n,
    description: 'Default expense',
    nextDue: '2026-01-01',
    accrualStart: null,
    accrualPolicy: AccrualPolicy.None,
    endDate: null,
    frequency: Frequency.Months,
    frequencyCount: 1,
    amount: 100,
    note: null,
    excludeFromCalcs: false,
    account: {
      rowId: 'account-1',
      description: 'Default account',
    },
    accrued: 0,
    ...overrides,
  };
}

export { createExpense };
