import type { ColumnDef, Row } from '@tanstack/react-table';

import { Badge } from '@/components/ui/badge';
import type { Frequency, MoneyValue } from '../../lib';
import { formatDate, formatMoneyValue, FrequencyDisplay } from '../../lib';
import DataTableColumnHeader from './DataTableColumnHeader';

// Parameter types for our column factory functions
type BaseColumnParams<TData> = {
  accessorKey: keyof TData & string;
  header: string;
  options?: Partial<ColumnDef<TData>>;
};

type MoneyColumnParams<TData> = BaseColumnParams<TData>;

type DateColumnParams<TData> = BaseColumnParams<TData> & {
  // Function type for providing custom null value handling in date columns.
  // When undefined is returned, no content is displayed for null dates.
  // When a string is returned, it's displayed with muted styling.
  getNullValue?: (row: Row<TData>) => string | undefined;
};

type FrequencyColumnParams<TData> = {
  countKey: keyof TData & string;
  frequencyKey: keyof TData & string;
  header: string;
  options?: Partial<ColumnDef<TData>>;
};

// Gets the money value from a row.
const getMoneyValue = <TData,>(row: Row<TData>, key: string): MoneyValue => {
  return parseFloat(row.getValue(key));
};

// Formats a money value as a string.
const formatCellMoneyValue = <TData,>(row: Row<TData>, key: string) => {
  const value = getMoneyValue(row, key);
  return formatMoneyValue(value);
};

// Mapping of plural frequency values to their singular forms, equivalent to:
//
// const frequencySingularMap: { [K in Frequency]: string } = {
//   Days: 'Day',
//   Weeks: 'Week',
//   Months: 'Month',
//   Years: 'Year',
// };
const frequencySingularMap: Record<Frequency, string> = {
  Days: 'Day',
  Weeks: 'Week',
  Months: 'Month',
  Years: 'Year',
  OneTime: 'One Time',
};

// Note: The trailing comma in <TData,> is necessary for TypeScript to differentiate it from a JSX element.

// Creates a column definition for a money value (right-aligned number) column.
//
// Using DataTableColumnHeader as decribed at https://ui.shadcn.com/docs/components/data-table#reusable-components
// for a sortable header with a title.
const createMoneyValueColumn = <TData,>(
  params: MoneyColumnParams<TData>,
): ColumnDef<TData> => {
  const { accessorKey, header, options = {} } = params;
  const { enableSorting = false, ...restOptions } = options;

  return {
    id: accessorKey,
    accessorKey,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const formattedValue = formatCellMoneyValue(row, accessorKey);

      return (
        <span className="min-w-[80px] inline-block">{formattedValue}</span>
      );
    },
    enableSorting,
    ...restOptions,
  };
};

const createDateColumn = <TData,>(
  params: DateColumnParams<TData>,
): ColumnDef<TData> => {
  const { accessorKey, header, getNullValue, options = {} } = params;
  const { enableSorting = false, ...restOptions } = options;

  return {
    id: accessorKey,
    accessorKey,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const rawValue = row.getValue(accessorKey) as string | Date;

      if (rawValue === null) {
        if (getNullValue) {
          const nullContent = getNullValue(row);

          if (typeof nullContent === 'string' && nullContent.length > 0) {
            return nullContent;
          }
        }

        return null;
      }

      // Handles Date and strings in yyyy-MM-dd format.
      const formattedValue = formatDate(rawValue);

      return (
        <span className="min-w-[80px] inline-block">{formattedValue}</span>
      );
    },
    enableSorting,
    ...restOptions,
  };
};

/**
 * Creates a column showing "<count> <frequency>" based on two keys.
 */
const createFrequencyColumn = <TData,>(
  params: FrequencyColumnParams<TData>,
): ColumnDef<TData> => {
  const { countKey, frequencyKey, header, options = {} } = params;
  const { enableSorting = false, ...restOptions } = options;

  return {
    id: `${frequencyKey}-${countKey}`,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const count = row.original[countKey] as number;
      const freq = row.original[frequencyKey] as Frequency;
      // Check if item is excluded (only applicable to expenses/incomes)
      const isExcluded = (row.original as any).excludeFromCalcs ?? false;

      // Special case: for 'OneTime', display the FrequencyDisplay label only, no count
      let displayValue;

      if (freq === 'OneTime') {
        displayValue = FrequencyDisplay[freq];
      } else {
        const frequencyLabel = count === 1 ? frequencySingularMap[freq] : freq;
        displayValue = `${count} ${frequencyLabel}`;
      }

      // For excluded items, show as plain red text instead of badge
      if (isExcluded) {
        return (
          <span className="text-red-600/70 dark:text-red-400/70">{displayValue}</span>
        );
      }

      // Color scheme for frequency badges
      const getBadgeClasses = (frequency: Frequency): string => {
        // Normal color scheme
        switch (frequency) {
          case 'Days':
            return 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300 border-blue-300 dark:border-blue-800';
          case 'Weeks':
            return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300 border-green-300 dark:border-green-800';
          case 'Months':
            return 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-300 border-purple-300 dark:border-purple-800';
          case 'Years':
            return 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300 border-amber-300 dark:border-amber-800';
          case 'OneTime':
            return 'bg-pink-100 text-pink-800 dark:bg-pink-900/30 dark:text-pink-300 border-pink-300 dark:border-pink-800';
          default:
            return 'bg-slate-100 text-slate-800 dark:bg-slate-900/30 dark:text-slate-300 border-slate-300 dark:border-slate-800';
        }
      };

      return (
        <Badge
          variant="secondary"
          className={`text-xs px-2 py-0.5 min-w-[80px] justify-center ${getBadgeClasses(freq)}`}
        >
          {displayValue}
        </Badge>
      );
    },
    enableSorting,
    ...restOptions,
  };
};

export { createDateColumn, createFrequencyColumn, createMoneyValueColumn };
export type {
  BaseColumnParams,
  DateColumnParams,
  FrequencyColumnParams,
  MoneyColumnParams,
};
