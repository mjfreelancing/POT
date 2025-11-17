# Frontend Developer Guide

Comprehensive guide for developers working on the POT React frontend application.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Coding Guidelines](#coding-guidelines)
  - [Project Organization](#project-organization)
  - [TypeScript Standards](#typescript-standards)
  - [Code Formatting](#code-formatting)
  - [Function Declarations](#function-declarations)
- [Custom Components](#custom-components)
  - [Table Components](#table-components)
  - [Cards](#cards)
  - [Dialog Components](#dialog-components)
  - [Feedback Components](#feedback-components)
  - [Filter Components](#filter-components)
  - [Input Components](#input-components)
  - [Layout Components](#layout-components)
  - [Navigation Components](#navigation-components)
  - [Picker Components](#picker-components)
  - [Theme Components](#theme-components)
  - [User Components](#user-components)
- [Error Handling](#error-handling)
- [State Management](#state-management)
- [Authentication & Authorization](#authentication--authorization)
- [Routing](#routing)
- [API Integration](#api-integration)
- [Import/Export Features](#importexport-features)
- [Environment Configuration](#environment-configuration)
- [Available Commands](#available-commands)

---

## Architecture Overview

**Tech Stack:**

- React 19 + TypeScript
- Vite 6 (build tool)
- Zustand (global state)
- React Query (server state)
- TailwindCSS + shadcn/ui (styling)

**Project Structure:**

```
src/
├── api/                 # API integration layer
│   ├── errors/          # Error types (apiErrors.ts)
│   ├── hooks/           # React Query hooks (useApi.ts, useApiExport.ts, useApiImport.ts)
│   ├── interceptors/    # Axios interceptors (axiosInterceptors.ts)
│   └── types/           # API type definitions
├── components/          # Shared components
│   ├── table/           # DataTable library
│   ├── feedback/        # ErrorSheet, toasts
│   ├── dialog/          # Dialog wrappers
│   ├── filters/         # Filter components
│   ├── input/           # Input components
│   ├── layout/          # Layout components
│   ├── nav/             # Navigation components
│   ├── picker/          # Date pickers
│   ├── theme/           # Theme components
│   ├── user/            # User-related components
│   └── ui/              # shadcn components (don't edit)
├── contexts/            # React contexts
├── data/                # Static data and constants
├── features/            # Feature modules
│   ├── accounts/        # Account management
│   ├── approvals/       # Platform admin approvals
│   ├── auth/            # Authentication & authorization
│   ├── dashboard/       # Dashboard widgets
│   ├── expenses/        # Expense tracking
│   ├── incomes/         # Income management
│   ├── maintenance/     # Import/export functionality
│   ├── projections/     # Financial projections
│   ├── users/           # User management
│   └── userSettings/    # User settings
├── hooks/               # Custom hooks (useLocalStorage, useToast, etc.)
├── lib/                 # Utilities (result.ts, cacheInvalidation.ts, logging.ts)
├── routes/              # React Router routes
└── stores/              # Zustand stores (authStore, etc.)
```

---

## Coding Guidelines

These guidelines ensure consistency, maintainability, and code quality across the frontend application.

### Project Organization

#### Feature-Based Architecture

The codebase follows a **feature-based organization** where related functionality is grouped together rather than by technical layer.

**Feature Structure:**

```
src/features/accounts/
├── AccountsPage.tsx       # Main feature page/container
├── components/            # Feature-specific components
├── create/                # Create account workflow
├── edit/                  # Edit account workflow
├── delete/                # Delete account workflow
├── hooks/                 # Feature-specific hooks
├── schemas/               # Zod validation schemas
└── utils/                 # Feature-specific utilities
```

**Benefits:**

- All code for a feature lives together
- Easy to locate and modify feature functionality
- Clear boundaries between features
- Simplifies deletion or extraction of features

#### Single Responsibility Per Module

Each module (file/folder) should have one clear purpose:

✅ **Good Examples:**

- `useExpenseStorage.ts` - Manages expense-related localStorage only
- `ExpensesTable.tsx` - Renders expense table only
- `expenseTableRowUtils.ts` - Utility functions for expense table rows only

❌ **Anti-Patterns:**

- Mixing API calls with UI components
- Combining multiple unrelated utilities in one file
- God objects that handle everything

#### Dependency Rules

**Features depend on API, never the reverse:**

```
✅ CORRECT:
features/expenses → api/hooks/useExpenses

❌ INCORRECT:
api/hooks/useExpenses → features/expenses/utils
```

**Why?** API layer should be agnostic of UI features. It provides data services that any feature can consume.

**API Organization Supports This:**

The `/api` folder structure enforces this separation:

```
src/api/
├── hooks/           # React Query hooks - features consume these
├── errors/          # Error types - shared across all features
├── interceptors/    # Axios interceptors - infrastructure concern
├── types/           # API response types - contracts with backend
├── axios.ts         # Axios instance configuration
└── authClient.ts    # Auth-specific Axios client
```

**Key Points:**

- `hooks/` exports React Query hooks that features import
- `errors/` provides normalized error types consumed by error handling
- `types/` defines API contracts independent of UI concerns
- Features never import from other features (use shared utilities instead)

### TypeScript Standards

#### No `any` Types

The `any` type is **prohibited** via ESLint configuration.

**ESLint Configuration:**

```javascript
// eslint.config.mjs
tseslint.configs.strict; // Includes @typescript-eslint/no-explicit-any: error
```

**Why Enforce This?**

1. **Type Safety** - `any` defeats TypeScript's purpose by disabling type checking
2. **Refactoring Safety** - Breaking changes go undetected with `any`
3. **IDE Support** - Autocomplete and IntelliSense don't work with `any`
4. **Runtime Errors** - `any` moves errors from compile-time to runtime

**Alternatives to `any`:**

```tsx
// ❌ BAD - defeats type safety
function process(data: any) {
  return data.value;
}

// ✅ GOOD - use unknown and type guards
function process(data: unknown) {
  if (typeof data === 'object' && data !== null && 'value' in data) {
    return (data as { value: string }).value;
  }

  throw new Error('Invalid data structure');
}

// ✅ GOOD - use generics
function process<T extends { value: string }>(data: T) {
  return data.value;
}

// ✅ GOOD - define proper types
type DataStructure = {
  value: string;
  meta?: Record<string, unknown>;
};

function process(data: DataStructure) {
  return data.value;
}
```

**ESLint Strict Mode Benefits:**

Beyond `no-explicit-any`, strict mode enforces:

- `@typescript-eslint/no-non-null-assertion` - Avoid `!` operator
- `@typescript-eslint/no-unnecessary-type-assertion` - Remove redundant assertions
- `@typescript-eslint/prefer-as-const` - Use `as const` for literal types
- `@typescript-eslint/no-floating-promises` - Always handle promises

### Code Formatting

#### Export Conventions

**All exports at bottom of file** for easy discoverability:

```tsx
// ❌ BAD - exports scattered throughout file
export function ComponentA() {}
function helperB() {}
export function ComponentC() {}

// ✅ GOOD - all exports at bottom
function ComponentA() {}
function helperB() {}
function ComponentC() {}

export { ComponentA, ComponentC };
```

**Type vs Value Exports:**

Use `export type` for types, regular `export` for values:

```tsx
// Type definitions
type User = { id: string; name: string };
type Config = { apiUrl: string };

// Functions/values
function getUser() {}
const API_KEY = 'key';

// Exports at bottom - types separate from values
export type { Config, User };
export { API_KEY, getUser };
```

**Barrel Files (index.ts):**

Barrel files re-export public API of a module:

```tsx
// src/components/table/index.ts

// Export everything (types and values) from modules
export * from './BulkActionsBar';
export * from './DataTable';
export * from './dataTableColumnFactories';
export * from './dataTableUtils';

// Export default exports explicitly
export { default as BulkActionsBar } from './BulkActionsBar';
export { default as DataTable } from './DataTable';
export { default as DataTableColumnHeader } from './DataTableColumnHeader';
```

**Why Barrel Files?**

- Cleaner imports: `import { DataTable } from '@/components/table'`
- Control public API surface
- Easy to add/remove exports
- Consistent import patterns

**Barrel File Pattern:**

1. Use `export *` for named exports (types and values)
2. Use `export { default as X }` for default exports
3. Only export what consumers should use (internal helpers stay private)

#### Blank Line Spacing

**Add blank lines before and after logical code blocks** for readability:

```tsx
// ✅ GOOD - blank lines improve readability
function ExpensesPage() {
  const [searchTerm, setSearchTerm] = useState('');
  const navigate = useNavigate();

  const { data } = useApiGetExpenses();

  useEffect(() => {
    logger.info('ExpensesPage', 'Mounted');

    return () => {
      logger.info('ExpensesPage', 'Unmounted');
    };
  }, []);

  useEffect(() => {
    if (data?.success) {
      setExpenses(data.value);
    }
  }, [data]);

  const handleSearch = (term: string) => {
    setSearchTerm(term);
  };

  return <div>...</div>;
}
```

**Spacing Rules:**

- Blank line before `useEffect` hooks
- Blank line before `return` statement
- Blank line between consecutive `if` statements
- Blank line after logging statements in logical groups
- Blank line before function declarations

**Consecutive Logging:**

```tsx
// ✅ GOOD - grouped logging
logger.info('Component', 'Processing started');
logger.debug('Component', 'Data:', data);

const result = processData(data);

logger.info('Component', 'Processing complete');
logger.debug('Component', 'Result:', result);
```

#### Braces and Control Flow

**Always use braces `{}` with `if` statements**, even for single lines:

```tsx
// ❌ BAD - no braces
if (isLoading) return <Spinner />;

// ✅ GOOD - always use braces
if (isLoading) {
  return <Spinner />;
}

// ❌ BAD - no braces
if (error) setError(error);

// ✅ GOOD - always use braces
if (error) {
  setError(error);
}
```

**Why?**

- Prevents bugs when adding lines later
- Consistent code style
- Easier to read and debug
- Matches backend C# conventions

### Function Declarations

**Prefer function declarations over arrow functions** for named functions:

```tsx
// ✅ GOOD - function declaration
function AccountsPage() {
  // ...
}

// ✅ GOOD - arrow function for callbacks/inline
const handleClick = () => {
  // ...
};

useEffect(() => {
  // ...
}, []);

// ❌ BAD - unnecessary arrow function for component
const AccountsPage = () => {
  // ...
};
```

**Why Function Declarations?**

- Better stack traces (named functions)
- Hoisted (can be called before declaration if needed)
- Clearer intent (this is a primary function, not a variable)
- Matches React documentation patterns

**When to Use Arrow Functions:**

- Event handlers assigned to variables
- Callbacks passed to hooks
- Inline functions
- Functions that need to capture `this` (rare in modern React)

---

## Custom Components

POT provides a library of reusable components organized by purpose. These components are built on shadcn/ui primitives and tailored to the application's needs.

**Location:** `src/components/`

**Important:** Components in `src/components/ui/` are **shadcn/ui components** — do not edit these directly. For customization, create wrapper components or use composition.

---

### Table Components

**Location:** `src/components/table/`

Comprehensive table system built on `@tanstack/react-table` with support for sorting, selection, bulk actions, and custom column rendering.

**Components:**

- **`DataTable`** - Main table component with sorting, selection, and bulk actions
- **`BulkActionsBar`** - Dropdown menu for bulk operations on selected rows
- **`DataTableHeader`** - Table header with column headers
- **`DataTableContent`** - Table body (extracted for reuse in custom layouts)
- **`DataTableColumnHeader`** - Individual sortable column header with sort indicators

**Column Factory Utilities:**

Pre-built column creators for common data types:

- **`createMoneyValueColumn`** - Right-aligned currency values
- **`createDateColumn`** - Formatted dates with optional null handling
- **`createFrequencyColumn`** - Displays frequency with count (e.g., "2 Weeks", "One Time")

**Utility Functions:**

- **`createRowIdGetter`** - Creates row ID getter for objects with `rowId` property
- **`getSelectedRows`** - Extracts selected data items from row selection state

**Quick Start:**

```tsx
import { DataTable } from '@/components/table';
import type { ColumnDef } from '@tanstack/react-table';

type Expense = {
  rowId: string;
  description: string;
  amount: number;
  date: Date;
};

const columns: ColumnDef<Expense>[] = [
  { accessorKey: 'description', header: 'Description' },
  { accessorKey: 'amount', header: 'Amount' },
  { accessorKey: 'date', header: 'Date' },
];

<DataTable
  columns={columns}
  data={expenses}
  enableRowSelection={true}
  bulkActions={[
    {
      label: 'Delete Selected',
      onClick: items => handleDelete(items),
    },
  ]}
  getRowId={createRowIdGetter<Expense>()}
/>;
```

**Using Column Factories:**

```tsx
import {
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
} from '@/components/table';

const columns = [
  createMoneyValueColumn<Expense>({
    accessorKey: 'amount',
    header: 'Amount',
  }),
  createDateColumn<Expense>({
    accessorKey: 'date',
    header: 'Due Date',
  }),
  createFrequencyColumn<Expense>({
    countKey: 'count',
    frequencyKey: 'frequency',
    header: 'Frequency',
  }),
];
```

**Row Highlighting:**

```tsx
<DataTable
  columns={columns}
  data={expenses}
  highlightRowFilter={row => row.original.rowId === selectedId}
  highlightClassName="bg-blue-100 dark:bg-blue-900"
/>
```

**Key Props:**

- `columns` - TanStack Table column definitions
- `data` - Array of data objects
- `enableRowSelection` - Enable checkbox selection
- `bulkActions` - Array of bulk action definitions
- `getRowId` - Function to extract unique row ID (important for stable selection)
- `highlightRowFilter` - Function to determine which rows to highlight
- `onSelectionChange` - Callback when selection changes

**Examples:** See `src/features/expenses/components/ExpensesTable.tsx`, `src/features/accounts/components/AccountsTable.tsx`

---

### Cards

**Location:** `src/components/cards/`

Card components for displaying structured content.

**Components:**

- **`ActionCard`** - Interactive card with icon, title, and description that becomes a clickable button when onClick is provided

**ActionCard Features:**

- Icon display (Lucide icons)
- Title and optional description
- Loading state with spinner
- Optional click handler - entire card becomes clickable button with hover effects
- Disabled state support
- Optional tooltip (hint prop)
- RotateCw icon indicator appears when card is clickable
- Keyboard accessible (Enter/Space keys)

**Usage:**

```tsx
import { ActionCard } from '@/components/cards';
import { DollarSign } from 'lucide-react';

<ActionCard
  icon={<DollarSign />}
  title="Total Balance"
  description="$1,234.56"
  isLoading={isLoading}
  onClick={() => refetch()}
  enabled={canRefresh}
  hint="Click to refresh balance"
/>;
```

**Examples:** Dashboard quick actions (`src/features/dashboard/`)

---

### Dialog Components

**Location:** `src/components/dialog/`

Reusable dialog components for user interactions.

**Components:**

- **`ConfirmationDialog`** - Alert dialog for confirming destructive or important actions

**ConfirmationDialog Features:**

- Modal dialog with backdrop
- Customizable title and description
- Configurable confirm/cancel button labels
- Accessible (ARIA attributes, keyboard navigation)
- Built on shadcn/ui AlertDialog

**Usage:**

```tsx
import { ConfirmationDialog } from '@/components/dialog';

<ConfirmationDialog
  open={isOpen}
  title="Delete Expense"
  description="Are you sure you want to delete this expense? This action cannot be undone."
  confirmLabel="Delete"
  cancelLabel="Cancel"
  onConfirm={handleDelete}
  onCancel={() => setIsOpen(false)}
/>;
```

**Examples:** Delete confirmations throughout features (expenses, accounts, incomes)

---

### Feedback Components

**Location:** `src/components/feedback/`

Components for providing user feedback including errors, loading states, badges, and toasts.

**Subfolders:**

- **`badge/`** - Status badges
- **`dialog/`** - Feedback dialogs
- **`message/`** - Message displays
- **`popover/`** - Popover feedback
- **`sheet/`** - Sheet-based feedback (ErrorSheet)
- **`spinner/`** - Loading spinners
- **`toast/`** - Toast notifications

**Key Components:**

- **`ErrorSheet`** - Full-width error banner at top of page for critical errors
- **`LoadingOverlay`** - Absolute positioned loading spinner with backdrop blur and message
- **`StatusBadge`** - Colored badge with 7 color variants (red, orange, green, yellow, blue, purple, gray) and optional tooltip
- **`SuccessToast`** - Success toast notification with custom icon, title, description, and optional details
- **`ErrorToast`** - Error toast notification (fixed AlertTriangle icon, title and description)
- **`IconToast`** - Generic toast with custom icon and styling

**ErrorSheet Usage:**

```tsx
import { ErrorSheet } from '@/components/feedback';

{
  error && (
    <ErrorSheet
      title={error.title}
      description={error.description}
      onDismiss={() => setError(null)}
    />
  );
}
```

**Toast Usage:**

```tsx
import { ErrorToast, SuccessToast } from '@/components/feedback';
import { toast } from '@/hooks/useToast';
import { CheckCircle } from 'lucide-react';

// Success toast (requires icon, title, description; optional details)
toast(
  <SuccessToast
    icon={CheckCircle}
    title="Saved"
    description="Changes saved successfully"
    details="Optional additional info"
  />,
);

// Error toast (no icon prop - uses AlertTriangle internally)
toast(<ErrorToast title="Error" description="Failed to save changes" />);
```

**Guidelines:**

- Use `ErrorSheet` for **critical/blocking errors** (API failures, auth issues)
- Use toasts for **transient feedback** (success confirmations, minor errors)
- Only one ErrorSheet visible at a time
- Keep toast messages concise and actionable
- See [Error Handling](#error-handling) for complete patterns and examples

**Examples:** Error handling throughout application, especially in page components

---

### Filter Components

**Location:** `src/components/filters/`

Reusable filter components for data filtering.

**Components:**

- **`AccountFilter`** - Dropdown to filter by account
- **`SearchInput`** - Text input for searching/filtering with clear button

**AccountFilter Usage:**

```tsx
import { AccountFilter } from '@/components/filters';

<AccountFilter
  accounts={accounts}
  selectedAccountId={selectedId}
  onAccountChange={handleAccountChange}
/>;
```

**SearchInput Usage:**

```tsx
import { SearchInput } from '@/components/filters';

<SearchInput
  value={searchTerm}
  onChange={setSearchTerm}
  placeholder="Search by description..."
  ariaLabel="Search expenses"
  name="expense-search"
/>;
```

**Examples:** Expenses, incomes, and accounts pages use these filters

---

### Input Components

**Location:** `src/components/input/`

Specialized input components with custom behavior.

**Components:**

- **`MoneyValueInput`** - Formatted currency input with automatic decimal handling

**MoneyValueInput Features:**

- Automatic 2 decimal place formatting
- Supports both string and number values
- Provides both string and parsed number in onChange event
- Handles incomplete values (empty, dot, minus)
- Formatted display on blur
- Supports negative values
- TypeScript-safe with custom event type

**Usage:**

```tsx
import { MoneyValueInput } from '@/components/input';

<MoneyValueInput
  value={amount}
  onChange={e => {
    const stringValue = e.target.value;
    const numberValue = e.target.number; // parsed number | undefined
    setAmount(stringValue);
  }}
  placeholder="0.00"
/>;
```

**Why Custom Component?**

- Standard HTML number inputs don't preserve trailing zeros or dots during typing
- Provides better UX with automatic formatting on blur
- Type-safe parsing of string to number

**Examples:** Create/edit forms for expenses, incomes, accounts

---

### Layout Components

**Location:** `src/components/layout/`

Structural components for page layout.

**Components:**

- **`PageHeader`** - Standard page header with title, subtitle, sidebar trigger, and user menu
- **`Toolbar`** - Horizontal toolbar with `justify-between` layout (left/right alignment), styled with background, border, and padding

**PageHeader Usage:**

```tsx
import { PageHeader } from '@/components/layout';

<PageHeader
  title="Expenses"
  subtitle="Manage your expenses and recurring bills"
  showSidebarTrigger={true}
/>;
```

**Toolbar Usage:**

```tsx
import { Toolbar } from '@/components/layout';

<Toolbar>
  <SearchInput value={search} onChange={setSearch} />
  <AccountFilter
    accounts={accounts}
    selectedAccountId={selectedId}
    onAccountChange={setSelectedId}
  />
  <Button>Add Expense</Button>
</Toolbar>;
```

**Layout Pattern:**

```tsx
<div className="flex flex-col h-screen">
  <PageHeader title="Page Title" />
  <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
    <Toolbar>{/* filters and actions */}</Toolbar>
    <div className="flex-1 min-h-0">{/* scrollable content */}</div>
  </div>
</div>
```

**Examples:** All feature pages (expenses, accounts, incomes, projections, dashboard)

---

### Navigation Components

**Location:** `src/components/nav/`

Application navigation components.

**Components:**

- **`AppSidebar`** - Main application sidebar with collapsible menu
- **`AppSidebarHeader`** - Sidebar header with app branding
- **`AppSidebarMenus`** - Sidebar menu items with permission-based visibility
- **`AppSidebarTrigger`** - Button to toggle sidebar open/closed
- **`MenuGroup`** - Grouped menu items with section headings

**Sidebar Features:**

- Collapsible to icon-only mode
- Permission-based menu item visibility
- Theme toggle in footer
- Responsive design
- Built on shadcn/ui Sidebar components

**Usage:**

```tsx
import { AppSidebar } from '@/components/nav';
import { SidebarProvider } from '@/components/ui/sidebar';

<SidebarProvider>
  <AppSidebar />
  <main>{/* page content */}</main>
</SidebarProvider>;
```

**Menu Item Permission Example:**

See [Permission Components](#permission-components) for detailed usage of `PermissionGuard` and `WithPermission`.

```tsx
// In AppSidebarMenus.tsx
<PermissionGuard permissions={['expense:view']} mode="all">
  <SidebarMenuItem>
    <SidebarMenuButton asChild>
      <Link to="/expenses">Expenses</Link>
    </SidebarMenuButton>
  </SidebarMenuItem>
</PermissionGuard>
```

**Examples:** See `src/App.tsx` for sidebar setup

---

### Picker Components

**Location:** `src/components/picker/`

Date and time picker components.

**Components:**

- **`EnrichedCalendar`** - Calendar component with Accept/Cancel buttons, month/year navigation, and keyboard support
- **`EnrichedDatePicker`** - Date picker with popover and custom trigger (uses `EnrichedCalendar`)

**EnrichedCalendar Features:**

- Built on shadcn Calendar component (which wraps react-day-picker)
- Accept/Cancel buttons
- Clear date option
- Month navigation (forward/backward by month or year)
- Keyboard navigation (arrows, Enter, Escape)
- Optional future date restriction

**EnrichedDatePicker Features:**

- Popover-based date selection using `EnrichedCalendar`
- Custom trigger button with formatting
- Accept/Cancel actions
- Keyboard navigation support
- Configurable alignment
- Accessible (ARIA labels, keyboard support)

**Usage:**

```tsx
import { EnrichedDatePicker } from '@/components/picker';

<EnrichedDatePicker
  selectedDate={date}
  onDateAccepted={setDate}
  onCancel={() => console.log('Cancelled')}
  triggerLabel={date => (date ? format(date, 'PPP') : 'Pick a date')}
  triggerClassName="w-full"
/>;
```

**Why "Enriched"?**

Standard calendar components don't include Accept/Cancel actions or month/year navigation buttons. These enriched versions provide better UX with explicit confirmation and enhanced navigation controls.

**Examples:** Date selection in create/edit forms for expenses and incomes

---

### Theme Components

**Location:** `src/components/theme/`

Theme management components.

**Components:**

- **`ThemeProvider`** - Context provider for theme management
- **`ThemeToggle`** - Button to toggle between light/dark/system themes

**Theme Features:**

- Three theme modes: light, dark, system
- Persists to localStorage
- Respects system preference
- Smooth transitions
- Accessible

**Usage:**

```tsx
import { ThemeProvider } from '@/components/theme';

// In App.tsx or root component
<ThemeProvider defaultTheme="system">
  <YourApp />
</ThemeProvider>;
```

**ThemeToggle:**

```tsx
import { ThemeToggle } from '@/components/theme';

<ThemeToggle />; // Displays in sidebar footer
```

**Examples:** See `src/App.tsx` for ThemeProvider setup, `src/components/nav/AppSidebar.tsx` for ThemeToggle usage

---

### User Components

**Location:** `src/components/user/`

User-related UI components.

**Components:**

- **`UserMenu`** - Dropdown menu with user actions (settings, logout)

**UserMenu Features:**

- Displays username (or "User" if not loaded)
- Opens AccountSettingsSheet for user settings
- Logout action via logoutManager
- Accessible dropdown
- Built on shadcn/ui DropdownMenu

**Usage:**

```tsx
import { UserMenu } from '@/components/user';

<UserMenu />; // Typically in PageHeader
```

**Examples:** Used in `PageHeader` component across all pages

---

## Error Handling

The application uses a centralized error handling pattern based on **ErrorContext** and **ErrorSheet** to provide consistent, user-friendly error display across all features.

### Core Components

#### ErrorContext

**Location:** `src/contexts/ErrorContext.tsx`

Provides centralized error state management for the application.

**API:**

```tsx
type DisplayError = {
  title: string;
  description: string;
};

type ErrorContextType = {
  error: DisplayError | null;
  setError: (error: DisplayError | null) => void;
};
```

**Usage:**

```tsx
import { useErrorContext } from '@/contexts';

function MyComponent() {
  const { error, setError } = useErrorContext();

  // Set an error
  setError({
    title: 'API Error',
    description: 'Failed to load data. Please try again.',
  });

  // Clear the error (e.g., typically before performing an action)
  setError(null);
}
```

#### ErrorSheet

**Location:** `src/components/feedback/sheet/ErrorSheet.tsx`

A prominent, full-width banner component that displays errors at the top of the page or feature container. Provides clear visual feedback for critical or blocking errors.

**Props:**

```tsx
type ErrorSheetProps = {
  title: string;
  description: string;
  onDismiss: () => void;
};
```

**Features:**

- Prominent red banner with alert icon
- Clear title and description
- Dismissible (user can close)
- Positioned at top of page for visibility

**Example:**

```tsx
import { ErrorSheet } from '@/components/feedback';

{
  error && (
    <ErrorSheet
      title={error.title}
      description={error.description}
      onDismiss={() => setError(null)}
    />
  );
}
```

### Standard Error Handling Pattern

The application follows a consistent pattern across all pages and features:

#### 1. Setup ErrorContext

Import and destructure error state:

```tsx
import { useErrorContext } from '@/contexts';

function MyPage() {
  const { error, setError } = useErrorContext();
  // ...
}
```

#### 2. Monitor API Results

Use `useEffect` to watch API results and set errors:

```tsx
import { useApiGetData } from '@/api/hooks';

const { data: result } = useApiGetData();

// Monitor result and set error state
useEffect(() => {
  if (result) {
    setError(
      result.success
        ? null
        : {
            title: result.error.code,
            description: result.error.description,
          },
    );
  }
}, [result, setError]);
```

#### 3. Display ErrorSheet

Place ErrorSheet conditionally in JSX. It will always display at the **top** of the viewport due to fixed positioning, regardless of where it appears in the JSX structure.

**Common placement patterns:**

```tsx
// Pages with nested routes - before <Outlet />
return (
  <div className="flex flex-col h-screen">
    {/* Page content */}
    <div className="flex-1 min-h-0 flex flex-col">{/* Main content */}</div>

    {/* Error display */}
    {error && (
      <ErrorSheet
        title={error.title}
        description={error.description}
        onDismiss={() => setError(null)}
      />
    )}

    <Outlet />
  </div>
);

// Forms - typically at the top of form content
return (
  <Form {...form}>
    <form onSubmit={form.handleSubmit(onSubmit)}>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      {/* Form fields */}
    </form>
  </Form>
);

// Dialogs/Modals - outside the Dialog component
return (
  <>
    {error && (
      <ErrorSheet
        title={error.title}
        description={error.description}
        onDismiss={() => setError(null)}
      />
    )}
    <Dialog open={isOpen}>{/* Dialog content */}</Dialog>
  </>
);
```

### Complete Example: Page-Level Error Handling

Here's a complete example from `ExpensesPage.tsx`:

```tsx
import { useEffect } from 'react';
import { Outlet } from 'react-router';

import { useApiGetAllExpenses } from '@/api/hooks';
import { ErrorSheet, LoadingOverlay } from '@/components/feedback';
import { PageHeader, Toolbar } from '@/components/layout';
import { useErrorContext } from '@/contexts';
import { logger } from '@/lib/logging';

function ExpensesPage() {
  const { error, setError } = useErrorContext();

  // Fetch expenses data
  const {
    data: expensesResult,
    isLoading,
    isFetching,
  } = useApiGetAllExpenses();

  // Monitor API result and set error
  useEffect(() => {
    if (expensesResult) {
      setError(
        expensesResult.success
          ? null
          : {
              title: expensesResult.error.code,
              description: expensesResult.error.description,
            },
      );
    }
  }, [expensesResult, setError]);

  // Extract data
  const expenses = expensesResult?.success ? expensesResult.value : [];

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <PageHeader title="Expenses" subtitle="Track and manage your expenses" />

      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <Toolbar>{/* Toolbar content */}</Toolbar>

        <div className="flex-1 min-h-0 flex flex-col relative">
          {(isLoading || isFetching) && <LoadingOverlay />}
          {/* Table or other content */}
        </div>
      </div>

      {/* Error display - renders at top of viewport */}
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </div>
  );
}

export default ExpensesPage;
```

### Mutation Error Handling

For create/update/delete operations, handle errors in the mutation callback:

```tsx
import { useApiPostData } from '@/api/hooks';
import { toast } from 'sonner';
import { SuccessToast, ErrorToast } from '@/components/feedback/toast';

function MyForm() {
  const { error, setError } = useErrorContext();
  const mutation = useApiPostData();

  const onSubmit = async (data: FormData) => {
    const result = await mutation.mutateAsync({ data });

    if (result.success) {
      // Success case
      toast(
        <SuccessToast
          icon={CheckIcon}
          title="Success"
          description="Operation completed successfully."
        />,
      );
      // Navigate away or close form
    } else {
      // Error case - set error for ErrorSheet display
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      {/* Form content */}

      {/* Display errors */}
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </form>
  );
}
```

### Storage/Hook Error Handling

When using custom hooks that may fail (e.g., localStorage operations), pass error callbacks:

```tsx
import useExpenseStorage from './hooks/useExpenseStorage';

const { getExpenseData, setExpenseData } = useExpenseStorage(error => {
  setError({
    title: 'Storage Error',
    description: error.description,
  });
});
```

### Early Return for Loading Errors

For Sheets or Dialogs that load data on mount, handle loading errors with early returns:

```tsx
function InviteUserSheet() {
  const navigate = useNavigate();

  // Fetch required data
  const rolesQuery = useRoles();

  // Show loading state
  if (rolesQuery.isLoading) {
    return <LoadingMessage />;
  }

  // Handle load errors with early return + ErrorSheet
  // Note: API errors are always provided in error state, no fallback messages needed
  if (!rolesQuery.data?.success) {
    return (
      <ErrorSheet
        title={rolesQuery.data.error.code}
        description={rolesQuery.data.error.description}
        onDismiss={() => navigate('/users')}
      />
    );
  }

  // Continue with normal component rendering
  const roles = rolesQuery.data.value;

  return <Sheet>{/* Sheet content */}</Sheet>;
}
```

### When to Use ErrorSheet vs Toasts

**Use ErrorSheet for:**

- Critical or blocking errors (API failures, authentication issues)
- Validation errors preventing form submission
- Data loading failures
- Errors that require user acknowledgment

**Use Toasts for:**

- Transient feedback (success confirmations)
- Minor, non-blocking errors
- Background operation status
- User action confirmations

See [Feedback Components](#feedback-components) for toast usage patterns.

### Error Handling Guidelines

1. **Always use ErrorContext** for page-level and form-level errors
2. **Monitor API results** with `useEffect` to catch failures
3. **Place ErrorSheet in JSX** (it displays at top of viewport for visibility)
4. **Clear errors on dismiss** with `onDismiss={() => setError(null)}`
5. **Handle mutation errors** in the mutation callback, not in useEffect
6. **Use early returns** for load errors in Sheets/Dialogs
7. **Pass error callbacks** to custom hooks that may fail
8. **Never ignore errors** - always provide user feedback
9. **Log errors** using the logger utility for debugging
10. **Normalize all errors** to `{ title, description }` format
11. **Never use fallback error messages** - API errors are always provided in error state (no `??` operators needed)

### Common Patterns

#### Pattern: Page with API Data

```tsx
const { error, setError } = useErrorContext();
const { data: result } = useApiGetData();

useEffect(() => {
  if (result) {
    setError(
      result.success
        ? null
        : { title: result.error.code, description: result.error.description },
    );
  }
}, [result, setError]);

// Display ErrorSheet (renders at top of viewport)
{
  error && (
    <ErrorSheet
      title={error.title}
      description={error.description}
      onDismiss={() => setError(null)}
    />
  );
}
```

#### Pattern: Form with Mutation

```tsx
const { error, setError } = useErrorContext();
const mutation = useApiPostData();

const onSubmit = async data => {
  const result = await mutation.mutateAsync({ data });
  if (result.success) {
    toast(<SuccessToast />);
  } else {
    setError({
      title: result.error.code,
      description: result.error.description,
    });
  }
};
```

#### Pattern: Sheet/Dialog with Data Loading

```tsx
const rolesQuery = useRoles();

if (rolesQuery.isLoading) return <LoadingMessage />;
if (!rolesQuery.data?.success) {
  return (
    <ErrorSheet
      title={rolesQuery.data.error.code}
      description={rolesQuery.data.error.description}
      onDismiss={() => navigate('/back')}
    />
  );
}
```

---

## State Management

### Global State (Zustand)

Use for app-wide state that needs to persist across navigation.

**Location:** `src/stores/`

**Example:**

```tsx
import useUserStore from '@/stores/useUserStore';

const { userInfo, setUserInfo, clearUserInfo } = useUserStore();
```

### Server State (React Query)

Use for all API data. React Query handles caching, refetching, and background updates.

**Location:** `src/api/hooks/`

**Pattern:**

```tsx
const { data, isLoading } = useGet<Account[]>('/accounts', ['accounts']);
const { setError } = useErrorContext();

useEffect(() => {
  if (data) {
    setError(
      data.success
        ? null
        : { title: data.error.code, description: data.error.description },
    );
  }
}, [data, setError]);

// Use data.value
const accounts = data?.success ? data.value : [];
```

### Local State (React Context)

Use for feature-specific state that doesn't need to be global.

**Location:** Within feature folders (e.g., `src/features/accounts/context/`)

### Local Storage

Use type-safe `useLocalStorage` hook for browser storage.

**Location:** `src/hooks/useLocalStorage.ts`

**Example:**

```tsx
const [theme, setTheme] = useLocalStorage<'light' | 'dark'>('theme', 'light');
```

---

## Authentication & Authorization

### AuthContext

Central authentication context providing user state, tokens, and login/logout operations.

**Location:** `src/features/auth/AuthContext.tsx`

**Usage:**

```tsx
import useAuthContext from '@/features/auth/AuthContext';

function MyComponent() {
  const { isAuthenticated, userInfo, login, logout } = useAuthContext();

  if (!isAuthenticated) {
    return <LoginPrompt />;
  }

  return <div>Welcome, {userInfo?.username}</div>;
}
```

**AuthContext API:**

- `tokens: AuthTokens | undefined` - Current JWT tokens (access + refresh)
- `userInfo: User | null` - Current user details (username, email, permissions)
- `isAuthenticated: boolean` - Whether user is logged in
- `login: (tokens: AuthTokens) => void` - Store tokens and fetch user info
- `logout: () => void` - Clear tokens, user info, and redirect to login
- `error?: DisplayError` - Authentication error if any

### Logout Manager Pattern

Use `logoutManager` for triggering logout from anywhere without circular dependencies.

**Location:** `src/features/auth/logoutManager.ts`

**Usage:**

```tsx
import logoutManager from '@/features/auth/logoutManager';

// In event handlers
const handleLogout = () => {
  logoutManager.logout();
};

// In route components
function LogoutRoute() {
  logoutManager.logout();
  return <Navigate to="/login" replace />;
}
```

**Why?** Prevents circular dependencies when non-auth components need to trigger logout (e.g., API interceptors, error handlers, timeout handlers).

### Permission Components

#### PermissionGuard

Conditionally renders children based on permissions. Hides content completely if permission check fails.

**Location:** `src/features/auth/components/PermissionGuard.tsx`

**Usage:**

```tsx
import { PermissionGuard } from '@/features/auth/components/PermissionGuard';

// Single permission
<PermissionGuard permissions={['expense:manage']} mode="all">
  <DeleteButton />
</PermissionGuard>

// Multiple permissions (all required) - AND logic
<PermissionGuard permissions={['expense:manage', 'expense:view']} mode="all">
  <ManageExpenses />
</PermissionGuard>

// Multiple permissions (any required) - OR logic
<PermissionGuard permissions={['expense:manage', 'expense:view']} mode="any">
  <ExpenseActions />
</PermissionGuard>
```

**Props:**

- `permissions: Permission[]` - Array of required permissions
- `mode: 'all' | 'any'` - Check mode (all = AND, any = OR)
- `children: ReactNode` - Content to conditionally render

#### WithPermission

Renders children in disabled state when permissions are missing. Shows functionality exists but is not available.

**Location:** `src/features/auth/components/WithPermission.tsx`

**Usage:**

```tsx
import { WithPermission } from '@/features/auth/components/WithPermission';

// Disable button without permission
<WithPermission permissions={['account:manage']} mode="all">
  <Button>Delete Account</Button>
</WithPermission>

// Multiple permissions (any required)
<WithPermission permissions={['account:manage', 'account:view']} mode="any">
  <Button>Account Actions</Button>
</WithPermission>
```

**Props:**

- `permissions: Permission[]` - Array of required permissions
- `mode: 'all' | 'any'` - Check mode (all = AND, any = OR)
- `children: ReactElement` - Interactive element to conditionally disable

**When to Use:**

- **PermissionGuard**: Completely hide features user shouldn't see
- **WithPermission**: Show features exist but are unavailable (better UX for discoverable features)

### Permission Format

All permissions use lowercase `resource:action` format:

- `account:view` - View accounts
- `account:manage` - Create, update, delete accounts
- `expense:view` - View expenses
- `expense:manage` - Manage expenses
- `platform:manage` - Platform administration

### usePermissions Hook

Low-level permission checking hook.

**Location:** `src/hooks/usePermissions.ts`

**Usage:**

```tsx
import { usePermissions } from '@/hooks';

function MyComponent() {
  const { hasAllPermissions, hasAnyPermission, permissions } = usePermissions();

  const canManage = hasAllPermissions(['expense:manage']);
  const canView = hasAnyPermission(['expense:view', 'expense:manage']);

  // All user permissions
  console.log(permissions);
}
```

---

## Routing

### Protected Routes

Routes that require authentication use the `ProtectedRoute` wrapper component.

**Location:** `src/routes/AppRoutes.tsx`

**Pattern:**

```tsx
import { Navigate, Outlet, Route, Routes } from 'react-router-dom';
import useAuthContext from '@/features/auth/AuthContext';

function ProtectedRoute() {
  const { isAuthenticated } = useAuthContext();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/logout" element={<LogoutRoute />} />

      <Route element={<ProtectedRoute />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/accounts" element={<AccountsPage />} />
        {/* All nested routes require authentication */}
      </Route>
    </Routes>
  );
}
```

### Recovery Routes

**Logout Route:** Provides recovery mechanism for users in bad application states.

```tsx
function LogoutRoute() {
  logoutManager.logout();
  return <Navigate to="/login" replace />;
}

// User can navigate to /logout to force clean logout
<Route path="/logout" element={<LogoutRoute />} />;
```

### Catch-all Routes

Unknown routes redirect authenticated users to dashboard.

```tsx
// Inside protected routes
<Route path="*" element={<Navigate replace to="/dashboard" />} />
```

**Why?** Better UX than showing 404 for authenticated users who mistype URLs.

---

## API Integration

### Result Pattern

All API hooks return `Result<TSuccess, TFail>` instead of throwing errors.

**Why?** Type-safe error handling without try/catch. No unhandled promise rejections.

**Success Result:**

```tsx
type SuccessResult<T> = {
  success: true;
  value: T;
};
```

**Failure Result:**

```tsx
type FailResult<TError> = {
  success: false;
  error: TError;
};
```

**Usage:**

```tsx
const result = await mutation.mutateAsync(data);

if (result.success) {
  // result.value is available
  invalidateCache(['accounts']);
  toast(<SuccessToast title="Success" />);
} else {
  // result.error is available
  toast(<ErrorToast description={result.error.description} />);
  logger.error('Component', 'Operation failed', result.error);
}
```

### Available API Hooks

**Location:** `src/api/hooks/useApi.ts`

**Query Hooks:**

- `useGet<TResponse>(url, queryKey, options?)` - GET with caching

**Mutation Hooks:**

- `usePost<TResponse, TData>(url)` - POST with optional body
- `usePut<TResponse, TData>(url)` - PUT with required body
- `useDelete<TResponse>(url)` - DELETE
- `usePutWithId<TResponse, TData>(urlFn)` - PUT with ID parameter
- `usePutWithIdNoData<TResponse>(urlFn)` - PUT with ID, no body
- `usePostWithId<TResponse, TData>(urlFn)` - POST with ID parameter
- `usePostWithIdNoData<TResponse>(urlFn)` - POST with ID, no body

All mutation hooks support `signal?: AbortSignal` for cancellation.

### Cache Invalidation

Use centralized cache invalidation to automatically handle dependency chains.

**Location:** `src/lib/cacheInvalidation.ts`

**Available Cache Keys:**

- `accounts` - Account data
- `expenses` - Expense records
- `incomes` - Income records
- `me` - Current user data
- `pending-approvals` - Pending user approvals
- `projections` - Financial projections
- `users` - User management data

**Dependency Chains:**

- `expenses` → invalidates `accounts` → invalidates `projections`
- `incomes` → invalidates `accounts` → invalidates `projections`

**Usage:**

```tsx
import { useCacheInvalidation } from '@/lib/cacheInvalidation';

const { queryClient } = useApiClient();
const { invalidateCache } = useCacheInvalidation(queryClient);

// After mutation
invalidateCache(['expenses']); // Also invalidates accounts and projections
```

### Error Types

**Location:** `src/api/errors/apiErrors.ts`

All errors are normalized by Axios interceptors into:

- `AuthenticationError` - Authentication failures (401)
- `ForbiddenError` - Authorization failures (403)
- `NotFoundError` - Resource not found (404)
- `MethodNotAllowedError` - HTTP method not allowed (405)
- `ConflictError` - Resource conflicts (409)
- `ValidationError` - Request validation failures (422)
- `RateLimitedError` - Rate limit exceeded (429)
- `NetworkError` - Network/connection errors
- `UnexpectedError` - Unexpected/unhandled errors

See [Error Handling](#error-handling) for complete error display patterns and usage.

---

## Import/Export Features

### Why Custom Hooks?

Import/export hooks **cannot** use generic `useGet()`/`usePost()` hooks.

**Export Hook (`useApiExport`):**

Cannot use `useGet()` because:

1. Requires `responseType: 'blob'` for file downloads
2. Needs both `response.data` (blob) AND `response.headers` (filename)
3. Uses `useMutation` (user action) not `useQuery` (auto-fetch)

**Import Hook (`useApiImport`):**

Cannot use `usePost()` because:

1. Requires `FormData` instead of JSON
2. Needs `Content-Type: multipart/form-data`
3. Takes `File` parameter instead of generic `TData`

### React Query Pattern: useQuery vs useMutation

**Export/Import are user actions** → Use `useMutation`:

```tsx
const { exportData } = useExport();
<Button onClick={exportData}>Export</Button>;
```

**Why not useQuery?**

- Would auto-execute on mount
- Would cache blob results unnecessarily
- Would refetch in background
- Doesn't match user intention (one-time action)

---

## Environment Configuration

### Environment Files

The application uses Vite's environment variable system with three environment files:

**`.env`** - Base configuration (shared across all environments):

```properties
# 30 seconds allows for Azure cold starts and network latency
VITE_API_TIMEOUT_MS=30000
```

**`.env.development`** - Local development (`npm run dev`):

```properties
# Development environment variables (npm run dev)

# API in docker
# VITE_API_BASE_URL=http://localhost:5241/api

# API in local machine
VITE_API_BASE_URL=http://localhost:5242/api
```

**`.env.production`** - Production builds (`npm run build`):

```properties
# Production environment variables (npm run build)
VITE_API_BASE_URL=/api
```

### Environment Variables

**Available Variables:**

- `VITE_API_BASE_URL` - Backend API base URL (required)
- `VITE_API_TIMEOUT_MS` - API request timeout in milliseconds (default: 30000)

**Important Rules:**

1. Only `VITE_` prefixed variables are exposed to the React application
2. Environment variables are embedded at **build time**, not runtime
3. Changes require rebuild (`npm run build`) or dev server restart (`npm run dev`)

### Usage in Code

```typescript
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
const apiTimeout = import.meta.env.VITE_API_TIMEOUT_MS;
```

### Development vs Production

**Development mode** (`npm run dev`):

- Uses `.env` + `.env.development`
- Hot module replacement enabled
- Points to backend running locally (`http://localhost:5242/api`)

**Production mode** (`npm run build`):

- Uses `.env` + `.env.production`
- Optimized bundle with minification
- Environment variables embedded at build time

**For Docker builds and deployment configuration, see:** [Docker Setup](../../Docker/DEVELOPER.md)

---

## Available Commands

### Development

**Start development server:**

```bash
npm run dev
```

Frontend runs on `http://localhost:5175` with hot module replacement.

**Build for production:**

```bash
npm run build
```

**Preview production build:**

```bash
npm run preview
```

### Testing & Quality

**Run unit tests:**

```bash
npm run test
```

Framework: Vitest. Tests located in `tests/` folder or `*.test.tsx` files.

**Type checking:**

```bash
npm run type:check
```

Runs TypeScript compiler in check mode (no output files).

**Linting:**

```bash
npm run lint
```

Runs ESLint with strict TypeScript rules.

**Format code:**

```bash
npm run prettier
```

Formats all files using Prettier configuration.

---

## Additional Resources

- [React Query Documentation](https://tanstack.com/query/latest)
- [Zustand Documentation](https://zustand-demo.pmnd.rs/)
- [TanStack Table Documentation](https://tanstack.com/table/latest)
- [Vite Documentation](https://vite.dev/)
- [shadcn/ui Components](https://ui.shadcn.com/)

---

**For backend integration patterns, see:** `Source/Server/DEVELOPER.md`

**For Docker setup, see:** `Source/Docker/DEVELOPER.md`
