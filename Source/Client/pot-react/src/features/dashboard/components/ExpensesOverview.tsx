import { ClockFading, DollarSign, ShoppingCart } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';

import { useApiGetAllExpenses } from '@/api/hooks';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Skeleton } from '@/components/ui/skeleton';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { EMPTY_EXPENSE_ARRAY } from '@/data';
import { formatMoneyValue, localToday, normalizeToEpoch } from '@/lib';

import type { PeriodDays } from '../hooks/useDashboardStorage';
import useDashboardStorage from '../hooks/useDashboardStorage';
import CollapsibleSection from './CollapsibleSection';
import CompactMetricsRow from './CompactMetricsRow';
import ExpenseCard from './ExpenseCard';

type ExpensesOverviewProps = {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
};

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

function ExpensesOverview({ isOpen, onOpenChange }: ExpensesOverviewProps) {
  useEffect(() => {
    logger.info('ExpensesOverview', 'Component mounted');

    return () => {
      logger.info('ExpensesOverview', 'Component unmounted');
    };
  }, []);

  const { getDashboardData, setDashboardData } = useDashboardStorage();
  const [selectedPeriod, setSelectedPeriod] = useState<PeriodDays>(
    getDashboardData().expensesPeriod,
  );

  // Update localStorage when period changes
  const handlePeriodChange = (value: string) => {
    const period = Number(value) as PeriodDays;

    setSelectedPeriod(period);
    setDashboardData({ expensesPeriod: period });

    logger.info('ExpensesOverview', `Period changed to ${period} days`);
  };

  const { data: expensesData, isLoading: expensesIsLoading } =
    useApiGetAllExpenses();

  const { error, setError } = useErrorContext();

  const expenses = useMemo(
    () => (expensesData?.success ? expensesData.value : EMPTY_EXPENSE_ARRAY),
    [expensesData],
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

  // Calculate 7-day metrics
  const { count: dueIn7Days, total: totalNext7Days } = useMemo(
    () => filteredExpenseInfo(7, expenses),
    [expenses],
  );

  // Calculate selected period metrics dynamically (only if not 7 days)
  const selectedPeriodMetrics = useMemo(
    () =>
      selectedPeriod > 7 ? filteredExpenseInfo(selectedPeriod, expenses) : null,
    [selectedPeriod, expenses],
  );

  const filteredExpenses = useMemo(
    () => filterExpenses(selectedPeriod, expenses),
    [selectedPeriod, expenses],
  );

  const metricsData = useMemo(() => {
    // Always show 7 days metrics
    const metrics = [
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
    ];

    // Add selected period metrics if > 7 days
    if (selectedPeriodMetrics) {
      metrics.push(
        {
          icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
          label: `Due Next ${selectedPeriod} Days`,
          value: selectedPeriodMetrics.count.toString(),
          className: 'text-information',
        },
        {
          icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
          label: `Total Next ${selectedPeriod} Days`,
          value: formatMoneyValue(selectedPeriodMetrics.total),
          className: 'text-information',
        },
      );
    }

    return metrics;
  }, [selectedPeriod, dueIn7Days, totalNext7Days, selectedPeriodMetrics]);

  return (
    <>
      <CollapsibleSection
        icon={<ShoppingCart className="h-5 w-5" aria-hidden="true" />}
        title="Expenses Overview"
        isOpen={isOpen}
        onOpenChange={onOpenChange}
      >
        <div className="space-y-5">
          {/* Period Filter */}
          <div className="flex items-center gap-3 justify-center sm:justify-end">
            <span className="text-sm text-muted-foreground">Show:</span>
            <Select
              value={selectedPeriod.toString()}
              onValueChange={handlePeriodChange}
            >
              <SelectTrigger className="w-[180px] sm:w-[200px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="7">Next 7 Days</SelectItem>
                <SelectItem value="14">Next 14 Days</SelectItem>
                <SelectItem value="30">Next 30 Days</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Metrics with background container */}
          <div className="bg-blue-100/60 dark:bg-blue-950/40 p-4 rounded-lg border border-blue-200/50 dark:border-blue-900/50">
            {expensesIsLoading ? (
              <Skeleton className="h-[72px] w-full rounded-lg" />
            ) : (
              <CompactMetricsRow metrics={metricsData} />
            )}
          </div>

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
              {filteredExpenses.map(expense => (
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
