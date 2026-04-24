import { useQueryClient } from '@tanstack/react-query';
import {
  CheckCircle,
  Copy,
  EyeOff,
  FastForward,
  MoreHorizontal,
  Pencil,
  Trash2,
} from 'lucide-react';
import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router';
import { toast } from 'sonner';

import {
  useApiRenewExpenses,
  useApiToggleExcludeExpenses,
} from '@/api/hooks/useExpenses';
import { ConfirmationDialog } from '@/components/dialog';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { SuccessToast } from '@/components/feedback/toast';
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
import { RenewalMode } from '@/lib';

import { renewExpenses, toggleExcludeExpenses } from '../bulkActions';
import useDeleteExpense from '../delete/hooks/useDeleteExpense';
import {
  getSingleExpenseActionability,
  shouldHideMarkAsPaidAction,
} from '../utils/expenseActionability';

type ExpenseActionsProps = {
  expense: Expense;
};

function ExpenseActions({ expense }: ExpenseActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showMarkAsPaidDialog, setShowMarkAsPaidDialog] = useState(false);
  const [isRenewing, setIsRenewing] = useState(false);
  const [isTogglingExclusion, setIsTogglingExclusion] = useState(false);
  const { error, setError } = useErrorContext();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const { deleteExpense } = useDeleteExpense(expense.rowId);
  const renewExpensesMutation = useApiRenewExpenses();
  const excludeExpensesMutation = useApiToggleExcludeExpenses();

  // Carry active list filters (for example accountId) into edit route.
  const searchSuffix = location.search;

  const {
    isExcludedForAction,
    isEndedForAction,
    isDueTodayForAction,
    isOverdueForAction,
  } = getSingleExpenseActionability(expense);
  const shouldShowMarkAsPaid = !shouldHideMarkAsPaidAction([expense]);
  const isActionInProgress = isRenewing || isTogglingExclusion;

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

  function getMarkAsPaidConfirmationMessage(): React.ReactNode {
    if (isExcludedForAction) {
      return (
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <EyeOff className="h-5 w-5 text-slate-600 dark:text-slate-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-slate-700 dark:text-slate-300">
                {expense.description}
              </span>{' '}
              is excluded from calculations and will be skipped.
            </div>
          </div>
          <div className="text-sm text-muted-foreground pt-2 border-t">
            This action cannot be undone.
          </div>
        </div>
      );
    }

    if (isEndedForAction) {
      return (
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <EyeOff className="h-5 w-5 text-slate-600 dark:text-slate-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-slate-700 dark:text-slate-300">
                {expense.description}
              </span>{' '}
              has ended and will be skipped.
            </div>
          </div>
          <div className="text-sm text-muted-foreground pt-2 border-t">
            This action cannot be undone.
          </div>
        </div>
      );
    }

    if (isOverdueForAction) {
      return (
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <FastForward className="h-5 w-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-red-700 dark:text-red-300">
                {expense.description}
              </span>{' '}
              is overdue and will be caught up to its next scheduled due date.
            </div>
          </div>
          <div className="text-sm text-muted-foreground pt-2 border-t">
            This action cannot be undone.
          </div>
        </div>
      );
    }

    if (isDueTodayForAction) {
      return (
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-amber-700 dark:text-amber-300">
                {expense.description}
              </span>{' '}
              is due today and will be processed and moved forward to the next
              period.
            </div>
          </div>
          <div className="text-sm text-muted-foreground pt-2 border-t">
            This action cannot be undone.
          </div>
        </div>
      );
    }

    return (
      <div className="space-y-3">
        <div className="flex items-start gap-3">
          <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400 flex-shrink-0 mt-0.5" />
          <div>
            <span className="font-bold text-green-700 dark:text-green-300">
              {expense.description}
            </span>{' '}
            will be marked as paid early and moved forward to the next period.
          </div>
        </div>
        <div className="text-sm text-muted-foreground pt-2 border-t">
          This action cannot be undone.
        </div>
      </div>
    );
  }

  const processMarkAsPaid = async () => {
    if (isExcludedForAction) {
      setShowMarkAsPaidDialog(false);
      toast(
        <SuccessToast
          icon={EyeOff}
          title="Expense Skipped"
          description={`${expense.description} is excluded and was not updated.`}
        />,
      );
      return;
    }

    if (isEndedForAction) {
      setShowMarkAsPaidDialog(false);
      toast(
        <SuccessToast
          icon={EyeOff}
          title="Expense Skipped"
          description={`${expense.description} has ended and was not updated.`}
        />,
      );
      return;
    }

    setIsRenewing(true);
    const mode =
      isOverdueForAction || isDueTodayForAction
        ? RenewalMode.Overdue
        : RenewalMode.Future;

    const result = await renewExpenses(
      [expense.rowId],
      mode,
      renewExpensesMutation,
      queryClient,
    );

    setIsRenewing(false);
    setShowMarkAsPaidDialog(false);

    if (result.success) {
      const icon =
        isOverdueForAction || isDueTodayForAction ? FastForward : CheckCircle;
      const title =
        isOverdueForAction || isDueTodayForAction
          ? 'Renewal Advanced'
          : 'Marked as Paid';
      const successDescription =
        isOverdueForAction || isDueTodayForAction
          ? `${expense.description} renewal advanced.`
          : `${expense.description} marked as paid.`;

      toast(
        <SuccessToast
          icon={icon}
          title={title}
          description={successDescription}
        />,
      );
      return;
    }

    setError({
      title: result.error.title,
      description: result.error.description,
    });
  };

  const processToggleExclusion = async () => {
    setIsTogglingExclusion(true);

    const result = await toggleExcludeExpenses(
      [expense],
      excludeExpensesMutation,
      queryClient,
    );

    setIsTogglingExclusion(false);

    if (result.success) {
      const isNowExcluded = !expense.excludeFromCalcs;
      const title = isNowExcluded
        ? 'Excluded from Calculations'
        : 'Included in Calculations';
      const successDescription = isNowExcluded
        ? `${expense.description} excluded from calculations.`
        : `${expense.description} included in calculations.`;

      toast(
        <SuccessToast
          icon={EyeOff}
          title={title}
          description={successDescription}
        />,
      );
      return;
    }

    setError({
      title: result.error.title,
      description: result.error.description,
    });
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

          {shouldShowMarkAsPaid && (
            <WithPermission permissions={['expense:manage']} mode="all">
              <DropdownMenuItem
                onClick={() => setShowMarkAsPaidDialog(true)}
                disabled={isActionInProgress}
              >
                <CheckCircle className="mr-2 h-4 w-4" />
                Mark as Paid
              </DropdownMenuItem>
            </WithPermission>
          )}

          <WithPermission permissions={['expense:manage']} mode="all">
            <DropdownMenuItem
              onClick={processToggleExclusion}
              disabled={isActionInProgress}
            >
              <EyeOff className="mr-2 h-4 w-4" />
              Toggle Exclusion
            </DropdownMenuItem>
          </WithPermission>

          <DropdownMenuSeparator />

          <WithPermission permissions={['expense:manage']} mode="all">
            <DropdownMenuItem
              onClick={() =>
                navigate(`/expenses/edit/${expense.rowId}${searchSuffix}`)
              }
              disabled={isActionInProgress}
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
              disabled={isActionInProgress}
            >
              <Copy className="mr-2 h-4 w-4" />
              Duplicate
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permissions={['expense:manage']} mode="all">
            <DropdownMenuItem
              className="text-destructive-high-contrast"
              onClick={() => setShowDeleteDialog(true)}
              disabled={isActionInProgress}
            >
              <Trash2 className="mr-2 h-4 w-4 text-destructive-high-contrast" />
              Delete
            </DropdownMenuItem>
          </WithPermission>
        </DropdownMenuContent>
      </DropdownMenu>

      <ConfirmationDialog
        open={showMarkAsPaidDialog}
        onConfirm={processMarkAsPaid}
        onCancel={() => setShowMarkAsPaidDialog(false)}
        title="Mark Expense as Paid"
        description={getMarkAsPaidConfirmationMessage()}
        confirmLabel="Proceed"
        cancelLabel="Cancel"
      />

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
