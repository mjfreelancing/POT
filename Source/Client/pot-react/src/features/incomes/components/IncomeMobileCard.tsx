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
import { ErrorSheet, NotePopover } from '@/components/feedback';
import { SuccessToast } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
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
import {
  cn,
  formatDate,
  formatMoneyValue,
  getDaysDue,
  RenewalMode,
} from '@/lib';

import { renewIncomes, toggleExcludeIncomes } from '../bulkActions';
import useDeleteIncome from '../delete/hooks/useDeleteIncome';
import { splitIncomesByActionability } from '../utils/splitIncomesByActionability';

type IncomeMobileCardProps = {
  income: Income;
};

function getUrgencyStyle(days: number): {
  borderClass: string;
  bgClass: string;
} {
  if (days < 0) {
    return {
      borderClass: 'border-red-500/30 border-l-2 border-l-red-500',
      bgClass: 'bg-red-50/50 dark:bg-red-950/20',
    };
  }

  if (days === 0) {
    return {
      borderClass: 'border-orange-500/30 border-l-2 border-l-orange-500',
      bgClass: 'bg-orange-50/50 dark:bg-orange-950/20',
    };
  }

  if (days <= 7) {
    return {
      borderClass: 'border-yellow-500/30',
      bgClass: 'bg-yellow-50/50 dark:bg-yellow-950/20',
    };
  }

  return {
    borderClass: 'border-green-500/30',
    bgClass: 'bg-green-50/50 dark:bg-green-950/20',
  };
}

/**
 * Mobile card component for displaying income information.
 * Uses green color palette to represent positive cash flow.
 */
