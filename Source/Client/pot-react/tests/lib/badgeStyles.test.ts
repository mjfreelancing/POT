import { describe, expect, test } from 'vitest';

import { getBadgeClass, getStatusBadgeClass, getTableBadgeClass } from '@/lib';

describe('badgeStyles', () => {
  describe('getTableBadgeClass', () => {
    test('returns filled table style without margin by default', () => {
      const className = getTableBadgeClass('green');

      expect(className).toContain(
        'text-[12px] px-2 py-1 min-w-[80px] justify-center',
      );
      expect(className).toContain(
        'bg-green-500 text-white dark:bg-green-600 dark:text-green-100',
      );

      expect(className).not.toContain(
        'ml-2 text-[12px] px-2 py-1 min-w-[80px] justify-center',
      );
    });

    test('returns outline table style with margin when requested', () => {
      const className = getTableBadgeClass('blue', 'outline', {
        withMargin: true,
      });

      expect(className).toContain(
        'ml-2 text-[12px] px-2 py-1 min-w-[80px] justify-center',
      );

      expect(className).toContain(
        'bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950 dark:text-blue-300 dark:border-blue-800',
      );
    });
  });

  describe('getStatusBadgeClass', () => {
    test('maps excluded status to dedicated excluded style', () => {
      expect(getStatusBadgeClass('excluded')).toBe(
        'ml-2 text-[12px] px-2 py-1 min-w-[80px] justify-center bg-slate-200 text-slate-700 dark:bg-slate-700 dark:text-slate-300',
      );
    });

    test('maps due-today status to amber filled table style with margin', () => {
      expect(getStatusBadgeClass('due-today')).toBe(
        getTableBadgeClass('amber', 'filled', { withMargin: true }),
      );
    });

    test('maps overdue status to red filled table style with margin', () => {
      expect(getStatusBadgeClass('overdue')).toBe(
        getTableBadgeClass('red', 'filled', { withMargin: true }),
      );
    });

    test('maps due-soon status to orange filled table style with margin', () => {
      expect(getStatusBadgeClass('due-soon')).toBe(
        getTableBadgeClass('orange', 'filled', { withMargin: true }),
      );
    });

    test('maps ended status to slate filled table style with margin', () => {
      expect(getStatusBadgeClass('ended')).toBe(
        getTableBadgeClass('slate', 'filled', { withMargin: true }),
      );
    });
  });

  describe('getBadgeClass', () => {
    test('returns filled badge style by default', () => {
      const className = getBadgeClass('purple');

      expect(className).toContain(
        'ml-2 text-[12px] px-2 py-0.5 min-w-[80px] justify-center',
      );
      expect(className).toContain(
        'bg-purple-500 text-white dark:bg-purple-600 dark:text-purple-100',
      );
    });

    test('returns outline badge style without filled margin class', () => {
      const className = getBadgeClass('pink', 'outline');

      expect(className).toContain('text-xs justify-center border');
      expect(className).toContain(
        'bg-pink-50 text-pink-700 border-pink-200 dark:bg-pink-950 dark:text-pink-300 dark:border-pink-800',
      );

      expect(className).not.toContain(
        'ml-2 text-[12px] px-2 py-0.5 min-w-[80px] justify-center',
      );
    });
  });
});
