import { ColumnDef } from '@tanstack/react-table';

import { LinkedDataBadge } from '@/components/feedback/badge';
import { createMoneyValueColumn } from '@/components/table';
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
                <LinkedDataBadge
                  type="expense"
                  count={account.linkedExpenses}
                />
              )}
              {account.linkedIncomes > 0 && (
                <LinkedDataBadge type="income" count={account.linkedIncomes} />
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
