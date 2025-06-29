import { ColumnDef, Row } from '@tanstack/react-table';
import { useParams } from 'react-router';

import {
  BulkAction,
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Expense } from '@/data';

import ExpenseActions from './ExpenseActions';

const columns: ColumnDef<Expense>[] = [
  {
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
  },
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
  createFrequencyColumn<Expense>('frequencyCount', 'frequency', 'Frequency'),
  {
    accessorKey: 'recurring',
    header: 'Recurring',
    cell: ({ row }) => <div>{row.original.recurring ? 'Yes' : 'No'}</div>,
  },
  createMoneyValueColumn<Expense>('amount', 'Amount'),
  createDateColumn<Expense>('accrualStart', 'Accrual Start'),
  createMoneyValueColumn<Expense>('accrued', 'Accrued'),
  createDateColumn<Expense>('nextDue', 'Next Due', 'Ongoing', {
    enableSorting: true,
    sortingFn: 'datetime',
  }),
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

  const bulkActions: BulkAction<Expense>[] = [
    {
      label: 'Auto Advance',
      onClick: (selectedItems: Expense[]) => {
        console.log('Auto Advance selected expenses:', selectedItems);
        // TODO: Implement auto advance functionality
      },
    },
  ];

  return (
    <Card className="card-elevated">
      <CardContent className="px-4">
        <DataTable
          columns={columns}
          data={filteredExpenses}
          enableRowSelection={true}
          bulkActions={bulkActions}
          highlightRowFilter={(row: Row<Expense>) =>
            row.original.rowId.toString() === editingId
          }
        />
      </CardContent>
    </Card>
  );
}

export default ExpensesTable;
export type { ExpensesTableProps };
