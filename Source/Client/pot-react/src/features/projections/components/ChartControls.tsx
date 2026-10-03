import { format } from 'date-fns';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { useState } from 'react';

import { EnrichedDatePicker } from '@/components/picker/EnrichedDatePicker';
import { Button } from '@/components/ui/button';
import type { ChartConfig } from '@/components/ui/chart';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import type { ProjectionInclude, ProjectionMetric } from '@/data/projection';
import { PROJECTION_METRICS } from '@/data/projection';
import { cn, localToday } from '@/lib';

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
  include: ProjectionInclude;
  onIncludeChange: (include: ProjectionInclude) => void;
};

// The combined series key; every other key is a real account.
const TOTAL_SERIES_KEY = 'global';

// The page always fetches a 12-month window, so the period is simply how many of
// those months to show: the complete domain is 1..12.
const PERIOD_MONTHS = Array.from({ length: 12 }, (_, index) => index + 1);

const INCLUDE_SWITCHES: { key: keyof ProjectionInclude; label: string }[] = [
  { key: 'reserved', label: 'Reserved' },
  { key: 'accruals', label: 'Accruals' },
  { key: 'arrears', label: 'Arrears' },
];

function formatMonths(months: number): string {
  return `${months} month${months === 1 ? '' : 's'}`;
}

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
  include,
  onIncludeChange,
}: ChartControlsProps) {
  // Small screens keep the facet controls behind a disclosure so the bar stays
  // short; from md up they are always shown.
  const [isOptionsOpen, setIsOptionsOpen] = useState(false);

  const activeIncludes = INCLUDE_SWITCHES.filter(item => include[item.key]).map(
    item => item.label,
  );

  // States the basis of the plotted balance at rest, so it never depends on a
  // hover tooltip (A-11).
  const includeCaption =
    activeIncludes.length === 0
      ? 'Balance'
      : `Balance less: ${activeIncludes.join(', ')}`;

  const accountKeys = seriesKeys.filter(key => key !== TOTAL_SERIES_KEY);
  const hasTotalSeries = seriesKeys.includes(TOTAL_SERIES_KEY);

  function renderSeriesToggle(key: string) {
    const config = chartConfig[key];
    const isVisible = seriesVisibility[key];

    return (
      <button
        key={key}
        onClick={() => onToggleSeries(key)}
        className={`flex max-w-[16rem] items-center gap-1.5 rounded border px-2.5 py-1 text-xs font-medium transition-all ${
          isVisible
            ? 'bg-background border-border hover:bg-muted shadow-sm'
            : 'bg-muted/50 border-muted-foreground/20 opacity-60 hover:opacity-80'
        }`}
        aria-label={`${isVisible ? 'Hide' : 'Show'} ${config.label} series on chart`}
        aria-pressed={isVisible}
        type="button"
      >
        <div
          className="h-2 w-2 shrink-0 rounded-full"
          style={{ backgroundColor: config.color }}
        />
        <span className="min-w-0 truncate">{config.label}</span>
      </button>
    );
  }

  return (
    <div className="px-4 py-3 border-b bg-muted/30 md:px-6 md:py-4">
      <div className="flex flex-col gap-3">
        {/* Small-screen disclosure for the facet controls; hidden from md up. */}
        <Button
          variant="outline"
          onClick={() => setIsOptionsOpen(!isOptionsOpen)}
          className="w-full gap-2 md:hidden"
          aria-label={isOptionsOpen ? 'Hide options' : 'Show options'}
          aria-expanded={isOptionsOpen}
        >
          {isOptionsOpen ? 'Hide options' : 'Options'}
          {isOptionsOpen ? (
            <ChevronUp className="h-4 w-4" />
          ) : (
            <ChevronDown className="h-4 w-4" />
          )}
        </Button>

        {/* The facet controls and the Accounts legend collapse together on small
            screens, so the bar is a single Options button until opened. */}
        <div
          className={cn(
            'flex-col gap-3',
            isOptionsOpen ? 'flex' : 'hidden md:flex',
          )}
        >
          {/* The query facets: metric, window start, period and the balance basis.
              Two columns below lg, where the sidebar can take 16rem of the width,
              and one row from lg up where the four facets fit side by side. */}
          <div className="grid grid-cols-2 items-end gap-x-3 gap-y-3 lg:grid-cols-[minmax(0,1.6fr)_minmax(0,1.1fr)_minmax(0,1fr)_minmax(0,1.3fr)]">
            {/* View: which metric the chart plots. */}
            <div
              className="col-span-2 flex min-w-0 flex-col gap-1.5 md:col-span-1"
              role="group"
              aria-labelledby="metric-label"
            >
              <span
                id="metric-label"
                className="text-xs font-medium uppercase tracking-wide text-muted-foreground"
              >
                View
              </span>
              <Select
                value={selectedMetric}
                onValueChange={(value: ProjectionMetric) =>
                  onMetricChange(value)
                }
                name="metric-select"
              >
                <SelectTrigger
                  id="metric-select"
                  className="w-full bg-background"
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

            {/* From: the first day shown. */}
            <div
              className="flex min-w-0 flex-col gap-1.5"
              role="group"
              aria-labelledby="date-range-label"
            >
              <span
                id="date-range-label"
                className="text-xs font-medium uppercase tracking-wide text-muted-foreground"
              >
                From
              </span>
              <EnrichedDatePicker
                selectedDate={startDate}
                minDate={localToday()}
                onDateAccepted={onStartDateChange}
                triggerClassName="w-full"
                triggerLabel={date => (
                  <span className="truncate">
                    {date ? format(date, 'MMM dd, yyyy') : 'Today'}
                  </span>
                )}
                triggerId="start-date-picker"
              />
            </div>

            {/* Period: how many of the 12 fetched months to show. */}
            <div
              className="flex min-w-0 flex-col gap-1.5"
              role="group"
              aria-labelledby="period-label"
            >
              <span
                id="period-label"
                className="text-xs font-medium uppercase tracking-wide text-muted-foreground"
              >
                Period
              </span>
              <Select
                value={String(period)}
                onValueChange={(value: string) => onPeriodChange(Number(value))}
                name="period-select"
              >
                <SelectTrigger
                  className="w-full bg-background"
                  aria-label="Select chart period in months"
                >
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {PERIOD_MONTHS.map(months => (
                    <SelectItem key={months} value={String(months)}>
                      {formatMonths(months)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Include: which standing obligations are deducted from the balance.
              Rendered only for the balance metric; the state persists, so hiding
              it never resets it. */}
            {selectedMetric === 'balance' && (
              <div
                className="col-span-2 flex min-w-0 flex-col gap-1.5 md:col-span-1"
                role="group"
                aria-labelledby="include-label"
              >
                <span
                  id="include-label"
                  className="text-xs font-medium uppercase tracking-wide text-muted-foreground"
                >
                  Include
                </span>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className="w-full justify-between gap-2 font-normal"
                      aria-label={`Include: ${includeCaption}`}
                    >
                      <span className="truncate">{includeCaption}</span>
                      <ChevronDown className="h-4 w-4 shrink-0 opacity-60" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent align="start" className="w-56 p-1.5">
                    <div
                      className="flex flex-col"
                      role="group"
                      aria-labelledby="include-label"
                    >
                      {INCLUDE_SWITCHES.map(item => (
                        <div
                          key={item.key}
                          className="flex min-h-9 items-center justify-between gap-3 rounded-md px-2 py-1 hover:bg-muted"
                        >
                          <label
                            htmlFor={`include-${item.key}`}
                            className="text-sm cursor-pointer select-none"
                          >
                            {item.label}
                          </label>
                          <Switch
                            id={`include-${item.key}`}
                            checked={include[item.key]}
                            onCheckedChange={checked =>
                              onIncludeChange({
                                ...include,
                                [item.key]: checked,
                              })
                            }
                            aria-label={`Include ${item.label}`}
                          />
                        </div>
                      ))}
                    </div>
                  </PopoverContent>
                </Popover>
              </div>
            )}
          </div>

          {/* Accounts: the legend and its visibility toggles, always visible. The
            combined total is not an account, so it sits outside the group. */}
          <div className="flex flex-wrap items-center gap-x-3 gap-y-2 border-t border-border/60 pt-3">
            <span
              id="accounts-label"
              className="shrink-0 text-xs font-medium uppercase tracking-wide text-muted-foreground"
            >
              Accounts
            </span>
            <div
              className="flex min-w-0 flex-wrap items-center gap-2"
              role="group"
              aria-labelledby="accounts-label"
            >
              {accountKeys.map(key => renderSeriesToggle(key))}
            </div>
            {hasTotalSeries && (
              <>
                <span
                  aria-hidden="true"
                  className="hidden h-4 w-px shrink-0 bg-border sm:block"
                />
                {renderSeriesToggle(TOTAL_SERIES_KEY)}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default ChartControls;
export type { ChartControlsProps };
