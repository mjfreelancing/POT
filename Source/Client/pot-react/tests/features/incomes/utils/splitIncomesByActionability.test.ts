import { describe, expect, test } from 'vitest';

import { splitIncomesByActionability } from '@/features/incomes/utils/splitIncomesByActionability';
import { dateIsoFormat, localToday } from '@/lib';

import { createIncome } from '../../../shared/factories/incomeFactory';

function createDateOffset(daysFromToday: number): string {
  const date = localToday();
  date.setDate(date.getDate() + daysFromToday);

  return dateIsoFormat(date);
}

describe('splitIncomesByActionability', () => {
  test('classifies excluded incomes as non-actionable', () => {
    const excludedIncome = createIncome({
      rowId: 'excluded-income',
      excludeFromCalcs: true,
      nextDue: createDateOffset(2),
    });

    const result = splitIncomesByActionability([excludedIncome]);

    expect(result.excludedIncomes).toEqual([excludedIncome]);
    expect(result.endedIncomes).toEqual([]);
    expect(result.actionableIncomes).toEqual([]);
    expect(result.futureIncomes).toEqual([]);
    expect(result.dueTodayIncomes).toEqual([]);
    expect(result.overdueIncomes).toEqual([]);
  });

  test('classifies ended incomes as non-actionable', () => {
    const endedIncome = createIncome({
      rowId: 'ended-income',
      excludeFromCalcs: false,
      endDate: createDateOffset(-1),
      nextDue: createDateOffset(2),
    });

    const result = splitIncomesByActionability([endedIncome]);

    expect(result.excludedIncomes).toEqual([]);
    expect(result.endedIncomes).toEqual([endedIncome]);
    expect(result.actionableIncomes).toEqual([]);
    expect(result.futureIncomes).toEqual([]);
    expect(result.dueTodayIncomes).toEqual([]);
    expect(result.overdueIncomes).toEqual([]);
  });

  test('treats end date today as actionable and due today', () => {
    const endingTodayIncome = createIncome({
      rowId: 'ending-today-income',
      excludeFromCalcs: false,
      endDate: createDateOffset(0),
      nextDue: createDateOffset(0),
    });

    const result = splitIncomesByActionability([endingTodayIncome]);

    expect(result.excludedIncomes).toEqual([]);
    expect(result.endedIncomes).toEqual([]);
    expect(result.actionableIncomes).toEqual([endingTodayIncome]);
    expect(result.futureIncomes).toEqual([]);
    expect(result.dueTodayIncomes).toEqual([endingTodayIncome]);
    expect(result.overdueIncomes).toEqual([]);
  });

  test('keeps mixed selection actionable when at least one item can be received', () => {
    const excludedIncome = createIncome({
      rowId: 'excluded-income',
      excludeFromCalcs: true,
      nextDue: createDateOffset(2),
    });

    const endedIncome = createIncome({
      rowId: 'ended-income',
      excludeFromCalcs: false,
      endDate: createDateOffset(-2),
      nextDue: createDateOffset(3),
    });

    const futureIncome = createIncome({
      rowId: 'future-income',
      excludeFromCalcs: false,
      nextDue: createDateOffset(4),
    });

    const overdueIncome = createIncome({
      rowId: 'overdue-income',
      excludeFromCalcs: false,
      nextDue: createDateOffset(-3),
    });

    const dueTodayIncome = createIncome({
      rowId: 'due-today-income',
      excludeFromCalcs: false,
      nextDue: createDateOffset(0),
    });

    const result = splitIncomesByActionability([
      excludedIncome,
      endedIncome,
      futureIncome,
      dueTodayIncome,
      overdueIncome,
    ]);

    expect(result.excludedIncomes).toEqual([excludedIncome]);
    expect(result.endedIncomes).toEqual([endedIncome]);
    expect(result.actionableIncomes).toEqual([
      futureIncome,
      dueTodayIncome,
      overdueIncome,
    ]);
    expect(result.futureIncomes).toEqual([futureIncome]);
    expect(result.dueTodayIncomes).toEqual([dueTodayIncome]);
    expect(result.overdueIncomes).toEqual([overdueIncome]);
  });
});
