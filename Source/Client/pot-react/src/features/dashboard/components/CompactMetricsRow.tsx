import type { ReactNode } from 'react';

import { cn } from '@/lib';

type MetricItem = {
  /** Icon to display for this metric */
  icon: ReactNode;
  /** Label text for the metric */
  label: string;
  /** The value to display (pre-formatted) */
  value: string | number | ReactNode;
  /** Optional CSS classes for the value */
  className?: string;
};

type CompactMetricsRowProps = {
  /** Array of metrics to display */
  metrics: MetricItem[];
  /** Optional CSS classes for the container */
  className?: string;
};

/**
 * Displays metrics in a compact single row on desktop, 2x2 grid on mobile.
 * Each metric shows an icon, value, and label.
 *
 * @example
 * <CompactMetricsRow
 *   metrics={[
 *     {
 *       icon: <DollarSign className="h-5 w-5" />,
 *       label: 'Total Balance',
 *       value: '$47,786.00',
 *       className: 'text-success'
 *     },
 *     // ... more metrics
 *   ]}
 * />
 */
function CompactMetricsRow({ metrics, className }: CompactMetricsRowProps) {
  return (
    <div className={cn('grid grid-cols-2 lg:grid-cols-4 gap-3', className)}>
      {metrics.map((metric, index) => (
        <div
          key={`${metric.label}-${index}`}
          className="flex items-center gap-3 p-4 rounded-lg bg-slate-100 dark:bg-slate-800/60 border-2 border-primary/30 shadow-lg hover:shadow-xl transition-shadow"
        >
          <div className="p-1.5 lg:p-2.5 rounded-lg bg-primary/5 border border-primary/30 flex-shrink-0 flex items-center justify-center [&>svg]:h-4 [&>svg]:w-4 lg:[&>svg]:h-5 lg:[&>svg]:w-5">
            {metric.icon}
          </div>
          <div className="flex flex-col min-w-0 flex-1 items-center">
            <div
              className={cn('text-xs lg:text-xl font-bold', metric.className)}
            >
              {metric.value}
            </div>
            <div className="text-[10px] lg:text-xs text-slate-700 dark:text-slate-300 font-medium">
              {metric.label}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

export default CompactMetricsRow;
export type { CompactMetricsRowProps, MetricItem };
