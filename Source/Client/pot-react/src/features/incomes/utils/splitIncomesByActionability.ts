import type { Income } from '@/data';
import { getDaysDue } from '@/lib';

type IncomeActionabilitySplit = {
  excludedIncomes: Income[];
  actionableIncomes: Income[];
  futureIncomes: Income[];
  overdueIncomes: Income[];
};

function splitIncomesByActionability(
  incomes: Income[],
): IncomeActionabilitySplit {
  const excludedIncomes = incomes.filter(income => income.excludeFromCalcs);
  const actionableIncomes = incomes.filter(income => !income.excludeFromCalcs);

  const futureIncomes = actionableIncomes.filter(
    income => getDaysDue(income.nextDue) > 0,
  );

  const overdueIncomes = actionableIncomes.filter(
    income => getDaysDue(income.nextDue) <= 0,
  );

  return {
    excludedIncomes,
    actionableIncomes,
    futureIncomes,
    overdueIncomes,
  };
}

export { splitIncomesByActionability };
