import { addDays, addMonths, format, parseISO } from 'date-fns';
import { useEffect, useState } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  XAxis,
  YAxis,
} from 'recharts';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { ChartContainer, ChartTooltip } from '@/components/ui/chart';
import type {
  Projection,
  ProjectionExpenseItem,
  ProjectionIncomeItem,
  ProjectionMetric,
} from '@/data/projection';
import { PROJECTION_METRICS } from '@/data/projection';
import { normalizeToLocalMidnight } from '@/lib/dateUtils';
import { formatMoneyValue } from '@/lib/moneyUtils';

import { useProjectionChartData } from '../hooks/useProjectionChartData';
import { formatTooltipDate, getStrokeWidth } from '../utils/chartHelpers';
import ChartControls from './ChartControls';
import ExpenseDetails from './ExpenseDetails';
import IncomeDetails from './IncomeDetails';
import NoProjectionData from './NoProjectionData';

type ProjectionChartProps = {
  data: Projection;
  startDate: Date;
  period: number;
  selectedMetric: ProjectionMetric;
  hiddenSeries: string[];
  onStartDateChange: (date: Date | undefined) => void;
  onPeriodChange: (period: number) => void;
  onMetricChange: (metric: ProjectionMetric) => void;
  onHiddenSeriesChange: (hiddenSeries: string[]) => void;
  // New props for details sheet
  isDetailsOpen: boolean;
  selectedDate: Date | null;
  onToggleDetails: (date: Date | null) => void;
};

type SeriesVisibility = Record<string, boolean>;

