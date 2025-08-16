import { format } from 'date-fns';

/**
 * Returns a date at local midnight (00:00:00)
 * @param date - A Date object or a string that can be parsed by the Date constructor
 * @returns A new Date object set to midnight (00:00:00) in local time
 */
function normalizeToLocalMidnight(date: Date | string): Date {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  return new Date(dateObj.getFullYear(), dateObj.getMonth(), dateObj.getDate());
}

/**
 * Returns today as a date at local midnight (00:00:00)
 */
function localToday(): Date {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  return date;
}

/**
 * Compares two dates and returns their difference in milliseconds (ignoring time)
 * @param date1 - First date to compare (left-hand side)
 * @param date2 - Second date to compare (right-hand side)
 * @returns A negative number if date1 is earlier than date2, positive if date1 is later than date2, or zero if they are the same date
 *
 * @example
 * // Returns negative number (first date is earlier)
 * compareDates(new Date('2025-01-01'), new Date('2025-01-02'))
 *
 * // Returns positive number (first date is later)
 * compareDates(new Date('2025-01-02'), new Date('2025-01-01'))
 *
 * // Returns 0 (same date)
 * compareDates(new Date('2025-01-01T12:00:00'), new Date('2025-01-01T15:30:00'))
 */
function compareDates(date1: Date | string, date2: Date | string): number {
  const lhsDate = normalizeToLocalMidnight(date1);
  const rhsDate = normalizeToLocalMidnight(date2);

  return lhsDate.getTime() - rhsDate.getTime();
}

/**
 * Checks if two dates represent the same calendar date (ignoring time)
 * @param date1 - First date to compare
 * @param date2 - Second date to compare
 * @returns true if both dates represent the same calendar date
 */
function isSameDate(date1: Date | string, date2: Date | string): boolean {
  const normalized1 = normalizeToLocalMidnight(date1);
  const normalized2 = normalizeToLocalMidnight(date2);

  return normalized1.valueOf() === normalized2.valueOf();
}

/**
 * Checks if the first date is before the second date (ignoring time)
 * @param date - The date to check
 * @param compareAgainst - The date to compare against
 * @returns true if date is before compareAgainst
 */
function isBeforeDate(
  date: Date | string,
  compareAgainst: Date | string,
): boolean {
  const normalized1 = normalizeToLocalMidnight(date);
  const normalized2 = normalizeToLocalMidnight(compareAgainst);

  return normalized1 < normalized2;
}

/**
 * Checks if the first date is after the second date (ignoring time)
 * @param date - The date to check
 * @param compareAgainst - The date to compare against
 * @returns true if date is after compareAgainst
 */
function isAfterDate(
  date: Date | string,
  compareAgainst: Date | string,
): boolean {
  const normalized1 = normalizeToLocalMidnight(date);
  const normalized2 = normalizeToLocalMidnight(compareAgainst);

  return normalized1 > normalized2;
}

/**
 * Formats a date as YYYY-MM-DD in local time (no timezone offset)
 */
function dateIsoFormat(date: Date): string {
  return format(date, 'yyyy-MM-dd');
}

/**
 * Returns today's date as YYYY-MM-DD in local time
 */
function todayIsoFormat(): string {
  return dateIsoFormat(localToday());
}

/**
 * Returns the day of the year (1-366) for a given date
 */
function dayOfYear(date: Date): number {
  const start = new Date(date.getFullYear(), 0, 1);

  return (
    Math.floor((date.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1
  );
}

/**
 * Formats a date using internationalization
 * @param date The Date, or a string that can be parsed by the Date constructor, to format
 * @param locale The BCP 47 language tag for formatting (defaults to 'en-AU')
 * @param options Optional Intl.DateTimeFormatOptions for custom formatting
 * @returns A formatted date string
 *
 * @example
 * Australian English locale (default)
 *   formatDate(new Date('2025-08-02'))                   // '02/08/2025'
 *
 * US English locale
 *   formatDate(new Date('2025-08-02'), 'en-US')          // '8/2/2025'
 *
 * Custom format (full month name, numeric day, year)
 *   formatDate(new Date('2025-08-02'), 'en-AU',
 *     { day: 'numeric', month: 'long', year: 'numeric' }) // '2 August 2025'
 */
function formatDate(
  date: Date | string,
  locale = 'en-AU',
  options: Intl.DateTimeFormatOptions = {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  },
): string {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat(locale, options).format(dateObj);
}

export {
  compareDates,
  dateIsoFormat,
  dayOfYear,
  formatDate,
  isAfterDate,
  isBeforeDate,
  isSameDate,
  localToday,
  normalizeToLocalMidnight,
  todayIsoFormat,
};
