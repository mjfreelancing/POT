import type { ReactNode } from 'react';

import { Badge } from '@/components/ui/badge';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

/**
 * Available color variants for the StatusBadge component.
 * Each color provides consistent styling with light/dark mode support.
 */
type BadgeColor =
  'red' | 'orange' | 'green' | 'yellow' | 'blue' | 'purple' | 'gray';

/**
 * Props for the StatusBadge component.
 */
type StatusBadgeProps = {
  /** The color variant to apply to the badge. Defaults to 'gray' if not provided */
  color?: BadgeColor;
  /** The content to display inside the badge */
  children: ReactNode;
  /** Additional CSS classes to apply to the badge for custom styling */
  className?: string;
  /** Optional tooltip text to show when hovering over the badge */
  tooltip?: string;
  /** Optional click handler to make the badge interactive */
  onClick?: () => void;
};

const colorConfig = {
  red: 'text-xs justify-center bg-red-50 text-red-700 border-red-200 dark:bg-red-950 dark:text-red-300 dark:border-red-800',

  orange:
    'text-xs justify-center bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950 dark:text-orange-300 dark:border-orange-800',

  green:
    'text-xs justify-center bg-green-50 text-green-700 border-green-200 dark:bg-green-950 dark:text-green-300 dark:border-green-800',

  yellow:
    'text-xs justify-center bg-yellow-50 text-yellow-700 border-yellow-200 dark:bg-yellow-950 dark:text-yellow-300 dark:border-yellow-800',

  blue: 'text-xs justify-center bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950 dark:text-blue-300 dark:border-blue-800',

  purple:
    'text-xs justify-center bg-purple-50 text-purple-700 border-purple-200 dark:bg-purple-950 dark:text-purple-300 dark:border-purple-800',

  gray: 'text-xs justify-center bg-gray-50 text-gray-700 border-gray-200 dark:bg-gray-950 dark:text-gray-300 dark:border-gray-800',
} as const;

function StatusBadge({
  color,
  children,
  className,
  tooltip,
  onClick,
}: StatusBadgeProps) {
  const badgeClassName = color ? colorConfig[color] : colorConfig['gray'];

  const badge = (
    <Badge
      variant="outline"
      className={`${badgeClassName} ${className || ''}`}
      onClick={onClick}
    >
      {children}
    </Badge>
  );

  if (tooltip) {
    return (
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger>{badge}</TooltipTrigger>
          <TooltipContent>{tooltip}</TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );
  }

  return badge;
}

export default StatusBadge;
export type { BadgeColor, StatusBadgeProps };
