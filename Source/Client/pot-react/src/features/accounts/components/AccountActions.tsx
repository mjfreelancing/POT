import { MoreHorizontal } from 'lucide-react';
import { useState } from 'react';
import { useErrorBoundary } from 'react-error-boundary';
import { useNavigate } from 'react-router';

import { ConfirmationDialog } from '@/components/dialog/confirmationDialog';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Account } from '@/data/accounts/account';

import { useDeleteAccount } from '../delete/hooks/useDeleteAccount';

type AccountActionsProps = {
  account: Account;
};

export const AccountActions = ({ account }: AccountActionsProps) => {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const navigate = useNavigate();
  const { deleteAccount } = useDeleteAccount(account.rowId);
  const { showBoundary } = useErrorBoundary();

  const handleDelete = async () => {
    try {
      await deleteAccount();
      setShowDeleteDialog(false);
    } catch (error) {
      showBoundary(error);
    }
  };

  return (
    <>
      <DropdownMenu modal={false}>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="h-8 w-8 p-0">
            <span className="sr-only">Open menu</span>
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuSeparator />
          <DropdownMenuItem
            onClick={() => navigate(`/accounts/edit/${account.rowId}`)}
          >
            Edit
          </DropdownMenuItem>
          <DropdownMenuItem onClick={() => setShowDeleteDialog(true)}>
            Delete
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
};
