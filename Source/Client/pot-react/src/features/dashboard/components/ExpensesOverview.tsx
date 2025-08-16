import { ColumnDef, Row } from '@tanstack/react-table';
import { ClockFading, DollarSign, ShoppingCart } from 'lucide-react';
import { useEffect, useMemo } from 'react';

import StatusBadge from '@/components/feedback/badge/StatusBadge';
import {
  createDateColumn,
  createMoneyValueColumn,
  DataTable,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Expense } from '@/data';
import {
  dayOfYear,
  formatMoneyValue,
  getAdornedExpenseDescription,
  getTableRowClassName,
  localToday,
} from '@/lib';

import { useDashboardContext } from '../context';
import expensesSummaryStore, {
  ExpensesSummary,
} from '../stores/useExpensesSummary';
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
  const today = localToday();
  const dueDate = new Date(nextDue);

  return dayOfYear(dueDate) - dayOfYear(today);
}

function filterExpenses(days: number, expenses: Expense[]): Expense[] {
  const today = localToday();
  const inDays = new Date(today);
  inDays.setDate(today.getDate() + days);

  return expenses.filter(expense => {
    const dueDate = new Date(expense.nextDue);
    return dueDate <= inDays;
  });
}

function filteredExpenseInfo(
  days: number,
  expenses: Expense[],
): FilteredExpenses {
  const today = localToday();
  const inDays = new Date(today);
  inDays.setDate(today.getDate() + days);

  const filtered = filterExpenses(days, expenses);

  return {
    count: filtered.length,
    total: filtered.reduce((sum, expense) => sum + expense.amount, 0),
  };
}

function ExpensesOverview() {
  const { expenses, expensesIsLoading } = useDashboardContext();

  const setSummary = expensesSummaryStore(
    (state: ExpensesSummary) => state.setSummary,
  );

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
        <CardContent className="px-4 -mt-2">
          <div className="flex flex-col xl:flex-row gap-6">
            <div className="flex-1 w-full max-w-2xl">
              {expensesIsLoading ? (
                <div className="grid grid-cols-2 gap-4">
                  {Array(4)
                    .fill(0)
                    .map((_, i) => (
                      <Skeleton key={i} className="h-[120px] rounded-lg" />
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
                        <div className="text-xl font-bold text-information">
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
                        <div className="text-xl font-bold text-information">
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
                        <div className="text-xl font-bold text-information">
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
                        <div className="text-xl font-bold text-information">
                          {formatMoneyValue(totalNext30Days)}
                        </div>
                      ),
                    },
                  ]}
                />
              )}
            </div>
            <div className="flex-1 w-full min-w-0 flex flex-col min-h-0 h-[292px]">
              {expensesIsLoading ? (
                <Skeleton className="h-[256px] w-full rounded-lg" />
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
