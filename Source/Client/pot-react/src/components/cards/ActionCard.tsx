import { RotateCw } from 'lucide-react';
import type { JSX, ReactNode } from 'react';

import { Card, CardContent } from '@/components/ui/card';
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import { cn } from '@/lib/utils';

import Spinner from '../feedback/spinner/LoadingSpinner';

/**
 * Component for handling the display of description text or loading message
 */
function DescriptionContent({
  text,
  isLoading,
}: {
  text?: string;
  isLoading?: boolean;
}): JSX.Element | null {
  if (!text) {
    return null;
  }

  if (isLoading) {
    return (
      <div className="mt-2">
        <Spinner />
      </div>
    );
  }

  return <div className="text-sm text-muted-foreground mt-1">{text}</div>;
}

/**
 * Props for the ActionCard component.
 */
type ActionCardProps = {
  /** The icon to display on the left side of the card. Can be any React element (typically a Lucide icon) */
  icon: ReactNode;
  /** The main title text displayed prominently in the card */
  title: string;
  /** Optional description text displayed below the title in muted colors. Hidden on mobile, visible on desktop screens. */
  description?: string;
  /** Optional boolean to indicate data is loading. The description will be replaced with a loading indicator when true. */
  isLoading?: boolean;
  /** Additional CSS classes to apply to the card for custom styling */
  className?: string;
  /** Optional additional content to display below the description */
  children?: ReactNode;
  /** Optional click handler. When provided, the card becomes visually interactive with hover effects, cursor pointer, and clickable behavior */
  onClick?: () => void;
  /** Optional boolean to indicate if the card is enabled. Defaults to true. */
  enabled?: boolean;
  /** Optional hint text displayed in a tooltip when hovering over the card */
  hint?: string;
};

/**
 * A reusable card component for displaying actionable content with an icon, title, and optional description.
 * The card automatically becomes visually interactive (hover effects, cursor pointer) when an onClick handler is provided.
 *
 * @param props - The component props
 * @param props.icon - The icon to display on the left side of the card. Can be any React element (typically a Lucide icon)
 * @param props.title - The main title text displayed prominently in the card
 * @param props.description - Optional description text displayed below the title in muted colors
 * @param props.isLoading - Optional boolean to indicate data is loading. The description will be replaced with a loading indicator when true
 * @param props.className - Additional CSS classes to apply to the card for custom styling
 * @param props.children - Optional additional content to display below the description
 * @param props.onClick - Optional click handler. When provided, the card becomes visually interactive with hover effects, cursor pointer, and clickable behavior
 * @param props.enabled - Optional boolean to indicate if the card is enabled. When false, the card appears disabled and is not interactive
 * @param props.hint - Optional text displayed in a tooltip when hovering over the card's content area
 *
 * @example
 * // Basic card with icon and title
 * <ActionCard
 *   icon={<Plus className="h-5 w-5" />}
 *   title="Add New Item"
 *   description="Create a new item in the system"
 *   onClick={() => console.log('Card clicked')}
 * />
 *
 * @example
 * // Card with additional content
 * <ActionCard
 *   icon={<Settings className="h-5 w-5" />}
 *   title="Settings"
 *   description="Configure account preferences"
 * >
 *   <div>Some additional information</div>
 * </ActionCard>
 */
function ActionCard({
  icon,
  title,
  description,
  isLoading,
  className,
  children,
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
    <div className="flex flex-col justify-center text-center sm:text-left flex-1">
      <div
        role="heading"
        aria-level={1}
        className="text-xs sm:text-base font-medium leading-tight"
      >
        {title}
      </div>
      <div className="hidden sm:block">
        <DescriptionContent text={description} isLoading={isLoading} />
      </div>
      {children}
    </div>
  );

  return (
    <Card
      role={canCallAction ? 'button' : undefined}
      tabIndex={canCallAction ? 0 : undefined}
      onKeyDown={canCallAction ? handleKeyDown : undefined}
      className={cn(
        'border-l border-primary/40 bg-slate-100 dark:bg-slate-900 transition-all duration-200',
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
      <CardContent className="p-2 sm:p-2.5 h-full flex items-center justify-center">
        <div className="flex flex-col items-center justify-center w-full gap-1 sm:gap-2">
          <div className="p-1.5 sm:p-2.5 rounded-lg bg-primary/5 flex-shrink-0 flex items-center justify-center">
            {icon}
          </div>
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
        </div>
      </CardContent>
    </Card>
  );
}

export default ActionCard;
export type { ActionCardProps };
