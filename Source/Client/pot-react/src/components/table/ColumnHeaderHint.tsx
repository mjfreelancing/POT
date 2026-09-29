import type { ReactNode } from 'react';

import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

type ColumnHeaderHintProps = {
  /** Hint shown on hover. When omitted the children are rendered untouched. */
  hint?: string;
  children: ReactNode;
};

// Shows a column heading's hint on hover. The provider is local and carries no delay so a heading
// behaves the same wherever a table is rendered and one hint does not hold up the next.
function ColumnHeaderHint({ hint, children }: ColumnHeaderHintProps) {
  if (!hint) {
    return children;
  }

  return (
    <TooltipProvider delayDuration={0}>
      <Tooltip>
        <TooltipTrigger asChild>{children}</TooltipTrigger>

        {/* The tooltip primitive carries `text-balance`, which re-balances the line lengths whenever a
            hint wraps and so reads as though it wrapped early. `text-wrap` restores ordinary greedy
            wrapping, and `max-w-sm` leaves the longer hints room to stay on one line. */}
        <TooltipContent className="max-w-sm text-wrap">{hint}</TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}

export default ColumnHeaderHint;
export type { ColumnHeaderHintProps };
