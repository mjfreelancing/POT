import { format, parseISO } from 'date-fns';
import { useMemo } from 'react';

import type { ChartConfig } from '@/components/ui/chart';
import type { Projection, ProjectionMetric } from '@/data/projection';

// Predefined colors for different accounts in the chart
// These are used in a round-robin fashion if there are more accounts than colors
const ACCOUNT_COLORS = [
  '#8884d8', // Purple
  '#82ca9d', // Green
  '#ffc658', // Yellow/Orange
  '#ff7300', // Orange
  '#8dd1e1', // Light Blue
] as const;

// Color used for the combined total of all accounts
const GLOBAL_SERIES_COLOR = '#2563eb'; // Blue

import type {
  ProjectionExpenseItemWithAccount,
  ProjectionIncomeItemWithAccount,
} from '@/data/projection';

/**
 * Represents a single data point on the chart.
 * Each point contains:
 * - date information (ISO string and formatted display)
 * - optional expense and income items for that date
 * - dynamic keys for each account's balance and the global balance
 *
 * The [key: string] allows us to dynamically add account balances
 * where the key is the account ID and the value is the balance amount
 */
type ChartDataPoint = {
  date: string;
  formattedDate: string;
  expenseItems?: ProjectionExpenseItemWithAccount[];
  incomeItems?: ProjectionIncomeItemWithAccount[];
  [key: string]:
    | string
    | number
    | ProjectionExpenseItemWithAccount[]
    | ProjectionIncomeItemWithAccount[]
    | undefined;
};

/**
 * The complete result returned by the useProjectionChartData hook
 * @property chartData - Array of data points ready for chart rendering
 * @property chartConfig - Visual configuration for each series (colors, labels)
 * @property seriesKeys - Array of keys to render (account IDs + 'global')
 * @property hasData - Whether there's any non-zero data to display
 */
type UseProjectionChartDataResult = {
  chartData: ChartDataPoint[];
  chartConfig: ChartConfig;
  seriesKeys: string[];
  hasData: boolean;
};

/**
 * Transforms raw projection data into a format suitable for chart rendering
 * while preserving transaction details for tooltips and detail views.
 *
 * @param data - Raw projection data from the API containing account and global metrics
 * @param metric - Which financial metric to display (e.g., balance, available funds)
 * @returns Processed data structure ready for chart consumption
 *
 * The hook performs several key transformations:
 * 1. Flattens the date-based data into chart points
 * 2. Adds account information to transactions
 * 3. Creates visual styling configuration
 * 4. Determines if there's meaningful data to display
 */
function useProjectionChartData(
  data: Projection,
  metric: ProjectionMetric = 'balance',
): UseProjectionChartDataResult {
  return useMemo(() => {
    // Extract the timeline from global data (pre-sorted from API)
    // All accounts share the same date points, so we can use global as reference
    const sortedDates = data.global.map(db => db.date);

    // Transform the raw data into chart points
    const chartData: ChartDataPoint[] = sortedDates.map(date => {
      // Create the base point with date information
      const point: ChartDataPoint = {
        date,
        formattedDate: format(parseISO(date), 'MMM dd'),
      };

      // Add individual account metrics for this date
      data.accounts.forEach(account => {
        const dateBalance = account.dates.find(db => db.date === date)!;
        const value = dateBalance[metric];
        point[account.rowId] = value; // Dynamic key based on account ID
      });

      // Add the combined total for all accounts
      const globalBalance = data.global.find(db => db.date === date)!;
      point['global'] = globalBalance[metric];

      // Process expense items - attach account information to each expense
      if (globalBalance.expenseItems) {
        const expenseItems = data.accounts.flatMap(account => {
          const accountDateData = account.dates.find(d => d.date === date);

          // Map each expense to include its account ID for UI display
          return (accountDateData?.expenseItems || []).map(item => ({
            ...item,
            accountRowId: account.rowId,
          }));
        });

        point.expenseItems = expenseItems;
      }

      // Process income items - same pattern as expenses
      if (globalBalance.incomeItems) {
        const incomeItems = data.accounts.flatMap(account => {
          const accountDateData = account.dates.find(d => d.date === date);

          // Map each income to include its account ID for UI display
          return (accountDateData?.incomeItems || []).map(item => ({
            ...item,
            accountRowId: account.rowId,
          }));
        });

        point.incomeItems = incomeItems;
      }

      return point;
    });

    // Create visual configuration for the chart
    const config: ChartConfig = {};
    const seriesKeys: string[] = [];

    // Configure each account's appearance
    data.accounts.forEach((account, index) => {
      // Round-robin through colors if we have more accounts than colors
      const fallbackColor = ACCOUNT_COLORS[index % ACCOUNT_COLORS.length];

      config[account.rowId] = {
        label: account.description,
        color: fallbackColor,
      };

      seriesKeys.push(account.rowId);
    });

    // Add configuration for the combined total series
    config['global'] = {
      label: 'Total (All Accounts)',
      color: GLOBAL_SERIES_COLOR,
    };
    seriesKeys.push('global');

    // Determine if we have any non-zero data to display
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
