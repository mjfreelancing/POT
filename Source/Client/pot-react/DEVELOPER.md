# Frontend Developer Guide

Comprehensive guide for developers working on the POT React frontend application.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Component Patterns](#component-patterns)
- [State Management](#state-management)
- [API Integration](#api-integration)
- [Import/Export Features](#importexport-features)
- [Environment Configuration](#environment-configuration)
- [Testing](#testing)

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
├── api/              # API integration layer
│   ├── hooks/        # React Query hooks
│   ├── errors/       # Error types
│   └── interceptors/ # Axios interceptors
├── components/       # Shared components
│   ├── table/        # DataTable library
│   ├── feedback/     # ErrorSheet, toasts
│   └── ui/           # shadcn components (don't edit)
├── features/         # Feature modules
│   ├── auth/
│   ├── accounts/
│   ├── expenses/
│   └── incomes/
├── lib/              # Utilities
├── store/            # Zustand stores
└── hooks/            # Custom hooks
```

**Key Principles:**

- Feature-based organization
- Single responsibility per module
- Features can depend on API, not vice versa
- No `any` types
- All exports at bottom of file

---

## Component Patterns

### DataTable Component Library

A comprehensive, reusable table system built on `@tanstack/react-table`.

**Components:**

- `DataTable` - Main table with sorting, selection, bulk actions
- `BulkActionsBar` - Dropdown for bulk operations
- `DataTableHeader` - Table headers with sorting
- `DataTableContent` - Table body (extracted for reuse)
- `DataTableColumnHeader` - Sortable column header

**Quick Start:**

```tsx
import { DataTable, BulkAction } from '@/components/table';
import { ColumnDef } from '@tanstack/react-table';

type Person = { id: number; name: string; email: string };

const columns: ColumnDef<Person>[] = [
  { accessorKey: 'name', header: 'Name' },
  { accessorKey: 'email', header: 'Email' },
];

const bulkActions: BulkAction<Person>[] = [
  {
    label: 'Delete Selected',
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
/>;
```

**Column Factory Utilities:**

```tsx
import {
  createMoneyValueColumn,
  createDateColumn,
  createFrequencyColumn,
} from '@/components/table';

const columns = [
  createMoneyValueColumn<Expense>('amount', 'Amount'),
  createDateColumn<Expense>('date', 'Date'),
  createFrequencyColumn<Expense>('count', 'frequency', 'Frequency'),
];
```

**Row Selection with Identity Objects:**

For objects with `rowId` property (Expense, Income, Account):

```tsx
import { createRowIdGetter } from '@/components/table';

<DataTable
  columns={columns}
  data={expenses}
  enableRowSelection={true}
  bulkActions={bulkActions}
  getRowId={createRowIdGetter<Expense>()}
/>;
```

**Row Highlighting:**

```tsx
<DataTable
  columns={columns}
  data={expenses}
  highlightRowFilter={row => row.original.id === activeId}
  highlightClassName="bg-blue-100 dark:bg-blue-900"
/>
```

**Props Reference:**

- `columns: ColumnDef<TData, TValue>[]` - Column definitions
- `data: TData[]` - Table data
- `highlightRowFilter?: (row: Row<TData>) => boolean` - Highlight specific rows
- `highlightClassName?: string` - CSS class for highlighted rows
- `enableRowSelection?: boolean` - Enable checkbox selection
- `bulkActions?: BulkAction<TData>[]` - Bulk operations
- `onSelectionChange?: (selectedItems: TData[]) => void` - Selection callback
- `getRowId?: (row: TData, index: number) => string` - Custom row ID for persistence

---

## State Management

### Global State (Zustand)

Use for app-wide state that needs to persist across navigation.

**Location:** `src/store/`

**Example:**

```tsx
import { useAuthStore } from '@/store/authStore';

const { user, isAuthenticated, logout } = useAuthStore();
```

### Server State (React Query)

Use for all API data. React Query handles caching, refetching, and background updates.

**Location:** `src/api/hooks/`

**Pattern:**

```tsx
const { data, isLoading } = useGet<Account[]>('/accounts', ['accounts']);

if (!data.success) {
  // Handle error
  return <ErrorDisplay error={data.error} />;
}

// Use data.value
const accounts = data.value;
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

- `ApiError` - General API errors
- `ValidationError` - Request validation failures
- `AuthenticationError` - Auth failures
- `NotFoundError` - 404 errors

### Error Display

**Critical/Blocking Errors:**

Use `ErrorSheet` at top of page/container:

```tsx
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';

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

**Transient Feedback:**

Use toasts:

```tsx
import { SuccessToast, ErrorToast } from '@/components/feedback/toast';
import { toast } from '@/hooks/useToast';

// Success
toast(
  <SuccessToast
    icon={CheckCircleIcon}
    title="Saved"
    description="Changes saved successfully"
  />,
);

// Error (fixed icon - don't pass icon prop)
toast(<ErrorToast title="Failed" description="Please try again" />);
```

**Key Rule:** One ErrorSheet at a time, no toasts for critical errors.

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

### CORS Configuration

For filename extraction from `Content-Disposition` header:

```csharp
// ASP.NET Core - expose header to client
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5175")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("content-disposition")); // Required
```

---

## Environment Configuration

### Environment Files

**`.env`** - Base configuration (shared):

```properties
VITE_API_TIMEOUT_MS=30000
```

**`.env.development`** - Local development:

```properties
VITE_API_BASE_URL=http://localhost:5242/api
```

**`.env.production`** - Production (for local Docker builds):

```properties
VITE_API_BASE_URL=/api
```

**`.env.local`** - Local overrides (git-ignored):

```properties
VITE_API_BASE_URL=http://custom-api.local/api
```

### Loading Priority (highest to lowest)

1. `.env.${mode}.local`
2. `.env.local`
3. `.env.${mode}`
4. `.env`

Where `${mode}` is `development` or `production`.

### Important Rules

1. Only `VITE_` prefixed variables are exposed to React
2. Never commit `.env.local` to git
3. Variables are embedded at **build time**, not runtime

### Available Variables

- `VITE_API_BASE_URL` - API endpoint (e.g., `http://localhost:5242/api`)
- `VITE_API_TIMEOUT_MS` - Request timeout (default: 30000 for Azure cold starts)

### Usage in Code

```typescript
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
const apiTimeout = import.meta.env.VITE_API_TIMEOUT_MS;
```

### Docker Builds

**Azure/Production builds** require build arguments:

```bash
docker build \
  --build-arg VITE_API_BASE_URL=https://api.yourdomain.com/api \
  --build-arg VITE_API_TIMEOUT_MS=30000 \
  -t pot-client:latest \
  -f Docker/Client/Dockerfile .
```

**Local Docker builds** use `.env.production` file automatically.

### Environment Modes

**Development mode** (`npm run dev`):

- Uses `.env.development`
- API: `http://localhost:5242/api` (ASP.NET running locally)

**Production mode** (`npm run build`):

- Uses `.env.production`
- API: `/api` (proxied by nginx to server container)

---

## Testing

### Unit Tests

**Framework:** Vitest

**Location:** `tests/` folder or `*.test.tsx` files

**Run tests:**

```bash
npm run test
```

### Type Checking

**Run TypeScript checks:**

```bash
npm run type:check
```

### Linting

**Run ESLint:**

```bash
npm run lint
```

**Format code:**

```bash
npm run prettier
```

---

## Development Workflow

1. **Start development server:**

   ```bash
   npm run dev
   ```

   Frontend runs on `http://localhost:5175`

2. **Build for production:**

   ```bash
   npm run build
   ```

3. **Preview production build:**
   ```bash
   npm run preview
   ```

---

## Best Practices

### TypeScript

- Use **types** instead of interfaces
- **Never** use `any` type
- Export all types/functions at bottom of file
- Use function declarations over arrow functions where appropriate

### Code Style

- Always use `{}` with `if` statements, even single lines
- Add blank line before and after `if` blocks
- Single responsibility per module
- No unused imports

### Component Design

- Keep components focused and small
- Extract reusable logic to hooks
- Use composition over inheritance
- Avoid prop drilling (use context or Zustand)

### Error Handling

- Always check `result.success` before accessing `result.value`
- Use ErrorSheet for critical errors
- Use toasts for transient feedback
- Log errors with context: `logger.error('Component', 'Message', error)`

### Performance

- Memoize expensive calculations with `useMemo`
- Memoize callback functions with `useCallback` when passing to children
- Use React Query for automatic caching
- Avoid premature optimization

---

## Common Pitfalls

### 1. Forgetting to Check result.success

```tsx
// ❌ BAD - crashes if error
const accounts = data.value;

// ✅ GOOD
if (!data.success) {
  return <ErrorDisplay error={data.error} />;
}
const accounts = data.value;
```

### 2. Using try/catch with API Hooks

```tsx
// ❌ BAD - hooks don't throw
try {
  const result = await mutation.mutateAsync(data);
} catch (error) {
  // Never reached
}

// ✅ GOOD - check success
const result = await mutation.mutateAsync(data);
if (!result.success) {
  // Handle error
}
```

### 3. Editing shadcn/ui Components

```tsx
// ❌ BAD - don't edit components/ui
// Edit: src/components/ui/button.tsx

// ✅ GOOD - create wrapper or use composition
// Create: src/components/custom-button.tsx
```

### 4. Cache Invalidation Without Dependencies

```tsx
// ❌ BAD - only invalidates expenses
queryClient.invalidateQueries(['expenses']);

// ✅ GOOD - also invalidates accounts and projections
invalidateCache(['expenses']);
```

### 5. Not Using getRowId for DataTable

```tsx
// ❌ BAD - selection breaks when data reorders
<DataTable data={expenses} enableRowSelection />

// ✅ GOOD - stable selection with rowId
<DataTable
  data={expenses}
  enableRowSelection
  getRowId={createRowIdGetter<Expense>()}
/>
```

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
