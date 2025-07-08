import { addDays, addMonths, format, parseISO } from 'date-fns';
import { useState } from 'react';
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
import {
  Projection,
  PROJECTION_METRICS,
  ProjectionMetric,
} from '@/data/projection';
import { normalizeToLocalMidnight } from '@/lib/dateUtils';
import { formatMoneyValue } from '@/lib/moneyUtils';

import { useProjectionChartData } from '../hooks/useProjectionChartData';
import { formatTooltipDate, getStrokeWidth } from '../utils/chartHelpers';
import ChartControls from './ChartControls';
import NoProjectionData from './NoProjectionData';

type ProjectionChartProps = {
  data: Projection;
  startDate: Date;
  period: number;
  onStartDateChange: (date: Date | undefined) => void;
  onPeriodChange: (period: number) => void;
};

type SeriesVisibility = Record<string, boolean>;

function ProjectionChart({
  data,
  startDate,
  period,
  onStartDateChange,
  onPeriodChange,
}: ProjectionChartProps) {
  // Metric selection state
  const [selectedMetric, setSelectedMetric] =
    useState<ProjectionMetric>('balance');

  // Transform data for chart consumption using custom hook
  const {
    chartData: allChartData,
    chartConfig,
    seriesKeys,
    hasData,
  } = useProjectionChartData(data, selectedMetric);

  // Calculate the end date for the selected period (for filtering only)
  const periodEndDate = addDays(addMonths(startDate, period), -1);

  // Filter chart data based on selected period
  const chartData = allChartData.filter(point => {
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

  // State for series visibility - show all series by default since all accounts have data
  const [seriesVisibility, setSeriesVisibility] = useState<SeriesVisibility>(
    () => {
      const initial: SeriesVisibility = {};

      seriesKeys.forEach(key => {
        initial[key] = true; // All series have data
      });

      return initial;
    },
  );

  // Get dynamic title based on selected metric
  const getChartTitle = () => {
    return PROJECTION_METRICS[selectedMetric].label;
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
  const toggleSeries = (seriesKey: string) => {
    setSeriesVisibility(prev => ({
      ...prev,
      [seriesKey]: !prev[seriesKey],
    }));
  };

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

  if (!hasData) {
    return <NoProjectionData />;
  }

  return (
    <Card className="flex flex-col h-full pb-0">
      <CardHeader className="flex-shrink-0">
        <div className="flex items-start justify-between gap-6">
          <div className="flex-1 space-y-1">
            <CardTitle>{getChartTitle()}</CardTitle>
            <CardDescription>{getDateRangeDescription()}</CardDescription>
          </div>
        </div>
      </CardHeader>
      <ChartControls
        selectedMetric={selectedMetric}
        onMetricChange={setSelectedMetric}
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
        <div
          className="flex-1 w-full min-h-0 px-6"
          style={{
            background:
              'linear-gradient(to bottom, rgba(148, 163, 184, 0.02), rgba(148, 163, 184, 0.08))',
            minHeight: '400px',
          }}
        >
          <ChartContainer
            config={chartConfig}
            className="w-full h-full aspect-auto"
            style={{ minHeight: '400px' }}
          >
            {getChartType() === 'line' ? (
              <LineChart
                data={chartData}
                margin={{ top: 20, right: 20, left: 20, bottom: 20 }}
              >
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
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
                  content={({ active, payload, label }) => {
                    if (!active || !payload?.length) {
                      return null;
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
                                  {chartConfig[entry.dataKey as string]
                                    ?.label || entry.dataKey}
                                </span>
                              </div>
                              <span className="font-semibold text-foreground">
                                {formatMoneyValue(entry.value as number)}
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>
                    );
                  }}
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
                margin={{ top: 20, right: 20, left: 20, bottom: 20 }}
              >
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
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
                  content={({ active, payload, label }) => {
                    if (!active || !payload?.length) return null;

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
                                  {chartConfig[entry.dataKey as string]
                                    ?.label || entry.dataKey}
                                </span>
                              </div>
                              <span className="font-semibold text-foreground">
                                {formatMoneyValue(entry.value as number)}
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>
                    );
                  }}
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
      </CardContent>
    </Card>
  );
}

export default ProjectionChart;
export type { ProjectionChartProps };
