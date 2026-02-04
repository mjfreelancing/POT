import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef, Row } from '@tanstack/react-table';
import { useParams } from 'react-router';

import {
  useApiRenewExpenses,
  useApiToggleExcludeExpenses,
} from '@/api/hooks/useExpenses';
import { ErrorSheet } from '@/components/feedback';
import type { BulkAction } from '@/components/table';
import {
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { usePermissions } from '@/hooks';
import {
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
  createFrequencyColumn<Expense>({
    countKey: 'frequencyCount',
    frequencyKey: 'frequency',
    header: 'Frequency',
  }),
  createDateColumn<Expense>({
    accessorKey: 'nextDue',
    header: 'Next Due',
    options: {
      enableSorting: true,
      sortingFn: 'datetime',
    },
  }),
  createDateColumn<Expense>({
    accessorKey: 'endDate',
    header: 'End Date',
    getNullValue: row =>
      row.original.frequency === Frequency.OneTime ? undefined : 'Ongoing',
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
  const { error, setError } = useErrorContext();

  const { hasPermission } = usePermissions();
  const canManageExpenses = hasPermission('expense:manage');

  const bulkActions: BulkAction<Expense>[] = [
    {
      label: 'Auto Renew',
      isDisabled: !canManageExpenses,
      onClick: async (selectedItems: Expense[]) => {
        const expenseRowIds = selectedItems.map(item => item.rowId);

        const result = await renewExpenses(
          expenseRowIds,
          renewExpensesMutation,
          queryClient,
        );

        if (!result.success) {
          setError(result.error);
        }
      },
      clearSelectionOnComplete: true,
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
            data={filteredExpenses}
            enableRowSelection={hasAnyBulkPermission}
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
