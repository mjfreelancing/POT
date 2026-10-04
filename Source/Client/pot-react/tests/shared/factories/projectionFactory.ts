import type { Projection } from '@/data/projection';

// Account values are chosen so a selected-accounts total can be composed from them.
function createProjection(overrides: Partial<Projection> = {}): Projection {
  return {
    accounts: [
      {
        rowId: 'account-1',
        description: 'Bills Account',
        dates: [
          {
            date: '2026-04-01',
            balance: 120,
            reserved: 10,
            arrears: 5,
            unpaidAccrual: 15,
            dailyAccrual: 10,
            incomeReceived: 0,
            expensesPaid: 30,
            expenseItems: [
              { rowId: 'expense-1', description: 'Rent', amount: 30 },
            ],
            incomeItems: [],
          },
          {
            date: '2026-04-02',
            balance: 150,
            reserved: 10,
            arrears: 5,
            unpaidAccrual: 25,
            dailyAccrual: 12,
            incomeReceived: 50,
            expensesPaid: 0,
            expenseItems: [],
            incomeItems: [
              { rowId: 'income-1', description: 'Salary', amount: 50 },
            ],
          },
        ],
      },
      {
        rowId: 'account-2',
        description: 'Spending Account',
        dates: [
          {
            date: '2026-04-01',
            balance: 80,
            reserved: 5,
            arrears: 0,
            unpaidAccrual: 5,
            dailyAccrual: 5,
            incomeReceived: 0,
            expensesPaid: 10,
            expenseItems: [
              { rowId: 'expense-2', description: 'Coffee', amount: 10 },
            ],
            incomeItems: [],
          },
          {
            date: '2026-04-02',
            balance: 90,
            reserved: 5,
            arrears: 0,
            unpaidAccrual: 5,
            dailyAccrual: 6,
            incomeReceived: 0,
            expensesPaid: 0,
            expenseItems: [],
            incomeItems: [],
          },
        ],
      },
    ],
    ...overrides,
  };
}

export { createProjection };
