import { useQueryClient } from '@tanstack/react-query';
import { ColumnDef, Row } from '@tanstack/react-table';
import { useState } from 'react';
import { useParams } from 'react-router';

import {
  useApiRenewExpenses,
  useApiToggleExcludeExpenses,
} from '@/api/hooks/useExpenses';
import { usePermissions } from '@/features/auth/usePermissions';
import { ErrorSheet } from '@/components/feedback';
import {
  BulkAction,
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Expense } from '@/data';
import {
  DisplayError,
  Frequency,
  getAdornedExpenseDescription,
  getTableRowClassName,
} from '@/lib';

import { renewExpenses, toggleExcludeExpenses } from '../bulkActions';
import ExpenseActions from './ExpenseActions';

const columns: ColumnDef<Expense>[] = [
  {
    id: 'description',
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
    cell: ({ row }) => getAdornedExpenseDescription(row),
  },
  createMoneyValueColumn<Expense>({
    accessorKey: 'amount',
    header: 'Amount',
    options: {
      enableSorting: true,
      sortingFn: 'basic',
    },
  }),
  createDateColumn<Expense>({
    accessorKey: 'nextDue',
    header: 'Next Due',
    options: {
      enableSorting: true,
      sortingFn: 'datetime',
    },
  }),
  createFrequencyColumn<Expense>({
    countKey: 'frequencyCount',
    frequencyKey: 'frequency',
    header: 'Frequency',
  }),
  createDateColumn<Expense>({
    accessorKey: 'accrualStart',
    header: 'Accrual Start',
  }),
  createMoneyValueColumn<Expense>({
    accessorKey: 'accrued',
    header: 'Accrued',
    options: {
      enableSorting: true,
      sortingFn: 'basic',
    },
  }),
  {
    id: 'accountDescription',
    header: 'Account',
    cell: ({ row }) => (
      <div
        className={
          !row.original.account?.description ? 'text-muted-foreground' : ''
        }
      >
        {row.original.account?.description ?? 'Not Assigned'}
      </div>
    ),
  },
  createDateColumn<Expense>({
    accessorKey: 'endDate',
    header: 'End Date',
    getNullValue: row =>
      row.original.frequency === Frequency.OneTime ? undefined : 'Ongoing',
  }),
  {
    id: 'actions',
    cell: ({ row }) => {
      const expense = row.original;
      return (
        <div className="flex justify-end">
          <ExpenseActions expense={expense} />
        </div>
      );
    },
  },
];

type ExpensesTableProps = {
  filteredExpenses: Expense[];
};

function ExpensesTable({ filteredExpenses }: ExpensesTableProps) {
  const { id: editingId } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const renewExpensesMutation = useApiRenewExpenses();
  const excludeExpensesMutation = useApiToggleExcludeExpenses();
  const [error, setError] = useState<DisplayError | null>(null);

  const { hasPermission } = usePermissions();
  const canManageExpenses = hasPermission('expense:manage');

  const bulkActions: BulkAction<Expense>[] = [
    {
      label: 'Auto Renew',
      isDisabled: !canManageExpenses,
      onClick: async (selectedItems: Expense[]) => {
        const result = await renewExpenses(
          selectedItems,
          renewExpensesMutation,
          queryClient,
        );

        if (!result.success) {
          setError(result.error);
        }
      },
    },
    {
      label: 'Toggle Exclusion',
      isDisabled: !canManageExpenses,
      onClick: async (selectedItems: Expense[]) => {
        const result = await toggleExcludeExpenses(
          selectedItems,
          excludeExpensesMutation,
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
            data={filteredExpenses}
            enableRowSelection={true}
            bulkActions={bulkActions}
            getRowId={createRowIdGetter<Expense>()}
            highlightRowFilter={(row: Row<Expense>) =>
              row.original.rowId.toString() === editingId
            }
            getRowClassName={(row: Row<Expense>) =>
              getTableRowClassName(row.original)
            }
          />
        </CardContent>
      </Card>
    </>
  );
}

export default ExpensesTable;
export type { ExpensesTableProps };
