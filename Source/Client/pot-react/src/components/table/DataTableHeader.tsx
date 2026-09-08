import type { HeaderGroup, RowData } from '@tanstack/react-table';
import { FlexRender } from '@tanstack/react-table';

import { TableHead, TableHeader, TableRow } from '../ui/table';
import type { AppTableFeatures } from './tableFeatures';

/**
 * Props for the DataTableHeader component.
 *
 * @template TData - The type of the data objects in the table.
 */
type DataTableHeaderProps<TData extends RowData> = {
  /** Header groups from react-table containing header information */
  headerGroups: HeaderGroup<AppTableFeatures, TData>[];
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
 * - Proper integration with react-table's FlexRender system
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
function DataTableHeader<TData extends RowData>({
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
                {header.isPlaceholder ? null : <FlexRender header={header} />}
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
