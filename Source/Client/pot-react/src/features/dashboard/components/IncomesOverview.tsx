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

  // Calculate 7-day metrics
  const { count: dueIn7Days, total: totalNext7Days } = useMemo(
    () => filteredIncomeInfo(7, incomes),
    [incomes],
  );

  // Calculate selected period metrics dynamically (only if not 7 days)
  const selectedPeriodMetrics = useMemo(
    () =>
      selectedPeriod > 7 ? filteredIncomeInfo(selectedPeriod, incomes) : null,
    [selectedPeriod, incomes],
  );

  const filteredIncomes = useMemo(
    () => filterIncomes(selectedPeriod, incomes),
    [selectedPeriod, incomes],
  );

  const metricsData = useMemo(() => {
    // Always show 7 days metrics
    const metrics = [
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

    // Add selected period metrics if > 7 days
    if (selectedPeriodMetrics) {
      metrics.push(
        {
          icon: <ClockFading className="h-6 w-6" aria-hidden="true" />,
          label: `Due Next ${selectedPeriod} Days`,
          value: selectedPeriodMetrics.count.toString(),
          className: 'text-success',
        },
        {
          icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
          label: `Total Next ${selectedPeriod} Days`,
          value: formatMoneyValue(selectedPeriodMetrics.total),
          className: 'text-success',
        },
      );
    }

    return metrics;
  }, [selectedPeriod, dueIn7Days, totalNext7Days, selectedPeriodMetrics]);

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
            <div className="flex items-center gap-3 justify-start">
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

            {/* Metrics */}
            {incomesIsLoading ? (
              <Skeleton className="h-[72px] w-full rounded-lg" />
            ) : (
              <CompactMetricsRow metrics={metricsData} />
            )}

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
