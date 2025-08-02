import { ColumnDef, Row } from '@tanstack/react-table';

import {
  formatMoneyValue,
  Frequency,
  FrequencyDisplay,
  MoneyValue,
} from '../../lib';
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

// Formats a date value as dd-MM-yyyy.
const formatCellDate = (date: Date): string => {
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  return `${day}-${month}-${year}`;
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
            // By applying opacity instead of a text color, we can let the
            // parent's color show through but with reduced opacity.
            // return <span className="opacity-70">{nullContent}</span>;
            return nullContent;
          }
        }

        return null;
      }

      // If the value is a string, assuming it is already formatted as yyyy-MM-dd.
      const formattedValue =
        rawValue instanceof Date ? formatCellDate(rawValue) : rawValue;

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

      // Special case: for 'OneTime', display the FrequencyDisplay label only, no count
      let displayValue;

      if (freq === 'OneTime') {
        displayValue = FrequencyDisplay[freq];
      } else {
        const frequencyLabel = count === 1 ? frequencySingularMap[freq] : freq;
        displayValue = `${count} ${frequencyLabel}`;
      }

      return <span className="inline-block">{displayValue}</span>;
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
