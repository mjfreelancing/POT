import { describe, expect, test } from 'vitest';

import { splitExpensesByActionability } from '@/features/expenses/utils/splitExpensesByActionability';
import { dateIsoFormat, localToday } from '@/lib';

import { createExpense } from '../../../shared/factories/expenseFactory';

function createDateOffset(daysFromToday: number): string {
  const date = localToday();
  date.setDate(date.getDate() + daysFromToday);

  return dateIsoFormat(date);
}

describe('splitExpensesByActionability', () => {
  test('classifies excluded expenses as non-actionable', () => {
    const excludedExpense = createExpense({
      rowId: 'excluded-expense',
      excludeFromCalcs: true,
      nextDue: createDateOffset(2),
    });

    const result = splitExpensesByActionability([excludedExpense]);

    expect(result.excludedExpenses).toEqual([excludedExpense]);
    expect(result.endedExpenses).toEqual([]);
    expect(result.actionableExpenses).toEqual([]);
    expect(result.futureExpenses).toEqual([]);
    expect(result.dueTodayExpenses).toEqual([]);
    expect(result.overdueExpenses).toEqual([]);
  });

  test('classifies ended expenses as non-actionable', () => {
    const endedExpense = createExpense({
      rowId: 'ended-expense',
      excludeFromCalcs: false,
      endDate: createDateOffset(-1),
      nextDue: createDateOffset(2),
    });

    const result = splitExpensesByActionability([endedExpense]);

    expect(result.excludedExpenses).toEqual([]);
    expect(result.endedExpenses).toEqual([endedExpense]);
    expect(result.actionableExpenses).toEqual([]);
    expect(result.futureExpenses).toEqual([]);
    expect(result.dueTodayExpenses).toEqual([]);
    expect(result.overdueExpenses).toEqual([]);
  });

  test('treats end date today as actionable and due today', () => {
    const endingTodayExpense = createExpense({
      rowId: 'ending-today-expense',
      excludeFromCalcs: false,
      endDate: createDateOffset(0),
      nextDue: createDateOffset(0),
    });

    const result = splitExpensesByActionability([endingTodayExpense]);

    expect(result.excludedExpenses).toEqual([]);
    expect(result.endedExpenses).toEqual([]);
    expect(result.actionableExpenses).toEqual([endingTodayExpense]);
    expect(result.futureExpenses).toEqual([]);
    expect(result.dueTodayExpenses).toEqual([endingTodayExpense]);
    expect(result.overdueExpenses).toEqual([]);
  });

  test('keeps mixed selection actionable when at least one item can be paid', () => {
    const excludedExpense = createExpense({
      rowId: 'excluded-expense',
      excludeFromCalcs: true,
      nextDue: createDateOffset(2),
    });

    const endedExpense = createExpense({
      rowId: 'ended-expense',
      excludeFromCalcs: false,
      endDate: createDateOffset(-2),
      nextDue: createDateOffset(3),
    });

    const futureExpense = createExpense({
      rowId: 'future-expense',
      excludeFromCalcs: false,
      nextDue: createDateOffset(4),
    });

    const overdueExpense = createExpense({
      rowId: 'overdue-expense',
      excludeFromCalcs: false,
      nextDue: createDateOffset(-3),
    });

    const dueTodayExpense = createExpense({
      rowId: 'due-today-expense',
      excludeFromCalcs: false,
      nextDue: createDateOffset(0),
    });

    const result = splitExpensesByActionability([
      excludedExpense,
      endedExpense,
      futureExpense,
      dueTodayExpense,
      overdueExpense,
    ]);

    expect(result.excludedExpenses).toEqual([excludedExpense]);
    expect(result.endedExpenses).toEqual([endedExpense]);
    expect(result.actionableExpenses).toEqual([
      futureExpense,
      dueTodayExpense,
      overdueExpense,
    ]);
    expect(result.futureExpenses).toEqual([futureExpense]);
    expect(result.dueTodayExpenses).toEqual([dueTodayExpense]);
    expect(result.overdueExpenses).toEqual([overdueExpense]);
  });
});
