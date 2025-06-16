import { format, parseISO } from 'date-fns';
import { useMemo } from 'react';

import { ChartConfig } from '@/components/ui/chart';
import { Projection } from '@/data/projection';

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
 * @returns Transformed data ready for chart rendering
 */
function useProjectionChartData(
  data: Projection,
): UseProjectionChartDataResult {
  return useMemo(() => {
    // Collect all unique dates from all accounts and global data
    const allDates = new Set<string>();

    // Add dates from accounts
    data.accounts.forEach(account => {
      account.dates.forEach(dateBalance => {
        allDates.add(dateBalance.date);
      });
    });

    // Add dates from global data
    data.global.forEach(dateBalance => {
      allDates.add(dateBalance.date);
    });

    // Sort dates chronologically
    const sortedDates = Array.from(allDates).sort();

    // Create chart data points
    const chartData: ChartDataPoint[] = sortedDates.map(date => {
      const point: ChartDataPoint = {
        date,
        formattedDate: format(parseISO(date), 'MMM dd'),
      };
      // Add account balances
      data.accounts.forEach(account => {
        const dateBalance = account.dates.find(db => db.date === date);
        const balance = dateBalance?.balance ?? 0;
        point[account.rowId] = balance;

        // Debug logging in development
        if (process.env.NODE_ENV === 'development' && date === sortedDates[0]) {
          console.log(
            `Account ${account.description} (${account.rowId}):`,
            balance,
          );
        }
      });

      // Add global balance
      const globalBalance = data.global.find(db => db.date === date);
      point['global'] = globalBalance?.balance ?? 0;

      return point;
    });

    // Create chart configuration
    const config: ChartConfig = {};
    const seriesKeys: string[] = []; // Add account series
    const accountColors = [
      '#8884d8',
      '#82ca9d',
      '#ffc658',
      '#ff7300',
      '#8dd1e1',
    ];
    data.accounts.forEach((account, index) => {
      const fallbackColor = accountColors[index % accountColors.length];
      config[account.rowId] = {
        label: account.description,
        color: fallbackColor, // Use explicit color for now
      };
      seriesKeys.push(account.rowId);
    });
    // Add global series
    config['global'] = {
      label: 'Total (All Accounts)',
      color: '#2563eb', // Explicit blue color
    };
    seriesKeys.push('global');

    // Check if we have any data to display
    const hasData =
      chartData.length > 0 &&
      seriesKeys.some(key =>
        chartData.some(point => (point[key] as number) !== 0),
      );

    return { chartData, chartConfig: config, seriesKeys, hasData };
  }, [data]);
}

export { useProjectionChartData };
export type { ChartDataPoint, UseProjectionChartDataResult };
