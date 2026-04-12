import { describe, expect, test } from 'vitest';

import {
  compareDates,
  normalizeToEpoch,
  normalizeToLocalMidnight,
} from '@/lib';

describe('Date Utils', () => {
  describe('normalizeToLocalMidnight', () => {
    test('should normalize Date input to local midnight', () => {
      const inputDate = new Date(2026, 3, 12, 17, 45, 30, 120);

      const result = normalizeToLocalMidnight(inputDate);

      expect(result.getFullYear()).toBe(2026);
      expect(result.getMonth()).toBe(3);
      expect(result.getDate()).toBe(12);
      expect(result.getHours()).toBe(0);
      expect(result.getMinutes()).toBe(0);
      expect(result.getSeconds()).toBe(0);
      expect(result.getMilliseconds()).toBe(0);
    });

    test('should normalize parseable string input to local midnight', () => {
      const result = normalizeToLocalMidnight('2026-04-12T23:59:59');

      expect(result.getFullYear()).toBe(2026);
      expect(result.getMonth()).toBe(3);
      expect(result.getDate()).toBe(12);
      expect(result.getHours()).toBe(0);
      expect(result.getMinutes()).toBe(0);
      expect(result.getSeconds()).toBe(0);
      expect(result.getMilliseconds()).toBe(0);
    });
  });

  describe('normalizeToEpoch', () => {
    test('should return same epoch for same calendar day regardless of time', () => {
      const morning = new Date(2026, 3, 12, 1, 5, 0);
      const evening = new Date(2026, 3, 12, 22, 40, 0);

      const morningEpoch = normalizeToEpoch(morning);
      const eveningEpoch = normalizeToEpoch(evening);

      expect(morningEpoch).toBe(eveningEpoch);
    });

    test('should match midnight Date epoch for string input on same day', () => {
      const expectedEpoch = new Date(2026, 3, 12, 0, 0, 0, 0).getTime();

      const result = normalizeToEpoch('2026-04-12T15:30:45');

      expect(result).toBe(expectedEpoch);
    });
  });

  describe('compareDates', () => {
    test('should return negative when left date is earlier', () => {
      const result = compareDates('2026-04-11', '2026-04-12');

      expect(result).toBeLessThan(0);
    });

    test('should return positive when left date is later', () => {
      const result = compareDates('2026-04-13', '2026-04-12');

      expect(result).toBeGreaterThan(0);
    });

    test('should return zero when calendar date is equal and time differs', () => {
      const result = compareDates(
        new Date(2026, 3, 12, 1, 0, 0),
        new Date(2026, 3, 12, 23, 59, 59),
      );

      expect(result).toBe(0);
    });
  });
});
