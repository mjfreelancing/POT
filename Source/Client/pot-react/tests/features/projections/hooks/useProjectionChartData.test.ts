import { renderHook } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import { useProjectionChartData } from '@/features/projections/hooks/useProjectionChartData';

import type { Projection } from '@/data/projection';

function createProjectionData(): Projection {
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
  };
}

describe('useProjectionChartData', () => {
  test('maps projection data into chart points, series config, and keys', () => {
    const projectionData = createProjectionData();

    const { result } = renderHook(() =>
      useProjectionChartData(projectionData, 'balance'),
    );

    expect(result.current.seriesKeys).toEqual([
      'account-1',
      'account-2',
      'global',
    ]);

    expect(result.current.chartConfig).toEqual({
      'account-1': {
        label: 'Bills Account',
        color: '#8884d8',
      },
      'account-2': {
        label: 'Spending Account',
        color: '#82ca9d',
      },
      global: {
        label: 'Total (All Accounts)',
        color: '#2563eb',
      },
    });

    expect(result.current.chartData).toHaveLength(2);
    expect(result.current.chartData[0]).toMatchObject({
      date: '2026-04-01',
      formattedDate: 'Apr 01',
      'account-1': 120,
      'account-2': 80,
      global: 200,
    });

    expect(result.current.hasData).toBe(true);
  });

  test('adds account identifiers to expense and income items for each point', () => {
    const projectionData = createProjectionData();

    const { result } = renderHook(() =>
      useProjectionChartData(projectionData, 'balance'),
    );

    expect(result.current.chartData[0]?.expenseItems).toEqual([
      {
        rowId: 'expense-1',
        description: 'Rent',
        amount: 30,
        accountRowId: 'account-1',
      },
      {
        rowId: 'expense-2',
        description: 'Coffee',
        amount: 10,
        accountRowId: 'account-2',
      },
    ]);

    expect(result.current.chartData[1]?.incomeItems).toEqual([
      {
        rowId: 'income-1',
        description: 'Salary',
        amount: 50,
        accountRowId: 'account-1',
      },
    ]);
  });

  test('reports hasData false when all series values are zero for selected metric', () => {
    const projectionData = createProjectionData();
    projectionData.accounts = projectionData.accounts.map(account => ({
      ...account,
      dates: account.dates.map(dateValue => ({
        ...dateValue,
        dailyAccrual: 0,
      })),
    }));
    projectionData.global = projectionData.global.map(dateValue => ({
      ...dateValue,
      dailyAccrual: 0,
    }));

    const { result } = renderHook(() =>
      useProjectionChartData(projectionData, 'dailyAccrual'),
    );

    expect(result.current.hasData).toBe(false);
  });
});
