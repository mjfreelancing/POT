import { describe, expect, expectTypeOf, test } from 'vitest';

import {
  DEFAULT_PROJECTION_METRIC,
  DEFAULT_PROJECTION_PERIOD,
  PROJECTION_METRICS,
  PROJECTION_PERIODS,
} from '@/data/projection';

describe('projection contracts', () => {
  test('exposes expected defaults', () => {
    expect(DEFAULT_PROJECTION_METRIC).toBe('balance');
    expect(DEFAULT_PROJECTION_PERIOD).toBe(6);
  });

  test('exposes expected metric config map', () => {
    expect(Object.keys(PROJECTION_METRICS)).toEqual([
      'balance',
      'available',
      'dailyAccrual',
      'incomeReceived',
      'expensesPaid',
    ]);

    expect(PROJECTION_METRICS.balance.chartType).toBe('line');
    expect(PROJECTION_METRICS.expensesPaid.chartType).toBe('bar');
  });

  test('exposes expected projection period options', () => {
    expect(PROJECTION_PERIODS).toEqual([
      { label: '1 mo', value: 1 },
      { label: '2 mo', value: 2 },
      { label: '3 mo', value: 3 },
      { label: '6 mo', value: 6 },
      { label: '9 mo', value: 9 },
      { label: '12 mo', value: 12 },
    ]);

    expectTypeOf(PROJECTION_PERIODS).toEqualTypeOf<
      readonly { label: string; value: number }[]
    >();
  });
});
