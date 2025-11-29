import { format } from 'date-fns';
import { XIcon } from 'lucide-react';

import { Button } from '@/components/ui/button';
import type { ChartConfig } from '@/components/ui/chart';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import type { ProjectionIncomeItemWithAccount } from '@/data/projection';
import { formatMoneyValue } from '@/lib/moneyUtils';

type IncomeDetailsProps = {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  date: Date | null;
  items: ProjectionIncomeItemWithAccount[];
  chartConfig: ChartConfig;
  hiddenSeries: string[];
};

function IncomeDetails({
  isOpen,
  onOpenChange,
  date,
  items,
  chartConfig,
  hiddenSeries,
}: IncomeDetailsProps) {
  if (!date) {
    return null;
  }

  // Calculate totals and filter items
  const totalIncome = items.reduce((sum, item) => sum + item.amount, 0);
  const visibleItems = items.filter(
    item => !hiddenSeries.includes(item.accountRowId),
  );
  const filteredTotal = visibleItems.reduce(
    (sum, item) => sum + item.amount,
    0,
  );

  return (
    <Sheet open={isOpen} modal={false}>
      <SheetContent
        side="right"
        className="w-full md:max-w-md overflow-y-auto p-0 [&>button:first-of-type]:hidden"
      >
        <SheetHeader className="px-6 pt-6 pb-4">
          <div className="flex items-center justify-between mb-6">
            <SheetTitle>{format(date, 'MMMM d, yyyy')}</SheetTitle>
            <Button
              variant="ghost"
              size="icon"
              aria-label="Close income details"
              onClick={() => onOpenChange(false)}
            >
              <XIcon className="size-5" />
            </Button>
          </div>

          <div className="space-y-2">
            <div>
              <div className="bg-primary/10 rounded-md space-y-1">
                <SheetDescription className="flex justify-between items-center text-sm p-3 pr-2">
                  <span className="font-semibold">Total Income</span>
                  <span className="font-semibold">
                    {formatMoneyValue(totalIncome)}
                  </span>
                </SheetDescription>
                {filteredTotal !== totalIncome && (
                  <SheetDescription className="flex justify-between items-center text-sm p-3 pr-2">
                    <span>Filtered Total</span>
                    <span>{formatMoneyValue(filteredTotal)}</span>
                  </SheetDescription>
                )}
              </div>
            </div>
          </div>
        </SheetHeader>

        <div className="px-6 pb-6">
          <div className="space-y-6">
            {/* Show Accounts section when there are visible items */}
            {visibleItems.length > 0 && (
              <div className="space-y-3">
                <h3 className="text-base font-semibold">Accounts</h3>
                <div className="space-y-3 px-3">
                  {Array.from(
                    new Set(visibleItems.map(item => item.accountRowId)),
                  ).map(accountId => (
                    <div key={accountId} className="flex items-center gap-3">
                      <div
                        className="w-3 h-3 rounded-full"
                        style={{
                          backgroundColor:
                            chartConfig[accountId]?.color || '#94a3b8',
                        }}
                      />
                      <span className="text-sm text-muted-foreground">
                        {chartConfig[accountId]?.label || 'Unknown Account'}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <h3 className="text-base font-semibold">Income Details</h3>
                <span className="text-sm bg-muted px-2 py-0.5 rounded-md">
                  {items.length} items
                </span>
              </div>
              {visibleItems.length === 0 ? (
                <div className="px-3 py-3 space-y-2 text-sm text-muted-foreground">
                  <p>All income is currently filtered out</p>
                  <p className="text-xs">
                    Unhide accounts in the chart to view income
                  </p>
                </div>
              ) : (
                <div className="space-y-2">
                  {visibleItems.map(item => {
                    return (
                      <div
                        key={item.rowId}
                        className="flex items-center justify-between p-3 pr-2 rounded-lg bg-muted/50"
                        style={{
                          borderLeft: `4px solid ${chartConfig[item.accountRowId]?.color || '#94a3b8'}`,
                        }}
                      >
                        <span className="text-sm">{item.description}</span>
                        <span className="text-sm font-medium min-w-[76px] text-right">
                          {formatMoneyValue(item.amount)}
                        </span>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}

export default IncomeDetails;
export type { IncomeDetailsProps };
