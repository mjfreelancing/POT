import { localToday, normalizeToLocalMidnight } from './dateUtils';

// Table row styling constants
const TABLE_ROW_STYLES = {
  EXCLUDED: 'text-muted-foreground italic',
  OVERDUE: 'text-red-600 dark:text-red-400 italic',
} as const;

type TableRowItem = {
  excludeFromCalcs: boolean; // applicable to incomes, accounts, and expenses
  nextDue?: string; // applicable to incomes and expenses
};

function getTableRowClassName<T extends TableRowItem>(
  item: T,
): string | undefined {
  if (item.excludeFromCalcs) {
    return TABLE_ROW_STYLES.EXCLUDED;
  }

  if (item.nextDue && normalizeToLocalMidnight(item.nextDue) < localToday()) {
    return TABLE_ROW_STYLES.OVERDUE;
  }

  return undefined;
}

export { TABLE_ROW_STYLES, getTableRowClassName };
