import { ColumnDef, Row, Table as ReactTable } from '@tanstack/react-table';

import { Table } from '../ui/table';
import DataTableContent from './DataTableContent';
import DataTableHeader from './DataTableHeader';

type StandardDataTableProps<TData, TValue = unknown> = {
  table: ReactTable<TData>;
  tableColumns: ColumnDef<TData, TValue>[];
  highlightRowFilter?: (row: Row<TData>) => boolean;
  highlightClassName?: string;
};

/**
 * StandardDataTable - Standard table implementation using shadcn Table component.
 *
 * Uses the default shadcn Table structure for normal (non-sticky) tables.
 */
function StandardDataTable<TData, TValue = unknown>({
  table,
  tableColumns,
  highlightRowFilter,
  highlightClassName,
}: StandardDataTableProps<TData, TValue>) {
  return (
    <Table>
      <DataTableHeader headerGroups={table.getHeaderGroups()} />
      <DataTableContent
        table={table}
        tableColumns={tableColumns}
        highlightRowFilter={highlightRowFilter}
        highlightClassName={highlightClassName}
      />
    </Table>
  );
}

export default StandardDataTable;
export type { StandardDataTableProps };
