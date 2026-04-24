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

import { useApiRenewIncomes, useApiToggleExcludeIncomes } from '@/api/hooks';
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
import type { Income } from '@/data';
import { WithPermission } from '@/features/auth/components';
import { RenewalMode } from '@/lib';

import { renewIncomes, toggleExcludeIncomes } from '../bulkActions';
import useDeleteIncome from '../delete/hooks/useDeleteIncome';
import {
  getSingleIncomeActionability,
  shouldHideMarkAsReceivedAction,
} from '../utils/incomeActionability';

type IncomeActionsProps = {
  income: Income;
};

function IncomeActions({ income }: IncomeActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showMarkAsReceivedDialog, setShowMarkAsReceivedDialog] =
    useState(false);
  const [isRenewing, setIsRenewing] = useState(false);
  const [isTogglingExclusion, setIsTogglingExclusion] = useState(false);
  const { error, setError } = useErrorContext();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const { deleteIncome } = useDeleteIncome(income.rowId);
  const renewIncomesMutation = useApiRenewIncomes();
  const excludeIncomesMutation = useApiToggleExcludeIncomes();

  // Carry active list filters (for example accountId) into edit route.
  const searchSuffix = location.search;

  const {
    isExcludedForAction,
    isEndedForAction,
    isDueTodayForAction,
    isOverdueForAction,
  } = getSingleIncomeActionability(income);
  const shouldShowMarkAsReceived = !shouldHideMarkAsReceivedAction([income]);
  const isActionInProgress = isRenewing || isTogglingExclusion;

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

  function getMarkAsReceivedConfirmationMessage(): React.ReactNode {
    if (isExcludedForAction) {
      return (
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <EyeOff className="h-5 w-5 text-slate-600 dark:text-slate-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-slate-700 dark:text-slate-300">
                {income.description}
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
                {income.description}
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
            <FastForward className="h-5 w-5 text-orange-600 dark:text-orange-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-orange-700 dark:text-orange-300">
                {income.description}
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
                {income.description}
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
              {income.description}
            </span>{' '}
            will be marked as received early and moved forward to the next
            period.
          </div>
        </div>
        <div className="text-sm text-muted-foreground pt-2 border-t">
          This action cannot be undone.
        </div>
      </div>
    );
  }

  const processMarkAsReceived = async () => {
    if (isExcludedForAction) {
      setShowMarkAsReceivedDialog(false);
      toast(
        <SuccessToast
          icon={EyeOff}
          title="Income Skipped"
          description={`${income.description} is excluded and was not updated.`}
        />,
      );
      return;
    }

    if (isEndedForAction) {
      setShowMarkAsReceivedDialog(false);
      toast(
        <SuccessToast
          icon={EyeOff}
          title="Income Skipped"
          description={`${income.description} has ended and was not updated.`}
        />,
      );
      return;
    }

    setIsRenewing(true);
    const mode =
      isOverdueForAction || isDueTodayForAction
        ? RenewalMode.Overdue
        : RenewalMode.Future;

    const result = await renewIncomes(
      [income.rowId],
      mode,
      renewIncomesMutation,
      queryClient,
    );

    setIsRenewing(false);
    setShowMarkAsReceivedDialog(false);

    if (result.success) {
      const icon =
        isOverdueForAction || isDueTodayForAction ? FastForward : CheckCircle;
      const title =
        isOverdueForAction || isDueTodayForAction
          ? 'Renewal Advanced'
          : 'Marked as Received';
      const successDescription =
        isOverdueForAction || isDueTodayForAction
          ? `${income.description} renewal advanced.`
          : `${income.description} marked as received.`;

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

    const result = await toggleExcludeIncomes(
      [income],
      excludeIncomesMutation,
      queryClient,
    );

    setIsTogglingExclusion(false);

    if (result.success) {
      const isNowExcluded = !income.excludeFromCalcs;
      const title = isNowExcluded
        ? 'Excluded from Calculations'
        : 'Included in Calculations';
      const successDescription = isNowExcluded
        ? `${income.description} excluded from calculations.`
        : `${income.description} included in calculations.`;

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

          {shouldShowMarkAsReceived && (
            <WithPermission permissions={['income:manage']} mode="all">
              <DropdownMenuItem
                onClick={() => setShowMarkAsReceivedDialog(true)}
                disabled={isActionInProgress}
              >
                <CheckCircle className="mr-2 h-4 w-4" />
                Mark as Received
              </DropdownMenuItem>
            </WithPermission>
          )}

          <WithPermission permissions={['income:manage']} mode="all">
            <DropdownMenuItem
              onClick={processToggleExclusion}
              disabled={isActionInProgress}
            >
              <EyeOff className="mr-2 h-4 w-4" />
              Toggle Exclusion
            </DropdownMenuItem>
          </WithPermission>

          <DropdownMenuSeparator />

          <WithPermission permissions={['income:manage']} mode="all">
            <DropdownMenuItem
              onClick={() =>
                navigate(`/incomes/edit/${income.rowId}${searchSuffix}`)
              }
              disabled={isActionInProgress}
            >
              <Pencil className="mr-2 h-4 w-4" />
              Edit
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permissions={['income:manage']} mode="all">
            <DropdownMenuItem
              onClick={() =>
                navigate(`/incomes/create?duplicate=${income.rowId}`)
              }
              disabled={isActionInProgress}
            >
              <Copy className="mr-2 h-4 w-4" />
              Duplicate
            </DropdownMenuItem>
          </WithPermission>

          <WithPermission permissions={['income:manage']} mode="all">
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
        open={showMarkAsReceivedDialog}
        title="Mark Income as Received"
        description={getMarkAsReceivedConfirmationMessage()}
        confirmLabel="Proceed"
        cancelLabel="Cancel"
        onConfirm={processMarkAsReceived}
        onCancel={() => setShowMarkAsReceivedDialog(false)}
      />

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
