import { addDays, addMonths, format, parseISO } from 'date-fns';
import { useState } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ReferenceLine,
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
import { useIsShortViewport } from '@/hooks/use-short-viewport';
import { formatMoneyValue, normalizeToLocalMidnight } from '@/lib';

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

  // On short viewports (e.g. a phone rotated to landscape) the md "fill the
  // card" layout can collapse the chart area to zero height, leaving nothing to
  // render or scroll. Fall back to the fixed-height mobile chart and let the
  // page scroll to it, so the chart always renders.
  const isShortViewport = useIsShortViewport();
  const cardClass = isShortViewport
    ? 'flex flex-col'
    : 'flex flex-col md:h-full';
  const chartAreaClass = isShortViewport
    ? 'flex-1 px-2 md:px-6 overflow-x-auto min-h-[500px]'
    : 'flex-1 px-2 md:px-6 overflow-x-auto min-h-[500px] md:min-h-0';
  const chartContainerClass = isShortViewport
    ? 'aspect-auto h-[500px]'
    : 'aspect-auto h-[500px] md:h-full';

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

  type VisibleValueRange = {
    min: number | undefined;
    max: number | undefined;
  };

  function getVisibleValueRange(): VisibleValueRange {
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

    return { min, max };
  }

  // Line charts keep symmetric min/max behavior and render a zero reference line
  // only when the visible Y-axis domain spans across zero.
  function getVisibleLineYStats(visibleRange: VisibleValueRange): {
    domain: [number, number];
    showZeroReferenceLine: boolean;
  } {
    const { min, max } = visibleRange;

    // If no visible data, fallback to 0-1
    if (min === undefined || max === undefined) {
      return {
        domain: [0, 1],
        showZeroReferenceLine: false,
      };
    }

    // Add a small margin for better visuals
    const range = max - min;

    const margin = range === 0 ? Math.abs(max) * 0.1 || 1 : range * 0.1;

    const domainMin = Math.floor(min - margin);
    const domainMax = Math.ceil(max + margin);

    return {
      domain: [domainMin, domainMax],
      showZeroReferenceLine: domainMin < 0 && domainMax > 0,
    };
  }

  // Bar charts should always start at zero and only scale upward.
  function getVisibleBarYDomain(
    visibleRange: VisibleValueRange,
  ): [number, number] {
    const { max } = visibleRange;

    if (max === undefined || max <= 0) {
      return [0, 1];
    }

    const margin = max * 0.1;

    return [0, Math.ceil(max + margin)];
  }

  const visibleValueRange = getVisibleValueRange();
  const { domain: visibleLineYDomain, showZeroReferenceLine } =
    getVisibleLineYStats(visibleValueRange);
  const visibleBarYDomain = getVisibleBarYDomain(visibleValueRange);

  const [selectedBarSeriesKey, setSelectedBarSeriesKey] = useState<
    string | null
  >(null);

  // Detail sheets are rendered from a selected date and bar scope.
  // We derive a single date key once so all item extraction uses identical matching.
  const selectedDateKey = selectedDate
    ? format(selectedDate, 'yyyy-MM-dd')
    : null;

  // Full-day item sets across all accounts. These drive denominator counts
  // so account-scoped sheets can show "X of N items" instead of losing context.
  const selectedDateIncomeItems = selectedDateKey
    ? data.accounts.flatMap(account => {
        const dateData = account.dates.find(d => d.date === selectedDateKey);

        if (!dateData?.incomeItems) {
          return [];
        }

        return dateData.incomeItems.map(item => ({
          ...item,
          accountRowId: account.rowId,
        }));
      })
    : [];

  // Scoping rule for detail sheets:
  // - null/global: include all accounts for the selected date
  // - account key: include only that account's items
  const scopedIncomeItems = selectedDateIncomeItems.filter(item => {
    return (
      selectedBarSeriesKey === null ||
      selectedBarSeriesKey === 'global' ||
      item.accountRowId === selectedBarSeriesKey
    );
  });

  const selectedDateExpenseItems = selectedDateKey
    ? data.accounts.flatMap(account => {
        const dateData = account.dates.find(d => d.date === selectedDateKey);

        if (!dateData?.expenseItems) {
          return [];
        }

        return dateData.expenseItems.map(item => ({
          ...item,
          accountRowId: account.rowId,
        }));
      })
    : [];

  // Same scoping semantics as income so both detail sheets behave identically.
  const scopedExpenseItems = selectedDateExpenseItems.filter(item => {
    return (
      selectedBarSeriesKey === null ||
      selectedBarSeriesKey === 'global' ||
      item.accountRowId === selectedBarSeriesKey
    );
  });

  function renderTooltipContent(
    active?: boolean,
    payload?: {
      color?: string;
      dataKey?: string | number;
      value?: string | number | (string | number)[];
      payload?: Record<string, unknown>;
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

    const hoveredEntry = payload[0];
    const hoveredSeriesKey = String(hoveredEntry?.dataKey || '');
    const hoveredPoint = hoveredEntry?.payload;
    const isBarChart = getChartType() === 'bar';

    // Recharts can provide an undefined label when shared={false};
    // fallback to the payload date to avoid tooltip date parsing errors.
    const tooltipDateValue =
      typeof hoveredPoint?.date === 'string'
        ? hoveredPoint.date
        : typeof label === 'string'
          ? label
          : null;

    // Total bar hover should always reveal all account values for that day,
    // even when some account series are currently hidden in the chart legend.
    if (!tooltipDateValue) {
      return null;
    }

    const tooltipEntries =
      isBarChart &&
      payload.length === 1 &&
      hoveredSeriesKey === 'global' &&
      hoveredPoint
        ? seriesKeys
            .map(key => ({
              color: chartConfig[key]?.color,
              dataKey: key,
              value: hoveredPoint[key] as
                string | number | (string | number)[] | undefined,
            }))
            .filter(entry => typeof entry.value === 'number')
        : payload;

    return (
      <div className="rounded-lg border bg-background p-3 shadow-md">
        <div className="mb-2 font-medium text-foreground">
          {formatTooltipDate(tooltipDateValue)}
        </div>
        <div className="space-y-1">
          {tooltipEntries.map((entry, index) => (
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
      className={cardClass}
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
          <div className={chartAreaClass}>
            <ChartContainer
              config={chartConfig}
              className={chartContainerClass}
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
                    domain={visibleLineYDomain}
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
                        type="monotone"
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
                  {/*
                    Zero baseline rendering strategy:
                    - Keep a hardcoded stroke fallback so the line stays visible across browsers/devices.
                    - Layer light/dark Tailwind stroke classes to keep it theme-aware where class-based SVG styling is honored.
                    Why: previous attempts using only `currentColor` or CSS variable `hsl(var(...))` for SVG stroke
                         were inconsistent in this chart path (line was not visible).
                  */}
                  {showZeroReferenceLine && (
                    <ReferenceLine
                      y={0}
                      stroke="#64748b"
                      className="!stroke-slate-400 dark:!stroke-slate-300"
                      strokeDasharray="6 6"
                      strokeWidth={1.5}
                      ifOverflow="extendDomain"
                      isFront={true}
                    />
                  )}
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
                    domain={visibleBarYDomain}
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
                        onClick={clickedBarDatum => {
                          const clickedDate =
                            typeof clickedBarDatum?.payload?.date === 'string'
                              ? clickedBarDatum.payload.date
                              : null;

                          if (!clickedDate) {
                            return;
                          }

                          // Scope detail sheets to the exact bar that was clicked.
                          setSelectedBarSeriesKey(key);
                          onToggleDetails(parseISO(clickedDate));
                        }}
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
          onOpenChange={open => {
            if (!open) {
              setSelectedBarSeriesKey(null);
            }

            onToggleDetails(open ? selectedDate : null);
          }}
          date={selectedDate}
          items={scopedIncomeItems}
          totalItemCount={selectedDateIncomeItems.length}
          chartConfig={chartConfig}
          hiddenSeries={hiddenSeries}
          // Only total-bar selection bypasses hidden-series filtering.
          showAllAccounts={selectedBarSeriesKey === 'global'}
        />
      )}

      {/* Render Expense details based on selected metric */}
      {selectedMetric === 'expensesPaid' && selectedDate && (
        <ExpenseDetails
          isOpen={isDetailsOpen}
          onOpenChange={open => {
            if (!open) {
              setSelectedBarSeriesKey(null);
            }

            onToggleDetails(open ? selectedDate : null);
          }}
          date={selectedDate}
          items={scopedExpenseItems}
          totalItemCount={selectedDateExpenseItems.length}
          chartConfig={chartConfig}
          hiddenSeries={hiddenSeries}
          // Only total-bar selection bypasses hidden-series filtering.
          showAllAccounts={selectedBarSeriesKey === 'global'}
        />
      )}
    </Card>
  );
}

export default ProjectionChart;
export type { ProjectionChartProps };
