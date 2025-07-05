import { Row, Table as ReactTable } from '@tanstack/react-table';

import DataTableContent from './DataTableContent';
import DataTableHeader from './DataTableHeader';

type StickyDataTableProps<TData> = {
  table: ReactTable<TData>;
  tableColumns: any[];
  highlightRowFilter?: (row: Row<TData>) => boolean;
  highlightClassName?: string;
};

/**
 * StickyDataTable - Table implementation with sticky headers.
 * 
 * Uses a custom table structure (bypassing shadcn Table component)
 * to ensure sticky positioning works correctly.
 */
function StickyDataTable<TData>({
  table,
  tableColumns,
  highlightRowFilter,
  highlightClassName,
}: StickyDataTableProps<TData>) {
  return (
    <div className="relative w-full">
      <table className="w-full caption-bottom text-sm">
        <DataTableHeader
          headerGroups={table.getHeaderGroups()}
          cellClassName="font-semibold uppercase sticky top-0 z-10 bg-gray-200 dark:bg-gray-700"
        />
        <DataTableContent
          table={table}
          tableColumns={tableColumns}
          highlightRowFilter={highlightRowFilter}
          highlightClassName={highlightClassName}
        />
      </table>
    </div>
  );
}

export default StickyDataTable;
export type { StickyDataTableProps };
