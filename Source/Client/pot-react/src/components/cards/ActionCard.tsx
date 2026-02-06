import { RotateCw } from 'lucide-react';
import type { ReactNode } from 'react';

import { Card, CardContent } from '@/components/ui/card';
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import { cn } from '@/lib/utils';

/**
 * Props for the ActionCard component.
 */
type ActionCardProps = {
  /** The icon to display. Can be any React element (typically a Lucide icon) */
  icon: ReactNode;
  /** The main title text displayed prominently in the card */
  title: string;
  /** Additional CSS classes to apply to the card for custom styling */
  className?: string;
  /** Optional click handler. When provided, the card becomes visually interactive with hover effects, cursor pointer, and clickable behavior */
  onClick?: () => void;
  /** Optional boolean to indicate if the card is enabled. Defaults to true. */
  enabled?: boolean;
  /** Optional hint text displayed in a tooltip when hovering over the card */
  hint?: string;
};

/**
 * A reusable card component for displaying actionable content with an icon and title.
 * The card uses a horizontal layout with icon on the left and text on the right.
 * Automatically becomes visually interactive (hover effects, cursor pointer) when an onClick handler is provided.
 *
 * @param props - The component props
 * @param props.icon - The icon to display. Can be any React element (typically a Lucide icon)
 * @param props.title - The main title text displayed prominently in the card
 * @param props.className - Additional CSS classes to apply to the card for custom styling
 * @param props.onClick - Optional click handler. When provided, the card becomes visually interactive with hover effects, cursor pointer, and clickable behavior
 * @param props.enabled - Optional boolean to indicate if the card is enabled. When false, the card appears disabled and is not interactive
 * @param props.hint - Optional text displayed in a tooltip when hovering over the card's content area
 *
 * @example
 * // Basic card with icon and title
 * <ActionCard
 *   icon={<Plus className="h-5 w-5" />}
 *   title="Add New Item"
 *   onClick={() => console.log('Card clicked')}
 * />
 */
function ActionCard({
  icon,
  title,
  className,
  onClick,
  enabled = true,
  hint,
}: ActionCardProps) {
  const canCallAction = enabled && onClick;

  function handleKeyDown(event: React.KeyboardEvent<HTMLDivElement>) {
    if (canCallAction) {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        onClick();
      }
    }
  }

  const mainContent = (
    <div className="flex items-center gap-3 flex-1 w-full">
      <div className="p-2 lg:p-3 rounded-lg bg-primary/5 flex-shrink-0 flex items-center justify-center [&>svg]:h-5 [&>svg]:w-5 lg:[&>svg]:h-6 lg:[&>svg]:w-6">
        {icon}
      </div>
      <div className="flex items-center justify-center text-center flex-1 min-w-0 pr-8">
        <div
          role="heading"
          aria-level={1}
          className="text-base lg:text-lg font-medium leading-snug"
        >
          {title}
        </div>
      </div>
    </div>
  );

  return (
    <Card
      role={canCallAction ? 'button' : undefined}
      tabIndex={canCallAction ? 0 : undefined}
      onKeyDown={canCallAction ? handleKeyDown : undefined}
      className={cn(
        'transition-all duration-200',
        'border-2 border-yellow-500/40 bg-yellow-50/30 dark:bg-yellow-900/10',
        'min-h-[60px]',
        canCallAction && [
          'cursor-pointer hover:shadow-lg hover:scale-[1.02] hover:border-primary/80 hover:shadow-primary/10',
        ],
        !enabled && 'opacity-50 cursor-not-allowed', // Add disabled style
        className,
        'relative', // Make Card relative for absolute positioning of the icon when clickable
      )}
      onClick={canCallAction ? onClick : undefined}
    >
      {canCallAction && (
        <RotateCw
          className="h-4 w-4 text-muted-foreground absolute right-4 top-4 z-10 cursor-pointer"
          aria-hidden="true"
        />
      )}
      <CardContent className="h-full flex items-center px-3 py-3 justify-start">
        {hint && enabled ? (
          <Tooltip>
            <TooltipTrigger asChild>{mainContent}</TooltipTrigger>
            <TooltipContent
              side="bottom"
              className="max-w-[300px] whitespace-pre-line"
            >
              {hint}
            </TooltipContent>
          </Tooltip>
        ) : (
          mainContent
        )}
      </CardContent>
    </Card>
  );
}

export default ActionCard;
export type { ActionCardProps };
