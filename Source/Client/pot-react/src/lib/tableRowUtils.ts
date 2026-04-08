import { localToday, normalizeToLocalMidnight } from './dateUtils';

// Table row styling constants
const TABLE_ROW_STYLES = {
  EXCLUDED:
    'border-l-4 border-l-slate-400 dark:border-l-slate-500 pl-3 bg-slate-50/40 dark:bg-slate-900/40 text-slate-700 dark:text-slate-400',
  OVERDUE: '',
} as const;

// Only applicable to incomes and expenses
type TableRowItem = {
  excludeFromCalcs: boolean;
  nextDue?: string;
};

type StyleCheck = keyof typeof TABLE_ROW_STYLES;

type TableRowOptions = {
  exclude?: StyleCheck[];
};

function checkExcluded<T extends TableRowItem>(item: T): string | undefined {
  if (item.excludeFromCalcs) {
    return TABLE_ROW_STYLES.EXCLUDED;
  }

  return undefined;
}

function checkOverdue<T extends TableRowItem>(item: T): string | undefined {
  if (item.nextDue && normalizeToLocalMidnight(item.nextDue) <= localToday()) {
    return TABLE_ROW_STYLES.OVERDUE;
  }

  return undefined;
}

type StyleCheckFunction<T extends TableRowItem> = (
  item: T,
) => string | undefined;

// Map each style check to its corresponding check function
const styleCheckMap: Record<StyleCheck, StyleCheckFunction<TableRowItem>> = {
  EXCLUDED: checkExcluded,
  OVERDUE: checkOverdue,
};

function getTableRowClassName<T extends TableRowItem>(
  item: T,
  options?: TableRowOptions,
): string | undefined {
  const checksToExclude = new Set(options?.exclude ?? []);

  // Get all style check names that aren't excluded
  const checksToRun = (Object.keys(TABLE_ROW_STYLES) as StyleCheck[]).filter(
    check => !checksToExclude.has(check),
  );

  // Run each check in order until one returns a style
  for (const check of checksToRun) {
    const result = styleCheckMap[check](item);
    if (result) {
      return result;
    }
  }

  return undefined;
}

export {
  getTableRowClassName,
  type StyleCheck,
  TABLE_ROW_STYLES,
  type TableRowOptions,
};
