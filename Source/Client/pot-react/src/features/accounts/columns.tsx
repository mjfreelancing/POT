import { ColumnDef } from '@tanstack/react-table';
import { MoreHorizontal } from 'lucide-react';

import { createMoneyValueColumn } from '@/components/ui/DataTable';
import { Button } from '@/components/ui/shadcn/button';
import { Dialog, DialogTrigger } from '@/components/ui/shadcn/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/shadcn/dropdown-menu';
import { Account } from '@/data/accounts/account';
import { ConfirmationDialog } from '@/components/dialogs/confirmationDialog';

import { EditAccountDialog } from './edit/EditAccountDialog';
import { useAccountDeletion } from './delete/hooks/useAccountDeletion';

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
      const { showDeleteDialog, setShowDeleteDialog, onDelete } =
        useAccountDeletion(account.rowId);

      return (
        <>
          <Dialog>
            <DropdownMenu modal={false}>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-8 w-8 p-0">
                  <span className="sr-only">Open menu</span>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuSeparator />
                <DialogTrigger asChild>
                  <DropdownMenuItem>Edit</DropdownMenuItem>
                </DialogTrigger>
                <DropdownMenuItem onClick={() => setShowDeleteDialog(true)}>
                  Delete
                </DropdownMenuItem>
                <DropdownMenuSeparator />
              </DropdownMenuContent>
            </DropdownMenu>

            <EditAccountDialog account={account} />
          </Dialog>

          <ConfirmationDialog
            open={showDeleteDialog}
            title="Delete Account"
            description={`Are you sure you want to delete account (${account.bsb}) ${account.number}?`}
            confirmLabel="Delete"
            cancelLabel="Cancel"
            onConfirm={onDelete}
            onCancel={() => setShowDeleteDialog(false)}
          />
        </>
      );
    },
  },
];
