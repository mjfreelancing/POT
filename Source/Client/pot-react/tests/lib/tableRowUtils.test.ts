import { describe, expect, test } from 'vitest';

import { getTableRowClassName, TABLE_ROW_STYLES } from '@/lib';

describe('Table Row Utils', () => {
  test('should return EXCLUDED style when item is excluded', () => {
    const result = getTableRowClassName({
      excludeFromCalcs: true,
      nextDue: '2099-12-31',
    });

    expect(result).toBe(TABLE_ROW_STYLES.EXCLUDED);
  });

  test('should return no additional class when EXCLUDED check is excluded via options', () => {
    const result = getTableRowClassName(
      {
        excludeFromCalcs: true,
        nextDue: '2099-12-31',
      },
      { exclude: ['EXCLUDED'] },
    );

    expect(result).toBeUndefined();
  });

  test('should return no additional class for non-excluded item with no due date', () => {
    const result = getTableRowClassName({
      excludeFromCalcs: false,
    });

    expect(result).toBeUndefined();
  });

  test('should return no additional class for future due date when not excluded', () => {
    const result = getTableRowClassName({
      excludeFromCalcs: false,
      nextDue: '2099-12-31',
    });

    expect(result).toBeUndefined();
  });

  test('should return no additional class for overdue item when OVERDUE style is not configured', () => {
    const result = getTableRowClassName({
      excludeFromCalcs: false,
      nextDue: '2000-01-01',
    });

    expect(result).toBeUndefined();
  });

  test('should prioritize EXCLUDED style before other checks', () => {
    const result = getTableRowClassName({
      excludeFromCalcs: true,
      nextDue: '2000-01-01',
    });

    expect(result).toBe(TABLE_ROW_STYLES.EXCLUDED);
  });
});
