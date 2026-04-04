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
  - [Sheet Components](#sheet-components)
- [Page Layout & Styling Patterns](#page-layout--styling-patterns)
  - [Projections Page](#projections-page)
  - [Dashboard Page](#dashboard-page)
  - [Accounts Page](#accounts-page)
  - [Expenses Page](#expenses-page)
  - [Incomes Page](#incomes-page)
  - [Reusable Layout Components](#reusable-layout-components)
- [Error Handling](#error-handling)
- [State Management](#state-management)
- [Authentication & Authorization](#authentication--authorization)
- [Routing](#routing)
- [API Integration](#api-integration)
- [Import/Export Features](#importexport-features)
- [Environment Configuration](#environment-configuration)
- [Progressive Web App (PWA)](#progressive-web-app-pwa)
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
├── concerns/            # Cross-cutting infrastructure (auth, logging, cache)
│   ├── auth/            # tokenProvider, logoutManager, permissions
│   ├── cache/           # cacheInvalidation
│   └── logging/         # logger
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
├── lib/                 # Pure utilities (dateUtils, moneyUtils, result.ts)
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

#### Concerns: Cross-Cutting Infrastructure

The `/concerns` folder houses **infrastructure-level utilities and singletons** that provide cross-cutting functionality used throughout the application. This pattern mirrors the backend's `Concerns/` folder structure.

**What Belongs in Concerns:**

✅ **Should be in concerns:**

- Global singleton instances (tokenProvider, logoutManager, logger)
- Cross-cutting functionality used by multiple features
- Infrastructure concerns (auth, logging, caching, permissions)
- Services that work independently of any specific feature

❌ **Should NOT be in concerns:**

- Pure utility functions (those belong in `lib/`)
- Feature-specific logic (belongs in `features/`)
- React components (belongs in `components/` or `features/`)
- Feature-specific hooks (belongs in feature's `hooks/`)

**Concerns Structure:**

```
src/concerns/
├── auth/
│   ├── tokenProvider.ts      # Global token provider singleton
│   ├── authTokenProvider.ts  # Token provider factory
│   ├── logoutManager.ts      # Centralized logout manager
│   ├── permissions.ts        # Permission constants and types
│   └── index.ts              # Auth barrel export
├── cache/
│   ├── cacheInvalidation.ts  # Centralized cache invalidation with dependencies
│   └── index.ts              # Cache barrel export
├── logging/
│   ├── logger.ts             # Global logger utility
│   └── index.ts              # Logging barrel export
└── index.ts                  # Root barrel export
```

**Usage Examples:**

```typescript
// Import from root barrel (recommended for mixed concerns)
import { tokenProvider, logger, useCacheInvalidation } from '@/concerns';
import type { Permission } from '@/concerns';

// Or import from specific concern (clearer when using many items from one concern)
import { tokenProvider, logoutManager, PERMISSIONS } from '@/concerns/auth';
import type { Permission } from '@/concerns/auth';

import { useCacheInvalidation, invalidateCache } from '@/concerns/cache';
import type { CacheKey } from '@/concerns/cache';

import { logger } from '@/concerns/logging';

// Both patterns work - choose based on readability
```

**Key Differences: `lib/` vs `concerns/`**

| Aspect           | `lib/`                           | `concerns/`                          |
| ---------------- | -------------------------------- | ------------------------------------ |
| **Purpose**      | Pure utility functions           | Singleton instances & infrastructure |
| **State**        | Stateless                        | May hold state (singletons)          |
| **Examples**     | dateUtils, moneyUtils, result.ts | tokenProvider, logger, logoutManager |
| **Dependencies** | Minimal (mostly pure functions)  | May depend on other infrastructure   |
| **Lifecycle**    | Function calls                   | Often initialized once (singletons)  |

**Why This Pattern?**

1. **Consistency with Backend** - Same organizational principle as server's `Concerns/` folder
2. **Clear Separation** - Distinguishes infrastructure from utilities and features
3. **Better Discoverability** - Singletons grouped by domain (auth, logging, cache)
4. **Prevents Circular Dependencies** - Centralized infrastructure doesn't depend on features
5. **Scalability** - Easy to add more concerns (analytics, monitoring, telemetry)

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

**Features depend on API and concerns, never the reverse:**

```
✅ CORRECT:
features/expenses → api/hooks/useExpenses
features/auth → concerns/auth/tokenProvider

❌ INCORRECT:
api/hooks/useExpenses → features/expenses/utils
concerns/logging → features/dashboard
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

### Badge Styling System

**Location:** `src/lib/badgeStyles.ts`

Centralized badge styling utilities for consistent badge appearances across the application. The system provides three main patterns optimized for different contexts.

#### Overview

Badge styling is **decoupled from semantics** - helpers generate className strings while the caller provides text content. This separation enables:

- Consistent visual appearance across features
- Type-safe color and variant choices
- Reusable patterns for common badge types
- Single source of truth for badge sizing and spacing

#### Three Badge Patterns

**1. Table Badges** (`getTableBadgeClass`)

- **Use For:** Frequency columns, non-date table badges, end date labels
- **Sizing:** Small and uniform - `text-[12px]`, `min-w-[80px]`, `px-2`, `py-1`
- **Variants:** Both filled and outline supported
- **Options:** Optional left margin (`withMargin: true`) for badges appearing after date text

**2. Status Badges** (`getStatusBadgeClass`)

- **Use For:** Next Due column status indicators (Overdue, Due Soon, etc.)
- **Predefined Types:** `excluded`, `due-today`, `overdue`, `due-soon`, `ended`
- **Always:** Filled variant with left margin
- **Color Meanings:** Red=overdue, amber=due today, orange=due soon, slate=excluded/ended
- **Why:** Enforces consistent status color semantics across the app

**3. General Badges** (`getBadgeClass`)

- **Use For:** Dashboard, custom locations, non-table contexts
- **Sizing:** Larger than table badges - `text-xs`, `min-w-[80px]`
- **Variants:** Both filled and outline supported
- **Spacing:** Filled variant includes `ml-2` margin

#### Decision Tree

```
Need a badge?
├─ In a table column?
│  ├─ Status indicator (Overdue, Due Soon)? → getStatusBadgeClass('overdue')
│  └─ Other (frequency, end date)? → getTableBadgeClass('green', 'filled')
└─ Outside table (dashboard, custom)? → getBadgeClass('blue', 'outline')
```

#### Color Palette

| Color  | Semantic Use               | Examples                          |
| ------ | -------------------------- | --------------------------------- |
| red    | Errors, overdue, urgent    | Overdue status, validation errors |
| orange | Warnings, due soon         | Due within 7 days                 |
| amber  | Due today, attention       | Due today status                  |
| green  | Success, active, recurring | Frequency badges                  |
| yellow | Caution                    | Custom warnings                   |
| blue   | Informational              | General info                      |
| purple | Special, unique            | Custom labels                     |
| pink   | Highlight                  | One-time labels, highlights       |
| slate  | Disabled, excluded, ended  | Excluded items, ended items       |

#### Variants

**Filled Variant:**

- Bold backgrounds with high contrast
- Use for emphasis and status indicators
- Example: Status badges (Overdue, Due Soon)

**Outline Variant:**

- Light backgrounds with subtle borders
- Use for informational, non-critical badges
- Example: End date badges, frequency badges with less emphasis

#### Usage Examples

**Table Frequency Badge (filled):**

```tsx
import { getTableBadgeClass } from '@/lib';
import { Badge } from '@/components/ui/badge';

<Badge variant="secondary" className={getTableBadgeClass('green', 'filled')}>
  2 Weeks
</Badge>;
```

**End Date Text (no badge):**

```tsx
<span>{formatDate(endDate)}</span>
```

**Note:** End dates are rendered as plain text to keep dates readable and avoid shrinking the date font.

**Status Badge with Margin:**

```tsx
import { getStatusBadgeClass } from '@/lib';

// In Next Due column - badge appears after date text
<div className="flex items-center">
  <span className="min-w-[80px]">{formattedDate}</span>
  <Badge variant="destructive" className={getStatusBadgeClass('overdue')}>
    Overdue
  </Badge>
</div>;
```

**Handling End Date Labels and One-Time:**

```tsx
// Excluded items use slate styling; one-time uses blue when active
const isExcluded = row.original.excludeFromCalcs;
const isOneTime = row.original.frequency === Frequency.OneTime;

if (isOneTime) {
  return (
    <Badge
      variant="secondary"
      className={getTableBadgeClass(
        isExcluded ? 'slate' : 'pink',
        isExcluded ? 'filled' : 'outline',
      )}
    >
      One-time
    </Badge>
  );
}

if (!endDate) {
  return null;
}
```

**Disabled Frequency Badge (outline, transparent background):**

```tsx
<Badge
  variant="secondary"
  className={`${getTableBadgeClass('slate', 'outline')} bg-transparent dark:bg-transparent text-slate-400 dark:text-slate-400`}
>
  {displayValue}
</Badge>
```

**Dashboard Badge:**

```tsx
import { getBadgeClass } from '@/lib';

<Badge variant="default" className={getBadgeClass('blue', 'filled')}>
  Active
</Badge>;
```

#### Best Practices

1. **Consistency Over Customization**

   - Always use badge helpers instead of inline Tailwind classes
   - Maintains visual consistency across features
   - Easier to update styling globally

2. **Color Semantics**

   - Follow established color meanings (red=error/overdue, green=success)
   - Use `getStatusBadgeClass` for status indicators to enforce semantic colors
   - Don't use red for non-critical states

3. **Table Alignment**

   - Use `getTableBadgeClass` for all table badges to ensure uniform sizing
   - Use `withMargin: true` only when badge appears after text (e.g., status after date)
   - Frequency and end date columns typically don't need margin

4. **Filled vs Outline**

   - Filled: Use for emphasis, status, active states
   - Outline: Use for informational, passive, descriptive badges
   - Mix variants in same table for visual hierarchy

5. **Avoid Anti-Patterns**
   - ❌ Don't create custom badge styles inline
   - ❌ Don't use different sizes for badges in same table column
   - ❌ Don't override color meanings (e.g., green for errors)
   - ✅ Use helper functions consistently
   - ✅ Keep badge text concise (2-3 words max)

#### Implementation Notes

**Type Safety:**

```typescript
type BadgeColor =
  | 'red'
  | 'orange'
  | 'amber'
  | 'green'
  | 'yellow'
  | 'blue'
  | 'purple'
  | 'pink'
  | 'slate';

type BadgeVariant = 'filled' | 'outline';

type StatusBadgeType =
  | 'excluded'
  | 'due-today'
  | 'overdue'
  | 'due-soon'
  | 'ended';
```

**Dark Mode Support:**

All color schemes include dark mode variants:

- Filled badges: Adjust opacity and use darker backgrounds
- Outline badges: Switch to darker borders and lighter text
- Automatic via Tailwind `dark:` prefix

**Examples in Codebase:**

- Status badges: `src/features/expenses/components/ExpensesTable.tsx`
- Frequency badges: `src/components/table/dataTableColumnFactories.tsx`
- End date badges: `src/features/incomes/components/IncomesTable.tsx`

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

### Sheet Components

**Location:** shadcn/ui Sheet (`src/components/ui/sheet.tsx`), with patterns implemented across feature modules

Sheet components provide slide-out panels for forms, details, and settings. POT implements two distinct sheet patterns based on use case.

#### Sheet Pattern Overview

| Aspect             | Form Sheets                                     | Info/Settings Sheets                                                    |
| ------------------ | ----------------------------------------------- | ----------------------------------------------------------------------- |
| **Use Case**       | Create/Edit forms (Accounts, Expenses, Incomes) | Details display, Settings (ExpenseDetails, IncomeDetails, UserSettings) |
| **Width**          | `sm:max-w-lg` (512px)                           | `md:max-w-md` (448px)                                                   |
| **Closing**        | Route-based navigation                          | Custom close button                                                     |
| **Modal Behavior** | `modal={false}`                                 | `modal={false}`                                                         |
| **Close Button**   | Hidden (no custom button)                       | Hidden default + custom button                                          |

#### Pattern 1: Form Sheets

**Use Case:** Create and edit forms that navigate between routes

**Key Characteristics:**

- Users arrive via routing (e.g., `/expenses/new`, `/accounts/edit/123`)
- Form cancel/save actions navigate away from the sheet route
- No explicit close button needed (routing handles dismissal)
- Wider width (`sm:max-w-lg` = 512px) for form fields

**Implementation:**

```tsx
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';

<Sheet open={true} modal={false}>
  <SheetContent className="p-6 sm:max-w-lg [&>button:first-of-type]:hidden overflow-y-auto">
    <SheetHeader>
      <SheetTitle>Create Expense</SheetTitle>
    </SheetHeader>

    {/* Form content */}
    <ExpenseForm
      onCancel={() => navigate('/expenses')}
      onSave={() => navigate('/expenses')}
    />
  </SheetContent>
</Sheet>;
```

**Key Props:**

- `open={true}` - Always open (controlled by routing, not state)
- `modal={false}` - Prevents closing on outside click (user must use form actions)
- `[&>button:first-of-type]:hidden` - Hides default X button (navigation handles closing)
- `sm:max-w-lg` - 512px width on small screens and up (adequate space for form fields)
- `overflow-y-auto` - Enables scrolling for long forms

**Examples:**

- `src/features/accounts/components/AccountSheet.tsx`
- `src/features/expenses/components/ExpenseSheet.tsx`
- `src/features/incomes/components/IncomeSheet.tsx`

#### Pattern 2: Info/Settings Sheets

**Use Case:** Display information details or user settings with explicit close action

**Key Characteristics:**

- Opened via state toggle (button click, row selection)
- Requires explicit close button for dismissal
- Narrower width (`md:max-w-md` = 448px) for focused information display
- Close button with icon in header

**Implementation:**

```tsx
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { XIcon } from 'lucide-react';

<Sheet open={isOpen} modal={false}>
  <SheetContent className="p-6 md:max-w-md [&>button:first-of-type]:hidden overflow-y-auto">
    <SheetHeader className="flex flex-row items-center justify-between">
      <SheetTitle>Expense Details</SheetTitle>
      <Button
        variant="ghost"
        size="icon"
        onClick={() => setIsOpen(false)}
        aria-label="Close"
      >
        <XIcon className="h-4 w-4" />
      </Button>
    </SheetHeader>

    {/* Display content */}
    <div className="space-y-4 pt-4">{/* Details */}</div>
  </SheetContent>
</Sheet>;
```

**Key Props:**

- `open={isOpen}` - Controlled by component state
- `modal={false}` - Prevents closing on outside click (only close button dismisses)
- `[&>button:first-of-type]:hidden` - Hides default X button (using custom button instead)
- `md:max-w-md` - 448px width on medium screens and up (focused for info display)
- `overflow-y-auto` - Enables scrolling for long content
- Custom Button with `onClick` handler - Explicit close action

**Why Custom Close Button:**

- Default shadcn Sheet close button conflicts with `modal={false}`
- Default X button requires `onOpenChange` prop, which interferes with modal behavior
- Custom button provides explicit `onClick={() => setIsOpen(false)}` control
- Positioned in SheetHeader for consistency with form sheets (which hide their default X)

**Examples:**

- `src/features/projections/components/ExpenseDetails.tsx`
- `src/features/projections/components/IncomeDetails.tsx`
- `src/features/userSettings/UserSettingsSheet.tsx`

#### Sheet Width Guidelines

**`sm:max-w-lg` (512px):**

- Use for forms with multiple input fields
- Adequate space for labels, inputs, dropdowns
- Prevents cramped form layouts
- Applied at `sm:` breakpoint (640px and up)

**`md:max-w-md` (448px):**

- Use for information display and settings
- More focused, less overwhelming
- Better for read-heavy content
- Applied at `md:` breakpoint (768px and up)

**Mobile Behavior:**

- All sheets default to `w-full` on mobile (full screen width)
- Width constraints only apply at their respective breakpoints

#### Modal Behavior: `modal={false}`

**Why Disable Modal Behavior:**

Default shadcn Sheet component allows:

- Clicking outside the sheet to close
- Pressing Escape to close

For POT sheets, we **always** use `modal={false}` to prevent accidental closing:

- **Form sheets:** Prevents data loss from accidental outside clicks while filling forms
- **Info sheets:** Requires explicit close button interaction (deliberate action)

**Implementation:**

```tsx
<Sheet open={open} modal={false}>
  {/* Sheet content */}
</Sheet>
```

#### Close Button Patterns

**Hiding Default Close Button:**

All POT sheets hide the default X button using:

```tsx
className = '[&>button:first-of-type]:hidden';
```

**Why:**

- Default button positioning conflicts with custom layouts
- Default button requires `onOpenChange` callback, which conflicts with `modal={false}`
- Custom button provides explicit control over close behavior

**Form Sheets:** No custom close button (routing handles navigation)

**Info Sheets:** Custom close button in SheetHeader:

```tsx
<SheetHeader className="flex flex-row items-center justify-between">
  <SheetTitle>Sheet Title</SheetTitle>
  <Button
    variant="ghost"
    size="icon"
    onClick={() => handleClose()}
    aria-label="Close"
  >
    <XIcon className="h-4 w-4" />
  </Button>
</SheetHeader>
```

#### Scrolling Behavior

**Always include `overflow-y-auto` on SheetContent:**

```tsx
<SheetContent className="... overflow-y-auto">
```

**Why:**

- Prevents content overflow on small viewports
- Ensures long forms/content are fully accessible
- Mobile devices require scrolling for sheets taller than viewport

#### Decision Guide: Which Pattern to Use?

**Use Form Sheet Pattern When:**

- Creating or editing entities (accounts, expenses, incomes, users)
- Route-based navigation (URL changes to show sheet)
- Form submission navigates away
- Need wider layout for input fields
- No explicit close button required

**Use Info/Settings Sheet Pattern When:**

- Displaying read-only details (expense breakdown, income details)
- User settings or preferences
- State-based opening (button click toggles visibility)
- Need explicit close action
- Narrower focused layout preferred

---

## Page Layout & Styling Patterns

This section documents the layout architecture and responsive styling patterns for major pages in the application. These patterns are essential for maintaining consistent behavior across desktop and mobile devices.

### Projections Page

**Location:** `src/features/projections/ProjectionsPage.tsx`, `src/features/projections/components/ProjectionChart.tsx`

The Projections page displays financial projection data with interactive charts. The layout implementation uses a carefully structured flex-based architecture to ensure proper viewport filling on desktop and natural scrolling on mobile.

#### Page Container Structure

**ProjectionsPage.tsx:**

```tsx
<div className="flex flex-col md:h-screen bg-card">
  <ProjectionsHeader />
  <div className="p-6 md:pb-6 md:flex-1 md:min-h-0 relative">
    <ProjectionChart {...props} />
  </div>
</div>
```

**Outer Container (`flex flex-col md:h-screen bg-card`):**

- **Mobile:** Natural height based on content - allows vertical scrolling when content exceeds viewport
- **Desktop (`md:h-screen`):** Full viewport height (100vh) - creates fixed viewport layout
- `bg-card`: Solid background matching Card component (prevents visual gaps from showing through)

**Content Container (`p-6 md:pb-6 md:flex-1 md:min-h-0 relative`):**

- `p-6`: 24px padding on all sides
- `md:pb-6`: 24px bottom padding on desktop
- `md:flex-1`: Grows to fill available space after header on desktop
- `md:min-h-0`: Critical for flex child shrinking - allows content to shrink below minimum content size
- `relative`: Positioning context for loading overlay

**Key Pattern:** The `md:` prefix on layout-critical classes ensures mobile gets natural flow while desktop gets viewport-constrained flex layout.

#### Chart Component Structure

**ProjectionChart.tsx Card Layout:**

```tsx
<Card
  className="flex flex-col md:h-full"
  style={{
    background:
      'linear-gradient(to bottom, rgba(148, 163, 184, 0.01), rgba(148, 163, 184, 0.04))',
  }}
>
  <CardHeader>
    <CardTitle>{getChartTitle()}</CardTitle>
    <CardDescription>{getDateRangeDescription()}</CardDescription>
  </CardHeader>
  <ChartControls {...controlProps} />
  <CardContent className="flex-1 flex flex-col p-0">
    <div className="flex-1 px-2 md:px-6 overflow-x-auto min-h-[400px] md:min-h-0">
      <ChartContainer
        config={chartConfig}
        className="aspect-auto h-[400px] md:h-full"
        style={{ minWidth: '600px' }}
      >
        <LineChart data={chartData} margin={chartMargins}>
          {/* Chart elements */}
        </LineChart>
      </ChartContainer>
    </div>
  </CardContent>
</Card>
```

**Card (`flex flex-col md:h-full`):**

- **Mobile:** Natural height based on content
- **Desktop (`md:h-full`):** Fills parent container height (100%)
- Subtle gradient background (`0.01` to `0.04` opacity) applied at Card level for visual depth without banding artifacts

**CardContent (`flex-1 flex flex-col p-0`):**

- `flex-1`: Grows to fill remaining Card height after header and controls
- `flex flex-col`: Vertical flex container for chart
- `p-0`: Removes default shadcn CardContent padding (default is `p-6`)

**Chart Container Div (`flex-1 px-2 md:px-6 overflow-x-auto min-h-[400px] md:min-h-0`):**

- `flex-1`: Fills CardContent height
- `px-2`: 8px horizontal padding on mobile (tight spacing for small screens)
- `md:px-6`: 24px horizontal padding on desktop (more breathing room)
- `overflow-x-auto`: Enables horizontal scrolling when chart exceeds container width
- `min-h-[400px]`: Minimum 400px height on mobile (ensures adequate chart visibility)
- `md:min-h-0`: Removes minimum height on desktop (allows flex to control height)

**ChartContainer (`aspect-auto h-[400px] md:h-full`):**

- `aspect-auto`: Overrides shadcn default `aspect-video` (16:9) constraint - critical for proper height calculation
- `h-[400px]`: Fixed 400px height on mobile - necessary for Recharts ResponsiveContainer to render on initial load
- `md:h-full`: Fill parent height on desktop (dynamic based on viewport)
- `style={{ minWidth: '600px' }}`: Minimum chart width ensures x-axis labels don't compress - triggers horizontal scroll on small screens

#### Chart Margins & XAxis Configuration

**Chart Margins:**

Both LineChart and BarChart use consistent margins:

```tsx
<LineChart
  data={chartData}
  margin={{
    top: 10,
    right: 20,
    left: 20,
    bottom: 5,
  }}
>

<BarChart
  data={chartData}
  margin={{
    top: 10,
    right: 20,
    left: 20,
    bottom: 5,
  }}
>
```

**XAxis Configuration:**

Both chart types use identical XAxis settings:

```tsx
<XAxis
  dataKey="date"
  tickFormatter={value => format(parseISO(value), 'MMM dd')}
  angle={-45}
  textAnchor="end"
  height={60}
  className="text-xs"
/>
```

**Key Points:**

- `angle={-45}`: Diagonal labels prevent overlap with many data points
- `textAnchor="end"`: Aligns rotated text properly at the end of the tick mark
- `height={60}`: Reserves 60px vertical space for the axis, accommodating rotated labels
- **Consistency:** Both LineChart and BarChart use `height={60}` to prevent scrollbar overlap issues
- Tight `bottom: 5` margin works because `height={60}` provides adequate space for labels

#### Key Learnings & Best Practices

**1. Pure CSS Responsive Approach:**

- Use Tailwind responsive classes (`md:`) for all layout and styling behavior
- Avoids hydration issues and flash of wrong layout on page refresh
- CSS-only approach is more maintainable and performant

**2. Explicit Heights on Mobile for Recharts:**

- Recharts ResponsiveContainer requires explicit height on mobile initial render
- Without `h-[400px]`, chart disappears after page refresh (ResponsiveContainer can't calculate dimensions from flex layout)
- Desktop can use `h-full` because flex calculations happen before ResponsiveContainer renders

**3. Aspect Ratio Constraints:**

- shadcn ChartContainer defaults to `aspect-video` (16:9 ratio)
- Must override with `aspect-auto` to allow flex layout to control height
- Without override, chart height becomes constrained by aspect ratio regardless of flex settings

**4. Min-Height Behavior:**

- `min-h-0` on flex children is critical for allowing shrinking below content size
- Without it, flex children won't shrink properly causing layout overflow
- Mobile needs explicit `min-h-[400px]` for chart visibility, desktop uses `md:min-h-0` for flex behavior

**5. Clean Component Structure:**

- CardHeader has no unnecessary flex constraints
- CardContent uses `p-0` to remove default padding for precise chart spacing
- Solid background (`bg-card`) on page container matches Card background, eliminating visual gaps
- Minimal, purposeful styling without redundant properties

**6. Page Background Strategy:**

- Use solid `bg-card` instead of gradient on page container
- Matches Card background perfectly, eliminating visual artifacts when scrolling
- Any gaps between Card edges and viewport are invisible due to matching colors

### Dashboard Page

**Location:** `src/features/dashboard/DashboardPage.tsx`

The Dashboard uses a widget-based layout pattern, distinct from the table-based pages.

**Container Structure:**

```tsx
<div className="flex flex-col min-h-screen bg-gradient-to-br from-background to-muted/20">
  <DashboardHeader />
  <div className="flex-1 p-4 sm:p-6 space-y-6 min-w-0">
    {/* Widget components */}
  </div>
</div>
```

**Key Differences from Other Pages:**

- Uses `min-h-screen` instead of `h-screen` or `md:h-screen` - allows natural scrolling for multiple widgets
- Responsive padding: `p-4 sm:p-6` (16px mobile, 24px desktop)
- `space-y-6` for vertical spacing between widgets (24px gap)
- `min-w-0` prevents content overflow in flex layout
- No Toolbar component - widgets are stacked vertically
- Permission-based widget visibility using `PermissionGuard`

### Table-Based Pages (Accounts, Expenses, Incomes)

**Locations:**

- `src/features/accounts/AccountsPage.tsx`
- `src/features/expenses/ExpensesPage.tsx`
- `src/features/incomes/IncomesPage.tsx`

These pages share a consistent layout pattern for data tables with filtering and CRUD operations.

**Standard Layout Structure:**

```tsx
<div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
  <PageHeader />
  <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
    <Toolbar>{/* Filters and action buttons */}</Toolbar>
    <div className="flex-1 min-h-0 flex flex-col relative">
      {isLoading && <LoadingOverlay />}
      <DataTable />
    </div>
  </div>
  {error && <ErrorSheet />}
  <Outlet />
</div>
```

**Container Breakdown:**

- **Outer container:** `flex flex-col h-screen` - Full viewport height layout
- **Content container:** `flex-1 min-h-0 flex flex-col p-6 gap-4`
  - `flex-1`: Grows to fill available space after header
  - `min-h-0`: Allows flex child shrinking for proper scrolling
  - `p-6`: 24px padding on all sides
  - `gap-4`: 16px spacing between Toolbar and table
- **Table container:** `flex-1 min-h-0 flex flex-col relative`
  - `flex-1`: Fills remaining space after toolbar
  - `min-h-0`: Critical for table scrolling behavior
  - `relative`: Positioning context for LoadingOverlay

**Common Features:**

- Search by description (persistent via localStorage)
- Account filtering (Expenses and Incomes only, synced with URL params)
- Permission-gated action buttons (`WithPermission` wrapper)
- Nested routing via `<Outlet />` for create/edit sheets
- URL state management for filters (Expenses/Incomes preserve filter state during edit operations)

**Filter Persistence:**

All table pages use custom storage hooks (`useAccountStorage`, `useExpenseStorage`, `useIncomeStorage`) to persist filter state:

- Search term saved to localStorage on change
- Account filter (Expenses/Incomes) synced with both localStorage and URL search params
- Filters restored on page mount

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
import { logger } from '@/concerns/logging';

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

- `accessToken: string | undefined` - Current access token in memory
- `userInfo: User | null` - Current user details (username, email, permissions)
- `isAuthenticated: boolean` - Whether user is logged in
- `login: (accessToken: string) => void` - Store access token and fetch user info
- `logout: () => void` - Clear authentication state and redirect to login
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

| Command           | Description                                                                     |
| ----------------- | ------------------------------------------------------------------------------- |
| `npm run dev`     | Start development server at `http://localhost:5175` with hot module replacement |
| `npm run build`   | Build optimized production bundle                                               |
| `npm run preview` | Preview production build locally                                                |

### Testing & Quality

| Command              | Description                                                       |
| -------------------- | ----------------------------------------------------------------- |
| `npm run test`       | Run unit tests (Vitest) - tests in `tests/` or `*.test.tsx` files |
| `npm run type:check` | Run TypeScript compiler type checking (no output)                 |
| `npm run lint`       | Run ESLint with strict TypeScript rules                           |
| `npm run prettier`   | Format all files using Prettier configuration                     |

---

## Progressive Web App (PWA)

### What is a PWA?

A Progressive Web App (PWA) is a web application that uses modern browser APIs to deliver capabilities traditionally associated with native mobile or desktop apps. A PWA can be:

- **Installed** on a device home screen (Android, iOS, desktop Chrome/Edge) without an app store
- **Launched** in standalone mode (no browser chrome — looks and behaves like a native app)
- **Cached** so the application shell loads instantly, even on slow networks
- **Available offline** for any cached static content

PWA features are progressive enhancements — the app works identically in a standard browser tab if the user never installs it.

### Why PWA Was Added to POT

POT is a personal cashflow tool used regularly to check balances, projections, and upcoming obligations. Adding PWA support provides:

- **Home screen access** — one tap from the device home screen instead of opening a browser and typing the URL
- **Standalone experience** — no browser address bar, matching a native app feel
- **Faster load times** — the application shell (HTML, CSS, JS, fonts, icons) is precached by the service worker after the first visit, so subsequent loads are near-instant
- **Resilience** — if the network is slow or briefly unavailable, the cached shell still loads

API data is intentionally **not cached** — all `/api` responses remain live network requests to ensure balance and projection data is always current.

---

### Setup Overview

PWA support was added using [`vite-plugin-pwa`](https://vite-pwa-org.netlify.app/), the standard Vite-native PWA plugin. It generates a service worker and web manifest at build time using [Workbox](https://developer.chrome.com/docs/workbox).

**Packages installed:**

| Package                      | Role                                                              |
| ---------------------------- | ----------------------------------------------------------------- |
| `vite-plugin-pwa@^1.2.0`     | Vite plugin — generates service worker and manifest at build time |
| `@vite-pwa/assets-generator` | CLI tool — generates PNG icon variants from the source SVG        |

---

### Security Fix: `serialize-javascript` Override

When `vite-plugin-pwa` was first installed, `npm audit` reported three high-severity CVEs in `serialize-javascript <=7.0.4` (reachable via `workbox-build` → `@rollup/plugin-terser`). The patched release is `7.0.5`.

Because `workbox-build` pins `@rollup/plugin-terser@0.4.4` which in turn pins `serialize-javascript@6.x`, the fix is applied via an `overrides` entry in `package.json`:

```json
"overrides": {
  "serialize-javascript": "^7.0.5"
}
```

**Risk context:** `serialize-javascript` is a **build-time only** dependency — it never executes in a user's browser. The CVE requires attacker-controlled input at build time (a supply-chain scenario), not a user-facing attack. The override eliminates the advisory cleanly.

---

### Icon Generation

Icons were generated from the existing `public/pot-icon.svg` using the assets generator CLI:

```bash
npx pwa-assets-generator --preset minimal-2023 public/pot-icon.svg
```

This produced the following files in `public/`:

| File                           | Purpose                                                |
| ------------------------------ | ------------------------------------------------------ |
| `pwa-64x64.png`                | Small icon (browser tab fallback)                      |
| `pwa-192x192.png`              | Standard Android home screen icon                      |
| `pwa-512x512.png`              | Large icon (splash screen, app stores)                 |
| `maskable-icon-512x512.png`    | Adaptive icon for Android (safe-zone cropping)         |
| `apple-touch-icon-180x180.png` | iOS add-to-home-screen icon                            |
| `favicon.ico`                  | Multi-size `.ico` favicon replacing the SVG-only entry |

The source SVG (`pot-icon.svg`) remains in `public/` and is referenced directly for SVG-capable browsers.

---

### `vite.config.ts` Changes

The `VitePWA` plugin was added to the `plugins` array:

```typescript
import { VitePWA } from 'vite-plugin-pwa';

VitePWA({
  registerType: 'prompt',
  injectRegister: 'auto',
  manifest: {
    name: 'POT - Pay On Time',
    short_name: 'POT',
    description: 'Pay On Time - Personal cashflow and projection tool',
    theme_color: '#2563eb',
    background_color: '#ffffff',
    display: 'standalone',
    start_url: '/',
    scope: '/',
    icons: [
      { src: 'pwa-64x64.png', sizes: '64x64', type: 'image/png' },
      { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
      { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
      {
        src: 'maskable-icon-512x512.png',
        sizes: '512x512',
        type: 'image/png',
        purpose: 'maskable',
      },
    ],
  },
  workbox: {
    globPatterns: ['**/*.{js,css,html,svg,png,ico,woff,woff2}'],
    navigateFallback: '/index.html',
    navigateFallbackDenylist: [/^\/api/],
    runtimeCaching: [],
  },
});
```

**Key configuration decisions:**

| Option                     | Value                                     | Reason                                                                                        |
| -------------------------- | ----------------------------------------- | --------------------------------------------------------------------------------------------- |
| `registerType`             | `prompt`                                  | Keeps new service workers waiting so the app can show the update toast before reloading       |
| `injectRegister`           | `auto`                                    | Maintains plugin-managed registration support while allowing app-side update hooks            |
| `display`                  | `standalone`                              | App launches without browser chrome (no address bar, tabs, or navigation buttons)             |
| `navigateFallbackDenylist` | `[/^\/api/]`                              | Prevents the service worker from intercepting API requests and serving stale cached fallbacks |
| `runtimeCaching`           | `[]`                                      | Explicitly disables runtime (network) caching — API responses are never cached                |
| `globPatterns`             | js, css, html, svg, png, ico, woff, woff2 | Only static assets are precached; no JSON or API responses                                    |

---

### Service Worker Registration and Update UX

POT now uses an explicit app-side registration flow so users get a clear update action when a new deployment is available.

- `src/concerns/pwa/index.ts` registers the service worker using `registerSW` from `virtual:pwa-register`
- `src/main.tsx` calls `registerServiceWorker()` during startup (production only)
- Update checks are triggered on startup, on window focus, on tab visibility return, and on a shared configured period while the tab is visible (currently 30 seconds for active testing)
- The update prompt can be triggered in two paths:
  - `onNeedRefresh` callback from `registerSW`
  - Explicit post-check detection when `registration.waiting` exists after `registration.update()`
- When a new waiting service worker is detected, the app shows a persistent top-center toast:
  - Message: `Update Available`
  - Action: `Refresh` (shows `Refreshing...`, attempts waiting-worker activation, then forces a hard reload fallback if no `controllerchange` is observed within 1.5 seconds)
  - Cancel: `Later`
- Prompt dedupe behavior:
  - Duplicate prompt events for the same waiting worker are suppressed while a prompt cycle is active
  - Choosing `Later` snoozes re-prompts for that same waiting worker key for the same configured period (currently 30 seconds for active testing)
  - After snooze expiry, the app can force a deferred re-prompt even if `registration.waiting` is no longer present at that exact moment
  - If the tab is visible, re-prompt can occur at snooze expiry without requiring a refocus click

This closes the stale-bundle gap where users might otherwise keep using an older version until a manual hard refresh.

Why `prompt` matters: with `autoUpdate`, a browser-triggered update can refresh immediately and skip the in-app toast path. `prompt` ensures the waiting state is exposed to `onNeedRefresh`, which drives POT's `Refresh`/`Later` UX.

Expected detection timing:

- If a user refocuses the app tab after deployment, detection should occur quickly via focus/visibility checks
- If a user keeps the tab open and idle, detection occurs within the configured periodic check window (currently 30 seconds for active testing)

---

### `index.html` Changes

The document `<head>` was updated with:

```html
<link rel="icon" href="/favicon.ico" sizes="48x48" />
<link rel="icon" href="/pot-icon.svg" sizes="any" type="image/svg+xml" />
<link rel="apple-touch-icon" href="/apple-touch-icon-180x180.png" />
<meta name="theme-color" content="#2563eb" />
<meta name="apple-mobile-web-app-capable" content="yes" />
```

- The SVG favicon entry was updated to follow the recommended `sizes="any"` pattern alongside the `.ico` file
- `theme-color` sets the browser toolbar and splash screen colour on Android/Chrome
- `apple-mobile-web-app-capable` enables standalone mode on iOS when added to the home screen
- The manifest link (`<link rel="manifest">`) is injected automatically by the plugin at build time

---

### TypeScript Support for PWA Virtual Modules

The `vite-plugin-pwa/client` type declarations are included so TypeScript recognises the `virtual:pwa-register` module:

```json
"types": ["vite-plugin-pwa/client"]
```

POT includes this in:

- `tsconfig.app.json` (`compilerOptions.types`)
- `src/vite-env.d.ts` (`/// <reference types="vite-plugin-pwa/client" />`)

This keeps TypeScript aware of PWA virtual modules across normal app code and editor tooling.

---

### Build Output

After running `npm run build`, the following PWA-specific files are emitted to `dist/`:

| File                        | Description                                           |
| --------------------------- | ----------------------------------------------------- |
| `dist/sw.js`                | Compiled Workbox service worker                       |
| `dist/workbox-*.js`         | Workbox runtime library (hashed filename)             |
| `dist/manifest.webmanifest` | Web app manifest (icons, name, display mode, colours) |

Service worker registration/update logic from `src/concerns/pwa/index.ts` is bundled into the hashed client JavaScript assets.

On a typical build, approximately 64 entries (~1.5 MB) are precached — the complete application shell.

---

### Validating PWA Behaviour

After building and previewing (`npm run preview`), open the browser DevTools:

1. **Application → Manifest** — Confirms name, icons, theme colour, and display mode are correctly parsed
2. **Application → Service Workers** — Confirms `sw.js` is registered and active
3. **Deploy update test** — Keep the app open, deploy a new client build, then verify the in-app toast appears after refocus (or within the configured periodic check window while the tab remains visible)
4. **Application → Cache Storage** — Shows precached assets under the `workbox-precache` key
5. **Lighthouse → PWA audit** — Run a full audit; all PWA criteria should pass
6. **Install prompt** — Chrome/Edge shows an install icon in the address bar when all criteria are met

**Note:** The service worker only activates in production builds. Running `npm run dev` uses Vite's dev server, which bypasses the service worker entirely.

### Local Browser Sanity Check Before Deploying

When the full stack is running through Docker, the client is served by nginx on `http://localhost:5175`. This is the preferred local pre-deploy validation path because it serves the same production build output that will be deployed, including the generated service worker and manifest.

Use the following sanity-check flow before deploying:

1. Start the Docker stack using the existing `docker-start-pot-client-server` task
2. Open `http://localhost:5175` in Chrome or Edge
3. Perform a hard refresh with `Ctrl+Shift+R` so the latest service worker registration is picked up
4. Open DevTools → **Application → Manifest** and confirm the app name, icons, theme colour, and `standalone` display mode
5. Open DevTools → **Application → Service Workers** and confirm `sw.js` is active; if it is waiting, click **skipWaiting**
6. Open DevTools → **Application → Cache Storage** and confirm a `workbox-precache` cache exists with static assets
7. Check the browser address bar for the install icon and confirm the app is installable
8. Run **Lighthouse → Progressive Web App** and confirm there are no blocking PWA failures
9. Open the installed app and verify it launches in a standalone window rather than a normal browser tab

Additional local-testing notes:

- `localhost` is allowed to use service workers without HTTPS, so local PWA testing works correctly on `http://localhost:5175`
- Docker/nginx uses the production client build, so this test path is more representative than `npm run dev`
- API requests are intentionally not cached by the service worker, so account balances and projection data should still come from live backend responses
- If the service worker appears stale, use DevTools → **Application → Service Workers** → **Unregister**, then hard refresh and test again

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
