# DataTable Component Library

A comprehensive, reusable table component library built on top of `@tanstack/react-table` with full TypeScript support.

## 📁 Components Overview

### Core Components

- **`DataTable`** - Main table component with sorting, selection, and bulk actions
- **`BulkActionsBar`** - Standalone bulk actions UI component
- **`DataTableHeader`** - Reusable table header component
- **`DataTableColumnHeader`** - Individual sortable column header component

### Utilities

- **`dataTableColumnFactories`** - Helper functions for common column types

---

## 🚀 Quick Start

### Basic Table

```tsx
import { DataTable } from '@/components/table';
import { ColumnDef } from '@tanstack/react-table';

type Person = {
  id: number;
  name: string;
  email: string;
};

const columns: ColumnDef<Person>[] = [
  {
    accessorKey: 'name',
    header: 'Name',
  },
  {
    accessorKey: 'email',
    header: 'Email',
  },
];

function MyTable() {
  return <DataTable columns={columns} data={people} />;
}
```

### Table with Row Selection and Bulk Actions

```tsx
import { DataTable, BulkAction } from '@/components/table';

const bulkActions: BulkAction<Person>[] = [
  {
    label: 'Delete Selected',
    onClick: selectedItems => {
      console.log('Deleting:', selectedItems);
      // Implement delete logic
    },
  },
  {
    label: 'Export to CSV',
    onClick: selectedItems => {
      console.log('Exporting:', selectedItems);
      // Implement export logic
    },
  },
];

function MyTableWithSelection() {
  return (
    <DataTable
      columns={columns}
      data={people}
      enableRowSelection={true}
      bulkActions={bulkActions}
      onSelectionChange={selected => console.log('Selected:', selected)}
    />
  );
}
```

### Table with Row Highlighting

```tsx
function MyTableWithHighlighting() {
  const [activeId, setActiveId] = useState<number | null>(null);

  return (
    <DataTable
      columns={columns}
      data={people}
      highlightRowFilter={row => row.original.id === activeId}
      highlightClassName="bg-blue-100 dark:bg-blue-900"
    />
  );
}
```

---

## 🏗️ Column Factory Functions

For common data types, use the provided factory functions:

### Money Values

```tsx
import { createMoneyValueColumn } from '@/components/table';

// Creates a right-aligned money column with proper formatting
createMoneyValueColumn<MyData>('amount', 'Amount', {
  enableSorting: true, // Optional: enable sorting
});
```

### Date Values

```tsx
import { createDateColumn } from '@/components/table';

// Creates a date column with dd-MM-yyyy format
createDateColumn<MyData>('createdAt', 'Created', 'Not Set', {
  enableSorting: true, // Optional: enable sorting
});
```

### Frequency Values

```tsx
import { createFrequencyColumn } from '@/components/table';

// Creates a column showing "count frequency" (e.g., "2 Weeks", "1 Month")
createFrequencyColumn<MyData>('count', 'frequency', 'Payment Frequency');
```

### Complete Example with Factory Functions

```tsx
import {
  DataTable,
  createMoneyValueColumn,
  createDateColumn,
  createFrequencyColumn,
} from '@/components/table';

const columns: ColumnDef<Expense>[] = [
  {
    accessorKey: 'description',
    header: 'Description',
    enableSorting: true,
  },
  createMoneyValueColumn<Expense>('amount', 'Amount'),
  createDateColumn<Expense>('dueDate', 'Due Date', 'Ongoing'),
  createFrequencyColumn<Expense>('frequencyCount', 'frequency', 'Frequency'),
];
```

---

## 🎨 Styling & Themes

### Default Styling

The DataTable comes with built-in light/dark theme support:

- Light theme: Gray headers, white rows
- Dark theme: Dark gray headers, dark rows
- Hover effects for interactive elements

### Custom Styling

Override default classes using component props:

```tsx
// Custom row highlighting
<DataTable
  highlightClassName="bg-emerald-100 dark:bg-emerald-900"
  // ... other props
/>

// Custom header styling (when using DataTableHeader directly)
<DataTableHeader
  headerGroups={headerGroups}
  headerClassName="bg-slate-100 dark:bg-slate-800"
  rowClassName="bg-slate-50 dark:bg-slate-700"
  cellClassName="font-bold text-lg"
/>
```

---

## 🔧 Advanced Usage

### Custom Bulk Actions

```tsx
const advancedBulkActions: BulkAction<MyData>[] = [
  {
    label: 'Archive Selected',
    onClick: async items => {
      try {
        await archiveItems(items.map(item => item.id));
        // Show success message
        toast.success(`Archived ${items.length} items`);
        // Refresh data
        refetch();
      } catch (error) {
        toast.error('Failed to archive items');
      }
    },
  },
  {
    label: 'Bulk Edit',
    onClick: items => {
      setBulkEditItems(items);
      setShowBulkEditModal(true);
    },
  },
];
```

### Custom Column Headers with Sorting

```tsx
import { DataTableColumnHeader } from '@/components/table';

const customColumns: ColumnDef<MyData>[] = [
  {
    accessorKey: 'name',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Full Name" />
    ),
    enableSorting: true,
  },
  // ... more columns
];
```

### Controlled Selection State

```tsx
function ControlledSelectionTable() {
  const [selection, setSelection] = useState<MyData[]>([]);

  return (
    <DataTable
      columns={columns}
      data={data}
      enableRowSelection={true}
      onSelectionChange={setSelection}
      // Selection state is controlled by parent component
    />
  );
}
```

---

## 📝 TypeScript Support

### Generic Type Safety

The DataTable is fully generic and type-safe:

```tsx
// Type is automatically inferred from your data
type Product = {
  id: number;
  name: string;
  price: number;
  category: string;
};

// DataTable<Product, any> - TData is inferred as Product
<DataTable<Product> columns={productColumns} data={products} />;

// Bulk actions are also type-safe
const actions: BulkAction<Product>[] = [
  {
    label: 'Update Prices',
    onClick: (products: Product[]) => {
      // TypeScript knows this is Product[]
      products.forEach(product => {
        console.log(product.price); // ✅ Type-safe access
      });
    },
  },
];
```

### Available Types

```tsx
import type {
  DataTableProps,
  BulkAction,
  BulkActionsBarProps,
  DataTableHeaderProps,
} from '@/components/table';
```

---

## 🧪 Testing

### Testing DataTable

```tsx
import { render, screen } from '@testing-library/react';
import { DataTable } from '@/components/table';

test('renders table with data', () => {
  const mockData = [
    { id: 1, name: 'John', email: 'john@example.com' },
    { id: 2, name: 'Jane', email: 'jane@example.com' },
  ];

  render(<DataTable columns={columns} data={mockData} />);

  expect(screen.getByText('John')).toBeInTheDocument();
  expect(screen.getByText('jane@example.com')).toBeInTheDocument();
});
```

### Testing Bulk Actions

```tsx
import userEvent from '@testing-library/user-event';

test('bulk actions work correctly', async () => {
  const mockAction = jest.fn();
  const bulkActions = [{ label: 'Test Action', onClick: mockAction }];

  render(
    <DataTable
      columns={columns}
      data={mockData}
      enableRowSelection={true}
      bulkActions={bulkActions}
    />,
  );

  // Select a row
  await userEvent.click(screen.getAllByRole('checkbox')[1]);

  // Click the actions dropdown
  await userEvent.click(screen.getByText('Actions'));

  // Click the test action
  await userEvent.click(screen.getByText('Test Action'));

  expect(mockAction).toHaveBeenCalledWith([mockData[0]]);
});
```

---
