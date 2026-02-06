import { Ban } from 'lucide-react';

import { NotePopover } from '@/components/feedback';
import StatusBadge from '@/components/feedback/badge/StatusBadge';
import { Card, CardContent } from '@/components/ui/card';
import type { Expense } from '@/data';
import {
  formatDate,
  formatMoneyValue,
  localToday,
  normalizeToEpoch,
} from '@/lib';
import { cn } from '@/lib/utils';

type ExpenseCardProps = {
  expense: Expense;
};

function getDaysDue(nextDue: string): number {
  const todayEpoch = normalizeToEpoch(localToday());
  const dueDateEpoch = normalizeToEpoch(nextDue);
  const diffMs = dueDateEpoch - todayEpoch;
  return Math.floor(diffMs / (1000 * 60 * 60 * 24));
}

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
  const days = getDaysDue(expense.nextDue);
  const { borderClass, bgClass, badge } = getUrgencyStyle(days);

  return (
    <Card
      className={cn(
        'transition-all duration-200 hover:shadow-md',
        borderClass,
        bgClass,
      )}
    >
      <CardContent className="p-3 lg:p-4">
        <div className="space-y-2 lg:space-y-3">
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
              <div className="text-[10px] lg:text-xs text-foreground/70 mt-3 pt-3 border-t border-border/50">
                {expense.account.description}
              </div>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default ExpenseCard;
