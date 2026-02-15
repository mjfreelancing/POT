/**
 * Badge Styling System
 *
 * Provides centralized, type-safe badge styling utilities for consistent badge appearances
 * across the application. These helpers generate Tailwind className strings that can be
 * applied to any badge component, with styling decoupled from semantics.
 *
 * THREE MAIN PATTERNS:
 *
 * 1. getTableBadgeClass() - For table badges (frequency, end dates, etc.)
 *    - Consistent sizing: text-[12px], min-w-[80px]
 *    - Supports both filled and outline variants
 *    - Optional left margin for badges appearing after dates
 *
 * 2. getStatusBadgeClass() - For semantic status indicators (due dates)
 *    - Predefined status types: excluded, due-today, overdue, due-soon, ended
 *    - Always filled variant with left margin
 *    - Color meanings: red=overdue, amber=due today, orange=due soon, slate=excluded/ended
 *
 * 3. getBadgeClass() - For general-purpose badges (dashboard, custom locations)
 *    - Flexible styling for non-table contexts
 *    - Larger size (text-xs) and different spacing
 *
 * USAGE GUIDELINES:
 *
 * - Table columns (frequency, end date): Use getTableBadgeClass()
 * - Status indicators (next due): Use getStatusBadgeClass()
 * - Dashboard/general use: Use getBadgeClass()
 *
 * COLOR PALETTE:
 * - red: Errors, overdue items
 * - orange: Warnings, due soon
 * - amber: Due today
 * - green: Success, active
 * - yellow: Caution
 * - blue: Informational
 * - purple: Special/unique
 * - pink: Highlight
 * - slate: Disabled, excluded, ended, neutral
 *
 * VARIANTS:
 * - filled: Bold backgrounds, high contrast (status badges)
 * - outline: Subtle borders, light backgrounds (informational badges)
 *
 * @example Table Frequency Badge
 * ```tsx
 * <Badge variant="secondary" className={getTableBadgeClass('green', 'filled')}>
 *   1 Week
 * </Badge>
 * ```
 *
 * @example End Date Badge (outline style)
 * ```tsx
 * <Badge variant="secondary" className={getTableBadgeClass('blue', 'outline')}>
 *   {formatDate(endDate)}
 * </Badge>
 * ```
 *
 * @example Status Badge (predefined semantic)
 * ```tsx
 * <Badge variant="destructive" className={getStatusBadgeClass('overdue')}>
 *   Overdue
 * </Badge>
 * ```
 *
 * @example Dashboard Badge (general purpose)
 * ```tsx
 * <Badge variant="default" className={getBadgeClass('blue', 'outline')}>
 *   Custom Badge
 * </Badge>
 * ```
 */

type StatusBadgeType =
  | 'excluded'
  | 'due-today'
  | 'overdue'
  | 'due-soon'
  | 'ended';
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

type TableBadgeOptions = {
  withMargin?: boolean;
};

const baseFilledStyles =
  'ml-2 text-[12px] px-2 py-0.5 min-w-[80px] justify-center';

const baseOutlineStyles = 'text-xs justify-center border';

const tableBadgeBaseClass = 'text-[12px] px-2 py-1 min-w-[80px] justify-center';
const tableBadgeWithMarginClass = `ml-2 ${tableBadgeBaseClass}`;

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
    slate: 'bg-slate-400 text-white dark:bg-slate-600 dark:text-slate-100',
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
      'bg-slate-50 text-slate-700 border-slate-200 dark:bg-slate-950 dark:text-slate-300 dark:border-slate-600',
  },
} as const;

function getBadgeColorClass(
  color: BadgeColor,
  variant: BadgeVariant = 'filled',
): string {
  return variant === 'filled'
    ? colorSchemes.filled[color as keyof typeof colorSchemes.filled] ||
        colorSchemes.filled.slate
    : colorSchemes.outline[color] || colorSchemes.outline.slate;
}

/**
 * Gets className for table badges with consistent sizing and spacing.
 *
 * PRIMARY USE: Frequency columns, end date columns, and other table badges that need
 * uniform appearance across rows.
 *
 * SIZING: text-[12px], px-2, py-1, min-w-[80px]
 *
 * @param color - Badge color (red, orange, amber, green, yellow, blue, purple, pink, slate)
 * @param variant - Style variant:
 *   - 'filled': Bold background, high contrast (default) - use for emphasis
 *   - 'outline': Light background, subtle border - use for informational
 * @param options - Optional configuration:
 *   - withMargin: Adds ml-2 (left margin) - use when badge appears after date text
 *
 * @example Frequency Badge (filled, no margin)
 * ```tsx
 * <Badge variant="secondary" className={getTableBadgeClass('green', 'filled')}>
 *   2 Weeks
 * </Badge>
 * ```
 *
 * @example End Date Badge (outline, no margin)
 * ```tsx
 * <Badge variant="secondary" className={getTableBadgeClass('blue', 'outline')}>
 *   {formatDate(endDate)}
 * </Badge>
 * ```
 *
 * @example Excluded Badge (filled, no margin)
 * ```tsx
 * <Badge variant="secondary" className={getTableBadgeClass('slate', 'filled')}>
 *   One-time
 * </Badge>
 * ```
 */
