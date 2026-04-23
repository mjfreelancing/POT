import type { Expense } from '@/data';
import { getDaysDue } from '@/lib';

type ExpenseActionabilitySplit = {
  excludedExpenses: Expense[];
  endedExpenses: Expense[];
  actionableExpenses: Expense[];
  futureExpenses: Expense[];
  dueTodayExpenses: Expense[];
  overdueExpenses: Expense[];
};

function isEndedExpense(expense: Expense): boolean {
  if (!expense.endDate) {
    return false;
  }

  return getDaysDue(expense.endDate) < 0;
}

function splitExpensesByActionability(
  expenses: Expense[],
): ExpenseActionabilitySplit {
  const excludedExpenses = expenses.filter(expense => expense.excludeFromCalcs);
  const endedExpenses = expenses.filter(
    expense => !expense.excludeFromCalcs && isEndedExpense(expense),
  );

  const actionableExpenses = expenses.filter(
    expense => !expense.excludeFromCalcs && !isEndedExpense(expense),
  );

  const futureExpenses = actionableExpenses.filter(
    expense => getDaysDue(expense.nextDue) > 0,
  );

  const dueTodayExpenses = actionableExpenses.filter(
    expense => getDaysDue(expense.nextDue) === 0,
  );

  const overdueExpenses = actionableExpenses.filter(
    expense => getDaysDue(expense.nextDue) < 0,
  );

  return {
    excludedExpenses,
    endedExpenses,
    actionableExpenses,
    futureExpenses,
    dueTodayExpenses,
    overdueExpenses,
  };
}

export { splitExpensesByActionability };
