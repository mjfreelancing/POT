import { renderHook } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import type {
  DateValues,
  Projection,
  ProjectionInclude,
} from '@/data/projection';
import { useProjectionChartData } from '@/features/projections/hooks/useProjectionChartData';

import { createProjection } from '../../../shared/factories/projectionFactory';

const INCLUDE_NONE: ProjectionInclude = {
  reserved: false,
  accruals: false,
  arrears: false,
};

const INCLUDE_ALL: ProjectionInclude = {
  reserved: true,
  accruals: true,
  arrears: true,
};

// Reference data for the two worked examples (account of 1000, 100 reserved, a
// weekly 70 bill that accrues 10 a day). The per-day balance, reserved, unpaid
// accrual and arrears values are transcribed from the server's worked-example
// fixtures, which publish them for days 0-7. Scenario B is the same account
// with a past-due one-time 50 bill, so only its arrears differ.
const SCENARIO_DATES = [
  '2025-01-15',
  '2025-01-16',
  '2025-01-17',
  '2025-01-18',
  '2025-01-19',
  '2025-01-20',
  '2025-01-21',
  '2025-01-22',
];

const SCENARIO_BALANCE = [930, 930, 930, 930, 930, 930, 930, 860];
const SCENARIO_RESERVED = 100;
const SCENARIO_UNPAID_ACCRUAL = [0, 10, 20, 30, 40, 50, 60, 0];
const SCENARIO_B_ARREARS = 50;

function createScenarioDates(arrears: number): DateValues[] {
  return SCENARIO_DATES.map((date, index) => ({
    date,
    balance: SCENARIO_BALANCE[index]!,
    reserved: SCENARIO_RESERVED,
    arrears,
    unpaidAccrual: SCENARIO_UNPAID_ACCRUAL[index]!,
    dailyAccrual: 10,
    incomeReceived: 0,
    expensesPaid: 0,
    expenseItems: [],
    incomeItems: [],
  }));
}

function sumDateValues(first: DateValues[], second: DateValues[]): DateValues[] {
  return first.map((item, index) => {
    const other = second[index]!;

    return {
      ...item,
      balance: item.balance + other.balance,
      reserved: item.reserved + other.reserved,
      arrears: item.arrears + other.arrears,
      unpaidAccrual: item.unpaidAccrual + other.unpaidAccrual,
      dailyAccrual: item.dailyAccrual + other.dailyAccrual,
    };
  });
}

function createScenarioProjection(): Projection {
  const scenarioA = createScenarioDates(0);
  const scenarioB = createScenarioDates(SCENARIO_B_ARREARS);

  return {
    accounts: [
      { rowId: 'scenario-a', description: 'Scenario A', dates: scenarioA },
      { rowId: 'scenario-b', description: 'Scenario B', dates: scenarioB },
    ],
    global: sumDateValues(scenarioA, scenarioB),
  };
}

// Plotted values from the worked-example tables for each switch combination.
// Scenario A has no arrears, so its Arrears switch changes nothing.
type ScenarioCase = {
  name: string;
  include: ProjectionInclude;
  scenarioA: number[];
  scenarioB: number[];
};

const SCENARIO_CASES: ScenarioCase[] = [
  {
    name: 'nothing',
    include: { reserved: false, accruals: false, arrears: false },
    scenarioA: [930, 930, 930, 930, 930, 930, 930, 860],
    scenarioB: [930, 930, 930, 930, 930, 930, 930, 860],
  },
  {
    name: 'arrears only (the default)',
    include: { reserved: false, accruals: false, arrears: true },
    scenarioA: [930, 930, 930, 930, 930, 930, 930, 860],
    scenarioB: [880, 880, 880, 880, 880, 880, 880, 810],
  },
  {
    name: 'accruals only',
    include: { reserved: false, accruals: true, arrears: false },
    scenarioA: [930, 920, 910, 900, 890, 880, 870, 860],
    scenarioB: [930, 920, 910, 900, 890, 880, 870, 860],
  },
  {
    name: 'accruals and arrears',
    include: { reserved: false, accruals: true, arrears: true },
    scenarioA: [930, 920, 910, 900, 890, 880, 870, 860],
    scenarioB: [880, 870, 860, 850, 840, 830, 820, 810],
  },
  {
    name: 'reserved only',
    include: { reserved: true, accruals: false, arrears: false },
    scenarioA: [830, 830, 830, 830, 830, 830, 830, 760],
    scenarioB: [830, 830, 830, 830, 830, 830, 830, 760],
  },
  {
    name: 'reserved and arrears',
    include: { reserved: true, accruals: false, arrears: true },
    scenarioA: [830, 830, 830, 830, 830, 830, 830, 760],
    scenarioB: [780, 780, 780, 780, 780, 780, 780, 710],
  },
  {
    name: 'reserved and accruals',
    include: { reserved: true, accruals: true, arrears: false },
    scenarioA: [830, 820, 810, 800, 790, 780, 770, 760],
    scenarioB: [830, 820, 810, 800, 790, 780, 770, 760],
  },
  {
    name: 'everything',
    include: { reserved: true, accruals: true, arrears: true },
    scenarioA: [830, 820, 810, 800, 790, 780, 770, 760],
    scenarioB: [780, 770, 760, 750, 740, 730, 720, 710],
  },
];

