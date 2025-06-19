import { format, parseISO } from 'date-fns';
import { useMemo } from 'react';

import { ChartConfig } from '@/components/ui/chart';
import { Projection, ProjectionMetric } from '@/data/projection';

const ACCOUNT_COLORS = [
  '#8884d8', // Purple
  '#82ca9d', // Green
  '#ffc658', // Yellow/Orange
  '#ff7300', // Orange
  '#8dd1e1', // Light Blue
] as const;

const GLOBAL_SERIES_COLOR = '#2563eb'; // Blue

type ChartDataPoint = {
  date: string;
  formattedDate: string;
  [key: string]: string | number; // Account balances and global balance
};

type UseProjectionChartDataResult = {
  chartData: ChartDataPoint[];
  chartConfig: ChartConfig;
  seriesKeys: string[];
  hasData: boolean;
};

/**
 * Custom hook to transform projection data for chart consumption
 * @param data - The projection data from the API
 * @param metric - The metric to display (balance, available, incomeReceived, expensesPaid)
 * @returns Transformed data ready for chart rendering
 */
function useProjectionChartData(
  data: Projection,
  metric: ProjectionMetric = 'balance',
): UseProjectionChartDataResult {
  return useMemo(() => {
    // Since all accounts and global data contain the same set of dates,
    // we can simply use the global dates (already sorted from the API)
    const sortedDates = data.global.map(db => db.date);

    // Create chart data points
    const chartData: ChartDataPoint[] = sortedDates.map(date => {
      const point: ChartDataPoint = {
        date,
        formattedDate: format(parseISO(date), 'MMM dd'),
      };

      // Add account balances
      data.accounts.forEach(account => {
        const dateBalance = account.dates.find(db => db.date === date)!;
        const value = dateBalance[metric];
        point[account.rowId] = value;
      });

      // Add global balance
      const globalBalance = data.global.find(db => db.date === date)!;
      point['global'] = globalBalance[metric];

      return point;
    });

    // Create chart configuration
    const config: ChartConfig = {};
    const seriesKeys: string[] = [];

    data.accounts.forEach((account, index) => {
      const fallbackColor = ACCOUNT_COLORS[index % ACCOUNT_COLORS.length];

      config[account.rowId] = {
        label: account.description,
        color: fallbackColor,
      };

      seriesKeys.push(account.rowId);
    });

    // Add global series
    config['global'] = {
      label: 'Total (All Accounts)',
      color: GLOBAL_SERIES_COLOR,
    };

    seriesKeys.push('global');

    // Check if we have any data to display
    const hasData =
      chartData.length > 0 &&
      seriesKeys.some(key =>
        chartData.some(point => (point[key] as number) !== 0),
      );

    return { chartData, chartConfig: config, seriesKeys, hasData };
  }, [data, metric]);
}

export { useProjectionChartData };
export type { ChartDataPoint, UseProjectionChartDataResult };
