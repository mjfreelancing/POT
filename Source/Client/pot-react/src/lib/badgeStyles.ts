/**
 * Badge styling helpers for creating consistent badge appearances.
 * These helpers generate Tailwind className strings that can be applied to any badge component.
 * The caller provides the text content - styling is decoupled from semantics.
 */

type StatusBadgeType = 'excluded' | 'due-today' | 'overdue' | 'due-soon';
type BadgeVariant = 'filled' | 'outline';
type BadgeColor =
  | 'red'
  | 'orange'
  | 'amber'
  | 'green'
  | 'yellow'
  | 'blue'
  | 'purple'
  | 'pink'
  | 'slate';

const baseFilledStyles =
  'ml-2 text-[11px] px-2 py-0.5 min-w-[80px] justify-center';

const baseOutlineStyles = 'text-xs justify-center border';

const colorSchemes = {
  filled: {
    red: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 border-red-300 dark:border-red-800',
    orange: 'bg-orange-500 text-white dark:bg-orange-600 dark:text-orange-100',
    amber: 'bg-amber-500 text-white dark:bg-amber-600 dark:text-amber-50',
    green: 'bg-green-500 text-white dark:bg-green-600 dark:text-green-100',
    yellow: 'bg-yellow-500 text-white dark:bg-yellow-600 dark:text-yellow-100',
    blue: 'bg-blue-500 text-white dark:bg-blue-600 dark:text-blue-100',
    purple: 'bg-purple-500 text-white dark:bg-purple-600 dark:text-purple-100',
    pink: 'bg-pink-500 text-white dark:bg-pink-600 dark:text-pink-100',
    slate: 'bg-slate-500 text-white dark:bg-slate-600 dark:text-slate-100',
  },
  outline: {
    red: 'bg-red-50 text-red-700 border-red-200 dark:bg-red-950 dark:text-red-300 dark:border-red-800',
    orange:
      'bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950 dark:text-orange-300 dark:border-orange-800',
    amber:
      'bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950 dark:text-amber-300 dark:border-amber-800',
    green:
      'bg-green-50 text-green-700 border-green-200 dark:bg-green-950 dark:text-green-300 dark:border-green-800',
    yellow:
      'bg-yellow-50 text-yellow-700 border-yellow-200 dark:bg-yellow-950 dark:text-yellow-300 dark:border-yellow-800',
    blue: 'bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950 dark:text-blue-300 dark:border-blue-800',
    purple:
      'bg-purple-50 text-purple-700 border-purple-200 dark:bg-purple-950 dark:text-purple-300 dark:border-purple-800',
    pink: 'bg-pink-50 text-pink-700 border-pink-200 dark:bg-pink-950 dark:text-pink-300 dark:border-pink-800',
    slate:
      'bg-slate-50 text-slate-700 border-slate-200 dark:bg-slate-950 dark:text-slate-300 dark:border-slate-800',
  },
} as const;

/**
 * Gets className for a specific status badge type (filled variant with semantic meaning).
 * Used in tables for status indicators like "Due Today", "Overdue", etc.
 */
function getStatusBadgeClass(badgeType: StatusBadgeType): string {
  switch (badgeType) {
    case 'excluded':
      return `${baseFilledStyles} ${colorSchemes.filled.red}`;
    case 'due-today':
      return `${baseFilledStyles} ${colorSchemes.filled.amber}`;
    case 'overdue':
      return `${baseFilledStyles} ${colorSchemes.filled.red}`;
    case 'due-soon':
      return `${baseFilledStyles} ${colorSchemes.filled.orange}`;
  }
}

/**
 * Gets className for a generic badge with specified color and variant.
 * Useful for dashboard or other badges where you want to parameterize styling separately from content.
 * @param color - The color of the badge
 * @param variant - The style variant: 'filled' for bold backgrounds, 'outline' for light backgrounds
 */
function getBadgeClass(
  color: BadgeColor,
  variant: BadgeVariant = 'filled',
): string {
  const baseStyle = variant === 'filled' ? baseFilledStyles : baseOutlineStyles;
  const colorStyle =
    variant === 'filled'
      ? colorSchemes.filled[color as keyof typeof colorSchemes.filled] ||
        colorSchemes.filled.slate
      : colorSchemes.outline[color] || colorSchemes.outline.slate;

  return `${baseStyle} ${colorStyle}`;
}

export { getBadgeClass, getStatusBadgeClass };
export type { BadgeColor, BadgeVariant, StatusBadgeType };
