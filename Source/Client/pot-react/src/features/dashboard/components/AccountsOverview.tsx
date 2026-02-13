import { Banknote, Calendar, DollarSign, PieChart } from 'lucide-react';
import { useEffect, useMemo } from 'react';

import { useApiGetAllAccounts } from '@/api/hooks';
import { Skeleton } from '@/components/ui/skeleton';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import { EMPTY_ACCOUNT_ARRAY } from '@/data';
import { formatMoneyValue } from '@/lib';

import type { AccountsSummary } from '../stores';
import { accountsSummaryStore } from '../stores';
import AccountCard from './AccountCard';
import CollapsibleSection from './CollapsibleSection';
import CompactMetricsRow from './CompactMetricsRow';

type AccountsOverviewProps = {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
};

function AccountsOverview({ isOpen, onOpenChange }: AccountsOverviewProps) {
  useEffect(() => {
    logger.info('AccountsOverview', 'Component mounted');

    return () => {
      logger.info('AccountsOverview', 'Component unmounted');
    };
  }, []);

  const { data: accountsData, isLoading: accountsIsLoading } =
    useApiGetAllAccounts();

  const { error, setError } = useErrorContext();

  const accounts = useMemo(
    () => (accountsData?.success ? accountsData.value : EMPTY_ACCOUNT_ARRAY),
    [accountsData],
  );

  const setSummary = accountsSummaryStore(
    (state: AccountsSummary) => state.setSummary,
  );

  // Handle API errors - only set errors, never clear them (DashboardPage handles clearing)
  useEffect(() => {
    // Only evaluate when we have data (not undefined)
    if (accountsData !== undefined) {
      if (error === null && accountsData.success === false) {
        setError({
          title: accountsData.error.code,
          description: accountsData.error.description,
        });
      }
    }
  }, [accountsData, error, setError]);

  // Recalculate and update summary when accounts change
  useEffect(() => {
    const totalBalance = accounts.reduce((sum, acct) => sum + acct.balance, 0);

    const totalReserved = accounts.reduce(
      (sum, acct) => sum + acct.reserved,
      0,
    );

    const totalAvailable = accounts.reduce(
      (sum, acct) => sum + acct.available,
      0,
    );

    const totalDailyAccrual = accounts.reduce(
      (sum, acct) => sum + acct.dailyExpenseAccrual,
      0,
    );

    setSummary(totalBalance, totalReserved, totalAvailable, totalDailyAccrual);
  }, [accounts, setSummary]);

  const totalBalance = accountsSummaryStore(
    (state: AccountsSummary) => state.totalBalance,
  );

  const totalReserved = accountsSummaryStore(
    (state: AccountsSummary) => state.totalReserved,
  );

  const totalAvailable = accountsSummaryStore(
    (state: AccountsSummary) => state.totalAvailable,
  );

  const totalDailyAccrual = accountsSummaryStore(
    (state: AccountsSummary) => state.totalDailyAccrual,
  );

  function getAmountClass(amount: number): string {
    if (amount >= 0) {
      return 'text-success';
    }

    return 'text-destructive-high-contrast';
  }

  const metricsData = useMemo(
    () => [
      {
        icon: <DollarSign className="h-6 w-6" aria-hidden="true" />,
        label: 'Total Balance',
        value: formatMoneyValue(totalBalance),
        className: getAmountClass(totalBalance),
      },
      {
        icon: <Banknote className="h-6 w-6" aria-hidden="true" />,
        label: 'Reserved Funds',
        value: formatMoneyValue(totalReserved),
        className: getAmountClass(totalReserved),
      },
      {
        icon: <Banknote className="h-6 w-6" aria-hidden="true" />,
        label: 'Total Available',
        value: formatMoneyValue(totalAvailable),
        className: getAmountClass(totalAvailable),
      },
      {
        icon: <Calendar className="h-6 w-6" aria-hidden="true" />,
        label: 'Daily Accrual',
        value: formatMoneyValue(totalDailyAccrual),
        className: getAmountClass(totalDailyAccrual),
      },
    ],
    [totalBalance, totalReserved, totalAvailable, totalDailyAccrual],
  );

  return (
    <>
      <CollapsibleSection
        icon={<PieChart className="h-5 w-5" aria-hidden="true" />}
        title="Accounts Overview"
        isOpen={isOpen}
        onOpenChange={onOpenChange}
      >
        <div className="space-y-3">
          {accountsIsLoading ? (
            <Skeleton className="h-[72px] w-full rounded-lg" />
          ) : (
            <CompactMetricsRow metrics={metricsData} />
          )}

          {accountsIsLoading ? (
            <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
              {Array(4)
                .fill(0)
                .map((_, i) => (
                  <Skeleton key={i} className="h-[140px] rounded-lg" />
                ))}
            </div>
          ) : (
            <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
              {accounts.map(account => (
                <AccountCard key={account.rowId} account={account} />
              ))}
            </div>
          )}
        </div>
      </CollapsibleSection>
    </>
  );
}

export default AccountsOverview;
