import { ColumnDef } from '@tanstack/react-table';

import { createMoneyValueColumn } from '@/components/table/DataTable';
import { Account } from '@/data/accounts/account';

import { AccountActions } from './components/AccountActions';

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
      return <AccountActions account={account} />;
    },
  },
];
