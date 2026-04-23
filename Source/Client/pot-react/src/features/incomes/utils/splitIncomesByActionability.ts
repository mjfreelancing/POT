import type { Income } from '@/data';
import { getDaysDue } from '@/lib';

type IncomeActionabilitySplit = {
  excludedIncomes: Income[];
  endedIncomes: Income[];
  actionableIncomes: Income[];
  futureIncomes: Income[];
  dueTodayIncomes: Income[];
  overdueIncomes: Income[];
};

function isEndedIncome(income: Income): boolean {
  if (!income.endDate) {
    return false;
  }

  return getDaysDue(income.endDate) < 0;
}

function splitIncomesByActionability(
  incomes: Income[],
): IncomeActionabilitySplit {
  const excludedIncomes = incomes.filter(income => income.excludeFromCalcs);
  const endedIncomes = incomes.filter(
    income => !income.excludeFromCalcs && isEndedIncome(income),
  );

  const actionableIncomes = incomes.filter(
    income => !income.excludeFromCalcs && !isEndedIncome(income),
  );

  const futureIncomes = actionableIncomes.filter(
    income => getDaysDue(income.nextDue) > 0,
  );

  const dueTodayIncomes = actionableIncomes.filter(
    income => getDaysDue(income.nextDue) === 0,
  );

  const overdueIncomes = actionableIncomes.filter(
    income => getDaysDue(income.nextDue) < 0,
  );

  return {
    excludedIncomes,
    endedIncomes,
    actionableIncomes,
    futureIncomes,
    dueTodayIncomes,
    overdueIncomes,
  };
}

export { splitIncomesByActionability };
