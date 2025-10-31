import { Copy, MoreHorizontal, Pencil, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router';

import { ConfirmationDialog } from '@/components/dialog';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { WithPermission } from '@/features/auth/components';

import useDeleteExpense from '../delete/hooks/useDeleteExpense';

type ExpenseActionsProps = {
  expense: Expense;
};

function ExpenseActions({ expense }: ExpenseActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { error, setError } = useErrorContext();
  const navigate = useNavigate();
  const { deleteExpense } = useDeleteExpense(expense.rowId);

  const handleDelete = async () => {
    const result = await deleteExpense();

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

          <WithPermission permissions={['expense:manage']} mode="all">
            <DropdownMenuItem
              onClick={() => navigate(`/expenses/edit/${expense.rowId}`)}
            >
              <Pencil className="mr-2 h-4 w-4" />
              Edit
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permissions={['expense:manage']} mode="all">
            <DropdownMenuItem
              onClick={() =>
                navigate(`/expenses/create?duplicate=${expense.rowId}`)
              }
            >
              <Copy className="mr-2 h-4 w-4" />
              Duplicate
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permissions={['expense:manage']} mode="all">
            <DropdownMenuItem
              className="text-destructive-high-contrast"
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
        title="Delete Expense"
        description={`Are you sure you want to delete '${expense.description}'?`}
        confirmLabel="Delete"
        cancelLabel="Cancel"
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteDialog(false)}
      />
    </>
  );
}

export default ExpenseActions;
