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

  // Period button style variables
  const selectedPeriodButtonClass =
    'group h-8 px-3 font-medium border border-black dark:border-white bg-black text-white dark:bg-white dark:text-slate-900';

  const selectedPeriodButtonHoverClass =
    'hover:bg-white hover:text-black hover:border-black dark:hover:bg-slate-900 dark:hover:text-white dark:hover:border-white';

  const unselectedPeriodButtonClass =
    'h-8 px-3 border border-transparent bg-white text-black dark:bg-slate-900 dark:text-white';

  const unselectedPeriodButtonHoverClass =
    'hover:bg-muted hover:text-black dark:hover:bg-slate-800 dark:hover:text-white';

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
              name="metric-select"
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
                {[
                  { label: '30d', value: '30' },
                  { label: '60d', value: '60' },
                  { label: '90d', value: '90' },
                  { label: 'All', value: 'all' },
                ].map(period => {
                  const isSelected = getSelectedPeriod() === period.value;
                  return (
                    <Button
                      key={period.value}
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        setDateRangePreset(
                          period.value === 'all' ? 'all' : Number(period.value),
                        )
                      }
                      className={
                        isSelected
                          ? `${selectedPeriodButtonClass} ${selectedPeriodButtonHoverClass}`
                          : `${unselectedPeriodButtonClass} ${unselectedPeriodButtonHoverClass}`
                      }
                      aria-label={`Set chart period to ${period.label}`}
                      role="radio"
                      aria-checked={isSelected}
                      tabIndex={isSelected ? 0 : -1}
                    >
                      {period.label}
                    </Button>
                  );
                })}
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
