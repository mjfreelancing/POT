import type { Expense } from '@/data';
import { getDaysDue } from '@/lib';

type ExpenseActionabilitySplit = {
  excludedExpenses: Expense[];
  actionableExpenses: Expense[];
  futureExpenses: Expense[];
  overdueExpenses: Expense[];
};

function splitExpensesByActionability(
  expenses: Expense[],
): ExpenseActionabilitySplit {
  const excludedExpenses = expenses.filter(expense => expense.excludeFromCalcs);
  const actionableExpenses = expenses.filter(
    expense => !expense.excludeFromCalcs,
  );

  const futureExpenses = actionableExpenses.filter(
    expense => getDaysDue(expense.nextDue) > 0,
  );

  const overdueExpenses = actionableExpenses.filter(
    expense => getDaysDue(expense.nextDue) <= 0,
  );

  return {
    excludedExpenses,
    actionableExpenses,
    futureExpenses,
    overdueExpenses,
  };
}

export { splitExpensesByActionability };
