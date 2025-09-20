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
import { Income } from '@/data';
import { WithPermission } from '@/features/auth/components';
import { DisplayError } from '@/lib';

import useDeleteIncome from '../delete/hooks/useDeleteIncome';

type IncomeActionsProps = {
  income: Income;
};

function IncomeActions({ income }: IncomeActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [error, setError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();
  const { deleteIncome } = useDeleteIncome(income.rowId);

  const handleDelete = async () => {
    const result = await deleteIncome();

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
          <DropdownMenuSeparator />
          <DropdownMenuLabel className="text-sm font-semibold">
            Actions
          </DropdownMenuLabel>

          <WithPermission permission="income:manage">
            <DropdownMenuItem
              onClick={() => navigate(`/incomes/edit/${income.rowId}`)}
            >
              <Pencil className="mr-2 h-4 w-4" />
              Edit
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permission="income:manage">
            <DropdownMenuItem
              onClick={() =>
                navigate(`/incomes/create?duplicate=${income.rowId}`)
              }
            >
              <Copy className="mr-2 h-4 w-4" />
              Duplicate
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permission="income:manage">
            <DropdownMenuItem
              className="text-destructive-high-contrast"
              onClick={() => setShowDeleteDialog(true)}
            >
              <Trash2 className="mr-2 h-4 w-4 text-destructive-high-contrast" />
              Delete
            </DropdownMenuItem>
          </WithPermission>

          <DropdownMenuSeparator />
        </DropdownMenuContent>
      </DropdownMenu>

      <ConfirmationDialog
        open={showDeleteDialog}
        title="Delete Income"
        description={`Are you sure you want to delete '${income.description}'?`}
        confirmLabel="Delete"
        cancelLabel="Cancel"
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteDialog(false)}
      />
    </>
  );
}

export default IncomeActions;
