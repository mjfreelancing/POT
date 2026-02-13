import { Ban } from 'lucide-react';

import { NotePopover } from '@/components/feedback';
import StatusBadge from '@/components/feedback/badge/StatusBadge';
import { Card, CardContent } from '@/components/ui/card';
import type { Income } from '@/data';
import {
  formatDate,
  formatMoneyValue,
  localToday,
  normalizeToEpoch,
} from '@/lib';
import { cn } from '@/lib/utils';

type IncomeCardProps = {
  income: Income;
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
  const days = getDaysDue(income.nextDue);
  const { borderClass, bgClass, badge } = getUrgencyStyle(days);

  return (
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
                <div className="text-[10px] lg:text-xs text-foreground/70 mt-3 pt-3 border-t border-border/50">
                  {income.account.description}
                </div>
              )}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default IncomeCard;