function IncomeMobileCard({ income }: IncomeMobileCardProps) {
  const {
    description,
    nextDue,
    endDate,
    amount,
    note,
    account,
    excludeFromCalcs,
  } = income;
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showMarkAsReceivedDialog, setShowMarkAsReceivedDialog] =
    useState(false);
  const [isRenewing, setIsRenewing] = useState(false);
  const [isTogglingExclusion, setIsTogglingExclusion] = useState(false);
  const { error, setError } = useErrorContext();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const location = useLocation();
  const { deleteIncome } = useDeleteIncome(income.rowId);
  const renewIncomesMutation = useApiRenewIncomes();
  const excludeIncomesMutation = useApiToggleExcludeIncomes();
  const days = getDaysDue(nextDue);
  const isEndedForDisplay = endDate ? getDaysDue(endDate) < 0 : false;
  const { borderClass, bgClass } = getUrgencyStyle(days);

  // Wrap the current item in a single-item array so mobile and table flows
  // share the same actionability rules (excluded/ended/due-today/future/overdue) in one place.
  const { excludedIncomes, endedIncomes, dueTodayIncomes, overdueIncomes } =
    splitIncomesByActionability([income]);

  const isExcludedForAction = excludedIncomes.length > 0;
  const isEndedForAction = endedIncomes.length > 0;
  const isDueTodayForAction = dueTodayIncomes.length > 0;
  const isOverdueForAction = overdueIncomes.length > 0;
  const canMarkAsReceived = !isExcludedForAction && !isEndedForAction;
  const isActionInProgress = isRenewing || isTogglingExclusion;

  function getMarkAsReceivedConfirmationMessage(): React.ReactNode {
    if (isExcludedForAction) {
      return (
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <EyeOff className="h-5 w-5 text-slate-600 dark:text-slate-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-slate-700 dark:text-slate-300">
                {description}
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
                {description}
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
                {description}
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
                {description}
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
              {description}
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

  // Carry active list filters (for example accountId) into edit route.
  const searchSuffix = location.search;

  const handleDelete = async () => {
    const result = await deleteIncome();
    setShowDeleteDialog(false);

    if (!result.success) {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  const processMarkAsReceived = async () => {
    if (isExcludedForAction) {
      setShowMarkAsReceivedDialog(false);
      toast(
        <SuccessToast
          icon={EyeOff}
          title="Income Skipped"
          description={`${description} is excluded and was not updated.`}
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
          description={`${description} has ended and was not updated.`}
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
          ? `${description} renewal advanced.`
          : `${description} marked as received.`;

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
      const isNowExcluded = !excludeFromCalcs;
      const title = isNowExcluded
        ? 'Excluded from Calculations'
        : 'Included in Calculations';
      const successDescription = isNowExcluded
        ? `${description} excluded from calculations.`
        : `${description} included in calculations.`;

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

      <Card
        className={cn(
          'transition-all duration-200 hover:shadow-md py-2 lg:py-2.5 gap-0 flex flex-col',
          excludeFromCalcs &&
            'opacity-70 border-slate-400/80 dark:border-slate-500/80',
          !excludeFromCalcs &&
            (isEndedForDisplay
              ? 'border-slate-400/80 dark:border-slate-500/80 bg-slate-100/45 dark:bg-slate-800/45'
              : borderClass),
          !excludeFromCalcs && !isEndedForDisplay && bgClass,
        )}
      >
        <CardContent className="px-2 lg:px-2.5 flex flex-col flex-1">
          <div className="flex flex-col h-full">
            {/* Income Description */}
            <div className="mb-0">
              <div
                className={cn(
                  'flex items-start justify-between gap-2',
                  excludeFromCalcs
                    ? 'text-slate-600 dark:text-slate-300'
                    : 'text-green-700 dark:text-green-300',
                )}
              >
                <span className="font-bold text-base lg:text-lg leading-tight">
                  {description}
                </span>
                {excludeFromCalcs ? (
                  <div
                    className="shrink-0 text-slate-500 dark:text-slate-400"
                    aria-label="Excluded from calculations"
                    title="Excluded from calculations"
                  >
                    <EyeOff className="h-4 w-4" aria-hidden="true" />
                    <span className="sr-only">Excluded from calculations</span>
                  </div>
                ) : note ? (
                  <div className="shrink-0">
                    <NotePopover note={note} />
                  </div>
                ) : null}
              </div>
            </div>

            {/* Spacer to push content to bottom */}
            <div className="flex-1" />

            {/* Bottom section with divider, details, and actions */}
            <div className="space-y-1.5 lg:space-y-2">
              {/* Divider */}
              <div className="border-t border-border" />

              {/* Income Details */}
              <div className="space-y-2">
                <div>
                  <div className="flex justify-between items-center">
                    <span className="text-[11px] lg:text-sm font-medium text-foreground">
                      Due:
                    </span>
                    <span className="text-xs lg:text-base font-semibold text-foreground">
                      {formatDate(nextDue)}
                    </span>
                  </div>
                  <div className="mt-px min-h-[0.875rem] flex items-center justify-end text-[10px] lg:text-xs leading-none text-muted-foreground">
                    {!excludeFromCalcs &&
                      (isEndedForDisplay ? (
                        <span className="text-slate-600 dark:text-slate-400">
                          Ended
                        </span>
                      ) : days > 0 ? (
                        `(${days} ${days === 1 ? 'day' : 'days'})`
                      ) : days < 0 ? (
                        <span className="text-red-600 dark:text-red-400">
                          Overdue
                        </span>
                      ) : (
                        <span className="text-orange-600 dark:text-orange-400">
                          Due Today
                        </span>
                      ))}
                  </div>
                </div>

                {endDate && (
                  <div className="flex justify-between items-center">
                    <span className="text-[11px] lg:text-sm font-medium text-foreground">
                      End Date:
                    </span>
                    <span className="text-xs lg:text-base font-semibold text-foreground">
                      {formatDate(endDate)}
                    </span>
                  </div>
                )}

                <div className="flex justify-between items-center">
                  <span className="text-[11px] lg:text-sm font-medium text-foreground">
                    Amount:
                  </span>
                  <span
                    className={cn(
                      'text-sm lg:text-lg font-bold',
                      excludeFromCalcs
                        ? 'text-slate-500 dark:text-slate-400'
                        : 'text-green-600 dark:text-green-400',
                    )}
                  >
                    {formatMoneyValue(amount)}
                  </span>
                </div>
              </div>

              {/* Account and Action Menu Row */}
              <div className="flex justify-between items-center mt-3 pt-3 border-t border-border/50">
                {/* Account */}
                {account && (
                  <div className="text-[10px] lg:text-xs text-foreground/70">
                    {account.description}
                  </div>
                )}

                {/* Action Menu */}
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button
                      variant="outline"
                      size="icon"
                      className="h-8 w-8 shrink-0"
                    >
                      <span className="sr-only">Open menu</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" side="top">
                    <DropdownMenuLabel className="text-sm font-semibold">
                      Actions
                    </DropdownMenuLabel>
                    {/* Excluded and ended items cannot be marked as received because renewal is skipped and due date does not change. */}
                    {canMarkAsReceived && (
                      <WithPermission
                        permissions={['income:manage']}
                        mode="all"
                      >
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
                          navigate(
                            `/incomes/edit/${income.rowId}${searchSuffix}`,
                          )
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
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <ConfirmationDialog
        open={showMarkAsReceivedDialog}
        onConfirm={processMarkAsReceived}
        onCancel={() => setShowMarkAsReceivedDialog(false)}
        title="Mark Income as Received"
        description={getMarkAsReceivedConfirmationMessage()}
        confirmLabel="Proceed"
        cancelLabel="Cancel"
      />

      <ConfirmationDialog
        open={showDeleteDialog}
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteDialog(false)}
        title="Delete Income"
        description={`Are you sure you want to delete "${income.description}"? This action cannot be undone.`}
        confirmLabel="Delete"
      />
    </>
  );
}

export default IncomeMobileCard;
export type { IncomeMobileCardProps };
