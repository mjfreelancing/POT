import type { ColumnDef } from '@tanstack/react-table';
import { Banknote, Calendar, DollarSign, PieChart } from 'lucide-react';
import { useEffect, useMemo } from 'react';

import { useApiGetAllAccounts } from '@/api/hooks';
import StatusBadge from '@/components/feedback/badge/StatusBadge';
import { createMoneyValueColumn, DataTable } from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Account } from '@/data';
import { EMPTY_ACCOUNT_ARRAY } from '@/data';
import { formatMoneyValue } from '@/lib';

import type { AccountsSummary } from '../stores';
import { accountsSummaryStore } from '../stores';
import DashboardCardHeader from './DashboardCardHeader';
import SummaryCardsGrid from './SummaryCardsGrid';

const columns: ColumnDef<Account>[] = [
  {
    id: 'bsb_number',
    accessorKey: 'bsb_number',
    header: 'Account',
    cell: ({ row }) => {
      const { bsb, number, description } = row.original;
      return (
        <div className="min-w-[140px]">
          <div className="font-medium">{description}</div>
          <div className="text-sm text-muted-foreground">
            <div>({bsb})</div>
            <div>{number}</div>
          </div>
        </div>
      );
    },
  },
  createMoneyValueColumn<Account>({
    accessorKey: 'balance',
    header: 'Balance',
  }),
  createMoneyValueColumn<Account>({
    accessorKey: 'available',
    header: 'Available',
  }),
  {
    id: 'status',
    header: 'Status',
    cell: ({ row }) => {
      const { balance, available } = row.original;

      const getAccountStatus = () => {
        if (available < 0) return 'overdrawn';
        if (available < balance * 0.1) return 'low';
        return 'active';
      };

      const status = getAccountStatus();

      switch (status) {
        case 'overdrawn':
          return (
            <StatusBadge className="w-20" color="red" aria-role="status">
              Overdrawn
            </StatusBadge>
          );

        case 'low':
          return (
            <StatusBadge className="w-20" color="orange" aria-role="status">
              Low
            </StatusBadge>
          );

        case 'active':
          return (
            <StatusBadge className="w-20" color="green" aria-role="status">
              Healthy
            </StatusBadge>
          );

        default:
          throw new Error(`Unknown account status: ${status}`);
      }
    },
  },
];

function AccountsOverview() {
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

  return (
    <>
      <Card>
        <DashboardCardHeader
          icon={<PieChart className="h-5 w-5" aria-hidden="true" />}
          title="Accounts Overview"
          description="Your bank accounts at a glance"
        />
        <CardContent className="px-4 -mt-2 min-w-0">
          <div className="flex flex-col xl:flex-row gap-3 min-w-0">
            {accountsIsLoading ? (
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
                    title: 'Total Balance',
                    icon: (
                      <DollarSign
                        className={`h-6 w-6 ${getAmountClass(totalBalance)}`}
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div
                        className={`text-xl font-bold text-center ${getAmountClass(totalBalance)}`}
                      >
                        {formatMoneyValue(totalBalance)}
                      </div>
                    ),
                  },
                  {
                    title: 'Reserved Funds',
                    icon: (
                      <Banknote
                        className={`h-6 w-6 ${getAmountClass(totalReserved)}`}
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div
                        className={`text-xl font-bold text-center ${getAmountClass(totalReserved)}`}
                      >
                        {formatMoneyValue(totalReserved)}
                      </div>
                    ),
                  },
                  {
                    title: 'Total Available',
                    icon: (
                      <Banknote
                        className={`h-6 w-6 ${getAmountClass(totalAvailable)}`}
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div
                        className={`text-xl font-bold text-center ${getAmountClass(totalAvailable)}`}
                      >
                        {formatMoneyValue(totalAvailable)}
                      </div>
                    ),
                  },
                  {
                    title: 'Daily Accrual',
                    icon: (
                      <Calendar
                        className={`h-6 w-6 ${getAmountClass(totalDailyAccrual)}`}
                        aria-hidden="true"
                      />
                    ),
                    value: (
                      <div
                        className={`text-xl font-bold text-center ${getAmountClass(totalDailyAccrual)}`}
                      >
                        {formatMoneyValue(totalDailyAccrual)}
                      </div>
                    ),
                  },
                ]}
              />
            )}
            <div className="w-full xl:flex-1 min-w-0 h-[456px] sm:h-[348px] overflow-auto rounded-lg border">
              {accountsIsLoading ? (
                <Skeleton className="h-full w-full" />
              ) : (
                <DataTable columns={columns} data={accounts} />
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default AccountsOverview;
