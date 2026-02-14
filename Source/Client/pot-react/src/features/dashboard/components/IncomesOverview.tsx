import { ClockFading, DollarSign, Landmark } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';

import { useApiGetAllIncomes } from '@/api/hooks';
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
import type { Income } from '@/data';
import { EMPTY_INCOME_ARRAY } from '@/data';
import { formatMoneyValue, localToday, normalizeToEpoch } from '@/lib';

import type { PeriodDays } from '../hooks/useDashboardStorage';
import useDashboardStorage from '../hooks/useDashboardStorage';
import type { IncomesSummary } from '../stores';
import { incomesSummaryStore } from '../stores';
import CollapsibleSection from './CollapsibleSection';
import CompactMetricsRow from './CompactMetricsRow';
import IncomeCard from './IncomeCard';

type IncomesOverviewProps = {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
};

type FilteredIncomes = {
  count: number;
  total: number;
};

function filterIncomes(days: number, incomes: Income[]): Income[] {
  const todayEpoch = normalizeToEpoch(localToday());
  const targetDateEpoch = todayEpoch + days * 24 * 60 * 60 * 1000;

  return incomes.filter(income => {
    // Exclude incomes marked as excluded from calculations
    if (income.excludeFromCalcs) {
      return false;
    }

    const dueDateEpoch = normalizeToEpoch(income.nextDue);
    return dueDateEpoch <= targetDateEpoch;
  });
}

function filteredIncomeInfo(days: number, incomes: Income[]): FilteredIncomes {
  const filtered = filterIncomes(days, incomes);

  return {
    count: filtered.length,
    total: filtered.reduce((sum, income) => sum + income.amount, 0),
  };
}

function IncomesOverview({ isOpen, onOpenChange }: IncomesOverviewProps) {
  useEffect(() => {
    logger.info('IncomesOverview', 'Component mounted');

    return () => {
      logger.info('IncomesOverview', 'Component unmounted');
    };
  }, []);

  const { getDashboardData, setDashboardData } = useDashboardStorage();
  const [selectedPeriod, setSelectedPeriod] = useState<PeriodDays>(
    getDashboardData().incomesPeriod,
  );

  // Update localStorage when period changes
  const handlePeriodChange = (value: string) => {
    const period = Number(value) as PeriodDays;
    setSelectedPeriod(period);
    setDashboardData({ incomesPeriod: period });
    logger.info('IncomesOverview', `Period changed to ${period} days`);
  };

  const { data: incomesData, isLoading: incomesIsLoading } =
    useApiGetAllIncomes();

  const { error, setError } = useErrorContext();

  const incomes = useMemo(
    () => (incomesData?.success ? incomesData.value : EMPTY_INCOME_ARRAY),
    [incomesData],
  );

  const setSummary = incomesSummaryStore(
    (state: IncomesSummary) => state.setSummary,
  );

  // Handle API errors - only set errors, never clear them (DashboardPage handles clearing)
  useEffect(() => {
    // Only evaluate when we have data (not undefined)
    if (incomesData !== undefined) {
      if (error === null && incomesData.success === false) {
        setError({
          title: incomesData.error.code,
          description: incomesData.error.description,
        });
      }
    }
  }, [incomesData, error, setError]);

  // Recalculate and update summary when incomes change
  useEffect(() => {
    const { count: dueIn7Days, total: totalNext7Days } = filteredIncomeInfo(
      7,
      incomes,
    );
    const { count: dueIn30Days, total: totalNext30Days } = filteredIncomeInfo(
      30,
      incomes,
    );

    setSummary(dueIn7Days, totalNext7Days, dueIn30Days, totalNext30Days);
  }, [incomes, setSummary]);

  const dueIn7Days = incomesSummaryStore(
    (state: IncomesSummary) => state.dueIn7Days,
  );

  const totalNext7Days = incomesSummaryStore(
    (state: IncomesSummary) => state.totalNext7Days,
  );

  const dueIn30Days = incomesSummaryStore(
    (state: IncomesSummary) => state.dueIn30Days,
  );

  const totalNext30Days = incomesSummaryStore(
    (state: IncomesSummary) => state.totalNext30Days,
  );

  // Calculate 14-day metrics for dynamic display
  const { count: dueIn14Days, total: totalNext14Days } = useMemo(
    () => filteredIncomeInfo(14, incomes),
    [incomes],
  );

  const filteredIncomes = useMemo(
    () => filterIncomes(selectedPeriod, incomes),
    [selectedPeriod, incomes],
  );

  const metricsData = useMemo(() => {
    // Dynamic metrics based on selected period
    if (selectedPeriod === 7) {
      return [
        {
          icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
          label: 'Due Next 7 Days',
          value: dueIn7Days.toString(),
          className: 'text-success',
        },
        {
          icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
          label: 'Total Next 7 Days',
          value: formatMoneyValue(totalNext7Days),
          className: 'text-success',
        },
      ];
    } else if (selectedPeriod === 14) {
      return [
        {
          icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
          label: 'Due Next 7 Days',
          value: dueIn7Days.toString(),
          className: 'text-success',
        },
        {
          icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
          label: 'Total Next 7 Days',
          value: formatMoneyValue(totalNext7Days),
          className: 'text-success',
        },
        {
          icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
          label: 'Due Next 14 Days',
          value: dueIn14Days.toString(),
          className: 'text-success',
        },
        {
          icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
          label: 'Total Next 14 Days',
          value: formatMoneyValue(totalNext14Days),
          className: 'text-success',
        },
      ];
    } else {
      // selectedPeriod === 30
      return [
        {
          icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
          label: 'Due Next 7 Days',
          value: dueIn7Days.toString(),
          className: 'text-success',
        },
        {
          icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
          label: 'Total Next 7 Days',
          value: formatMoneyValue(totalNext7Days),
          className: 'text-success',
        },
        {
          icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
          label: 'Due Next 30 Days',
          value: dueIn30Days.toString(),
          className: 'text-success',
        },
        {
          icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
          label: 'Total Next 30 Days',
          value: formatMoneyValue(totalNext30Days),
          className: 'text-success',
        },
      ];
    }
  }, [
    selectedPeriod,
    dueIn7Days,
    totalNext7Days,
    dueIn14Days,
    totalNext14Days,
    dueIn30Days,
    totalNext30Days,
  ]);

  return (
    <>
      <div className="relative">
        <CollapsibleSection
          icon={<Landmark className="h-5 w-5" aria-hidden="true" />}
          title="Income Overview"
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
            <div className="bg-green-100/60 dark:bg-green-950/40 p-4 rounded-lg border border-green-200/50 dark:border-green-900/50">
              {incomesIsLoading ? (
                <Skeleton className="h-[72px] w-full rounded-lg" />
              ) : (
                <CompactMetricsRow metrics={metricsData} />
              )}
            </div>

            {incomesIsLoading ? (
              <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
                {Array(4)
                  .fill(0)
                  .map((_, i) => (
                    <Skeleton key={i} className="h-[140px] rounded-lg" />
                  ))}
              </div>
            ) : (
              <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
                {filteredIncomes.map(income => (
                  <IncomeCard key={income.rowId} income={income} />
                ))}
              </div>
            )}
          </div>
        </CollapsibleSection>
      </div>
    </>
  );
}

export default IncomesOverview;
