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
}: ChartControlsProps) {
  const today = new Date();

  // Helper function to calculate days between two dates
  const getDaysBetween = (start: Date, end: Date): number => {
    return Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
  };

  // Helper to determine which period is currently selected
  const getSelectedPeriod = (): '30' | '60' | '90' | 'all' | 'custom' => {
    if (!startDate && !endDate) return 'all';

    if (startDate && endDate) {
      const days = getDaysBetween(startDate, endDate);
      if (days === 30) return '30';
      if (days === 60) return '60';
      if (days === 90) return '90';
    }

    return 'custom';
  };

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
          <div
            className="flex items-center gap-2"
            role="group"
            aria-labelledby="metric-label"
          >
            <span
              id="metric-label"
              className="text-sm font-medium text-muted-foreground"
            >
              View:
            </span>
            <Select
              value={selectedMetric}
              onValueChange={(value: ProjectionMetric) => onMetricChange(value)}
            >
              <SelectTrigger
                id="metric-select"
                className="w-[160px] h-8"
                aria-label="Select chart metric to display"
              >
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
          <div className="flex items-center gap-6">
            {/* Period Controls Group */}
            <div
              className="flex items-center gap-2 px-3 py-2 bg-muted/30 rounded-md"
              role="group"
              aria-labelledby="period-label"
            >
              <span
                id="period-label"
                className="text-sm font-medium text-muted-foreground"
              >
                Period:
              </span>
              <div
                className="flex gap-1"
                role="radiogroup"
                aria-labelledby="period-label"
              >
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset(30)}
                  className={`h-8 px-3 ${
                    getSelectedPeriod() === '30'
                      ? 'bg-primary text-primary-foreground border-primary shadow-sm font-medium hover:bg-primary/90 dark:bg-slate-200 dark:text-slate-900 dark:border-slate-200'
                      : ''
                  }`}
                  aria-label="Set chart period to 30 days"
                  role="radio"
                  aria-checked={getSelectedPeriod() === '30'}
                  tabIndex={getSelectedPeriod() === '30' ? 0 : -1}
                >
                  30d
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset(60)}
                  className={`h-8 px-3 ${
                    getSelectedPeriod() === '60'
                      ? 'bg-primary text-primary-foreground border-primary shadow-sm font-medium hover:bg-primary/90 dark:bg-slate-200 dark:text-slate-900 dark:border-slate-200'
                      : ''
                  }`}
                  aria-label="Set chart period to 60 days"
                  role="radio"
                  aria-checked={getSelectedPeriod() === '60'}
                  tabIndex={getSelectedPeriod() === '60' ? 0 : -1}
                >
                  60d
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset(90)}
                  className={`h-8 px-3 ${
                    getSelectedPeriod() === '90'
                      ? 'bg-primary text-primary-foreground border-primary shadow-sm font-medium hover:bg-primary/90 dark:bg-slate-200 dark:text-slate-900 dark:border-slate-200'
                      : ''
                  }`}
                  aria-label="Set chart period to 90 days"
                  role="radio"
                  aria-checked={getSelectedPeriod() === '90'}
                  tabIndex={getSelectedPeriod() === '90' ? 0 : -1}
                >
                  90d
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDateRangePreset('all')}
                  className={`h-8 px-3 ${
                    getSelectedPeriod() === 'all'
                      ? 'bg-primary text-primary-foreground border-primary shadow-sm font-medium hover:bg-primary/90 dark:bg-slate-200 dark:text-slate-900 dark:border-slate-200'
                      : ''
                  }`}
                  aria-label="Show all data without date filtering"
                  role="radio"
                  aria-checked={getSelectedPeriod() === 'all'}
                  tabIndex={getSelectedPeriod() === 'all' ? 0 : -1}
                >
                  All
                </Button>
              </div>
            </div>
            {/* Custom Date Range Group */}
            <div
              className="flex items-center gap-3 px-3 py-2 bg-muted/30 rounded-md"
              role="group"
              aria-labelledby="date-range-label"
            >
              <span id="date-range-label" className="sr-only">
                Custom date range selection
              </span>
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
                  triggerId="start-date-picker"
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
                  triggerId="end-date-picker"
                />
              </div>
            </div>
          </div>
        </div>
        {/* Row 2: Legend */}
        <div
          className="flex items-center gap-3"
          role="group"
          aria-labelledby="legend-label"
        >
          <span
            id="legend-label"
            className="text-sm font-medium text-muted-foreground"
          >
            Show:
          </span>
          <div
            className="flex flex-wrap gap-2"
            role="group"
            aria-labelledby="legend-label"
          >
            {seriesKeys.map(key => {
              const config = chartConfig[key];
              const isVisible = seriesVisibility[key];

              return (
                <button
                  key={key}
                  onClick={() => onToggleSeries(key)}
                  className={`flex items-center gap-1.5 px-2.5 py-1 rounded text-xs font-medium border transition-all ${
                    isVisible
                      ? 'bg-background border-border hover:bg-muted shadow-sm'
                      : 'bg-muted/50 border-muted-foreground/20 opacity-60 hover:opacity-80'
                  }`}
                  aria-label={`${isVisible ? 'Hide' : 'Show'} ${config.label} series on chart`}
                  aria-pressed={isVisible}
                  type="button"
                >
                  <div
                    className="w-2 h-2 rounded-full"
                    style={{ backgroundColor: config.color }}
                    aria-hidden="true"
                  />
                  <span>{config.label}</span>
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
export type { ChartControlsProps };
