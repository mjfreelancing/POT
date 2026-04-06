import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef } from '@tanstack/react-table';
import { BanknoteArrowDown, BanknoteArrowUp } from 'lucide-react';
import { useNavigate, useParams } from 'react-router';

import { useApiAccrueAccountExpenses } from '@/api/hooks';
import { ErrorSheet, StatusBadge } from '@/components/feedback';
import type { BulkAction } from '@/components/table';
import {
  createActionsColumn,
  createMoneyValueColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { Account } from '@/data';
import { usePermissions } from '@/hooks';

import { accrueAllAccountExpenses } from '../utils/bulkActions';
import AccountActions from './AccountActions';

type AccountsTableProps = {
  accounts: Account[];
};

function AccountsTable({ accounts }: AccountsTableProps) {
  const { id: editingId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const accrueExpensesMutation = useApiAccrueAccountExpenses();
  const { error, setError } = useErrorContext();

  const { hasPermission } = usePermissions();
  const canManageExpenses = hasPermission('expense:manage');

  const columns: ColumnDef<Account>[] = [
    {
      id: 'bsb_number',
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
      id: 'description',
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
            {account.description}
            {hasLinkedData && (
              <div className="flex gap-2 ml-2">
                {account.linkedExpenses > 0 && (
                  <StatusBadge
                    color="yellow"
                    tooltip={`View ${account.linkedExpenses} linked ${account.linkedExpenses === 1 ? 'expense' : 'expenses'}`}
                    onClick={() =>
                      navigate(`/expenses?accountId=${account.rowId}`)
                    }
                    className="cursor-pointer hover:opacity-80 transition-opacity"
                  >
                    <BanknoteArrowDown />
                    {account.linkedExpenses}
                  </StatusBadge>
                )}
                {account.linkedIncomes > 0 && (
                  <StatusBadge
                    color="green"
                    tooltip={`View ${account.linkedIncomes} linked ${account.linkedIncomes === 1 ? 'income' : 'incomes'}`}
                    onClick={() =>
                      navigate(`/incomes?accountId=${account.rowId}`)
                    }
                    className="cursor-pointer hover:opacity-80 transition-opacity"
                  >
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
      options: {
        enableSorting: true,
        sortingFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'reserved',
      header: 'Reserved',
      options: {
        enableSorting: true,
        sortingFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'totalExpenseAccrued',
      header: 'Total Accrued',
      options: {
        enableSorting: true,
        sortingFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'dailyExpenseAccrual',
      header: 'Daily Accrual',
      options: {
        enableSorting: true,
        sortingFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'available',
      header: 'Available',
      options: {
        enableSorting: true,
        sortingFn: 'basic',
      },
    }),
    createActionsColumn<Account>(account => (
      <AccountActions account={account} />
    )),
  ];

  const bulkActions: BulkAction<Account>[] = [
    {
      label: 'Accrue Expenses',
      isDisabled: !canManageExpenses,
      onClick: async (selectedItems: Account[]) => {
        const accountRowIds = selectedItems.map(item => item.rowId);

        const result = await accrueAllAccountExpenses(
          accountRowIds,
          accrueExpensesMutation,
          queryClient,
        );

        if (!result.success) {
          setError(result.error);
        }
      },
      clearSelectionOnComplete: true,
    },
  ];

  // Enable multi-select only if user has permission for any bulk action
  // (isDisabled is based on permissions within each bulk action)
  const hasAnyBulkPermission = bulkActions.some(action => !action.isDisabled);

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
            enableRowSelection={hasAnyBulkPermission}
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
