import {
  type Cell,
  type Column,
  type ColumnDef,
  createSortedRowModel,
  type Header,
  type HeaderContext,
  type Row,
  type RowData,
  rowSelectionFeature,
  rowSortingFeature,
  sortFns,
  type Table,
  tableFeatures,
  type TableOptions,
} from '@tanstack/react-table';

// Per-column meta shared by every app table (rendered by DataTableHeader /
// DataTable). In react-table v9 the column `meta` shape is declared via the
// `columnMeta` type-only slot on the features object (this replaces the old
// `declare module` ColumnMeta augmentation used in v8).
type DataTableColumnMeta = {
  headerClassName?: string;
  cellClassName?: string;
};

// The single feature registry used by every table (react-table 8 -> 9).
// Feature-provided APIs (sorting, row selection) only type-check when column
// definitions and components are typed against the SAME features instance.
const appTableFeatures = tableFeatures({
  rowSortingFeature,
  rowSelectionFeature,
  sortedRowModel: createSortedRowModel(),
  sortFns,
  columnMeta: {} as DataTableColumnMeta,
});

type AppTableFeatures = typeof appTableFeatures;

// Convenience aliases that thread the canonical features type through the
// shared table layer and all consumer column definitions, so call sites only
// worry about their own TData/TValue.
type AppColumnDef<TData extends RowData, TValue = unknown> = ColumnDef<
  AppTableFeatures,
  TData,
  TValue
>;
type AppColumn<TData extends RowData, TValue = unknown> = Column<
  AppTableFeatures,
  TData,
  TValue
>;
type AppRow<TData extends RowData> = Row<AppTableFeatures, TData>;
type AppCell<TData extends RowData, TValue = unknown> = Cell<
  AppTableFeatures,
  TData,
  TValue
>;
type AppHeader<TData extends RowData, TValue = unknown> = Header<
  AppTableFeatures,
  TData,
  TValue
>;
type AppHeaderContext<TData extends RowData, TValue = unknown> = HeaderContext<
  AppTableFeatures,
  TData,
  TValue
>;
type AppTable<TData extends RowData> = Table<AppTableFeatures, TData>;
type AppTableOptions<TData extends RowData> = TableOptions<
  AppTableFeatures,
  TData
>;

export { appTableFeatures };
export type {
  AppCell,
  AppColumn,
  AppColumnDef,
  AppHeader,
  AppHeaderContext,
  AppRow,
  AppTable,
  AppTableFeatures,
  AppTableOptions,
  DataTableColumnMeta,
};
