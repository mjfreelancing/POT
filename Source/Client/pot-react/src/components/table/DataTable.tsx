import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  HeaderContext,
  Row,
  RowSelectionState,
  SortingState,
  TableOptions,
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

  /**
   * Optional function to provide unique row identifiers for maintaining selection state.
   * When not provided, defaults to using array index (react-table default behavior).
   */
  getRowId?: (row: TData, index: number) => string;

  /**
   * Get the class name for a row based on its data, allowing for custom styling.
   * When a function is provided, it receives the row object and should return either:
   * - A string with the CSS class name to apply to the row
   * - undefined to apply no additional styling
   * When not provided, no additional styling is applied.
   */
  getRowClassName?: RowClassNameFunction<TData>;
};

// Extend ColumnMeta to allow headerClassName and cellClassName
// DataTableColumnMeta allows per-column styling for both header and body cells.
// - headerClassName: applies only to the <th> (header cell) for this column. Use for alignment, width, etc.
// - cellClassName: applies only to the <td> (body cell) for this column. Use for alignment, width, etc.
//
// DataTableHeader also accepts a cellClassName prop, which acts as a default for all header cells.
// If a column's meta.headerClassName is set, it overrides the default cellClassName for that column's header.
// This allows you to have a global default style for headers, but override it for special columns (like selection checkboxes).
type DataTableColumnMeta = {
  /**
   * CSS class for the <th> (header cell) of this column.
   * Overrides the cellClassName prop on DataTableHeader for this column only.
   */
  headerClassName?: string;
  /**
   * CSS class for the <td> (body cell) of this column.
   * Used for per-column body cell styling.
   */
  cellClassName?: string;
};

/**
 * Function type for providing custom row styling based on row data.
 * When undefined is returned, no additional class is applied.
 * @template TData - The type of data in the row.
 */
type RowClassNameFunction<TData> = (row: Row<TData>) => string | undefined;

function DataTable<TData, TValue>({
  columns,
  data,
  highlightRowFilter,
  highlightClassName = DEFAULT_HIGHLIGHT_ROW_CLASS,
  enableRowSelection = false,
  bulkActions = [],
  onSelectionChange,
  getRowId,
  getRowClassName,
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
          header: ({ table }: HeaderContext<TData, TValue>) => (
            <div className="flex justify-center w-full">
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
            </div>
          ),
          cell: ({ row }: { row: Row<TData> }) => (
            <div className="flex justify-center w-full">
              <Checkbox
                checked={row.getIsSelected()}
                onCheckedChange={value => row.toggleSelected(!!value)}
                aria-label={`Select row ${row.id}`}
              />
            </div>
          ),
          enableSorting: false, // Selection column doesn't need sorting
          enableHiding: false, // Selection column shouldn't be hideable
          // Add className for header/cell alignment
          meta: {
            headerClassName: 'text-center px-0 w-[40px]',
            cellClassName: 'text-center px-0 w-[40px]',
          },
        },
        ...columns, // Spread the user-provided columns after the selection column
      ]
    : columns; // If selection is disabled, use columns as-is

  // Initialize the react-table instance with our configuration
  const tableOptions: TableOptions<TData> = {
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
    getRowId: getRowId, // Use custom getRowId function if provided, otherwise let react-table use default (index-based)
    state: {
      sorting, // Current sort state
      rowSelection, // Current selection state
    },
  };

  const table = useReactTable(tableOptions);

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
    <div className="flex flex-col h-full">
      <BulkActionsBar
        selectedCount={selectedCount}
        selectedItems={selectedItems}
        bulkActions={bulkActions}
        isVisible={enableRowSelection}
      />
      <div className="flex-1 min-h-0 border rounded-md">
        <div className="h-full overflow-auto">
          <div className="relative w-full h-full">
            <table className="w-full caption-bottom text-sm">
              <DataTableHeader
                headerGroups={table.getHeaderGroups()}
                headerClassName="sticky top-0 z-20 bg-gray-200 dark:bg-gray-700"
                cellClassName="font-semibold uppercase"
              />
              {/*
                 Hover effects were not working when applied to the <tr> so using the Tailwind arbitrary value syntax [&_tr:hover] to create a higher
                 specifity selector, which means "apply this style to any tr element that is being hovered and is a descendant of this element".
               */}
              <tbody className="[&_tr:hover]:bg-gray-100 dark:[&_tr:hover]:bg-gray-800">
                {table.getRowModel().rows?.length ? (
                  table.getRowModel().rows.map(row => (
                    <tr
                      key={row.id}
                      data-state={row.getIsSelected() ? 'selected' : undefined}
                      className={[
                        highlightRowFilter?.(row)
                          ? highlightClassName
                          : undefined,
                        getRowClassName ? getRowClassName(row) : undefined,
                        'even:bg-gray-100 dark:even:bg-gray-900',
                      ]
                        .filter(Boolean)
                        .join(' ')}
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
export type { DataTableColumnMeta, DataTableProps, RowClassNameFunction };
