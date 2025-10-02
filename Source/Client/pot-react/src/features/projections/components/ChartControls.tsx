import { format } from 'date-fns';

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
import {
  PROJECTION_METRICS,
  PROJECTION_PERIODS,
  ProjectionMetric,
} from '@/data/projection';
import { localToday } from '@/lib';

type ChartControlsProps = {
  selectedMetric: ProjectionMetric;
  onMetricChange: (metric: ProjectionMetric) => void;
  startDate: Date | undefined;
  onStartDateChange: (date: Date | undefined) => void;
  period: number;
  onPeriodChange: (period: number) => void;
  seriesKeys: string[];
  seriesVisibility: Record<string, boolean>;
  onToggleSeries: (seriesKey: string) => void;
  chartConfig: ChartConfig;
};

function ChartControls({
  selectedMetric,
  onMetricChange,
  startDate,
  onStartDateChange,
  period,
  onPeriodChange,
  seriesKeys,
  seriesVisibility,
  onToggleSeries,
  chartConfig,
}: ChartControlsProps) {
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
                className="w-[170px] h-8"
                aria-label="Select chart metric to display"
              >
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {Object.entries(PROJECTION_METRICS).map(([key, config]) => (
                  <SelectItem key={key} value={key}>
                    {config.filterLabel}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Right-aligned controls group */}
          <div className="flex items-center gap-6">
            {/* Start Date Picker Only */}
            <div
              className="flex items-center gap-3 px-3 py-2 bg-muted/30 rounded-md"
              role="group"
              aria-labelledby="date-range-label"
            >
              <span id="date-range-label" className="sr-only">
                Custom date selection
              </span>
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-muted-foreground">
                  From:
                </span>
                <EnrichedDatePicker
                  selectedDate={startDate}
                  minDate={localToday()}
                  onDateAccepted={onStartDateChange}
                  triggerClassName="w-[140px] h-8"
                  triggerLabel={date =>
                    date ? format(date, 'MMM dd, yyyy') : 'Today'
                  }
                  triggerId="start-date-picker"
                />
              </div>
            </div>
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
                {PROJECTION_PERIODS.map(opt => {
                  const isSelected = period === opt.value;
                  return (
                    <Button
                      key={opt.value}
                      variant="outline"
                      size="sm"
                      onClick={() => onPeriodChange(opt.value)}
                      className={
                        isSelected
                          ? `${selectedPeriodButtonClass} ${selectedPeriodButtonHoverClass}`
                          : `${unselectedPeriodButtonClass} ${unselectedPeriodButtonHoverClass}`
                      }
                      aria-label={`Set chart period to ${opt.label}`}
                      role="radio"
                      aria-checked={isSelected}
                      tabIndex={isSelected ? 0 : -1}
                    >
                      {opt.label}
                    </Button>
                  );
                })}
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
