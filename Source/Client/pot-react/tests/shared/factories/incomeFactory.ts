import type { Income } from '@/data';
import { Frequency } from '@/lib';

function createIncome(overrides: Partial<Income> = {}): Income {
  return {
    rowId: 'income-1',
    etag: 0n,
    description: 'Default income',
    nextDue: '2026-01-01',
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
    ...overrides,
  };
}

export { createIncome };
