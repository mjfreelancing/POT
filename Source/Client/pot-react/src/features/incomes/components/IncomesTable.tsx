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
import { Income } from '@/data';

import IncomeActions from './IncomeActions';

type IncomesTableProps = {
  filteredIncomes: Income[];
};

const columns: ColumnDef<Income>[] = [
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
  createFrequencyColumn<Income>('frequencyCount', 'frequency', 'Frequency'),
  createMoneyValueColumn<Income>('amount', 'Amount'),
  createDateColumn<Income>('nextDue', 'Next Due', 'Ongoing', {
    enableSorting: true,
    sortingFn: 'datetime',
  }),
  createDateColumn<Income>('endDate', 'End Date'),
  {
    id: 'actions',
    cell: ({ row }) => {
      const income = row.original;
      return (
        <div className="flex justify-end">
          <IncomeActions income={income} />
        </div>
      );
    },
  },
];

function IncomesTable({ filteredIncomes }: IncomesTableProps) {
  const { id: editingId } = useParams<{ id: string }>();

  const bulkActions: BulkAction<Income>[] = [
    {
      label: 'Auto Renew',
      onClick: (selectedItems: Income[]) => {
        console.log('Auto renew selected incomes:', selectedItems);
        // TODO: Implement auto renew functionality
      },
    },
  ];

  return (
    <Card className="card-elevated">
      <CardContent className="px-4">
        <DataTable
          columns={columns}
          data={filteredIncomes}
          enableRowSelection={true}
          bulkActions={bulkActions}
          highlightRowFilter={(row: Row<Income>) =>
            row.original.rowId.toString() === editingId
          }
        />
      </CardContent>
    </Card>
  );
}

export default IncomesTable;
export type { IncomesTableProps };
