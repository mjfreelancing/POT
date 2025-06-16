import { format, parseISO } from 'date-fns';
import { useEffect, useRef, useState } from 'react';
import { CartesianGrid, Line, LineChart, XAxis, YAxis } from 'recharts';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { ChartContainer, ChartTooltip } from '@/components/ui/chart';
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
  const containerRef = useRef<HTMLDivElement>(null);
  const [dimensions, setDimensions] = useState({ width: 800, height: 400 });

  // Transform data for chart consumption using custom hook
  const { chartData, chartConfig, seriesKeys, hasData } =
    useProjectionChartData(data);

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

  // Update dimensions when container size changes
  useEffect(() => {
    const updateDimensions = () => {
      if (containerRef.current) {
        const { clientWidth, clientHeight } = containerRef.current;

        setDimensions({
          width: Math.max(300, clientWidth - 40), // Account for padding
          height: Math.max(300, clientHeight - 20), // Account for margins
        });
      }
    };

    updateDimensions();
    window.addEventListener('resize', updateDimensions);
    return () => window.removeEventListener('resize', updateDimensions);
  }, []);

  // Toggle series visibility
  const toggleSeries = (seriesKey: string) => {
    setSeriesVisibility(prev => ({
      ...prev,
      [seriesKey]: !prev[seriesKey],
    }));
  };

  // Custom legend component
  const CustomLegend = () => (
    <div className="flex flex-wrap gap-4 justify-center mb-4">
      {seriesKeys.map(key => {
        const config = chartConfig[key];
        const isVisible = seriesVisibility[key];
        const hasData = hasSeriesData(key);
        return (
          <button
            key={key}
            onClick={() => toggleSeries(key)}
            className={`flex items-center gap-2 px-3 py-1 rounded-md border transition-all ${
              isVisible
                ? 'bg-background border-border hover:bg-muted'
                : 'bg-muted border-muted-foreground/20 opacity-50 hover:opacity-75'
            }`}
            title={!hasData ? 'This account has no data to display' : undefined}
          >
            <div
              className="w-3 h-3 rounded-full"
              style={{ backgroundColor: config.color }}
            />
            <span
              className={`text-sm font-medium ${!hasData ? 'text-muted-foreground' : ''}`}
            >
              {config.label}
              {!hasData && ' (no data)'}
            </span>
          </button>
        );
      })}
    </div>
  );

  if (!hasData) {
    return <NoProjectionData />;
  }

  return (
    <Card className="flex flex-col h-full">
      <CardHeader className="flex-shrink-0">
        <CardTitle>{title}</CardTitle>
        <CardDescription>
          Projected account balances over {chartData.length} days
        </CardDescription>
      </CardHeader>
      <CardContent className="flex-1 flex flex-col p-0">
        <div className="flex-shrink-0">
          <CustomLegend />
        </div>
        <div className="flex-1 w-full min-h-0" ref={containerRef}>
          <ChartContainer config={chartConfig} className="w-full h-full">
            <LineChart
              width={dimensions.width}
              height={dimensions.height}
              data={chartData}
              margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />{' '}
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