function ProjectionChart({
  data,
  startDate,
  period,
  selectedMetric,
  hiddenSeries,
  onStartDateChange,
  onPeriodChange,
  onMetricChange,
  onHiddenSeriesChange,
  isDetailsOpen,
  selectedDate,
  onToggleDetails,
}: ProjectionChartProps) {
  // Transform data for chart consumption using custom hook
  const {
    chartData: allChartData,
    chartConfig,
    seriesKeys,
    hasData,
  } = useProjectionChartData(data, selectedMetric);

  // Type assertion for chart data to include our transaction items
  type ChartDataPoint = (typeof allChartData)[number] & {
    incomeItems?: ProjectionIncomeItem[];
    expenseItems?: ProjectionExpenseItem[];
  };

  // Calculate the end date for the selected period (for filtering only)
  const periodEndDate = addDays(addMonths(startDate, period), -1);

  // Filter chart data based on selected period
  const chartData = allChartData.filter((point): point is ChartDataPoint => {
    const pointDate = normalizeToLocalMidnight(parseISO(point.date));

    const normalizedStart = startDate
      ? normalizeToLocalMidnight(startDate)
      : undefined;

    const normalizedEnd = periodEndDate
      ? normalizeToLocalMidnight(periodEndDate)
      : undefined;

    const afterStart = !normalizedStart || pointDate >= normalizedStart;
    const beforeEnd = !normalizedEnd || pointDate <= normalizedEnd;

    return afterStart && beforeEnd;
  });

  // Derive seriesVisibility from props instead of state.
  // This avoids React errors about updating a parent component during render,
  // because derived values are always in sync with the latest props and do not
  // require setState or useEffect, which could cause updates during render.
  const seriesVisibility: SeriesVisibility = {};
  seriesKeys.forEach(key => {
    seriesVisibility[key] = !hiddenSeries.includes(key);
  });

  // Get dynamic title based on selected metric
  const getChartTitle = () => {
    return PROJECTION_METRICS[selectedMetric].title;
  };

  // Get chart type based on selected metric
  const getChartType = () => {
    return PROJECTION_METRICS[selectedMetric].chartType;
  };

  // Get date range for description
  const getDateRangeDescription = () => {
    if (chartData.length === 0) {
      return 'No data available for selected date range';
    }

    const firstDate = parseISO(chartData[0].date);
    const lastDate = parseISO(chartData[chartData.length - 1].date);

    const startText = format(firstDate, 'MMM dd, yyyy');
    const endText = format(lastDate, 'MMM dd, yyyy');

    if (chartData.length === 1) {
      return `Projection for ${startText}`;
    }

    return `${startText} → ${endText} (${chartData.length} days)`;
  };

  // Toggle series visibility
  function toggleSeries(seriesKey: string) {
    // Compute new hidden series based on current visibility
    const newHiddenSeries = seriesKeys.filter(key => {
      if (key === seriesKey) {
        return seriesVisibility[key]; // will be toggled
      }
      return !seriesVisibility[key];
    });

    onHiddenSeriesChange(newHiddenSeries);
  }

  // Calculate min and max for visible series only
  function getVisibleYDomain(): [number, number] {
    const visibleKeys = seriesKeys.filter(key => seriesVisibility[key]);
    let min: number | undefined;
    let max: number | undefined;

    for (const point of chartData) {
      for (const key of visibleKeys) {
        const value = point[key];
        if (typeof value === 'number' && !isNaN(value)) {
          if (min === undefined || value < min) {
            min = value;
          }

          if (max === undefined || value > max) {
            max = value;
          }
        }
      }
    }

    // If no visible data, fallback to 0-1
    if (min === undefined || max === undefined) {
      return [0, 1];
    }

    // Add a small margin for better visuals
    const range = max - min;

    const margin = range === 0 ? Math.abs(max) * 0.1 || 1 : range * 0.1;

    return [Math.floor(min - margin), Math.ceil(max + margin)];
  }

  function renderTooltipContent(
    active?: boolean,
    payload?: {
      color?: string;
      dataKey?: string | number;
      value?: string | number | (string | number)[];
    }[],
    label?: string | number,
    chartConfig?: Record<string, { label?: React.ReactNode; color?: string }>,
  ) {
    if (!active || !payload || payload.length === 0 || !chartConfig) {
      return null;
    }

    function getDisplayValue(
      value: string | number | (string | number)[] | undefined,
    ): string {
      if (Array.isArray(value)) {
        return value.join(', ');
      }

      if (typeof value === 'number') {
        return formatMoneyValue(value);
      }

      if (typeof value === 'string') {
        return value;
      }

      return '';
    }

    return (
      <div className="rounded-lg border bg-background p-3 shadow-md">
        <div className="mb-2 font-medium text-foreground">
          {formatTooltipDate(label as string)}
        </div>
        <div className="space-y-1">
          {payload.map((entry, index) => (
            <div
              key={index}
              className="flex items-center justify-between gap-3"
            >
              <div className="flex items-center gap-2">
                <div
                  className="h-3 w-3 rounded-full"
                  style={{ backgroundColor: entry.color }}
                />
                <span className="text-sm text-muted-foreground">
                  {chartConfig[String(entry.dataKey)]?.label || entry.dataKey}
                </span>
              </div>
              <span className="font-semibold text-foreground">
                {getDisplayValue(entry.value)}
              </span>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <Card
      className="flex flex-col md:h-full"
      style={{
        background:
          'linear-gradient(to bottom, rgba(148, 163, 184, 0.01), rgba(148, 163, 184, 0.04))',
      }}
    >
      <CardHeader>
        <div className="flex items-start justify-between gap-6">
          <div className="flex-1 space-y-1">
            <CardTitle>{getChartTitle()}</CardTitle>
            <CardDescription>{getDateRangeDescription()}</CardDescription>
          </div>
        </div>
      </CardHeader>
      <ChartControls
        selectedMetric={selectedMetric}
        onMetricChange={onMetricChange}
        startDate={startDate}
        onStartDateChange={onStartDateChange}
        period={period}
        onPeriodChange={onPeriodChange}
        seriesKeys={seriesKeys}
        seriesVisibility={seriesVisibility}
        onToggleSeries={toggleSeries}
        chartConfig={chartConfig}
      />
      <CardContent className="flex-1 flex flex-col p-0">
        {hasData ? (
          <div className="flex-1 px-2 md:px-6 overflow-x-auto min-h-[400px] md:min-h-0">
            <ChartContainer
              config={chartConfig}
              className="aspect-auto h-[400px] md:h-full"
              style={{ minWidth: '600px' }}
            >
              {getChartType() === 'line' ? (
                <LineChart
                  data={chartData}
                  margin={{
                    top: 10,
                    right: 20,
                    left: 20,
                    bottom: 5,
                  }}
                >
                  <CartesianGrid
                    strokeDasharray="3 3"
                    className="stroke-muted"
                  />
                  <XAxis
                    dataKey="date"
                    tickFormatter={value => format(parseISO(value), 'MMM dd')}
                    angle={-45}
                    textAnchor="end"
                    height={60}
                    className="text-xs"
                  />
                  <YAxis
                    tickFormatter={value => formatMoneyValue(value)}
                    className="text-xs"
                    domain={getVisibleYDomain()}
                  />
                  <ChartTooltip
                    content={({ active, payload, label }) =>
                      renderTooltipContent(active, payload, label, chartConfig)
                    }
                  />
                  {seriesKeys.map(key => {
                    const isVisible = seriesVisibility[key];
                    const strokeColor = chartConfig[key]?.color || '#8884d8';
                    return (
                      <Line
                        key={key}
                        type="basis"
                        dataKey={key}
                        stroke={strokeColor}
                        strokeWidth={getStrokeWidth(key)}
                        dot={false}
                        hide={!isVisible}
                        connectNulls={false}
                        activeDot={{ r: 4 }}
                        isAnimationActive={true}
                        animationDuration={700}
                        animationEasing="ease-in-out"
                      />
                    );
                  })}
                </LineChart>
              ) : (
                <BarChart
                  data={chartData}
                  margin={{
                    top: 10,
                    right: 20,
                    left: 20,
                    bottom: 5,
                  }}
                  onClick={event => {
                    if (event?.activePayload?.[0]) {
                      const date = parseISO(
                        event.activePayload[0].payload.date,
                      );
                      onToggleDetails(date);
                    }
                  }}
                >
                  <CartesianGrid
                    strokeDasharray="3 3"
                    className="stroke-muted"
                  />
                  <XAxis
                    dataKey="date"
                    tickFormatter={value => format(parseISO(value), 'MMM dd')}
                    angle={-45}
                    textAnchor="end"
                    height={35}
                    className="text-xs"
                  />
                  <YAxis
                    tickFormatter={value => formatMoneyValue(value)}
                    className="text-xs"
                    domain={getVisibleYDomain()}
                  />
                  <ChartTooltip
                    content={({ active, payload, label }) =>
                      renderTooltipContent(active, payload, label, chartConfig)
                    }
                  />
                  {seriesKeys.map(key => {
                    const isVisible = seriesVisibility[key];
                    const fillColor = chartConfig[key]?.color || '#8884d8';

                    return (
                      <Bar
                        key={key}
                        dataKey={key}
                        fill={fillColor}
                        hide={!isVisible}
                        opacity={0.8}
                        cursor="pointer"
                        isAnimationActive={true}
                        animationDuration={700}
                        animationEasing="ease-in-out"
                      />
                    );
                  })}
                </BarChart>
              )}
            </ChartContainer>
          </div>
        ) : (
          <NoProjectionData />
        )}
      </CardContent>

      {/* Render Income details based on selected metric */}
      {selectedMetric === 'incomeReceived' && selectedDate && (
        <IncomeDetails
          isOpen={isDetailsOpen}
          onOpenChange={open => onToggleDetails(open ? selectedDate : null)}
          date={selectedDate}
          items={data.accounts
            .map(account => {
              const dateData = account.dates.find(
                d => d.date === format(selectedDate, 'yyyy-MM-dd'),
              );

              if (!dateData?.incomeItems) {
                return [];
              }

              return dateData.incomeItems.map(item => ({
                ...item,
                accountRowId: account.rowId,
              }));
            })
            .flat()}
          chartConfig={chartConfig}
          hiddenSeries={hiddenSeries}
        />
      )}

      {/* Render Expense details based on selected metric */}
      {selectedMetric === 'expensesPaid' && selectedDate && (
        <ExpenseDetails
          isOpen={isDetailsOpen}
          onOpenChange={open => onToggleDetails(open ? selectedDate : null)}
          date={selectedDate}
          items={data.accounts
            .map(account => {
              const dateData = account.dates.find(
                d => d.date === format(selectedDate, 'yyyy-MM-dd'),
              );

              if (!dateData?.expenseItems) {
                return [];
              }

              return dateData.expenseItems.map(item => ({
                ...item,
                accountRowId: account.rowId,
              }));
            })
            .flat()}
          chartConfig={chartConfig}
          hiddenSeries={hiddenSeries}
        />
      )}
    </Card>
  );
}

export default ProjectionChart;
export type { ProjectionChartProps };
