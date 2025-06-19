import { addDays, format, parseISO } from 'date-fns';
import { useState } from 'react';
import { CartesianGrid, Line, LineChart, XAxis, YAxis } from 'recharts';

import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { ChartContainer, ChartTooltip } from '@/components/ui/chart';
import { EnrichedDatePicker } from '@/components/picker/EnrichedDatePicker';
import { Projection } from '@/data/projection';
import { formatMoneyValue } from '@/lib/moneyUtils';

import { useProjectionChartData } from '../hooks/useProjectionChartData';
import { formatTooltipDate, getStrokeWidth } from '../utils/chartHelpers';
import NoProjectionData from './NoProjectionData';

// Extract the curve type from Line component props
type LineProps = React.ComponentProps<typeof Line>;
type CurveType = NonNullable<LineProps['type']>;

type ProjectionChartProps = {
  data: Projection;
  title?: string;
  curveType?: CurveType;
};

type SeriesVisibility = Record<string, boolean>;

function ProjectionChart({
  data,
  title = 'Account Balance Projections',
  curveType = 'basis',
}: ProjectionChartProps) {
  // Date range filtering state
  const today = new Date();
  const [startDate, setStartDate] = useState<Date | undefined>(today);
  const [endDate, setEndDate] = useState<Date | undefined>(undefined);

  // Transform data for chart consumption using custom hook
  const {
    chartData: allChartData,
    chartConfig,
    seriesKeys,
    hasData,
  } = useProjectionChartData(data);

  // Filter chart data based on date range
  const chartData = allChartData.filter(point => {
    const pointDate = parseISO(point.date);
    const afterStart = !startDate || pointDate >= startDate;
    const beforeEnd = !endDate || pointDate <= endDate;
    return afterStart && beforeEnd;
  });

  // Helper function to check if a series has meaningful data
  const hasSeriesData = (seriesKey: string): boolean => {
    if (seriesKey === 'global') {
      // Always show the global series
      return true;
    }

    // Find the account and check if it has any dates
    const account = data.accounts.find(acc => acc.rowId === seriesKey);
    return account ? account.dates.length > 0 : false;
  };

  // State for series visibility - hide accounts with no data by default
  const [seriesVisibility, setSeriesVisibility] = useState<SeriesVisibility>(
    () => {
      const initial: SeriesVisibility = {};

      seriesKeys.forEach(key => {
        initial[key] = hasSeriesData(key);
      });

      return initial;
    },
  );

  // Get date range for description
  const getDateRangeDescription = () => {
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

  // Date range preset handlers
  const setDateRangePreset = (days: number | 'all') => {
    if (days === 'all') {
      setStartDate(undefined);
      setEndDate(undefined);
    } else {
      setStartDate(today);
      setEndDate(addDays(today, days));
    }
  };

  // Validation for date range
  const handleStartDateChange = (date: Date | undefined) => {
    setStartDate(date);

    // If end date is before start date, clear it
    if (date && endDate && endDate < date) {
      setEndDate(undefined);
    }
  };

  const handleEndDateChange = (date: Date | undefined) => {
    // Don't allow end date before start date
    if (date && startDate && date < startDate) {
      return;
    }

    setEndDate(date);
  };

  if (!hasData) {
    return <NoProjectionData />;
  }

  return (
    <Card className="flex flex-col h-full">
      <CardHeader className="flex-shrink-0">
        <div className="flex items-start justify-between gap-6">
          <div className="flex-1 space-y-1">
            <CardTitle>{title}</CardTitle>
            <CardDescription>{getDateRangeDescription()}</CardDescription>
          </div>
        </div>
      </CardHeader>
      {/* Chart Controls Section */}
      <div className="px-6 py-4 border-b bg-muted/30">
        <div className="space-y-3">
          {/* Date Range Controls */}
          <div className="flex flex-wrap gap-4 items-center justify-between">
            <div className="flex flex-wrap gap-2">
              <span className="text-sm font-medium text-muted-foreground self-center">
                Period:
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setDateRangePreset(30)}
                className={!startDate && !endDate ? 'bg-muted' : ''}
              >
                30 Days
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setDateRangePreset(60)}
              >
                60 Days
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setDateRangePreset(90)}
              >
                90 Days
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setDateRangePreset('all')}
                className={!startDate && !endDate ? 'bg-muted' : ''}
              >
                All Time
              </Button>
            </div>

            {/* Custom Date Range */}
            <div className="flex gap-3 items-center">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-muted-foreground">
                  From:
                </span>
                <EnrichedDatePicker
                  selectedDate={startDate}
                  onDateAccepted={handleStartDateChange}
                  triggerClassName="w-auto h-8"
                  triggerLabel={date =>
                    date ? format(date, 'MMM dd, yyyy') : 'Today'
                  }
                />
              </div>
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-muted-foreground">
                  To:
                </span>
                <EnrichedDatePicker
                  selectedDate={endDate}
                  onDateAccepted={handleEndDateChange}
                  triggerClassName="w-auto h-8"
                  triggerLabel={date =>
                    date ? format(date, 'MMM dd, yyyy') : 'No limit'
                  }
                />
              </div>
            </div>
          </div>

          {/* Legend */}
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium text-muted-foreground">
              Show:
            </span>
            <div className="flex flex-wrap gap-2">
              {seriesKeys.map(key => {
                const config = chartConfig[key];
                const isVisible = seriesVisibility[key];
                const hasData = hasSeriesData(key);

                return (
                  <button
                    key={key}
                    onClick={() => toggleSeries(key)}
                    className={`flex items-center gap-1.5 px-2.5 py-1 rounded text-xs font-medium border transition-all ${
                      isVisible
                        ? 'bg-background border-border hover:bg-muted shadow-sm'
                        : 'bg-muted/50 border-muted-foreground/20 opacity-60 hover:opacity-80'
                    }`}
                    title={
                      !hasData
                        ? 'This account has no data to display'
                        : undefined
                    }
                  >
                    <div
                      className="w-2 h-2 rounded-full"
                      style={{ backgroundColor: config.color }}
                    />
                    <span className={!hasData ? 'text-muted-foreground' : ''}>
                      {config.label}
                      {!hasData && ' (no data)'}
                    </span>
                  </button>
                );
              })}
            </div>
          </div>
        </div>
      </div>
      <CardContent className="flex-1 flex flex-col p-0">
        <div
          className="flex-1 w-full min-h-0 px-6"
          style={{
            background:
              'linear-gradient(to bottom, rgba(148, 163, 184, 0.02), rgba(148, 163, 184, 0.08))',
          }}
        >
          <ChartContainer
            config={chartConfig}
            className="w-full h-full aspect-auto"
          >
            <LineChart
              data={chartData}
              margin={{ top: 20, right: 0, left: 0, bottom: 0 }}
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
                                {chartConfig[entry.dataKey as string]?.label ||
                                  entry.dataKey}
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
                    type={curveType}
                    dataKey={key}
                    stroke={strokeColor}
                    strokeWidth={getStrokeWidth(key)}
                    dot={false}
                    hide={!isVisible}
                    connectNulls={false}
                    activeDot={{ r: 4 }}
                  />
                );
              })}
            </LineChart>
          </ChartContainer>
        </div>
      </CardContent>
    </Card>
  );
}

export default ProjectionChart;
