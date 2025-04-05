import { ColumnDef } from '@tanstack/react-table';
import { MoreHorizontal } from 'lucide-react';

import { Button } from '@/components/ui/shadcn/button';
import { Dialog, DialogTrigger } from '@/components/ui/shadcn/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/shadcn/dropdown-menu';
import { createCurrencyColumn } from '@/components/ui/DataTable';
import { Account } from '@/data/accounts/account';

import { EditAccountDialog } from './edit/EditAccountDialog';

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
  createCurrencyColumn<Account>('balance', 'Balance'),
  createCurrencyColumn<Account>('reserved', 'Reserved'),
  createCurrencyColumn<Account>('allocated', 'Allocated'),
  createCurrencyColumn<Account>('dailyAccrual', 'Daily Accrual'),
  createCurrencyColumn<Account>('available', 'Available'),
  {
    id: 'actions',
    cell: ({ row }) => {
      const account = row.original;

      return (
        <Dialog>
          <DropdownMenu modal={false}>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <span className="sr-only">Open menu</span>
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DialogTrigger asChild>
                <DropdownMenuItem>Edit</DropdownMenuItem>
              </DialogTrigger>
              <DropdownMenuSeparator />
            </DropdownMenuContent>
          </DropdownMenu>

          <EditAccountDialog account={account} />
        </Dialog>
      );
    },
  },
];
