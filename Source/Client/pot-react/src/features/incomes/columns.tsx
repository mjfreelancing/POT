import { ColumnDef } from '@tanstack/react-table';

import {
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
} from '@/components/table/DataTable';
import { Income } from '@/data/incomes/income';

import { IncomeActions } from './components/IncomeActions';

export const columns: ColumnDef<Income>[] = [
  {
    accessorKey: 'description',
    header: 'Description',
  },
  {
    accessorKey: 'account.description',
    header: 'Account',
  },
  createFrequencyColumn<Income>('frequencyCount', 'frequency', 'Frequency'),
  createMoneyValueColumn<Income>('amount', 'Amount'),
  createDateColumn<Income>('nextDue', 'Next Due'),
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
