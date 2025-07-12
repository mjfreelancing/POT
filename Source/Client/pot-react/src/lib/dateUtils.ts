import { format } from 'date-fns';

/**
 * Returns a date at local midnight (00:00:00)
 */
function normalizeToLocalMidnight(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
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
  return dateIsoFormat(new Date());
}

export { dateIsoFormat, localToday, normalizeToLocalMidnight, todayIsoFormat };
