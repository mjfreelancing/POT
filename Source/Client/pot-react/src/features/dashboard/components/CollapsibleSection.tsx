import { ChevronDown } from 'lucide-react';
import type { ReactNode } from 'react';

import { Card, CardContent } from '@/components/ui/card';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { cn } from '@/lib';

type CollapsibleSectionProps = {
  /** The title displayed in the section header */
  title: string;
  /** Icon displayed next to the title */
  icon: ReactNode;
  /** Content to display inside the collapsible section */
  children: ReactNode;
  /** Optional CSS classes for the card */
  className?: string;
  /** Controlled open state */
  isOpen: boolean;
  /** Callback when open state changes */
  onOpenChange: (isOpen: boolean) => void;
};

/**
 * A collapsible section component for the dashboard.
 * Wraps content in a card with an expandable/collapsible header.
 *
 * This is a controlled component - state is managed by the parent.
 *
 * @example
 * <CollapsibleSection
 *   title="Accounts Overview"
 *   icon={<PieChart className="h-5 w-5" />}
 *   isOpen={accountsOpen}
 *   onOpenChange={setAccountsOpen}
 * >
 *   <div>Account content here</div>
 * </CollapsibleSection>
 */
function CollapsibleSection({
  title,
  icon,
  children,
  className,
  isOpen,
  onOpenChange,
}: CollapsibleSectionProps) {
  return (
    <Card className={cn('overflow-hidden border-2', className)}>
      <Collapsible open={isOpen} onOpenChange={onOpenChange}>
        <CollapsibleTrigger className="w-full">
          <div className="flex items-center justify-between px-4 py-4 bg-slate-100 dark:bg-slate-800/40 hover:bg-slate-200 dark:hover:bg-slate-700/40 transition-colors border-b">
            <div className="flex items-center gap-3">
              <div className="flex items-center justify-center p-2 rounded-lg bg-primary/10">
                {icon}
              </div>
              <div className="text-left">
                <h2 className="text-xl font-bold">{title}</h2>
              </div>
            </div>
            <ChevronDown
              className={cn(
                'h-6 w-6 text-muted-foreground transition-transform duration-200',
                isOpen ? 'transform rotate-180' : '',
              )}
              aria-hidden="true"
            />
          </div>
        </CollapsibleTrigger>
        <CollapsibleContent>
          <CardContent className="px-4 pb-0 pt-4">{children}</CardContent>
        </CollapsibleContent>
      </Collapsible>
    </Card>
  );
}

export default CollapsibleSection;
export type { CollapsibleSectionProps };
