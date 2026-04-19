import { renderHook } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import { useProjectionChartData } from '@/features/projections/hooks/useProjectionChartData';

import { createProjection } from '../../../shared/factories/projectionFactory';

describe('useProjectionChartData', () => {
  test('maps projection data into chart points, series config, and keys', () => {
    const projectionData = createProjection();

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
    const projectionData = createProjection();

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
    const projectionData = createProjection();
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
