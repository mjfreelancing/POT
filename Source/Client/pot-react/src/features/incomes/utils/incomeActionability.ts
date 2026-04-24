import type { Income } from '@/data';

import { splitIncomesByActionability } from './splitIncomesByActionability';

type SingleIncomeActionability = {
  canMarkAsReceived: boolean;
  isExcludedForAction: boolean;
  isEndedForAction: boolean;
  isDueTodayForAction: boolean;
  isOverdueForAction: boolean;
};

function shouldHideMarkAsReceivedAction(incomes: Income[]): boolean {
  if (incomes.length === 0) {
    return false;
  }

  const { actionableIncomes } = splitIncomesByActionability(incomes);

  return actionableIncomes.length === 0;
}

function getSingleIncomeActionability(
  income: Income,
): SingleIncomeActionability {
  const { excludedIncomes, endedIncomes, dueTodayIncomes, overdueIncomes } =
    splitIncomesByActionability([income]);

  const isExcludedForAction = excludedIncomes.length > 0;
  const isEndedForAction = endedIncomes.length > 0;
  const isDueTodayForAction = dueTodayIncomes.length > 0;
  const isOverdueForAction = overdueIncomes.length > 0;

  return {
    canMarkAsReceived: !isExcludedForAction && !isEndedForAction,
    isExcludedForAction,
    isEndedForAction,
    isDueTodayForAction,
    isOverdueForAction,
  };
}

export { getSingleIncomeActionability, shouldHideMarkAsReceivedAction };
export type { SingleIncomeActionability };
