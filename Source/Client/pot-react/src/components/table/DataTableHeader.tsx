import { flexRender, HeaderGroup } from '@tanstack/react-table';

import { TableHead, TableHeader, TableRow } from '../ui/table';

/**
 * Props for the DataTableHeader component.
 *
 * @template TData - The type of the data objects in the table.
 */
type DataTableHeaderProps<TData> = {
  /** Header groups from react-table containing header information */
  headerGroups: HeaderGroup<TData>[];
  /** CSS class for the TableHeader wrapper element */
  headerClassName?: string;
  /** CSS class for each TableRow in the header */
  rowClassName?: string;
  /** CSS class for each TableHead cell */
  cellClassName?: string;
};

/**
 * DataTableHeader - A reusable component for rendering table headers with consistent styling.
 *
 * This component handles:
 * - Rendering header groups and individual headers
 * - Applying consistent styling across header elements
 * - Proper integration with react-table's flexRender system
 * - Handling placeholder headers (empty cells)
 *
 * Features:
 * - Configurable CSS classes for all header elements
 * - Automatic handling of react-table header groups
 * - Proper TypeScript support with generics
 * - Consistent styling defaults for light/dark themes
 *
 * Usage example:
 * ```tsx
 * <DataTableHeader
 *   headerGroups={table.getHeaderGroups()}
 *   headerClassName="bg-gray-200 dark:bg-gray-700"
 *   rowClassName="bg-gray-200 dark:bg-gray-700"
 *   cellClassName="font-semibold uppercase"
 * />
 * ```
 *
 * @template TData - The type of data items in the table
 */
function DataTableHeader<TData>({
  headerGroups,
  headerClassName = 'bg-gray-200 dark:bg-gray-700',
  rowClassName = 'bg-gray-200 dark:bg-gray-700',
  cellClassName = 'font-semibold uppercase',
}: DataTableHeaderProps<TData>) {
  return (
    <TableHeader className={headerClassName}>
      {headerGroups.map(headerGroup => (
        <TableRow key={headerGroup.id} className={rowClassName}>
          {headerGroup.headers.map(header => {
            // Use meta.headerClassName if present, else fallback to cellClassName
            const customClass =
              header.column.columnDef.meta?.headerClassName ?? cellClassName;
            return (
              <TableHead key={header.id} className={customClass}>
                {/* Only render header content if it's not a placeholder */}
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
  );
}

export default DataTableHeader;

// Export types for use in other components
export type { DataTableHeaderProps };

// TypeScript module augmentation for @tanstack/react-table
// This allows us to add custom properties (headerClassName, cellClassName) to the meta field of ColumnDef.
// Without this, TypeScript will not recognize these properties and will show type errors when accessing them.
// This is only for type safety and editor support; it does not affect runtime behavior.
declare module '@tanstack/react-table' {
  // Must use interface for module augmentation.
  // eslint-disable-next-line @typescript-eslint/consistent-type-definitions, @typescript-eslint/no-unused-vars
  interface ColumnMeta<TData = unknown, TValue = unknown> {
    headerClassName?: string;
    cellClassName?: string;
  }
}
