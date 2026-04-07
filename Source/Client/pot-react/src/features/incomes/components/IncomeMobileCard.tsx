import { Copy, MoreHorizontal, Pencil, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router';

import { ConfirmationDialog } from '@/components/dialog';
import { ErrorSheet, NotePopover } from '@/components/feedback';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useErrorContext } from '@/contexts';
import type { Income } from '@/data';
import { WithPermission } from '@/features/auth/components';
import { formatDate, formatMoneyValue, getDaysDue } from '@/lib';
import { cn } from '@/lib/utils';

import useDeleteIncome from '../delete/hooks/useDeleteIncome';

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
  const { description, nextDue, endDate, amount, note, account } = income;
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { error, setError } = useErrorContext();
  const navigate = useNavigate();
  const location = useLocation();
  const { deleteIncome } = useDeleteIncome(income.rowId);
  const days = getDaysDue(nextDue);
  const { borderClass, bgClass } = getUrgencyStyle(days);

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
          borderClass,
          bgClass,
        )}
      >
        <CardContent className="px-2 lg:px-2.5 flex flex-col flex-1">
          <div className="flex flex-col h-full">
            {/* Income Description */}
            <div className="mb-0.5">
              <div className="flex items-start justify-between gap-2 text-green-700 dark:text-green-300">
                <span className="font-bold text-base lg:text-lg leading-tight">
                  {description}
                </span>
                {note && (
                  <div className="shrink-0">
                    <NotePopover note={note} />
                  </div>
                )}
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
                    {days > 0 ? (
                      `(${days} ${days === 1 ? 'day' : 'days'})`
                    ) : days < 0 ? (
                      <span className="text-red-600 dark:text-red-400">
                        Overdue
                      </span>
                    ) : (
                      <span className="text-orange-600 dark:text-orange-400">
                        Due Today
                      </span>
                    )}
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
                  <span className="text-sm lg:text-lg font-bold text-green-600 dark:text-green-400">
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
                    <WithPermission permissions={['income:manage']} mode="all">
                      <DropdownMenuItem
                        onClick={() =>
                          navigate(
                            `/incomes/edit/${income.rowId}${searchSuffix}`,
                          )
                        }
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
                      >
                        <Copy className="mr-2 h-4 w-4" />
                        Duplicate
                      </DropdownMenuItem>
                    </WithPermission>

                    <WithPermission permissions={['income:manage']} mode="all">
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
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

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
