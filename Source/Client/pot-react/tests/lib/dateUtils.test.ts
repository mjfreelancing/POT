import {
  afterAll,
  afterEach,
  beforeAll,
  describe,
  expect,
  test,
  vi,
} from 'vitest';

import {
  compareDates,
  getDaysDue,
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

  describe('getDaysDue', () => {
    // Pinned to a DST timezone: the contract under test is "number of CALENDAR
    // days", which only diverges from millisecond arithmetic when the UTC offset
    // changes inside the measured window (Australia springs forward on the first
    // Sunday in October). Without a DST timezone the guard would be vacuous.
    const originalTimeZone = process.env.TZ;

    beforeAll(() => {
      process.env.TZ = 'Australia/Sydney';
    });

    afterAll(() => {
      process.env.TZ = originalTimeZone;
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    function givenTodayIs(year: number, monthIndex: number, day: number): void {
      vi.useFakeTimers();
      vi.setSystemTime(new Date(year, monthIndex, day, 12, 0, 0));
    }

    test('counts calendar days across a spring-forward transition', () => {
      // 2026-10-04 is the Australian spring-forward; 2026-10-05 is two calendar
      // days after 2026-10-03 but only 47 wall-clock hours later.
      givenTodayIs(2026, 9, 3);

      expect(getDaysDue('2026-10-05')).toBe(2);
    });

    test('counts a due date exactly 30 calendar days out across a spring-forward', () => {
      givenTodayIs(2026, 9, 1);

      expect(getDaysDue('2026-10-31')).toBe(30);
    });

    test('counts calendar days across a fall-back transition', () => {
      // 2026-04-05 is the Australian fall-back; 2026-05-01 is 30 calendar days
      // after 2026-04-01 but 30 days and one hour of wall-clock time.
      givenTodayIs(2026, 3, 1);

      expect(getDaysDue('2026-05-01')).toBe(30);
    });

    test('returns zero for today and a negative count when overdue', () => {
      givenTodayIs(2026, 9, 5);

      expect(getDaysDue('2026-10-05')).toBe(0);
      expect(getDaysDue('2026-10-01')).toBe(-4);
    });
  });
});
