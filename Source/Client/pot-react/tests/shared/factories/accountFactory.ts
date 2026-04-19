import type { Account } from '@/data';

function createAccount(overrides: Partial<Account> = {}): Account {
  return {
    rowId: 'account-1',
    etag: 0n,
    bsb: '000-000',
    number: '00000000',
    description: 'Default account',
    balance: 0,
    reserved: 0,
    totalExpenseAccrued: 0,
    dailyExpenseAccrual: 0,
    stableExpenseAccrual: 0,
    available: 0,
    linkedExpenses: 0,
    linkedIncomes: 0,
    ...overrides,
  };
}

function createAccountWithIdentity(
  rowId: string,
  description: string,
  overrides: Partial<Account> = {},
): Account {
  return createAccount({
    ...overrides,
    rowId,
    description,
  });
}

export { createAccount, createAccountWithIdentity };
