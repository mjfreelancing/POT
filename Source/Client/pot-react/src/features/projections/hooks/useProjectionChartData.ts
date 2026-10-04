import { format, parseISO } from 'date-fns';
import { useMemo } from 'react';

import type { ChartConfig } from '@/components/ui/chart';
import type {
  DateValues,
  Projection,
  ProjectionInclude,
  ProjectionMetric,
} from '@/data/projection';
import {
  DEFAULT_PROJECTION_INCLUDE,
  TOTAL_SERIES_KEY,
} from '@/data/projection';

// Predefined colors for different accounts in the chart
// These are used in a round-robin fashion if there are more accounts than colors
const ACCOUNT_COLORS = [
  '#8884d8', // Purple
  '#82ca9d', // Green
  '#ffc658', // Yellow/Orange
  '#ff7300', // Orange
  '#8dd1e1', // Light Blue
] as const;

// Color used for the combined total of the selected accounts
const TOTAL_SERIES_COLOR = '#2563eb'; // Blue

import type {
  ProjectionExpenseItemWithAccount,
  ProjectionIncomeItemWithAccount,
} from '@/data/projection';

/**
 * Represents a single data point on the chart.
 * Each point contains:
 * - date information (ISO string and formatted display)
 * - optional expense and income items for that date
 * - dynamic keys for each account's balance and the combined total
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
 * @property seriesKeys - Array of keys to render (account IDs + the combined total)
 * @property hasData - Whether there's any non-zero data to display
 */
type UseProjectionChartDataResult = {
  chartData: ChartDataPoint[];
  chartConfig: ChartConfig;
  seriesKeys: string[];
  hasData: boolean;
};

/**
 * Resolves the value plotted for one series on one date. Only the balance metric
 * is composed: each enabled switch subtracts its own published component. Every
 * other metric reads its member directly.
 */
function resolveMetricValue(
  values: DateValues,
  metric: ProjectionMetric,
  include: ProjectionInclude,
): number {
  if (metric !== 'balance') {
    return values[metric];
  }

  const reserved = include.reserved ? values.reserved : 0;
  const unpaidAccrual = include.accruals ? values.unpaidAccrual : 0;
  const arrears = include.arrears ? values.arrears : 0;

  return values.balance - reserved - unpaidAccrual - arrears;
}

/**
 * Transforms raw projection data into a format suitable for chart rendering
 * while preserving transaction details for tooltips and detail views.
 *
 * @param data - Raw projection data from the API containing per-account metrics
 * @param metric - Which financial metric to display (e.g., balance, daily accrual)
 * @param include - Which components are subtracted from the balance metric; ignored by other metrics
 * @param hiddenSeries - Series keys hidden in the legend; the combined total sums the rest
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
  include: ProjectionInclude = DEFAULT_PROJECTION_INCLUDE,
  hiddenSeries: string[] = [],
): UseProjectionChartDataResult {
  const { reserved, accruals, arrears } = include;

  return useMemo(() => {
    const activeInclude: ProjectionInclude = { reserved, accruals, arrears };

    // The timeline comes from the first account's dates; every account publishes
    // the same date points. A payload with no accounts keeps the no-data state.
    const firstAccount = data.accounts[0];
    const sortedDates = firstAccount
      ? firstAccount.dates.map(db => db.date)
      : [];

    // Only the accounts currently shown in the legend contribute to the total.
    const visibleAccounts = data.accounts.filter(
      account => !hiddenSeries.includes(account.rowId),
    );

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
        const value = resolveMetricValue(dateBalance, metric, activeInclude);
        point[account.rowId] = value; // Dynamic key based on account ID
      });

      // Add the combined total of the accounts shown in the legend
      point[TOTAL_SERIES_KEY] = visibleAccounts.reduce((total, account) => {
        const dateBalance = account.dates.find(db => db.date === date)!;
        return total + resolveMetricValue(dateBalance, metric, activeInclude);
      }, 0);

      // Process expense items - attach account information to each expense
      point.expenseItems = data.accounts.flatMap(account => {
        const accountDateData = account.dates.find(d => d.date === date);

        // Map each expense to include its account ID for UI display
        return (accountDateData?.expenseItems || []).map(item => ({
          ...item,
          accountRowId: account.rowId,
        }));
      });

      // Process income items - same pattern as expenses
      point.incomeItems = data.accounts.flatMap(account => {
        const accountDateData = account.dates.find(d => d.date === date);

        // Map each income to include its account ID for UI display
        return (accountDateData?.incomeItems || []).map(item => ({
          ...item,
          accountRowId: account.rowId,
        }));
      });

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

    // Add configuration for the combined total of the selected accounts
    config[TOTAL_SERIES_KEY] = {
      label: 'Total (Selected Accounts)',
      color: TOTAL_SERIES_COLOR,
    };
    seriesKeys.push(TOTAL_SERIES_KEY);

    // Determine if we have any non-zero data to display. This reads the plotted
    // (composed) values, so a window that nets to zero shows the no-data state.
    const hasData =
      chartData.length > 0 &&
      seriesKeys.some(key =>
        chartData.some(point => (point[key] as number) !== 0),
      );

    return { chartData, chartConfig: config, seriesKeys, hasData };
  }, [data, metric, reserved, accruals, arrears, hiddenSeries]);
}

export { useProjectionChartData };
export type { ChartDataPoint, UseProjectionChartDataResult };
