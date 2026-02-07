import { format } from 'date-fns';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { useState } from 'react';

import { EnrichedDatePicker } from '@/components/picker/EnrichedDatePicker';
import { Button } from '@/components/ui/button';
import type { ChartConfig } from '@/components/ui/chart';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { ProjectionMetric } from '@/data/projection';
import { PROJECTION_METRICS, PROJECTION_PERIODS } from '@/data/projection';
import { useIsMobile } from '@/hooks/use-mobile';
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
  const isMobile = useIsMobile();
  const [isExpanded, setIsExpanded] = useState(false);

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
        {/* Row 1: Metric Selection (View) + Date/Period controls (desktop) OR collapse button (mobile) */}
        <div
          className={
            isMobile
              ? 'flex flex-col gap-2'
              : 'flex flex-wrap gap-3 items-center'
          }
        >
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
                className="w-full md:w-[170px] h-8"
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

          {/* Desktop: Date and Period controls on same row, flowing left */}
          {!isMobile && (
            <>
              {/* Start Date Picker */}
              <div
                className="flex items-center gap-2 px-3 py-2 bg-muted/30 rounded-md"
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
              {/* Period Controls */}
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
                  className="flex flex-wrap gap-1"
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
            </>
          )}

          {/* Mobile: Collapse/Expand button */}
          {isMobile && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => setIsExpanded(!isExpanded)}
              className="gap-2 h-8 w-full"
              aria-label={isExpanded ? 'Hide filters' : 'Show filters'}
            >
              {isExpanded ? (
                <>
                  Hide
                  <ChevronUp className="h-4 w-4" />
                </>
              ) : (
                <>
                  Filters
                  <ChevronDown className="h-4 w-4" />
                </>
              )}
            </Button>
          )}
        </div>

        {/* Mobile collapsible section: Date and Period controls */}
        {isMobile && isExpanded && (
          <div className="space-y-3">
            {/* Start Date Picker */}
            <div
              className="flex items-center gap-2 px-3 py-2 bg-muted/30 rounded-md"
              role="group"
              aria-labelledby="date-range-label-mobile"
            >
              <span id="date-range-label-mobile" className="sr-only">
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
                  triggerId="start-date-picker-mobile"
                />
              </div>
            </div>
            {/* Period Controls */}
            <div
              className="flex items-center gap-2 px-3 py-2 bg-muted/30 rounded-md"
              role="group"
              aria-labelledby="period-label-mobile"
            >
              <span
                id="period-label-mobile"
                className="text-sm font-medium text-muted-foreground"
              >
                Period:
              </span>
              <div
                className="flex flex-wrap gap-1"
                role="radiogroup"
                aria-labelledby="period-label-mobile"
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
        )}

        {/* Legend row - always below on desktop, below collapsible on mobile when expanded */}
        {(!isMobile || isExpanded) && (
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
                    />
                    {config.label}
                  </button>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default ChartControls;
export type { ChartControlsProps };
