import { describe, expect, test } from 'vitest';

import { FORM_SHEET_STYLES } from '@/lib';

describe('FORM_SHEET_STYLES', () => {
  test('should expose all expected style keys', () => {
    expect(Object.keys(FORM_SHEET_STYLES)).toEqual([
      'SHEET_CONTENT',
      'SHEET_INNER',
      'ACTION_SECTION',
      'ACTION_ROW',
      'ACTION_BUTTON_WIDTH',
      'DATE_ROW',
      'DATE_TRIGGER',
      'DATE_ACTION_ROW',
      'DATE_ACTION_BUTTON',
    ]);
  });

  test('should keep expected class values for sheet and action layout', () => {
    expect(FORM_SHEET_STYLES.SHEET_CONTENT).toBe(
      'p-6 sm:max-w-lg [&>button:first-of-type]:hidden overflow-y-auto',
    );

    expect(FORM_SHEET_STYLES.SHEET_INNER).toBe('space-y-6 pr-6 pl-6');
    expect(FORM_SHEET_STYLES.ACTION_SECTION).toBe('space-y-4 pt-2');
    expect(FORM_SHEET_STYLES.ACTION_ROW).toBe('flex justify-end space-x-4');
    expect(FORM_SHEET_STYLES.ACTION_BUTTON_WIDTH).toBe('w-24');
  });

  test('should keep expected class values for date row layout', () => {
    expect(FORM_SHEET_STYLES.DATE_ROW).toBe(
      'flex flex-col gap-0 sm:flex-row sm:items-center sm:gap-2',
    );

    expect(FORM_SHEET_STYLES.DATE_TRIGGER).toBe('w-full min-w-0 sm:flex-1');
    expect(FORM_SHEET_STYLES.DATE_ACTION_ROW).toBe(
      '-mt-0.5 flex w-full justify-end sm:mt-0 sm:w-auto sm:flex-none',
    );

    expect(FORM_SHEET_STYLES.DATE_ACTION_BUTTON).toBe('sm:w-16');
  });
});
