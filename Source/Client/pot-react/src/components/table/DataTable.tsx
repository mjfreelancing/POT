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
import React, { useEffect, useState } from 'react';

import { Checkbox } from '../ui/checkbox';
import BulkActionsBar, { BulkAction } from './BulkActionsBar';
import DataTableHeader from './DataTableHeader';

const DEFAULT_HIGHLIGHT_ROW_CLASS = 'bg-yellow-200 dark:bg-yellow-800';

/**
 * DataTable Props
 * @template TData - The type of the data objects in the table.
 * @template TValue - The type of the value for each column.
 */
type DataTableProps<TData, TValue> = {
  /**
   * Column definitions array describing how to render and access data for each column.
   * Use @tanstack/react-table ColumnDef format, or the provided column factory functions.
   */
  columns: ColumnDef<TData, TValue>[];

  /** Array of data objects to display in the table rows. */
  data: TData[];

  /**
   * Optional function to determine if a row should be highlighted.
   * Receives the row object and returns true if the row should be highlighted.
   */
  highlightRowFilter?: (row: Row<TData>) => boolean;

  /**
   * Optional Tailwind CSS class(es) to apply to rows that match the highlightRowFilter.
   * Defaults to 'bg-yellow-200 dark:bg-yellow-800' if not provided.
   */
  highlightClassName?: string;

  /**
   * Whether to enable row selection with checkboxes.
   * When true, adds a selection column and enables bulk actions.
   */
  enableRowSelection?: boolean;

  /**
   * Array of bulk actions to show when items are selected.
   * Only displayed when enableRowSelection is true.
   * Each action receives the array of selected items when triggered.
   */
  bulkActions?: BulkAction<TData>[];

  /**
   * Callback function called when the selection changes.
   * Receives the array of currently selected data items.
   */
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
}: DataTableProps<TData, TValue>): React.ReactElement {
  // Table state management
  const [sorting, setSorting] = useState<SortingState>([]);
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

  // Create columns with optional selection column prepended
  // When row selection is enabled, we add a checkbox column at the beginning
  const tableColumns: ColumnDef<TData, TValue>[] = enableRowSelection
    ? [
        {
          id: 'select',
          // Header checkbox - allows selecting/deselecting all rows on the current page
          header: ({ table }: HeaderContext<TData, TValue>) => (
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
          // Row checkbox - allows selecting/deselecting individual rows
          cell: ({ row }: { row: Row<TData> }) => (
            <Checkbox
              checked={row.getIsSelected()}
              onCheckedChange={value => row.toggleSelected(!!value)}
              aria-label={`Select row ${row.id}`}
            />
          ),
          // Selection column configuration
          enableSorting: false, // Selection column doesn't need sorting
          enableHiding: false, // Selection column shouldn't be hideable
        },
        ...columns, // Spread the user-provided columns after the selection column
      ]
    : columns; // If selection is disabled, use columns as-is

  // Initialize the react-table instance with our configuration
  const table = useReactTable({
    data,
    columns: tableColumns,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    // Table behavior configuration
    onSortingChange: setSorting, // Handle sort state changes
    onRowSelectionChange: setRowSelection, // Handle selection state changes
    enableSortingRemoval: true, // Allow removing sort by clicking again
    enableMultiSort: false, // Only allow single column sorting
    enableRowSelection: enableRowSelection, // Enable/disable row selection feature
    state: {
      sorting, // Current sort state
      rowSelection, // Current selection state
    },
  });

  // Extract selection information for bulk actions and parent notifications
  const selectedRows = table.getFilteredSelectedRowModel().rows;
  const selectedItems = selectedRows.map(row => row.original);
  const selectedCount = selectedRows.length;

  // Notify parent component when selection changes
  useEffect(() => {
    if (onSelectionChange) {
      onSelectionChange(selectedItems);
    }
  }, [selectedItems, onSelectionChange]);

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
    <div className="h-full flex flex-col">
      <BulkActionsBar
        selectedCount={selectedCount}
        selectedItems={selectedItems}
        bulkActions={bulkActions}
        isVisible={enableRowSelection}
      />
      <div className="flex-1 min-h-0 border rounded-md">
        <div className="h-full overflow-auto">
          <div className="relative w-full">
            <table className="w-full caption-bottom text-sm">
              <DataTableHeader
                headerGroups={table.getHeaderGroups()}
                cellClassName="font-semibold uppercase sticky top-0 z-10 bg-gray-200 dark:bg-gray-700"
              />
              <tbody>
                {table.getRowModel().rows?.length ? (
                  table.getRowModel().rows.map(row => (
                    <tr
                      key={row.id}
                      data-state={row.getIsSelected() ? 'selected' : undefined}
                      className={
                        highlightRowFilter?.(row)
                          ? highlightClassName
                          : undefined
                      }
                    >
                      {row.getVisibleCells().map(cell => (
                        <td key={cell.id} className="p-4 align-middle">
                          {flexRender(
                            cell.column.columnDef.cell,
                            cell.getContext(),
                          )}
                        </td>
                      ))}
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td
                      colSpan={tableColumns.length}
                      className="h-24 text-center p-4"
                    >
                      No results.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}

export default DataTable;
export { DEFAULT_HIGHLIGHT_ROW_CLASS };
export type { DataTableProps };
