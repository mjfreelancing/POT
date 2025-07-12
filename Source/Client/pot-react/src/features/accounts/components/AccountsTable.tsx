import { useQueryClient } from '@tanstack/react-query';
import { ColumnDef } from '@tanstack/react-table';
import { Receipt, TrendingUp } from 'lucide-react';
import { useState } from 'react';
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

import { accrueAllExpenses } from '../utils/bulkActions';
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

      return (
        <div>
          <div className="text-sm text-muted-foreground">({bsb})</div>
          <span>{number}</span>
        </div>
      );
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

  const bulkActions: BulkAction<Account>[] = [
    {
      label: 'Accrue Expenses',
      onClick: async (selectedItems: Account[]) => {
        const result = await accrueAllExpenses(
          selectedItems,
          accrueExpensesMutation,
          queryClient,
        );

        if (!result.success) {
          setError(result.error);
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
      <Card className="card-elevated flex flex-col flex-1 min-h-0">
        <CardContent className="px-4 flex-1 min-h-0 flex flex-col">
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
