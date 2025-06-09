import { DropdownMenuLabel } from '@radix-ui/react-dropdown-menu';
import {
  CircleDollarSignIcon,
  MoreHorizontal,
  Pencil,
  ShoppingCart,
  Trash2,
} from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router';

import { ConfirmationDialog } from '@/components/dialog';
import { ErrorSheet } from '@/components/feedback';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Account } from '@/data';
import { DisplayError } from '@/lib/errors/displayError';

import useDeleteAccount from '../delete/hooks/useDeleteAccount';

type AccountActionsProps = {
  account: Account;
};

function AccountActions({ account }: AccountActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [error, setError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();
  const { deleteAccount } = useDeleteAccount(account.rowId);

  const handleDelete = async () => {
    const result = await deleteAccount();

    // Always close the dialog regardless of success or failure
    setShowDeleteDialog(false);

    if (!result.success) {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <DropdownMenu modal={false}>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="h-8 w-8 p-0">
            <span className="sr-only">Open menu</span>
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuSeparator />
          <DropdownMenuLabel className="text-sm font-semibold">
            Actions
          </DropdownMenuLabel>
          <DropdownMenuItem
            onClick={() => navigate(`/accounts/edit/${account.rowId}`)}
          >
            <Pencil className="mr-2 h-4 w-4" />
            Edit
          </DropdownMenuItem>
          <DropdownMenuItem
            className="text-red-600"
            onClick={() => setShowDeleteDialog(true)}
          >
            <Trash2 className="mr-2 h-4 w-4 text-red-600" />
            Delete
          </DropdownMenuItem>
          <DropdownMenuSeparator />
          <DropdownMenuLabel className="text-sm font-semibold">
            Linked Data
          </DropdownMenuLabel>
          <DropdownMenuItem
            disabled={account.linkedExpenses === 0}
            // This needs to navigate to the expenses page, filtered by the account
            onClick={() => navigate(`/expenses/edit/${account.rowId}`)}
          >
            <ShoppingCart className="mr-2 h-4 w-4" />
            Expenses
            {account.linkedExpenses > 0 && `(${account.linkedExpenses})`}
          </DropdownMenuItem>
          <DropdownMenuItem
            disabled={account.linkedIncomes === 0}
            // This needs to navigate to the incomes page, filtered by the account
            onClick={() => navigate(`/incomes/edit/${account.rowId}`)}
          >
            <CircleDollarSignIcon className="mr-2 h-4 w-4" />
            Income {account.linkedIncomes > 0 && `(${account.linkedIncomes})`}
          </DropdownMenuItem>
          <DropdownMenuSeparator />
        </DropdownMenuContent>
      </DropdownMenu>

      <ConfirmationDialog
        open={showDeleteDialog}
        title="Delete Account"
        description={`Are you sure you want to delete account (${account.bsb}) ${account.number}?`}
        confirmLabel="Delete"
        cancelLabel="Cancel"
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteDialog(false)}
      />
    </>
  );
}

export default AccountActions;
