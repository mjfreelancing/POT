import { useQueryClient } from '@tanstack/react-query';
import { ColumnDef } from '@tanstack/react-table';
import { Receipt, TrendingUp } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router';

import { useApiAccrueExpenses } from '@/api/hooks';
import { ErrorSheet } from '@/components/feedback';
import { StatusBadge } from '@/components/feedback/badge';
import {
  BulkAction,
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

type AccountsTableProps = {
  accounts: Account[];
};

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
  createMoneyValueColumn<Account>('totalExpenseAccrued', 'Total Accrued'),
  createMoneyValueColumn<Account>('dailyExpenseAccrual', 'Daily Accrual'),
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

function AccountsTable({ accounts }: AccountsTableProps) {
  const { id: editingId } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const accrueExpensesMutation = useApiAccrueExpenses();
  const [error, setError] = useState<DisplayError | null>(null);

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

    const totalAccrued = accounts.reduce(
      (sum, acct) => sum + acct.totalExpenseAccrued,
      0,
    );

    const totalDailyAccrual = accounts.reduce(
      (sum, acct) => sum + acct.dailyExpenseAccrual,
      0,
    );

    setSummary(totalBalance, totalReserved, totalAccrued, totalDailyAccrual);
  }, [accounts, setSummary]);

  const bulkActions: BulkAction<Account>[] = [
    {
      label: 'Accrue Expenses',
      onClick: async (selectedItems: Account[]) => {
        const rowIds = selectedItems.map(account => account.rowId);

        const result = await accrueExpensesMutation.mutateAsync({
          data: { rowIds },
        });

        if (result.success) {
          queryClient.invalidateQueries({ queryKey: ['accounts'] });
          queryClient.invalidateQueries({ queryKey: ['expenses'] });
        } else {
          setError({
            title: result.error.code,
            description: result.error.description,
          });
        }
      },
    },
  ];

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <Card className="card-elevated h-full flex flex-col">
        <CardContent className="px-4 py-4 flex-1 overflow-hidden">
          <DataTable
            columns={columns}
            data={accounts}
            enableRowSelection={true}
            bulkActions={bulkActions}
            highlightRowFilter={row =>
              row.original.rowId.toString() === editingId
            }
          />
        </CardContent>
      </Card>
    </>
  );
}

export default AccountsTable;
export type { AccountsTableProps };
