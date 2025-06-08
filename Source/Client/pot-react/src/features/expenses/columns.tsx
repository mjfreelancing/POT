import { ColumnDef } from '@tanstack/react-table';

import {
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
} from '@/components/table';
import { Expense } from '@/data';

import ExpenseActions from './components/ExpenseActions';

export const columns: ColumnDef<Expense>[] = [
  {
    accessorKey: 'description',
    header: 'Description',
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
  createDateColumn<Expense>('nextDue', 'Next Due'),
  createDateColumn<Expense>('accrualStart', 'Accrual Start'),
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
