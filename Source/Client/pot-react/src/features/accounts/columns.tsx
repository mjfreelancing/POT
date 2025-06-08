import { ColumnDef } from '@tanstack/react-table';
import { CircleDollarSignIcon, ShoppingCart } from 'lucide-react';

import { createMoneyValueColumn } from '@/components/table/DataTable';
import { Badge } from '@/components/ui/badge';
import { Account } from '@/data/accounts/account';

import AccountActions from './components/AccountActions';

export const columns: ColumnDef<Account>[] = [
  {
    accessorKey: 'bsb_number',
    header: 'BSB / Number',
    cell: ({ row }) => {
      const { bsb, number } = row.original;
      return `(${bsb}) ${number}`;
    },
  },
  {
    accessorKey: 'description',
    header: 'Description',
    cell: ({ row }) => {
      const account = row.original;
      const hasLinkedData =
        account.linkedExpenses > 0 || account.linkedIncomes > 0;

      return (
        <div className="flex items-center gap-2">
          <span>{account.description}</span>
          {hasLinkedData && (
            <div className="flex gap-1">
              {account.linkedExpenses > 0 && (
                <Badge
                  variant="outline"
                  className="text-xs bg-red-50 text-red-700 border-red-200 dark:bg-red-950 dark:text-red-300 dark:border-red-800"
                >
                  <ShoppingCart className="w-3 h-3" />
                  {account.linkedExpenses}
                </Badge>
              )}
              {account.linkedIncomes > 0 && (
                <Badge
                  variant="outline"
                  className="text-xs bg-green-50 text-green-700 border-green-200 dark:bg-green-950 dark:text-green-300 dark:border-green-800"
                >
                  <CircleDollarSignIcon className="w-3 h-3" />
                  {account.linkedIncomes}
                </Badge>
              )}
            </div>
          )}
        </div>
      );
    },
  },
  createMoneyValueColumn<Account>('balance', 'Balance'),
  createMoneyValueColumn<Account>('reserved', 'Reserved'),
  createMoneyValueColumn<Account>('allocated', 'Allocated'),
  createMoneyValueColumn<Account>('dailyAccrual', 'Daily Accrual'),
  createMoneyValueColumn<Account>('available', 'Available'),
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
