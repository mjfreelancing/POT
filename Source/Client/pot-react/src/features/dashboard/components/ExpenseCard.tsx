import { useQueryClient } from '@tanstack/react-query';
import { Ban, CheckCircle, FastForward, MoreHorizontal } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import { useApiRenewExpenses } from '@/api/hooks';
import { ConfirmationDialog } from '@/components/dialog';
import { NotePopover } from '@/components/feedback';
import StatusBadge from '@/components/feedback/badge/StatusBadge';
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
import { useCacheInvalidation } from '@/concerns/cache/cacheInvalidation';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { WithPermission } from '@/features/auth/components';
import {
  formatDate,
  formatMoneyValue,
  getDaysDue,
  RenewalMode,
  todayIsoFormat,
} from '@/lib';
import { cn } from '@/lib/utils';

type ExpenseCardProps = {
  expense: Expense;
};

function getUrgencyStyle(days: number): {
  borderClass: string;
  bgClass: string;
  badge: React.ReactNode;
} {
  if (days <= 0) {
    return {
      borderClass: 'border-red-500/30 border-l-2 border-l-red-500',
      bgClass: 'bg-red-50/50 dark:bg-red-950/20',
      badge: (
        <StatusBadge
          color="red"
          className="w-16 lg:w-24 text-[10px] lg:text-sm"
          aria-role="status"
        >
          {days === 0 ? 'Due Today' : 'Past Due'}
        </StatusBadge>
      ),
    };
  }

  if (days <= 7) {
    return {
      borderClass: 'border-orange-500/30',
      bgClass: 'bg-orange-50/50 dark:bg-orange-950/20',
      badge: (
        <StatusBadge
          color="orange"
          className="w-16 lg:w-24 text-[10px] lg:text-sm"
          aria-role="status"
        >
          Due Soon
        </StatusBadge>
      ),
    };
  }

  return {
    borderClass: 'border-blue-500/30',
    bgClass: 'bg-blue-50/50 dark:bg-blue-950/20',
    badge: null,
  };
}

function ExpenseCard({ expense }: ExpenseCardProps) {
  const [isRenewing, setIsRenewing] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);

  const days = getDaysDue(expense.nextDue);
  const { borderClass, bgClass, badge } = getUrgencyStyle(days);

  const { setError } = useErrorContext();
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const renewExpensesMutation = useApiRenewExpenses();

  const isOverdue = days <= 0;

  function getConfirmationMessage(): React.ReactNode {
    if (isOverdue) {
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

  async function processMarkAsPaid() {
    setIsRenewing(true);

    const mode = isOverdue ? RenewalMode.Overdue : RenewalMode.Future;
    const result = await renewExpensesMutation.mutateAsync({
      data: {
        rowIds: [expense.rowId],
        mode,
        asOfDate: todayIsoFormat(),
      },
    });

    setIsRenewing(false);
    setShowConfirmation(false);

    if (result.success) {
      invalidateCache(['expenses']);
      const icon = isOverdue ? FastForward : CheckCircle;
      const title = isOverdue ? 'Renewal Advanced' : 'Marked as Paid';
      const description = isOverdue
        ? `${expense.description} renewal advanced.`
        : `${expense.description} marked as paid.`;

      toast(
        <SuccessToast icon={icon} title={title} description={description} />,
      );
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  }

  return (
    <>
      <ConfirmationDialog
        open={showConfirmation}
        title="Mark Expense as Paid"
        description={getConfirmationMessage()}
        confirmLabel="Proceed"
        cancelLabel="Cancel"
        onConfirm={processMarkAsPaid}
        onCancel={() => setShowConfirmation(false)}
      />

      <Card
        className={cn(
          'transition-all duration-200 hover:shadow-md py-2 lg:py-2.5 gap-0 flex flex-col',
          borderClass,
          bgClass,
        )}
      >
        <CardContent className="px-2 lg:px-2.5 flex flex-col flex-1">
          <div className="flex flex-col h-full">
            {/* Expense Name */}
            <div>
              <div className="font-bold text-base lg:text-lg leading-tight flex items-center gap-2 text-blue-700 dark:text-blue-300">
                <span>{expense.description}</span>
                {expense.note && <NotePopover note={expense.note} />}
                {expense.excludeFromCalcs && (
                  <StatusBadge color="red" tooltip="Excluded from calculations">
                    <Ban className="h-3 w-3" />
                  </StatusBadge>
                )}
              </div>
            </div>

            {/* Spacer to push content to bottom */}
            <div className="flex-1" />

            {/* Bottom section with divider, details, and badge */}
            <div className="space-y-1.5 lg:space-y-2">
              {/* Divider */}
              <div className="border-t border-border" />

              {/* Expense Details */}
              <div className="space-y-2">
                <div>
                  <div className="flex justify-between items-center">
                    <span className="text-[11px] lg:text-sm font-medium text-foreground">
                      Due Date:
                    </span>
                    <span className="text-xs lg:text-base font-semibold text-foreground">
                      {formatDate(expense.nextDue)}
                    </span>
                  </div>
                  {days >= 0 && (
                    <div className="text-right text-[10px] lg:text-xs text-muted-foreground mt-0.5">
                      ({days} {days === 1 ? 'day' : 'days'})
                    </div>
                  )}
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[11px] lg:text-sm font-medium text-foreground">
                    Amount:
                  </span>
                  <span className="text-sm lg:text-lg font-bold text-blue-600 dark:text-blue-400">
                    {formatMoneyValue(expense.amount)}
                  </span>
                </div>
                {badge && <div className="flex justify-end">{badge}</div>}
                {expense.account && (
                  <div className="flex items-center justify-between mt-3 pt-3 border-t border-border/50">
                    <div className="text-[10px] lg:text-xs text-foreground/70">
                      {expense.account.description}
                    </div>
                    <WithPermission permissions={['expense:manage']} mode="all">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6 p-0"
                            disabled={isRenewing}
                          >
                            <MoreHorizontal className="h-4 w-4" />
                            <span className="sr-only">Expense actions</span>
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuLabel className="text-sm font-semibold">
                            Actions
                          </DropdownMenuLabel>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => setShowConfirmation(true)}
                            disabled={isRenewing}
                          >
                            <CheckCircle className="mr-2 h-4 w-4" />
                            Mark as Paid
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </WithPermission>
                  </div>
                )}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default ExpenseCard;