function getTableBadgeClass(
  color: BadgeColor,
  variant: BadgeVariant = 'filled',
  options?: TableBadgeOptions,
): string {
  const baseClass = options?.withMargin
    ? tableBadgeWithMarginClass
    : tableBadgeBaseClass;

  return `${baseClass} ${getBadgeColorClass(color, variant)}`;
}

/**
 * Gets className for semantic status badges with predefined color/style mappings.
 *
 * PRIMARY USE: Next Due column badges that convey status meaning (Overdue, Due Soon, etc.)
 *
 * ALWAYS: Filled variant with left margin (ml-2) - appears after date text
 *
 * STATUS TYPES & COLORS:
 * - 'excluded': Slate - Item excluded from calculations
 * - 'due-today': Amber - Due today (high visibility)
 * - 'overdue': Red - Past due date (urgent)
 * - 'due-soon': Orange - Due within 7 days (warning)
 * - 'ended': Slate - Item has ended/expired
 *
 * WHY USE THIS: Enforces consistent color meanings for status across the application.
 * Prevents accidental misuse of colors (e.g., using green for overdue).
 *
 * @param badgeType - The semantic status type
 *
 * @example Overdue Badge
 * ```tsx
 * <Badge variant="destructive" className={getStatusBadgeClass('overdue')}>
 *   Overdue
 * </Badge>
 * ```
 *
 * @example Due Today Badge
 * ```tsx
 * <Badge variant="default" className={getStatusBadgeClass('due-today')}>
 *   Due Today
 * </Badge>
 * ```
 *
 * @example Excluded Badge
 * ```tsx
 * <Badge variant="secondary" className={getStatusBadgeClass('excluded')}>
 *   Excluded
 * </Badge>
 * ```
 */
function getStatusBadgeClass(badgeType: StatusBadgeType): string {
  switch (badgeType) {
    case 'excluded':
      return getTableBadgeClass('slate', 'filled', { withMargin: true });
    case 'due-today':
      return getTableBadgeClass('amber', 'filled', { withMargin: true });
    case 'overdue':
      return getTableBadgeClass('red', 'filled', { withMargin: true });
    case 'due-soon':
      return getTableBadgeClass('orange', 'filled', { withMargin: true });
    case 'ended':
      return getTableBadgeClass('slate', 'filled', { withMargin: true });
  }
}

/**
 * Gets className for general-purpose badges outside of table contexts.
 *
 * PRIMARY USE: Dashboard badges, custom locations, non-table badges
 *
 * SIZING: Larger than table badges - text-xs for filled, min-w-[80px]
 * SPACING: Includes ml-2 for filled variant
 *
 * WHEN TO USE:
 * - Dashboard cards
 * - Custom components outside tables
 * - When you need flexible badge styling not bound to table constraints
 *
 * WHEN NOT TO USE:
 * - Table columns → Use getTableBadgeClass() instead
 * - Status indicators in tables → Use getStatusBadgeClass() instead
 *
 * @param color - The color of the badge (red, orange, amber, green, yellow, blue, purple, pink, slate)
 * @param variant - The style variant:
 *   - 'filled': Bold background with white text, includes ml-2 margin (default)
 *   - 'outline': Light background with colored text and border, no margin
 *
 * @example Dashboard Badge (filled)
 * ```tsx
 * <Badge variant="default" className={getBadgeClass('blue', 'filled')}>
 *   Total: $5,000
 * </Badge>
 * ```
 *
 * @example Custom Outline Badge
 * ```tsx
 * <Badge variant="secondary" className={getBadgeClass('green', 'outline')}>
 *   Active
 * </Badge>
 * ```
 */
function getBadgeClass(
  color: BadgeColor,
  variant: BadgeVariant = 'filled',
): string {
  const baseStyle = variant === 'filled' ? baseFilledStyles : baseOutlineStyles;
  const colorStyle = getBadgeColorClass(color, variant);

  return `${baseStyle} ${colorStyle}`;
}

export { getBadgeClass, getStatusBadgeClass, getTableBadgeClass };
export type { BadgeColor, BadgeVariant, StatusBadgeType, TableBadgeOptions };
