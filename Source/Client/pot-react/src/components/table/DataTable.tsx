import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  Row,
  useReactTable,
} from '@tanstack/react-table';

import { formatMoneyValue } from '../../lib/money/moneyUtils';
import { Frequency, MoneyValue } from '../../lib/types';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table';

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
export const createMoneyValueColumn = <TData,>(
  accessorKey: keyof TData & string, // Ensure this is both a key of TData and a string
  header: string,
): ColumnDef<TData> => ({
  accessorKey,
  header: () => <div>{header}</div>,
  cell: ({ row }) => <div>{formatCellMoneyValue(row, accessorKey)}</div>,
  // header: () => <div className="text-right">{header}</div>,
  // cell: ({ row }) => (
  //   <div className="text-right">{formatCellMoneyValue(row, accessorKey)}</div>
  // ),
});

export const createDateColumn = <TData,>(
  accessorKey: keyof TData & string, // Ensure this is both a key of TData and a string
  header: string,
  nullValue = 'Ongoing',
): ColumnDef<TData> => ({
  accessorKey,
  header: () => <div>{header}</div>,
  cell: ({ row }) => {
    const rawValue = row.getValue(accessorKey) as string | Date;

    if (rawValue === null) {
      return <div className="text-muted-foreground">{nullValue}</div>;
    }

    const dateValue = rawValue instanceof Date ? rawValue : new Date(rawValue);
    return <div>{formatCellDate(dateValue)}</div>;
  },
});

/**
 * Creates a column showing "<count> <frequency>" based on two keys.
 */
export const createFrequencyColumn = <TData,>(
  countKey: keyof TData & string,
  frequencyKey: keyof TData & string,
  header: string,
): ColumnDef<TData> => ({
  id: `${frequencyKey}-${countKey}`,
  header: () => <div>{header}</div>,
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
});

/** Default highlight class for rows marked by `highlightRowFilter` */
export const DEFAULT_HIGHLIGHT_ROW_CLASS = 'bg-blue-100 dark:bg-blue-900';

/**
 * Props for the `DataTable` component.
 *
 * @template TData - The type of the data objects in the table.
 * @template TValue - The type of the value for each column.
 *
 * @property columns - An array of column definitions describing how to render and access data for each column.
 * @property data - The array of data objects to display in the table.
 * @property [highlightRowFilter] - Optional function to determine if a row should be highlighted. Receives a row object and returns a boolean.
 * @property [highlightClassName] - Optional Tailwind CSS class(es) to apply to rows that match the `highlightRowFilter`.
 */
type DataTableProps<TData, TValue> = {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
  highlightRowFilter?: (row: Row<TData>) => boolean;
  highlightClassName?: string;
};

export function DataTable<TData, TValue>({
  columns,
  data,
  highlightRowFilter,
  highlightClassName = DEFAULT_HIGHLIGHT_ROW_CLASS,
}: DataTableProps<TData, TValue>) {
  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  // To put the table in a rounded border, wrap everything in
  // <div className="rounded-md border"></div>

  // Scenarios for header styling:
  // 1. Style on <TableHeader> (<thead>) paints the entire header section uniformly.
  //    - Pros: Single spot for styling, automatically covers all header rows.
  //    - Cons: Any custom background on individual <tr> or <th> can override it or introduce gaps.
  // 2. Style on each <TableRow> (<tr>) allows different header rows to have distinct backgrounds.
  //    - Pros: Fine-grained control per row; guarantees that each row’s own class takes precedence.
  //    - Cons: You must remember to apply the class to every header <tr>; new rows need manual updates.
  // 3. Style on each <TableHead> (<th>) controls text appearance (uppercase, bold) without affecting background.
  //    - Pros: Keeps background styling separate from text styling, consistent typography.
  //    - Cons: Does not affect row background, so must be combined with header or row styles for full effect.
  return (
    <Table>
      <TableHeader className="bg-gray-200 dark:bg-gray-700">
        {table.getHeaderGroups().map(headerGroup => (
          <TableRow
            key={headerGroup.id}
            className="bg-gray-200 dark:bg-gray-700"
          >
            {headerGroup.headers.map(header => {
              return (
                <TableHead key={header.id} className="font-semibold uppercase">
                  {header.isPlaceholder
                    ? null
                    : flexRender(
                        header.column.columnDef.header,
                        header.getContext(),
                      )}
                </TableHead>
              );
            })}
          </TableRow>
        ))}
      </TableHeader>
      <TableBody>
        {table.getRowModel().rows?.length ? (
          table.getRowModel().rows.map(row => (
            <TableRow
              key={row.id}
              data-state={row.getIsSelected() ? 'selected' : undefined}
              className={
                highlightRowFilter?.(row) ? highlightClassName : undefined
              }
            >
              {row.getVisibleCells().map(cell => (
                <TableCell key={cell.id}>
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </TableCell>
              ))}
            </TableRow>
          ))
        ) : (
          <TableRow>
            <TableCell colSpan={columns.length} className="h-24 text-center">
              No results.
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  );
}
