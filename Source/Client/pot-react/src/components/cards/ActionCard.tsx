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
  const canCallAction = enabled && onClick !== undefined;

  function handleKeyDown(event: React.KeyboardEvent<HTMLDivElement>) {
    if (canCallAction) {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        onClick?.();
      }
    }
  }

  const mainContent = (
    <div className="flex items-center justify-start gap-3 w-full">
      <div className="p-1.5 lg:p-2 rounded-lg bg-primary/5 border border-primary/30 flex-shrink-0 flex items-center justify-center [&>svg]:h-4 [&>svg]:w-4 lg:[&>svg]:h-5 lg:[&>svg]:w-5">
        {icon}
      </div>
      <div className="flex items-center justify-center text-center flex-1 min-w-0 px-1.5 lg:px-0">
        <div
          role="heading"
          aria-level={1}
          className="text-base lg:text-lg font-medium leading-tight lg:leading-snug max-w-[6.25rem] lg:max-w-none"
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
        'py-1 gap-0',
        'border-2 border-yellow-500/40 bg-yellow-50/30 dark:bg-yellow-900/10',
        'min-h-[54px] lg:min-h-[60px]',
        canCallAction && [
          'border-yellow-500/60 shadow-sm lg:shadow-none',
          'cursor-pointer hover:shadow-lg hover:scale-[1.02] hover:border-primary/80 hover:shadow-primary/10',
          'active:scale-[0.99] active:shadow-md lg:active:scale-[1.02] lg:active:shadow-lg',
        ],
        !enabled && [
          'border-slate-300/80 dark:border-slate-700/80',
          'bg-slate-100/70 dark:bg-slate-900/45',
          'text-slate-500 dark:text-slate-400',
          '[&_svg]:opacity-70 [&_svg]:saturate-0',
          'shadow-none cursor-not-allowed',
        ], // Add disabled style
        className,
      )}
      onClick={canCallAction ? onClick : undefined}
    >
      <CardContent className="h-full flex items-center justify-start px-4 py-2.5 lg:py-3.5">
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
