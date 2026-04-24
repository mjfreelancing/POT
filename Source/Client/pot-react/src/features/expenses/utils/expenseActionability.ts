import type { Expense } from '@/data';

import { splitExpensesByActionability } from './splitExpensesByActionability';

type SingleExpenseActionability = {
  canMarkAsPaid: boolean;
  isExcludedForAction: boolean;
  isEndedForAction: boolean;
  isDueTodayForAction: boolean;
  isOverdueForAction: boolean;
};

function shouldHideMarkAsPaidAction(expenses: Expense[]): boolean {
  if (expenses.length === 0) {
    return false;
  }

  const { actionableExpenses } = splitExpensesByActionability(expenses);

  return actionableExpenses.length === 0;
}

function getSingleExpenseActionability(
  expense: Expense,
): SingleExpenseActionability {
  const { excludedExpenses, endedExpenses, dueTodayExpenses, overdueExpenses } =
    splitExpensesByActionability([expense]);

  const isExcludedForAction = excludedExpenses.length > 0;
  const isEndedForAction = endedExpenses.length > 0;
  const isDueTodayForAction = dueTodayExpenses.length > 0;
  const isOverdueForAction = overdueExpenses.length > 0;

  return {
    canMarkAsPaid: !isExcludedForAction && !isEndedForAction,
    isExcludedForAction,
    isEndedForAction,
    isDueTodayForAction,
    isOverdueForAction,
  };
}

export { getSingleExpenseActionability, shouldHideMarkAsPaidAction };
export type { SingleExpenseActionability };
