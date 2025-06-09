import { ColumnDef } from '@tanstack/react-table';

import {
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  DataTableColumnHeader,
} from '@/components/table';
import { Income } from '@/data';

import IncomeActions from './components/IncomeActions';

export const columns: ColumnDef<Income>[] = [
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
