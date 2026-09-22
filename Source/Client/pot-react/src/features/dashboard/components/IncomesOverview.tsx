import { Landmark } from 'lucide-react';
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
import { localToday, normalizeToEpoch } from '@/lib';

import type { PeriodDays } from '../hooks/useDashboardStorage';
import useDashboardStorage from '../hooks/useDashboardStorage';
import CollapsibleSection from './CollapsibleSection';
import IncomeCard from './IncomeCard';

type IncomesOverviewProps = {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
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

  const filteredIncomes = useMemo(
    () => filterIncomes(selectedPeriod, incomes),
    [selectedPeriod, incomes],
  );

  return (
    <>
      <CollapsibleSection
        icon={<Landmark className="h-5 w-5" aria-hidden="true" />}
        title="Incomes Overview"
        isOpen={isOpen}
        onOpenChange={onOpenChange}
      >
        <div className="space-y-5">
          {/* Period Filter */}
          <div className="flex items-center gap-3 justify-start">
            <span className="text-sm text-muted-foreground">Next Due:</span>
            <Select
              value={selectedPeriod.toString()}
              onValueChange={handlePeriodChange}
            >
              <SelectTrigger className="w-[180px] sm:w-[200px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="7">Within 7 Days</SelectItem>
                <SelectItem value="14">Within 14 Days</SelectItem>
                <SelectItem value="30">Within 30 Days</SelectItem>
              </SelectContent>
            </Select>
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
    </>
  );
}

export default IncomesOverview;
