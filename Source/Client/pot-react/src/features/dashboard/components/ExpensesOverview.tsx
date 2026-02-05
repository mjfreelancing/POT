import type { ColumnDef, Row } from '@tanstack/react-table';
import { ClockFading, DollarSign, ShoppingCart } from 'lucide-react';
import { useEffect, useMemo } from 'react';

import { useApiGetAllExpenses } from '@/api/hooks';
import StatusBadge from '@/components/feedback/badge/StatusBadge';
import {
  createDateColumn,
  createMoneyValueColumn,
  DataTable,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { EMPTY_EXPENSE_ARRAY } from '@/data';
import {
  formatMoneyValue,
  getAdornedExpenseDescription,
  getTableRowClassName,
  localToday,
  normalizeToEpoch,
} from '@/lib';

import type { ExpensesSummary } from '../stores';
import { expensesSummaryStore } from '../stores';
import DashboardCardHeader from './DashboardCardHeader';
import SummaryCardsGrid from './SummaryCardsGrid';

type FilteredExpenses = {
  count: number;
  total: number;
};

// Table columns for due in next 30 days
const columns: ColumnDef<Expense>[] = [
  {
    id: 'description',
    accessorKey: 'description',
    header: 'Description',
    cell: ({ row }) => getAdornedExpenseDescription(row),
  },
  createDateColumn<Expense>({
    accessorKey: 'nextDue',
    header: 'Next Due',
  }),
  createMoneyValueColumn<Expense>({
    accessorKey: 'amount',
    header: 'Amount',
  }),
  {
    id: 'daysDue',
    header: 'Days Due',
    cell: ({ row }) => {
      const days = getDaysDue(row.original.nextDue);
      let badge = null;

      if (days <= 0) {
        badge = (
          <StatusBadge
            color="red"
            className="w-20"
            aria-role="status"
            aria-live="polite"
          >
            {days === 0 ? 'Due Today' : 'Past Due'}
          </StatusBadge>
        );
      }

      if (days > 0 && days <= 7) {
        badge = (
          <StatusBadge color="orange" className="w-20" aria-role="status">
            Due Soon
          </StatusBadge>
        );
      }

      return (
        <div className="flex items-center gap-4">
          {days >= 0 && <span>{days}</span>}
          {badge}
        </div>
      );
    },
  },
];

function getDaysDue(nextDue: string): number {
  const todayEpoch = normalizeToEpoch(localToday());
  const dueDateEpoch = normalizeToEpoch(nextDue);

  // Calculate difference in milliseconds and convert to days
  const diffMs = dueDateEpoch - todayEpoch;
  return Math.floor(diffMs / (1000 * 60 * 60 * 24));
}

function filterExpenses(days: number, expenses: Expense[]): Expense[] {
  const todayEpoch = normalizeToEpoch(localToday());
  const targetDateEpoch = todayEpoch + days * 24 * 60 * 60 * 1000;

  return expenses.filter(expense => {
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

  return (
    <>
      <Card>
        <DashboardCardHeader
          icon={<ShoppingCart className="h-5 w-5" aria-hidden="true" />}
          title="Expenses Overview"
          description="Your expenses at a glance"
        />
        <CardContent className="px-4 -mt-2 min-w-0">
          <div className="flex flex-col xl:flex-row gap-3 min-w-0">
            {expensesIsLoading ? (
              <div className="grid grid-cols-2 gap-2 max-w-xl w-full">
                {Array(4)
                  .fill(0)
                  .map((_, i) => (
                    <Skeleton
                      key={i}
                      className="h-[145px] sm:h-[168px] rounded-lg"
                    />
                  ))}
              </div>
            ) : (
              <SummaryCardsGrid
                cards={[
                  {
                    title: 'Due Next 7 Days',
                    icon: (
                      <ClockFading
                        className="h-6 w-6 text-information"
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div className="text-xl font-bold text-information text-center">
                        {dueIn7Days}
                      </div>
                    ),
                  },
                  {
                    title: 'Total Next 7 Days',
                    icon: (
                      <DollarSign
                        className="h-6 w-6 text-information"
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div className="text-xl font-bold text-information text-center">
                        {formatMoneyValue(totalNext7Days)}
                      </div>
                    ),
                  },
                  {
                    title: 'Due Next 30 Days',
                    icon: (
                      <ClockFading
                        className="h-6 w-6 text-information"
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div className="text-xl font-bold text-information text-center">
                        {dueIn30Days}
                      </div>
                    ),
                  },
                  {
                    title: 'Total Next 30 Days',
                    icon: (
                      <DollarSign
                        className="h-6 w-6 text-information"
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div className="text-xl font-bold text-information text-center">
                        {formatMoneyValue(totalNext30Days)}
                      </div>
                    ),
                  },
                ]}
              />
            )}
            <div className="w-full xl:flex-1 min-w-0 h-[456px] sm:h-[348px] overflow-auto rounded-lg border">
              {expensesIsLoading ? (
                <Skeleton className="h-full w-full" />
              ) : (
                <DataTable
                  columns={columns}
                  data={dueIn30DaysExpenses}
                  getRowClassName={(row: Row<Expense>) =>
                    getTableRowClassName(row.original, { exclude: ['OVERDUE'] })
                  }
                />
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default ExpensesOverview;
