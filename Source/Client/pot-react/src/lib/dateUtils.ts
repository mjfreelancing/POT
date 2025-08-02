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

export {
  dateIsoFormat,
  dayOfYear,
  localToday,
  normalizeToLocalMidnight,
  todayIsoFormat,
};
