import { ColumnDef } from '@tanstack/react-table';
import { Receipt, TrendingUp } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { StatusBadge } from '@/components/feedback/badge';
import {
  createMoneyValueColumn,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Account } from '@/data';
import { DisplayError } from '@/lib';

import accountsSummaryStore, {
  AccountsSummary,
} from '../stores/useAccountsSummary';
import AccountActions from './AccountActions';

const columns: ColumnDef<Account>[] = [
  {
    accessorKey: 'bsb_number',
    header: 'BSB / Number',
    cell: ({ row }) => {
      const { bsb, number } = row.original;
      return `(${bsb}) ${number}`;
    },
  },
  {
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
    cell: ({ row }) => {
      const account = row.original;
      const hasLinkedData =
        account.linkedExpenses > 0 || account.linkedIncomes > 0;

      return (
        <div className="flex items-center gap-2">
          <span>{account.description}</span>
          {hasLinkedData && (
            <div className="flex gap-1">
              {account.linkedExpenses > 0 && (
                <StatusBadge color="yellow">
                  <Receipt />
                  {account.linkedExpenses}
                </StatusBadge>
              )}
              {account.linkedIncomes > 0 && (
                <StatusBadge color="blue">
                  <TrendingUp />
                  {account.linkedIncomes}
                </StatusBadge>
              )}
            </div>
          )}
        </div>
      );
    },
  },
  createMoneyValueColumn<Account>('balance', 'Balance'),
  createMoneyValueColumn<Account>('reserved', 'Reserved'),
  createMoneyValueColumn<Account>('allocated', 'Allocated'),
  createMoneyValueColumn<Account>('dailyAccrual', 'Daily Accrual'),
  createMoneyValueColumn<Account>('available', 'Available'),
  {
    id: 'actions',
    cell: ({ row }) => {
      const account = row.original;
      return (
        <div className="flex justify-end">
          <AccountActions account={account} />
        </div>
      );
    },
  },
];

function AccountsTable() {
  const { id: editingId } = useParams<{ id: string }>();
  const { data: result, isLoading } = useApiGetAllAccounts();
  const [error, setError] = useState<DisplayError | null>(null);

  const setSummary = accountsSummaryStore(
    (state: AccountsSummary) => state.setSummary,
  );

  // Memoize 'accounts' so its array reference only changes when `result` changes.
  // Without this, each render would create a new array (even with identical data), causing the summary effect to rerun every time.
  const accounts = useMemo(() => {
    return result?.success ? result.value : [];
  }, [result]);

  // Recalculates and pushes summary; 'setSummary' is in deps to satisfy exhaustive-deps and use the latest setter
  useEffect(() => {
    const totalBalance = accounts.reduce((sum, acct) => sum + acct.balance, 0);

    const totalReserved = accounts.reduce(
      (sum, acct) => sum + acct.reserved,
      0,
    );

    const totalAllocated = accounts.reduce(
      (sum, acct) => sum + acct.allocated,
      0,
    );

    const totalDailyAccrual = accounts.reduce(
      (sum, acct) => sum + acct.dailyAccrual,
      0,
    );

    setSummary(totalBalance, totalReserved, totalAllocated, totalDailyAccrual);
  }, [accounts, setSummary]);

  useEffect(() => {
    // Transient error handling - resetting error state when success, such as after a network loss
    if (result) {
      setError(
        result.success
          ? null
          : {
              title: result.error.code,
              description: result.error.description,
            },
      );
    }
  }, [result]);

  return (
    <>
      <Card className="card-elevated">
        <CardContent className="px-4">
          <DataTable
            columns={columns}
            data={accounts}
            highlightRowFilter={row =>
              row.original.rowId.toString() === editingId
            }
          />
        </CardContent>
      </Card>
      <LoadingMessage isLoading={isLoading} />
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </>
  );
}

export default AccountsTable;
