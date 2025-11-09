# DataTable Component Library

A comprehensive, reusable table system built on top of `@tanstack/react-table` with full TypeScript support, advanced features, and a modular architecture.

## Components

- **DataTable**: Main table component. Handles configuration, sorting, selection, bulk actions, and row highlighting.
- **BulkActionsBar**: Dropdown menu for bulk actions, shown when row selection is enabled.
- **DataTableHeader**: Renders table headers, supports custom classes, and integrates with react-table header groups.
- **DataTableContent**: Renders table body and rows, extracted for code reuse.
- **DataTableColumnHeader**: Sortable column header with sort icons and three-state sorting.
- **dataTableColumnFactories**: Utility functions for common column types: money, date, frequency.
- **dataTableUtils**: Utility functions for row identification and selection persistence.

All components and utilities are exported from `index.ts` for convenient import.

---

## Quick Start

### Basic Table

```tsx
import { DataTable } from '@/components/table';
import { ColumnDef } from '@tanstack/react-table';

type Person = { id: number; name: string; email: string };

const columns: ColumnDef<Person>[] = [
  { accessorKey: 'name', header: 'Name' },
  { accessorKey: 'email', header: 'Email' },
];

<DataTable columns={columns} data={people} />;
```

### Row Selection and Bulk Actions

```tsx
import { DataTable, BulkAction } from '@/components/table';

const bulkActions: BulkAction<Person>[] = [
  {
    label: 'Delete Selected',
    onClick: items => {
      /* ... */
    },
  },
  {
    label: 'Export to CSV',
    onClick: items => {
      /* ... */
    },
  },
];

<DataTable
  columns={columns}
  data={people}
  enableRowSelection={true}
  bulkActions={bulkActions}
  onSelectionChange={selected => {
    /* ... */
  }}
/>;
```

### Row Highlighting

```tsx
<DataTable
  columns={columns}
  data={people}
  highlightRowFilter={row => row.original.id === activeId}
  highlightClassName="bg-blue-100 dark:bg-blue-900"
/>
```

---

## Column Factory Utilities

Import from `dataTableColumnFactories` for common column types:

- `createMoneyValueColumn<T>()`: Right-aligned, formatted money column
- `createDateColumn<T>()`: Date column with formatting and null handling
- `createFrequencyColumn<T>()`: Frequency display (e.g., "2 Weeks")

Example:

```tsx
import {
  createMoneyValueColumn,
  createDateColumn,
  createFrequencyColumn,
} from '@/components/table';

const columns = [
  createMoneyValueColumn<MyType>('amount', 'Amount'),
  createDateColumn<MyType>('createdAt', 'Created'),
  createFrequencyColumn<MyType>('count', 'frequency', 'Frequency'),
];
```

---

## Customization & Styling

- **Row Highlighting**: Use `highlightRowFilter` and `highlightClassName` props.
- **Header Styling**: Use `DataTableHeader` with custom class props.
- **Bulk Actions**: Pass an array of actions to `bulkActions`.

---

## Utility Functions

- **createRowIdGetter<TData extends Identity>()**: Creates a getRowId function for Identity-based objects (objects with rowId property). Perfect for Expense, Income, and Account.

Example:

```tsx
import { DataTable, createRowIdGetter } from '@/components/table';

// For Identity-based objects (recommended)
<DataTable
  columns={columns}
  data={expenses}
  enableRowSelection={true}
  bulkActions={bulkActions}
  getRowId={createRowIdGetter<Expense>()}
/>;
```

---

## Advanced Usage

- **Custom Column Headers**: Use `DataTableColumnHeader` for sortable headers.
- **Controlled Selection**: Use `onSelectionChange` to manage selection state externally.
- **Reusable Table Body**: Use `DataTableContent` for custom table implementations.
- **Selection Persistence**: By default, the table uses array indices for row identification (standard react-table behavior). For data that changes and reorders, provide a `getRowId` function or use the provided `createRowIdGetter()` utility for Identity-based objects.

---

## TypeScript Support

- All components and utilities are fully generic and type-safe.
- See `DataTableProps`, `BulkAction`, and other exported types for details.

---

## Testing

- Use React Testing Library to test rendering, selection, and bulk actions.

---

## API Reference

### DataTable Props

- `columns`: ColumnDef<TData, TValue>[] — Column definitions
- `data`: TData[] — Table data
- `highlightRowFilter?`: (row: Row<TData>) => boolean — Highlight row filter
- `highlightClassName?`: string — Class for highlighted rows
- `enableRowSelection?`: boolean — Enable row selection
- `bulkActions?`: BulkAction<TData>[] — Bulk actions for selected rows
- `onSelectionChange?`: (selectedItems: TData[]) => void — Selection callback
- `getRowId?`: (row: TData, index: number) => string — Custom row ID function for selection persistence (defaults to index)

### BulkActionsBar Props

- `selectedCount`: number
- `selectedItems`: TData[]
- `bulkActions`: BulkAction<TData>[]
- `isVisible`: boolean

### DataTableHeader Props

- `headerGroups`: HeaderGroup<TData>[]
- `headerClassName?`, `rowClassName?`, `cellClassName?`: string

### DataTableContent Props

- `table`: Table<TData>
- `tableColumns`: ColumnDef<TData, TValue>[]
- `highlightRowFilter?`, `highlightClassName?`: as above

### DataTableColumnHeader Props

- `column`: Column<TData, TValue>
- `title`: string
- `className?`: string

### Utility Functions

#### createRowIdGetter<TData extends Identity>()

Creates a type-safe getRowId function for objects that extend Identity (have rowId property).

---