describe('useProjectionChartData', () => {
  test('maps projection data into chart points, series config, and keys', () => {
    const projectionData = createProjection();

    const { result } = renderHook(() =>
      useProjectionChartData(projectionData, 'balance', INCLUDE_NONE),
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

  describe('balance composition', () => {
    // The first date of the shared fixture: balance 120, reserved 10, arrears 5,
    // unpaid accrual 15 for account-1; the global series sums both accounts.
    function plotBalance(include?: ProjectionInclude) {
      const projectionData = createProjection();

      const { result } = renderHook(() =>
        useProjectionChartData(projectionData, 'balance', include),
      );

      return result.current.chartData[0]!;
    }

    test('plots the default include of arrears only when none is supplied', () => {
      const point = plotBalance();

      expect(point['account-1']).toBe(115);
      expect(point['account-2']).toBe(80);
      expect(point.global).toBe(195);
    });

    test('plots the unadjusted balance when every switch is off', () => {
      const point = plotBalance(INCLUDE_NONE);

      expect(point['account-1']).toBe(120);
      expect(point['account-2']).toBe(80);
      expect(point.global).toBe(200);
    });

    test('reserved subtracts only the reserved component', () => {
      const include: ProjectionInclude = { ...INCLUDE_NONE, reserved: true };
      const point = plotBalance(include);

      expect(point['account-1']).toBe(110);
      expect(point['account-2']).toBe(75);
      expect(point.global).toBe(185);
    });

    test('accruals subtracts only the unpaid accrual component', () => {
      const include: ProjectionInclude = { ...INCLUDE_NONE, accruals: true };
      const point = plotBalance(include);

      expect(point['account-1']).toBe(105);
      expect(point['account-2']).toBe(75);
      expect(point.global).toBe(180);
    });

    test('arrears subtracts only the arrears component', () => {
      const include: ProjectionInclude = { ...INCLUDE_NONE, arrears: true };
      const point = plotBalance(include);

      expect(point['account-1']).toBe(115);
      expect(point['account-2']).toBe(80);
      expect(point.global).toBe(195);
    });

    test('plots the retired available figure when every switch is on', () => {
      const point = plotBalance(INCLUDE_ALL);

      expect(point['account-1']).toBe(90);
      expect(point['account-2']).toBe(70);
      expect(point.global).toBe(160);
    });

    test('plots the global series as the sum of the account series for every combination', () => {
      const projectionData = createProjection();

      SCENARIO_CASES.forEach(({ include }) => {
        const { result } = renderHook(() =>
          useProjectionChartData(projectionData, 'balance', include),
        );

        result.current.chartData.forEach(point => {
          const accountSum =
            (point['account-1'] as number) + (point['account-2'] as number);

          expect(point.global).toBe(accountSum);
        });
      });
    });

    test('leaves the other metrics untouched by the switches', () => {
      const projectionData = createProjection();

      const { result } = renderHook(() =>
        useProjectionChartData(projectionData, 'dailyAccrual', INCLUDE_ALL),
      );

      expect(result.current.chartData[0]).toMatchObject({
        'account-1': 10,
        'account-2': 5,
        global: 15,
      });
    });

    test('reports hasData false when the composed values are all zero', () => {
      const projectionData = createProjection();
      const netToZero = (values: DateValues): DateValues => ({
        ...values,
        balance: values.reserved + values.arrears + values.unpaidAccrual,
      });

      projectionData.accounts = projectionData.accounts.map(account => ({
        ...account,
        dates: account.dates.map(netToZero),
      }));
      projectionData.global = projectionData.global.map(netToZero);

      const { result: allOn } = renderHook(() =>
        useProjectionChartData(projectionData, 'balance', INCLUDE_ALL),
      );

      const { result: allOff } = renderHook(() =>
        useProjectionChartData(projectionData, 'balance', INCLUDE_NONE),
      );

      expect(allOn.current.hasData).toBe(false);
      expect(allOff.current.hasData).toBe(true);
    });
  });

  describe('worked examples', () => {
    test.each(SCENARIO_CASES)(
      'plots scenario A and B with $name',
      ({ include, scenarioA, scenarioB }) => {
        const projectionData = createScenarioProjection();

        const { result } = renderHook(() =>
          useProjectionChartData(projectionData, 'balance', include),
        );

        const plottedA = result.current.chartData.map(
          point => point['scenario-a'],
        );
        const plottedB = result.current.chartData.map(
          point => point['scenario-b'],
        );
        const plottedGlobal = result.current.chartData.map(
          point => point.global,
        );

        const expectedGlobal = scenarioA.map(
          (value, index) => value + scenarioB[index]!,
        );

        expect(plottedA).toEqual(scenarioA);
        expect(plottedB).toEqual(scenarioB);
        expect(plottedGlobal).toEqual(expectedGlobal);
      },
    );

    test('falls by exactly 10 a day across both payment days when only accruals is on', () => {
      const projectionData = createScenarioProjection();
      const include: ProjectionInclude = { ...INCLUDE_NONE, accruals: true };

      const { result } = renderHook(() =>
        useProjectionChartData(projectionData, 'balance', include),
      );

      const plotted = result.current.chartData.map(
        point => point['scenario-a'] as number,
      );

      const steps = plotted
        .slice(1)
        .map((value, index) => plotted[index]! - value);

      expect(plotted[0]).toBe(930);
      expect(plotted[7]).toBe(860);
      expect(steps).toEqual([10, 10, 10, 10, 10, 10, 10]);
    });
  });
});
