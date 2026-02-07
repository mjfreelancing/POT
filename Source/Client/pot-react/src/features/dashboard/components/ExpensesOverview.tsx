import { ClockFading, DollarSign, ShoppingCart } from 'lucide-react';
import { useEffect, useMemo } from 'react';

import { useApiGetAllExpenses } from '@/api/hooks';
import { Skeleton } from '@/components/ui/skeleton';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { EMPTY_EXPENSE_ARRAY } from '@/data';
import { formatMoneyValue, localToday, normalizeToEpoch } from '@/lib';

import type { ExpensesSummary } from '../stores';
import { expensesSummaryStore } from '../stores';
import CollapsibleSection from './CollapsibleSection';
import CompactMetricsRow from './CompactMetricsRow';
import ExpenseCard from './ExpenseCard';

type FilteredExpenses = {
  count: number;
  total: number;
};

function filterExpenses(days: number, expenses: Expense[]): Expense[] {
  const todayEpoch = normalizeToEpoch(localToday());
  const targetDateEpoch = todayEpoch + days * 24 * 60 * 60 * 1000;

  return expenses.filter(expense => {
    // Exclude expenses marked as excluded from calculations
    if (expense.excludeFromCalcs) {
      return false;
    }

    const dueDateEpoch = normalizeToEpoch(expense.nextDue);
    return dueDateEpoch <= targetDateEpoch;
  });
}

function filteredExpenseInfo(
  days: number,
  expenses: Expense[],
): FilteredExpenses {
  const filtered = filterExpenses(days, expenses);

  return {
    count: filtered.length,
    total: filtered.reduce((sum, expense) => sum + expense.amount, 0),
  };
}

function ExpensesOverview() {
  useEffect(() => {
    logger.info('ExpensesOverview', 'Component mounted');

    return () => {
      logger.info('ExpensesOverview', 'Component unmounted');
    };
  }, []);

  const { data: expensesData, isLoading: expensesIsLoading } =
    useApiGetAllExpenses();

  const { error, setError } = useErrorContext();

  const expenses = useMemo(
    () =>
      expensesData?.success ? expensesData.value.results : EMPTY_EXPENSE_ARRAY,
    [expensesData],
  );

  const setSummary = expensesSummaryStore(
    (state: ExpensesSummary) => state.setSummary,
  );

  // Handle API errors - only set errors, never clear them (DashboardPage handles clearing)
  useEffect(() => {
    // Only evaluate when we have data (not undefined)
    if (expensesData !== undefined) {
      if (error === null && expensesData.success === false) {
        setError({
          title: expensesData.error.code,
          description: expensesData.error.description,
        });
      }
    }
  }, [expensesData, error, setError]);

  // Recalculate and update summary when expenses change
  useEffect(() => {
    const { count: dueIn7Days, total: totalNext7Days } = filteredExpenseInfo(
      7,
      expenses,
    );
    const { count: dueIn30Days, total: totalNext30Days } = filteredExpenseInfo(
      30,
      expenses,
    );

    setSummary(dueIn7Days, totalNext7Days, dueIn30Days, totalNext30Days);
  }, [expenses, setSummary]);

  const dueIn7Days = expensesSummaryStore(
    (state: ExpensesSummary) => state.dueIn7Days,
  );

  const totalNext7Days = expensesSummaryStore(
    (state: ExpensesSummary) => state.totalNext7Days,
  );

  const dueIn30Days = expensesSummaryStore(
    (state: ExpensesSummary) => state.dueIn30Days,
  );

  const totalNext30Days = expensesSummaryStore(
    (state: ExpensesSummary) => state.totalNext30Days,
  );

  const dueIn30DaysExpenses = useMemo(
    () => filterExpenses(30, expenses),
    [expenses],
  );

  const metricsData = useMemo(
    () => [
      {
        icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
        label: 'Due Next 7 Days',
        value: dueIn7Days.toString(),
        className: 'text-information',
      },
      {
        icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
        label: 'Total Next 7 Days',
        value: formatMoneyValue(totalNext7Days),
        className: 'text-information',
      },
      {
        icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
        label: 'Due Next 30 Days',
        value: dueIn30Days.toString(),
        className: 'text-information',
      },
      {
        icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
        label: 'Total Next 30 Days',
        value: formatMoneyValue(totalNext30Days),
        className: 'text-information',
      },
    ],
    [dueIn7Days, totalNext7Days, dueIn30Days, totalNext30Days],
  );

  return (
    <>
      <CollapsibleSection
        icon={<ShoppingCart className="h-5 w-5" aria-hidden="true" />}
        title="Expenses Overview"
        defaultOpen
      >
        <div className="space-y-3">
          {expensesIsLoading ? (
            <Skeleton className="h-[72px] w-full rounded-lg" />
          ) : (
            <CompactMetricsRow metrics={metricsData} />
          )}

          {expensesIsLoading ? (
            <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
              {Array(4)
                .fill(0)
                .map((_, i) => (
                  <Skeleton key={i} className="h-[140px] rounded-lg" />
                ))}
            </div>
          ) : (
            <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
              {dueIn30DaysExpenses.map(expense => (
                <ExpenseCard key={expense.rowId} expense={expense} />
              ))}
            </div>
          )}
        </div>
      </CollapsibleSection>
    </>
  );
}

export default ExpensesOverview;
