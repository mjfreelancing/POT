import type { RowData } from '@tanstack/react-table';
import { FlexRender } from '@tanstack/react-table';

import { TableBody, TableCell, TableRow } from '../ui/table';
import type { AppColumnDef, AppRow, AppTable } from './tableFeatures';

type DataTableContentProps<TData extends RowData, TValue = unknown> = {
  table: AppTable<TData>;
  tableColumns: AppColumnDef<TData, TValue>[];
  highlightRowFilter?: (row: AppRow<TData>) => boolean;
  highlightClassName?: string;
};

/**
 * DataTableContent - Renders the table body with rows and cells.
 *
 * This component is extracted to avoid code duplication between
 * the sticky and standard table implementations.
 */
function DataTableContent<TData extends RowData, TValue = unknown>({
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
            {row.getAllCells().map(cell => (
              <TableCell key={cell.id}>
                <FlexRender cell={cell} />
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
