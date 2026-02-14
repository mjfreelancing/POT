import { useQueryClient } from '@tanstack/react-query';
import { Ban, CheckCircle, FastForward, MoreHorizontal } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import { useApiRenewIncomes } from '@/api/hooks';
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
import type { Income } from '@/data';
import { WithPermission } from '@/features/auth/components';
import {
  formatDate,
  formatMoneyValue,
  getDaysDue,
  RenewalMode,
  todayIsoFormat,
} from '@/lib';
import { cn } from '@/lib/utils';

type IncomeCardProps = {
  income: Income;
};

function getUrgencyStyle(days: number): {
  borderClass: string;
  bgClass: string;
  badge: React.ReactNode;
} {
  if (days <= 0) {
    return {
      borderClass: 'border-orange-500/30 border-l-2 border-l-orange-500',
      bgClass: 'bg-orange-50/50 dark:bg-orange-950/20',
      badge: (
        <StatusBadge
          color="orange"
          className="w-16 lg:w-24 text-[10px] lg:text-sm"
          aria-role="status"
        >
          {days === 0 ? 'Due Today' : 'Overdue'}
        </StatusBadge>
      ),
    };
  }

  if (days <= 7) {
    return {
      borderClass: 'border-yellow-500/30',
      bgClass: 'bg-yellow-50/50 dark:bg-yellow-950/20',
      badge: (
        <StatusBadge
          color="yellow"
          className="w-16 lg:w-24 text-[10px] lg:text-sm"
          aria-role="status"
        >
          Due Soon
        </StatusBadge>
      ),
    };
  }

  return {
    borderClass: 'border-green-500/30',
    bgClass: 'bg-green-50/50 dark:bg-green-950/20',
    badge: null,
  };
}

function IncomeCard({ income }: IncomeCardProps) {
  const [isRenewing, setIsRenewing] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);

  const days = getDaysDue(income.nextDue);
  const { borderClass, bgClass, badge } = getUrgencyStyle(days);

  const { setError } = useErrorContext();
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const renewIncomesMutation = useApiRenewIncomes();

  const isOverdue = days <= 0;

  function getConfirmationMessage(): React.ReactNode {
    if (isOverdue) {
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

  async function processMarkAsReceived() {
    setIsRenewing(true);

    const mode = isOverdue ? RenewalMode.Overdue : RenewalMode.Future;
    const result = await renewIncomesMutation.mutateAsync({
      data: {
        rowIds: [income.rowId],
        mode,
        asOfDate: todayIsoFormat(),
      },
    });

    setIsRenewing(false);
    setShowConfirmation(false);

    if (result.success) {
      invalidateCache(['incomes']);
      const icon = isOverdue ? FastForward : CheckCircle;
      const title = isOverdue ? 'Renewal Advanced' : 'Marked as Received';
      const description = isOverdue
        ? `${income.description} renewal advanced.`
        : `${income.description} marked as received.`;

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
        title="Mark Income as Received"
        description={getConfirmationMessage()}
        confirmLabel="Proceed"
        cancelLabel="Cancel"
        onConfirm={processMarkAsReceived}
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
            {/* Income Name */}
            <div>
              <div className="font-bold text-base lg:text-lg leading-tight flex items-center gap-2 text-green-700 dark:text-green-300">
                <span>{income.description}</span>
                {income.note && <NotePopover note={income.note} />}
                {income.excludeFromCalcs && (
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

              {/* Income Details */}
              <div className="space-y-2">
                <div>
                  <div className="flex justify-between items-center">
                    <span className="text-[11px] lg:text-sm font-medium text-foreground">
                      Due Date:
                    </span>
                    <span className="text-xs lg:text-base font-semibold text-foreground">
                      {formatDate(income.nextDue)}
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
                  <span className="text-sm lg:text-lg font-bold text-green-600 dark:text-green-400">
                    {formatMoneyValue(income.amount)}
                  </span>
                </div>
                {badge && <div className="flex justify-end">{badge}</div>}
                {income.account && (
                  <div className="flex items-center justify-between mt-3 pt-3 border-t border-border/50">
                    <div className="text-[10px] lg:text-xs text-foreground/70">
                      {income.account.description}
                    </div>
                    <WithPermission permissions={['income:manage']} mode="all">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6 p-0"
                            disabled={isRenewing}
                          >
                            <MoreHorizontal className="h-4 w-4" />
                            <span className="sr-only">Income actions</span>
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
                            Mark as Received
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

export default IncomeCard;
