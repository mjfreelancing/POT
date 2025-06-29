import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  HeaderContext,
  Row,
  RowSelectionState,
  SortingState,
  useReactTable,
} from '@tanstack/react-table';
import { useEffect, useState } from 'react';

import { Checkbox } from '../ui/checkbox';
import { Table, TableBody, TableCell, TableRow } from '../ui/table';
import BulkActionsBar, { BulkAction } from './BulkActionsBar';
import DataTableHeader from './DataTableHeader';

const DEFAULT_HIGHLIGHT_ROW_CLASS = 'bg-yellow-200 dark:bg-yellow-800';

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
 * @property [enableRowSelection] - Whether to enable row selection with checkboxes.
 * @property [bulkActions] - Array of bulk actions to show when items are selected.
 * @property [onSelectionChange] - Callback when selection changes.
 */
type DataTableProps<TData, TValue> = {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
  highlightRowFilter?: (row: Row<TData>) => boolean;
  highlightClassName?: string;
  enableRowSelection?: boolean;
  bulkActions?: BulkAction<TData>[];
  onSelectionChange?: (selectedItems: TData[]) => void;
};

function DataTable<TData, TValue>({
  columns,
  data,
  highlightRowFilter,
  highlightClassName = DEFAULT_HIGHLIGHT_ROW_CLASS,
  enableRowSelection = false,
  bulkActions = [],
  onSelectionChange,
}: DataTableProps<TData, TValue>) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

  // Create columns with optional selection column
  const tableColumns = enableRowSelection
    ? [
        {
          id: 'select',
          header: ({ table }: HeaderContext<TData, unknown>) => (
            <Checkbox
              checked={
                table.getIsAllPageRowsSelected() ||
                (table.getIsSomePageRowsSelected() && 'indeterminate')
              }
              onCheckedChange={value =>
                table.toggleAllPageRowsSelected(!!value)
              }
              aria-label="Select all"
            />
          ),
          cell: ({ row }: { row: Row<TData> }) => (
            <Checkbox
              checked={row.getIsSelected()}
              onCheckedChange={value => row.toggleSelected(!!value)}
              aria-label="Select row"
            />
          ),
          enableSorting: false,
          enableHiding: false,
        },
        ...columns,
      ]
    : columns;

  const table = useReactTable({
    data,
    columns: tableColumns,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    onSortingChange: setSorting,
    onRowSelectionChange: setRowSelection,
    enableSortingRemoval: true,
    enableMultiSort: false,
    enableRowSelection: enableRowSelection,
    state: {
      sorting,
      rowSelection,
    },
  });

  const selectedRows = table.getFilteredSelectedRowModel().rows;
  const selectedItems = selectedRows.map(row => row.original);
  const selectedCount = selectedRows.length;

  // Notify parent of selection changes
  useEffect(() => {
    if (onSelectionChange) {
      onSelectionChange(selectedItems);
    }
  }, [selectedItems, onSelectionChange]);

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
    <div>
      <BulkActionsBar
        selectedCount={selectedCount}
        selectedItems={selectedItems}
        bulkActions={bulkActions}
        onClearSelection={() => setRowSelection({})}
        isVisible={enableRowSelection}
      />
      <Table>
        <DataTableHeader headerGroups={table.getHeaderGroups()} />
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
              <TableCell
                colSpan={tableColumns.length}
                className="h-24 text-center"
              >
                No results.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </div>
  );
}

export default DataTable;
export { DEFAULT_HIGHLIGHT_ROW_CLASS };
export type { DataTableProps };
