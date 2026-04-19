import type { Projection } from '@/data/projection';

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
            available: 90,
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
            available: 110,
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
            available: 70,
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
            available: 80,
            dailyAccrual: 6,
            incomeReceived: 0,
            expensesPaid: 0,
            expenseItems: [],
            incomeItems: [],
          },
        ],
      },
    ],
    global: [
      {
        date: '2026-04-01',
        balance: 200,
        available: 160,
        dailyAccrual: 15,
        incomeReceived: 0,
        expensesPaid: 40,
        expenseItems: [
          { rowId: 'expense-1', description: 'Rent', amount: 30 },
          { rowId: 'expense-2', description: 'Coffee', amount: 10 },
        ],
        incomeItems: [],
      },
      {
        date: '2026-04-02',
        balance: 240,
        available: 190,
        dailyAccrual: 18,
        incomeReceived: 50,
        expensesPaid: 0,
        expenseItems: [],
        incomeItems: [{ rowId: 'income-1', description: 'Salary', amount: 50 }],
      },
    ],
    ...overrides,
  };
}

export { createProjection };
