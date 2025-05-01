import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  Row,
  useReactTable,
} from '@tanstack/react-table';

import { formatMoneyValue } from '../../lib/moneyUtils';
import { MoneyValue } from '../../lib/valueTypes';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './shadcn/table';

// Gets the money value from a row.
const getMoneyValue = <TData,>(row: Row<TData>, key: string): MoneyValue => {
  return parseFloat(row.getValue(key));
};

// Formats a money value as a string.
const formatCellMoneyValue = <TData,>(row: Row<TData>, key: string) => {
  const value = getMoneyValue(row, key);
  return <div className="text-right">{formatMoneyValue(value)}</div>;
};

// Creates a column definition for a money value (right-aligned number) column.
export const createMoneyValueColumn = <TData,>(
  accessorKey: keyof TData & string, // Ensure this is both a key of TData and a string
  header: string,
): ColumnDef<TData> => ({
  accessorKey,
  header: () => <div className="text-right">{header}</div>,
  cell: ({ row }) => formatCellMoneyValue(row, accessorKey),
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
