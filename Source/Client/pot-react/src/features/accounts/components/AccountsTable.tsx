import { useQueryClient } from '@tanstack/react-query';
import { ColumnDef } from '@tanstack/react-table';
import { BanknoteArrowDown, BanknoteArrowUp } from 'lucide-react';
import { useState } from 'react';
import { useParams } from 'react-router';

import { useApiAccrueExpenses } from '@/api/hooks';
import { ErrorSheet, StatusBadge } from '@/components/feedback';
import {
  BulkAction,
  createMoneyValueColumn,
  createRowIdGetter,
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
            <div className="flex gap-2 ml-2">
              {account.linkedExpenses > 0 && (
                <StatusBadge color="yellow">
                  <BanknoteArrowDown />
                  {account.linkedExpenses}
                </StatusBadge>
              )}
              {account.linkedIncomes > 0 && (
                <StatusBadge color="green">
                  <BanknoteArrowUp />
                  {account.linkedIncomes}
                </StatusBadge>
              )}
            </div>
          )}
        </div>
      );
    },
  },
  createMoneyValueColumn<Account>({
    accessorKey: 'balance',
    header: 'Balance',
  }),
  createMoneyValueColumn<Account>({
    accessorKey: 'reserved',
    header: 'Reserved',
  }),
  createMoneyValueColumn<Account>({
    accessorKey: 'totalExpenseAccrued',
    header: 'Total Accrued',
  }),
  createMoneyValueColumn<Account>({
    accessorKey: 'dailyExpenseAccrual',
    header: 'Daily Accrual',
  }),
  createMoneyValueColumn<Account>({
    accessorKey: 'available',
    header: 'Available',
  }),
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
            getRowId={createRowIdGetter<Account>()}
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
