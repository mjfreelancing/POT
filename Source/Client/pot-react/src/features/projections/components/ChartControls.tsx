import { addDays, format } from 'date-fns';

import { EnrichedDatePicker } from '@/components/picker/EnrichedDatePicker';
import { Button } from '@/components/ui/button';
import { ChartConfig } from '@/components/ui/chart';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { PROJECTION_METRICS, ProjectionMetric } from '@/data/projection';

type ChartControlsProps = {
  selectedMetric: ProjectionMetric;
  onMetricChange: (metric: ProjectionMetric) => void;
  startDate: Date | undefined;
  endDate: Date | undefined;
  onStartDateChange: (date: Date | undefined) => void;
  onEndDateChange: (date: Date | undefined) => void;
  seriesKeys: string[];
  seriesVisibility: Record<string, boolean>;
  onToggleSeries: (seriesKey: string) => void;
  chartConfig: ChartConfig;
  hasSeriesData: (seriesKey: string) => boolean;
};

function ChartControls({
  selectedMetric,
  onMetricChange,
  startDate,
  endDate,
  onStartDateChange,
  onEndDateChange,
  seriesKeys,
  seriesVisibility,
  onToggleSeries,
  chartConfig,
  hasSeriesData,
}: ChartControlsProps) {
  const today = new Date();

  // Date range preset handlers
  const setDateRangePreset = (days: number | 'all') => {
    if (days === 'all') {
      onStartDateChange(undefined);
      onEndDateChange(undefined);
    } else {
      onStartDateChange(today);
      onEndDateChange(addDays(today, days));
    }
  };

  // Validation for date range
  const handleStartDateChange = (date: Date | undefined) => {
    onStartDateChange(date);

    // If end date is before start date, clear it
    if (date && endDate && endDate < date) {
      onEndDateChange(undefined);
    }
  };

  const handleEndDateChange = (date: Date | undefined) => {
    // Don't allow end date before start date
    if (date && startDate && date < startDate) {
      return;
    }

    onEndDateChange(date);
  };

  return (
    <div className="px-6 py-4 border-b bg-muted/30">
      <div className="space-y-3">
        {/* Row 1: Metric Dropdown | Period Controls + Date Range (right-aligned) */}
        <div className="flex flex-wrap gap-4 items-center justify-between">
          {/* Metric Selection */}
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium text-muted-foreground">
              View:
            </span>
            <Select
              value={selectedMetric}
              onValueChange={(value: ProjectionMetric) => onMetricChange(value)}
            >
              <SelectTrigger className="w-[160px] h-8">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {Object.entries(PROJECTION_METRICS).map(([key, config]) => (
                  <SelectItem key={key} value={key}>
                    {config.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Right-aligned controls group */}
          <div className="flex items-center gap-4">
            {/* Period Controls */}
            <div className="flex items-center gap-2">
              <span className="text-sm font-medium text-muted-foreground">
                Period:
              </span>
              <div className="flex gap-1">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset(30)}
                  className="h-8 px-3"
                >
                  30d
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset(60)}
                  className="h-8 px-3"
                >
                  60d
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset(90)}
                  className="h-8 px-3"
                >
                  90d
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset('all')}
                  className={`h-8 px-3 ${!startDate && !endDate ? 'bg-muted' : ''}`}
                >
                  All
                </Button>
              </div>
            </div>
            {/* Vertical Divider */}
            <div className="h-6 w-px bg-border"></div> {/* Custom Date Range */}
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-muted-foreground">
                  From:
                </span>
                <EnrichedDatePicker
                  selectedDate={startDate}
                  onDateAccepted={handleStartDateChange}
                  triggerClassName="w-[140px] h-8"
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
                  triggerClassName="w-[140px] h-8"
                  triggerLabel={date =>
                    date ? format(date, 'MMM dd, yyyy') : 'No limit'
                  }
                />
              </div>
            </div>
          </div>
        </div>
        {/* Row 2: Legend */}
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
                  onClick={() => onToggleSeries(key)}
                  className={`flex items-center gap-1.5 px-2.5 py-1 rounded text-xs font-medium border transition-all ${
                    isVisible
                      ? 'bg-background border-border hover:bg-muted shadow-sm'
                      : 'bg-muted/50 border-muted-foreground/20 opacity-60 hover:opacity-80'
                  }`}
                  title={
                    !hasData ? 'This account has no data to display' : undefined
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
  );
}

export default ChartControls;
