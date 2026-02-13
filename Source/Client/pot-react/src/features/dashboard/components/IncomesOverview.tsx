import { ClockFading, DollarSign, Landmark } from 'lucide-react';
import { useEffect, useMemo } from 'react';

import { useApiGetAllIncomes } from '@/api/hooks';
import { Skeleton } from '@/components/ui/skeleton';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Income } from '@/data';
import { EMPTY_INCOME_ARRAY } from '@/data';
import { formatMoneyValue, localToday, normalizeToEpoch } from '@/lib';

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

  const dueIn30DaysIncomes = useMemo(
    () => filterIncomes(30, incomes),
    [incomes],
  );

  const metricsData = useMemo(
    () => [
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
    ],
    [dueIn7Days, totalNext7Days, dueIn30Days, totalNext30Days],
  );

  return (
    <>
      <div className="relative">
        <CollapsibleSection
          icon={<Landmark className="h-5 w-5" aria-hidden="true" />}
          title="Income Overview"
          isOpen={isOpen}
          onOpenChange={onOpenChange}
        >
          <div className="space-y-3">
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
                {dueIn30DaysIncomes.map(income => (
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
