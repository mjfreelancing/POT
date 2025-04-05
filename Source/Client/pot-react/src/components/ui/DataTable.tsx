import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  Row,
  useReactTable,
} from '@tanstack/react-table';

import { formatCurrency } from '../../lib/currencyUtils';
import { Currency } from '../../lib/valueTypes';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './shadcn/table';

// Gets the currency value from a row.
const getCurrencyValue = <TData,>(row: Row<TData>, key: string): Currency => {
  return parseFloat(row.getValue(key));
};

// Formats a currency value as a string.
const formatCellCurrency = <TData,>(row: Row<TData>, key: string) => {
  const value = getCurrencyValue(row, key);
  return <div className="text-right">{formatCurrency(value)}</div>;
};

// Creates a column definition for a currency (right-aligned number) column.
export const createCurrencyColumn = <TData,>(
  accessorKey: keyof TData & string, // Ensure this is both a key of TData and a string
  header: string,
): ColumnDef<TData> => ({
  accessorKey,
  header: () => <div className="text-right">{header}</div>,
  cell: ({ row }) => formatCellCurrency(row, accessorKey),
});

type DataTableProps<TData, TValue> = {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
};

export function DataTable<TData, TValue>({
  columns,
  data,
}: DataTableProps<TData, TValue>) {
  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map(headerGroup => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map(header => {
                return (
                  <TableHead key={header.id}>
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
                data-state={row.getIsSelected() && 'selected'}
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
    </div>
  );
}
