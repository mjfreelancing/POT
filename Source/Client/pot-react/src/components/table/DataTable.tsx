import type {
  ColumnDef,
  RowData,
  RowSelectionState,
  SortingState,
} from '@tanstack/react-table';
import { FlexRender, useTable } from '@tanstack/react-table';
import React, { useEffect, useState } from 'react';

import { Checkbox } from '../ui/checkbox';
import type { BulkAction } from './BulkActionsBar';
import BulkActionsBar from './BulkActionsBar';
import DataTableHeader from './DataTableHeader';
import {
  type AppColumnDef,
  type AppHeaderContext,
  type AppRow,
  type AppTableFeatures,
  appTableFeatures,
} from './tableFeatures';

const DEFAULT_HIGHLIGHT_ROW_CLASS = 'bg-yellow-200 dark:bg-yellow-800';

/**
 * DataTable Props
 * @template TData - The type of the data objects in the table.
 * @template TValue - The type of the value for each column.
 */
type DataTableProps<TData extends RowData, TValue = unknown> = {
  /**
   * Column definitions array describing how to render and access data for each column.
   * Use @tanstack/react-table ColumnDef format (typed via AppColumnDef), or the
   * provided column factory functions.
   */
  columns: AppColumnDef<TData, TValue>[];

  /** Array of data objects to display in the table rows. */
  data: TData[];

  /**
   * Optional function to determine if a row should be highlighted.
   * Receives the row object and returns true if the row should be highlighted.
   */
  highlightRowFilter?: (row: AppRow<TData>) => boolean;

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

/**
 * Function type for providing custom row styling based on row data.
 * When undefined is returned, no additional class is applied.
 * @template TData - The type of data in the row.
 */
type RowClassNameFunction<TData extends RowData> = (
  row: AppRow<TData>,
) => string | undefined;

function DataTable<TData extends RowData, TValue = unknown>({
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
  const tableColumns: AppColumnDef<TData, TValue>[] = enableRowSelection
    ? [
        {
          id: 'select',
          header: ({ table }: AppHeaderContext<TData, TValue>) => (
            <div className="flex justify-center w-full">
              <Checkbox
                checked={
                  table.getIsAllRowsSelected() ||
                  (table.getIsSomeRowsSelected() && 'indeterminate')
                }
                onCheckedChange={value => table.toggleAllRowsSelected(!!value)}
                aria-label="Select all"
              />
            </div>
          ),
          cell: ({ row }: { row: AppRow<TData> }) => (
            <div className="flex justify-center w-full">
              <Checkbox
                checked={row.getIsSelected()}
                onCheckedChange={value => row.toggleSelected(!!value)}
                aria-label={`Select row ${row.id}`}
              />
            </div>
          ),
          enableSorting: false, // Selection column doesn't need sorting
          meta: {
            headerClassName: 'text-center px-0 w-[40px]',
            cellClassName: 'text-center px-0 w-[40px]',
          },
        },
        ...columns, // Spread the user-provided columns after the selection column
      ]
    : columns; // If selection is disabled, use columns as-is

  // Initialize the react-table instance with our configuration.
  // react-table v9 requires the shared appTableFeatures registry so the
  // sorting + row-selection APIs type-check and are wired into the row models.
  const table = useTable<typeof appTableFeatures, TData>({
    features: appTableFeatures,
    data,
    // react-table v9 types column values as `unknown` on the options object;
    // the friendly AppColumnDef<TData, TValue> is structurally the same.
    columns: tableColumns as ColumnDef<AppTableFeatures, TData, unknown>[],
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

  return (
    <div className="flex flex-col h-full">
      <BulkActionsBar
        selectedCount={selectedCount}
        selectedItems={selectedItems}
        bulkActions={bulkActions}
        isVisible={enableRowSelection}
        onActionCompleted={() => setRowSelection({})}
      />
      <div className="flex-1 min-h-0 border rounded-md">
        <div className="h-full overflow-auto">
          <div className="relative w-full h-full">
            <table className="w-full caption-bottom text-sm">
              <DataTableHeader
                headerGroups={table.getHeaderGroups()}
                headerClassName="sticky top-0 z-20 bg-gray-200 dark:bg-gray-700"
                cellClassName="font-semibold uppercase px-4"
              />
              {/*
                 Hover effects were not working when applied to the <tr> so using the Tailwind arbitrary value syntax [&_tr:hover] to create a higher
                 specifity selector, which means "apply this style to any tr element that is being hovered and is a descendant of this element".
               */}
              <tbody className="[&_tr:hover]:bg-gray-100 dark:[&_tr:hover]:bg-gray-700 dark:bg-gray-900">
                {table.getRowModel().rows?.length ? (
                  table.getRowModel().rows.map(row => (
                    <tr
                      key={row.id}
                      data-state={row.getIsSelected() ? 'selected' : undefined}
                      className={[
                        'even:bg-gray-100 dark:even:bg-gray-800',
                        // IMPORTANT: Use border-bottom-color specifically instead of border-slate-200/50
                        // to avoid overriding border-l-x colors from getRowClassName (e.g., excluded items with red left border).
                        // Using border-slate-200/50 would set color for ALL borders, breaking left border styling.
                        'border-b [border-bottom-color:rgb(226_232_240/0.5)] dark:[border-bottom-color:rgb(51_65_85/0.5)]',
                        highlightRowFilter?.(row)
                          ? highlightClassName
                          : undefined,
                        getRowClassName ? getRowClassName(row) : undefined,
                      ]
                        .filter(Boolean)
                        .join(' ')}
                    >
                      {row.getAllCells().map(cell => (
                        <td key={cell.id} className="px-4 py-2.5 align-middle">
                          <FlexRender cell={cell} />
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
export type { DataTableProps, RowClassNameFunction };
