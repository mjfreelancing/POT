import { MoreHorizontal, Pencil, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router';

import { ConfirmationDialog } from '@/components/dialog';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useErrorContext } from '@/contexts';
import type { Account } from '@/data';
import { WithPermission } from '@/features/auth/components';

import useDeleteAccount from '../delete/hooks/useDeleteAccount';

type AccountActionsProps = {
  account: Account;
};

function AccountActions({ account }: AccountActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { setError } = useErrorContext();
  const navigate = useNavigate();
  const { deleteAccount } = useDeleteAccount(account.rowId);
  const hasLinkedData = account.linkedExpenses > 0 || account.linkedIncomes > 0;

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
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="h-8 w-8 p-0">
            <span className="sr-only">Open menu</span>
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuLabel className="text-sm font-semibold">
            Actions
          </DropdownMenuLabel>
          <WithPermission permissions={['account:manage']} mode="all">
            <DropdownMenuItem
              onClick={() => navigate(`/accounts/edit/${account.rowId}`)}
            >
              <Pencil className="text-action-neutral mr-2 h-4 w-4" />
              Edit
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permissions={['account:manage']} mode="all">
            <DropdownMenuItem
              className="text-destructive-high-contrast"
              disabled={hasLinkedData}
              onClick={() => setShowDeleteDialog(true)}
            >
              <Trash2 className="mr-2 h-4 w-4 text-destructive-high-contrast" />
              Delete
            </DropdownMenuItem>
          </WithPermission>
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
export type { AccountActionsProps };
