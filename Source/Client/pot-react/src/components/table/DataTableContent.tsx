import {
  ColumnDef,
  flexRender,
  Row,
  Table as ReactTable,
} from '@tanstack/react-table';

import { TableBody, TableCell, TableRow } from '../ui/table';

type DataTableContentProps<TData, TValue = unknown> = {
  table: ReactTable<TData>;
  tableColumns: ColumnDef<TData, TValue>[];
  highlightRowFilter?: (row: Row<TData>) => boolean;
  highlightClassName?: string;
};

/**
 * DataTableContent - Renders the table body with rows and cells.
 *
 * This component is extracted to avoid code duplication between
 * the sticky and standard table implementations.
 */
function DataTableContent<TData, TValue = unknown>({
  table,
  tableColumns,
  highlightRowFilter,
  highlightClassName,
}: DataTableContentProps<TData, TValue>) {
  return (
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
          <TableCell colSpan={tableColumns.length} className="h-24 text-center">
            No results.
          </TableCell>
        </TableRow>
      )}
    </TableBody>
  );
}

export default DataTableContent;
export type { DataTableContentProps };
