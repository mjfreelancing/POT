import { ChevronDown } from 'lucide-react';
import type { ReactNode } from 'react';
import { useState } from 'react';

import { Card, CardContent } from '@/components/ui/card';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { cn } from '@/lib/utils';

type CollapsibleSectionProps = {
  /** The title displayed in the section header */
  title: string;
  /** Icon displayed next to the title */
  icon: ReactNode;
  /** Whether the section is open by default */
  defaultOpen?: boolean;
  /** Content to display inside the collapsible section */
  children: ReactNode;
  /** Optional CSS classes for the card */
  className?: string;
};

/**
 * A collapsible section component for the dashboard.
 * Wraps content in a card with an expandable/collapsible header.
 *
 * @example
 * <CollapsibleSection
 *   title="Accounts Overview"
 *   icon={<PieChart className="h-5 w-5" />}
 *   defaultOpen={true}
 * >
 *   <div>Account content here</div>
 * </CollapsibleSection>
 */
function CollapsibleSection({
  title,
  icon,
  defaultOpen = true,
  children,
  className,
}: CollapsibleSectionProps) {
  const [isOpen, setIsOpen] = useState(defaultOpen);

  return (
    <Card className={cn('overflow-hidden border-2', className)}>
      <Collapsible open={isOpen} onOpenChange={setIsOpen}>
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
