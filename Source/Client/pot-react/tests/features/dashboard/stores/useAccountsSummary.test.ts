import { beforeEach, describe, expect, test } from 'vitest';

import accountsSummaryStore from '@/features/dashboard/stores/useAccountsSummary';

describe('useAccountsSummary store', () => {
  beforeEach(() => {
    accountsSummaryStore.setState({
      totalBalance: 0,
      totalReserved: 0,
      totalAvailable: 0,
      totalDailyAccrual: 0,
      totalStableExpenseAccrual: 0,
    });
  });

  test('starts with zeroed summary values', () => {
    const state = accountsSummaryStore.getState();

    expect(state.totalBalance).toBe(0);
    expect(state.totalReserved).toBe(0);
    expect(state.totalAvailable).toBe(0);
    expect(state.totalDailyAccrual).toBe(0);
    expect(state.totalStableExpenseAccrual).toBe(0);
  });

  test('setSummary updates all summary totals in one state transition', () => {
    accountsSummaryStore.getState().setSummary(1200, 450, 750, 35, 22);

    const state = accountsSummaryStore.getState();

    expect(state.totalBalance).toBe(1200);
    expect(state.totalReserved).toBe(450);
    expect(state.totalAvailable).toBe(750);
    expect(state.totalDailyAccrual).toBe(35);
    expect(state.totalStableExpenseAccrual).toBe(22);
  });
});
