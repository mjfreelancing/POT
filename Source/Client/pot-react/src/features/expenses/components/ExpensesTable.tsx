import { useQueryClient } from '@tanstack/react-query';
import { ColumnDef, Row } from '@tanstack/react-table';
import { useState } from 'react';
import { useParams } from 'react-router';

import { useApiRenewExpenses } from '@/api/hooks/useExpenses';
import { ErrorSheet, NotePopover } from '@/components/feedback';
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
import { DisplayError } from '@/lib';

import { renewAllExpenses } from '../utils/bulkActions';
import ExpenseActions from './ExpenseActions';

const columns: ColumnDef<Expense>[] = [
  {
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
    cell: ({ row }) => (
      <div className="flex items-center gap-1">
        {row.original.description}
        <NotePopover note={row.original.note} />
      </div>
    ),
  },
  createMoneyValueColumn<Expense>('amount', 'Amount'),
  createDateColumn<Expense>('nextDue', 'Next Due', 'Ongoing', {
    enableSorting: true,
    sortingFn: 'datetime',
  }),
  createFrequencyColumn<Expense>('frequencyCount', 'frequency', 'Frequency'),
  createDateColumn<Expense>('accrualStart', 'Accrual Start'),
  createMoneyValueColumn<Expense>('accrued', 'Accrued'),
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
  createDateColumn<Expense>('endDate', 'End Date'),
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
  const [error, setError] = useState<DisplayError | null>(null);

  const bulkActions: BulkAction<Expense>[] = [
    {
      label: 'Auto Renew',
      onClick: async (selectedItems: Expense[]) => {
        const result = await renewAllExpenses(
          selectedItems,
          renewExpensesMutation,
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
          />
        </CardContent>
      </Card>
    </>
  );
}

export default ExpensesTable;
export type { ExpensesTableProps };
