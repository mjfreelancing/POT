import {
  ColumnDef,
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
import BulkActionsBar, { BulkAction } from './BulkActionsBar';
import StandardDataTable from './StandardDataTable';
import StickyDataTable from './StickyDataTable';

const DEFAULT_HIGHLIGHT_ROW_CLASS = 'bg-yellow-200 dark:bg-yellow-800';

/**
 * DataTable - A comprehensive, reusable table component built on top of @tanstack/react-table.
 *
 * This component provides a complete table solution with the following features:
 *
 * ## Core Features:
 * - **Sortable columns** with visual sort indicators
 * - **Row highlighting** with customizable filter and styling
 * - **Bulk row selection** with checkboxes and selection management
 * - **Bulk actions** via dropdown menu for selected items
 * - **Sticky header** option to keep headers visible while scrolling
 * - **Responsive design** with proper light/dark theme support
 * - **Type-safe** with full TypeScript generic support
 *
 * ## Architecture:
 * The DataTable uses a componentized architecture for better maintainability and code reusability:
 *
 * ### Core Components:
 * - **`DataTable`** - Main orchestrator component that handles configuration and delegates rendering
 * - **`StandardDataTable`** - Renders tables using shadcn Table components (default behavior)
 * - **`StickyDataTable`** - Renders tables with sticky headers using custom table structure
 * - **`DataTableContent`** - Shared component for table body rendering (eliminates code duplication)
 * - **`DataTableHeader`** - Handles table header rendering with sorting capabilities
 * - **`BulkActionsBar`** - Manages bulk actions UI and selection state
 *
 * ### Supporting Utilities:
 * - **`dataTableColumnFactories`** - Factory functions for common column types
 * - **`DataTableColumnHeader`** - Sortable column header component
 *
 * ## Implementation Details:
 * When `stickyHeader={false}` (default): Uses `StandardDataTable` with shadcn Table components.
 * When `stickyHeader={true}`: Uses `StickyDataTable` with custom table structure to enable
 * CSS sticky positioning on header cells.
 *
 * ## Column Types:
 * Use the provided column factory functions for common data types:
 * - `createMoneyValueColumn()` - For currency/monetary values
 * - `createDateColumn()` - For date values with custom null handling
 * - `createFrequencyColumn()` - For frequency display (count + period)
 *
 * ## Basic Usage:
 * ```tsx
 * const columns: ColumnDef<MyData>[] = [
 *   {
 *     accessorKey: 'name',
 *     header: 'Name',
 *   },
 *   createMoneyValueColumn<MyData>('amount', 'Amount'),
 *   createDateColumn<MyData>('date', 'Date'),
 * ];
 *
 * <DataTable
 *   columns={columns}
 *   data={myData}
 * />
 * ```
 *
 * ## With Row Selection and Bulk Actions:
 * ```tsx
 * const bulkActions: BulkAction<MyData>[] = [
 *   {
 *     label: 'Delete Selected',
 *     onClick: (items) => handleDelete(items)
 *   },
 *   {
 *     label: 'Export to CSV',
 *     onClick: (items) => handleExport(items)
 *   }
 * ];
 *
 * <DataTable
 *   columns={columns}
 *   data={myData}
 *   enableRowSelection={true}
 *   bulkActions={bulkActions}
 *   onSelectionChange={(selectedItems) => setSelection(selectedItems)}
 * />
 * ```
 *
 * ## With Row Highlighting:
 * ```tsx
 * <DataTable
 *   columns={columns}
 *   data={myData}
 *   highlightRowFilter={(row) => row.original.id === activeId}
 *   highlightClassName="bg-blue-100 dark:bg-blue-900"
 * />
 * ```
 *
 * ## With Sticky Headers:
 * ```tsx
 * <DataTable
 *   columns={columns}
 *   data={myData}
 *   stickyHeader={true}
 * />
 * ```
 *
 * ## Performance Considerations:
 * - The component automatically switches between optimized implementations based on `stickyHeader` prop
 * - Table body rendering logic is shared between implementations to avoid code duplication
 * - Row selection state is managed efficiently with react-table's built-in selection handling
 * - Memoization is recommended for `columns`, `data`, `bulkActions`, and callback props
 *
 * ## Component Files:
 * ```
 * components/table/
 * ├── DataTable.tsx           # Main orchestrator component
 * ├── DataTableContent.tsx    # Shared table body rendering
 * ├── StickyDataTable.tsx     # Sticky header implementation
 * ├── StandardDataTable.tsx   # Standard table implementation
 * ├── DataTableHeader.tsx     # Header rendering with sorting
 * ├── BulkActionsBar.tsx      # Bulk actions UI
 * └── dataTableColumnFactories.ts # Column utility functions
 * ```
 *
 * @template TData - The type of the data objects in the table
 * @template TValue - The type of the value for each column (usually inferred)
 */

/**
 * Props for the DataTable component.
 *
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
   * Commonly used to highlight the currently selected/active row.
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
   * Useful for tracking selection state in parent components.
   */
  onSelectionChange?: (selectedItems: TData[]) => void;

  /**
   * Whether to enable sticky headers that remain visible while scrolling.
   * When true, uses StickyDataTable implementation for proper header positioning.
   * When false (default), uses StandardDataTable with shadcn Table components.
   */
  stickyHeader?: boolean;
};

function DataTable<TData, TValue>({
  columns,
  data,
  highlightRowFilter,
  highlightClassName = DEFAULT_HIGHLIGHT_ROW_CLASS,
  enableRowSelection = false,
  bulkActions = [],
  onSelectionChange,
  stickyHeader = false,
}: DataTableProps<TData, TValue>) {
  // Table state management
  const [sorting, setSorting] = useState<SortingState>([]);
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

  // Create columns with optional selection column prepended
  // When row selection is enabled, we add a checkbox column at the beginning
  const tableColumns = enableRowSelection
    ? [
        {
          id: 'select',
          // Header checkbox - allows selecting/deselecting all rows on the current page
          header: ({ table }: HeaderContext<TData, unknown>) => (
            <Checkbox
              checked={
                table.getIsAllPageRowsSelected() ||
                (table.getIsSomePageRowsSelected() && 'indeterminate')
              }
              onCheckedChange={(value: boolean) =>
                table.toggleAllPageRowsSelected(value)
              }
              aria-label="Select all"
            />
          ),
          // Row checkbox - allows selecting/deselecting individual rows
          cell: ({ row }: { row: Row<TData> }) => (
            <Checkbox
              checked={row.getIsSelected()}
              onCheckedChange={(value: boolean) => row.toggleSelected(value)}
              aria-label="Select row"
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
          {stickyHeader ? (
            <StickyDataTable
              table={table}
              tableColumns={tableColumns}
              highlightRowFilter={highlightRowFilter}
              highlightClassName={highlightClassName}
            />
          ) : (
            <StandardDataTable
              table={table}
              tableColumns={tableColumns}
              highlightRowFilter={highlightRowFilter}
              highlightClassName={highlightClassName}
            />
          )}
        </div>
      </div>
    </div>
  );
}

export default DataTable;
export { DEFAULT_HIGHLIGHT_ROW_CLASS };
export type { DataTableProps };
