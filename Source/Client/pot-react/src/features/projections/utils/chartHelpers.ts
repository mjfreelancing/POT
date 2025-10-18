import { format, parseISO } from 'date-fns';

import { logger } from '@/lib';

import type { ChartDataPoint } from '../hooks/useProjectionChartData';

/**
 * Format X-axis labels based on the data range to avoid overcrowding
 * @param value - The date string value
 * @param chartData - The chart data array
 * @returns Formatted label or empty string if should be hidden
 */
function formatXAxisLabel(value: string, chartData: ChartDataPoint[]): string {
  try {
    const index = chartData.findIndex(d => d.date === value);
    if (index === -1) return '';

    // Calculate interval to show approximately 8-12 labels
    const targetLabels = Math.min(
      12,
      Math.max(4, Math.floor(chartData.length / 8)),
    );
    const interval = Math.ceil(chartData.length / targetLabels);

    // Always show first, last, and evenly spaced dates
    if (
      index === 0 ||
      index === chartData.length - 1 ||
      index % interval === 0
    ) {
      const date = parseISO(value);
      if (chartData.length <= 90) {
        return format(date, 'MMM dd'); // Jan 15
      } else {
        return format(date, 'MMM'); // Jan
      }
    }

    return '';
  } catch (error) {
    logger.warn('chartHelpers', 'Error formatting X-axis label', error);
    return '';
  }
}

/**
 * Format tooltip date label for better readability
 * @param value - The date string value
 * @returns Formatted date string
 */
function formatTooltipDate(value: string): string {
  return format(parseISO(value), 'EEEE, MMM dd, yyyy');
}

/**
 * Get stroke width for different series types
 * @param seriesKey - The series identifier
 * @returns Stroke width number
 */
function getStrokeWidth(seriesKey: string): number {
  return seriesKey === 'global' ? 2 : 1.5;
}

export { formatTooltipDate, formatXAxisLabel, getStrokeWidth };
