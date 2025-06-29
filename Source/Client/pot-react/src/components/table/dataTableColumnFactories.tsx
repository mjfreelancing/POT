import { ColumnDef, Row } from '@tanstack/react-table';

import { formatMoneyValue, Frequency, MoneyValue } from '../../lib';
import DataTableColumnHeader from './DataTableColumnHeader';

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
};

// Note: The trailing comma in <TData,> is necessary for TypeScript to differentiate it from a JSX element.

// Creates a column definition for a money value (right-aligned number) column.
//
// Using DataTableColumnHeader as decribed at https://ui.shadcn.com/docs/components/data-table#reusable-components
// for a sortable header with a title.
const createMoneyValueColumn = <TData,>(
  accessorKey: keyof TData & string, // Ensure this is both a key of TData and a string
  header: string,
  options: Partial<ColumnDef<TData>> = {},
): ColumnDef<TData> => {
  const { enableSorting = false, ...restOptions } = options;

  return {
    accessorKey,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => <div>{formatCellMoneyValue(row, accessorKey)}</div>,
    enableSorting,
    ...restOptions,
  };
};

const createDateColumn = <TData,>(
  accessorKey: keyof TData & string, // Ensure this is both a key of TData and a string
  header: string,
  nullValue = 'Ongoing',
  options: Partial<ColumnDef<TData>> = {},
): ColumnDef<TData> => {
  const { enableSorting = false, ...restOptions } = options;

  return {
    accessorKey,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const rawValue = row.getValue(accessorKey) as string | Date;

      if (rawValue === null) {
        return <div className="text-muted-foreground">{nullValue}</div>;
      }

      const dateValue =
        rawValue instanceof Date ? rawValue : new Date(rawValue);
      return <div>{formatCellDate(dateValue)}</div>;
    },
    enableSorting,
    ...restOptions,
  };
};

/**
 * Creates a column showing "<count> <frequency>" based on two keys.
 */
const createFrequencyColumn = <TData,>(
  countKey: keyof TData & string,
  frequencyKey: keyof TData & string,
  header: string,
  options: Partial<ColumnDef<TData>> = {},
): ColumnDef<TData> => {
  const { enableSorting = false, ...restOptions } = options;

  return {
    id: `${frequencyKey}-${countKey}`,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const count = row.original[countKey] as number;
      const freq = row.original[frequencyKey] as Frequency;
      const frequencyLabel = count === 1 ? frequencySingularMap[freq] : freq;

      return (
        <div>
          {count} {frequencyLabel}
        </div>
      );
    },
    enableSorting,
    ...restOptions,
  };
};

export { createDateColumn, createFrequencyColumn, createMoneyValueColumn };
