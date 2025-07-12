import { ColumnDef } from '@tanstack/react-table';
import { Banknote, Calendar, DollarSign, PieChart, Wallet } from 'lucide-react';
import { useEffect } from 'react';

import { ActionCard } from '@/components/cards';
import StatusBadge from '@/components/feedback/badge/StatusBadge';
import { createMoneyValueColumn, DataTable } from '@/components/table';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Account } from '@/data';
import { formatMoneyValue } from '@/lib';

import accountsSummaryStore, {
  AccountsSummary,
} from '../stores/useAccountsSummary';

const columns: ColumnDef<Account>[] = [
  {
    accessorKey: 'bsb_number',
    header: 'Account',
    cell: ({ row }) => {
      const { bsb, number, description } = row.original;
      return (
        <div>
          <div className="font-medium">{description}</div>
          <div className="text-sm text-muted-foreground">
            ({bsb}) {number}
          </div>
        </div>
      );
    },
  },
  createMoneyValueColumn<Account>('balance', 'Balance'),
  createMoneyValueColumn<Account>('available', 'Available'),
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
            <StatusBadge className="w-20" color="red">
              Overdrawn
            </StatusBadge>
          );

        case 'low':
          return (
            <StatusBadge className="w-20" color="orange">
              Low
            </StatusBadge>
          );

        case 'active':
          return (
            <StatusBadge className="w-20" color="green">
              Healthy
            </StatusBadge>
          );

        default:
          throw new Error(`Unknown account status: ${status}`);
      }
    },
  },
];

type AccountsOverviewProps = {
  accounts: Account[];
};

function AccountsOverview({ accounts }: AccountsOverviewProps) {
  const setSummary = accountsSummaryStore(
    (state: AccountsSummary) => state.setSummary,
  );

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

    return 'text-destructive';
  }

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <div className="p-2 rounded-lg bg-primary/10 text-primary">
              <PieChart className="h-5 w-5" />
            </div>
            <div>
              <CardTitle>Accounts Overview</CardTitle>
              <CardDescription className="mt-1">
                Your bank accounts at a glance
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent className="px-4 -mt-2">
          <div className="flex flex-col xl:flex-row gap-6">
            <div className="flex-1 w-full max-w-2xl">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ActionCard
                  title="Total Balance"
                  icon={
                    <Wallet
                      className={`h-6 w-6 ${getAmountClass(totalBalance)}`}
                    />
                  }
                >
                  <div
                    className={`text-xl font-bold ${getAmountClass(totalBalance)}`}
                  >
                    {formatMoneyValue(totalBalance)}
                  </div>
                </ActionCard>
                <ActionCard
                  title="Reserved Funds"
                  icon={
                    <DollarSign
                      className={`h-6 w-6 ${getAmountClass(totalReserved)}`}
                    />
                  }
                >
                  <div
                    className={`text-xl font-bold ${getAmountClass(totalReserved)}`}
                  >
                    {formatMoneyValue(totalReserved)}
                  </div>
                </ActionCard>
                <ActionCard
                  title="Total Available"
                  icon={
                    <Banknote
                      className={`h-6 w-6 ${getAmountClass(totalAvailable)}`}
                    />
                  }
                >
                  <div
                    className={`text-xl font-bold ${getAmountClass(totalAvailable)}`}
                  >
                    {formatMoneyValue(totalAvailable)}
                  </div>
                </ActionCard>
                <ActionCard
                  title="Daily Accrual"
                  icon={
                    <Calendar
                      className={`h-6 w-6 ${getAmountClass(totalDailyAccrual)}`}
                    />
                  }
                >
                  <div
                    className={`text-xl font-bold ${getAmountClass(totalDailyAccrual)}`}
                  >
                    {formatMoneyValue(totalDailyAccrual)}
                  </div>
                </ActionCard>
              </div>
            </div>
            <div className="flex-1 w-full min-w-0 flex flex-col min-h-0 h-[292px]">
              <DataTable columns={columns} data={accounts} />
            </div>
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default AccountsOverview;
export type { AccountsOverviewProps };
