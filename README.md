# POT - Financial Management Made Simple

<img src="AppLogo.png" alt="POT Logo" style="width:200px;"/>

**A comprehensive financial management application to ensure debts are Paid On Time**

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-0.1-green.svg)](https://github.com/mjfreelancing/POT)
[![Docker](https://img.shields.io/badge/docker-supported-blue.svg)](Source/Docker)

## Table of Contents

- [About POT](#about-pot)
  - [Why POT?](#why-pot)
- [Features](#features)
  - [Dashboard](#dashboard)
  - [Financial Projections](#financial-projections)
  - [Accounts Management](#accounts-management)
    - [Accessing Account Features](#accessing-account-features)
    - [Error Handling](#error-handling)
      - [Toast Notifications](#toast-notifications)
      - [Error Sheets](#error-sheets)
      - [Error Types](#error-types)
      - [Error Boundaries](#error-boundaries)
  - [Expenses Management](#expenses-management)
  - [Income Management](#income-management)
  - [Data Management](#data-management)
- [Quick Start Guide](#quick-start-guide)
  - [Clone the repository](#clone-the-repository)
  - [Run the application(s)](#run-the-applications)
    - [Using Docker](#using-docker)
    - [Manually](#manually)
- [Navigation and Usage](#navigation-and-usage)
  - [Application Structure](#application-structure)
  - [Accessibility Features](#accessibility-features)
  - [Environment Configuration](#environment-configuration)
    - [Client Environment Variables](#client-environment-variables)
    - [Server Environment Variables](#server-environment-variables)
- [Scripts](#scripts)
  - [Development](#development)
  - [Production](#production)
  - [Code Quality](#code-quality)
  - [Testing](#testing)
  - [Type Checking](#type-checking)
- [Development Configuration](#development-configuration)
  - [Architecture and Technology Stack](#architecture-and-technology-stack)
  - [UI/UX Design](#uiux-design)
  - [TypeScript Configuration](#typescript-configuration)
  - [Path Aliases](#path-aliases)
  - [ESLint Configuration](#eslint-configuration)
  - [Vite Configuration](#vite-configuration)
  - [Testing](#testing-1)
- [Performance and Monitoring](#performance-and-monitoring)
  - [Performance Optimization](#performance-optimization)
  - [Resource Monitoring](#resource-monitoring)
- [Security and Data Privacy](#security-and-data-privacy)
  - [Data Storage](#data-storage)
  - [Security Considerations](#security-considerations)
  - [Project Structure](#project-structure)
- [License](#license)
- [Acknowledgments](#acknowledgments)
  - [Frontend](#frontend)
  - [Backend](#backend)
  - [DevOps](#devops)
- [Version History](#version-history)
  - [Release Notes](#release-notes)

## About POT

**POT** is a modern financial application designed to help you project your financial status based on current bank account balances and future income (credits) and expenses (debits) to ensure all debts are **Paid On Time**. With interactive visualizations and projections, **POT** gives you a clear picture of your financial future, helping you make informed decisions about your money.

### Why POT?

- 📊 **Visual Financial Projections** - See where your money will be months in advance
- 💸 **Expense Tracking** - Keep tabs on all your recurring and one-time expenses
- 💰 **Income Management** - Track multiple income sources and payment schedules
- 🏦 **Account Management** - Monitor balances across all your financial accounts
- 🔄 **Data Portability** - Export and import your financial data with ease

# Features

POT offers a comprehensive set of financial management features:

## Dashboard

A centralized overview of your financial situation:

- Quick summary of accounts and their balances
- Overview of upcoming expenses
- Quick action buttons for common tasks
- Real-time financial status indicators

## Financial Projections

The projections feature provides a comprehensive visualization system for financial data:

### Component Implementation

#### ProjectionChart Component

Main visualization component with the following features:

- Smart chart type switching (line/bar) based on metric type
- Line charts for continuous trend data (Balance/Available)
- Bar charts for discrete events (Income/Expenses)
- Empty state handling with accessible UI
- Currency formatting via `formatMoneyValue`
- Series visibility management with state indicators

Props:

- `data`: Projection data from API
- `startDate`: Chart start date
- `period`: Display period in months
- `selectedMetric`: Active metric
- `hiddenSeries`: Series to hide
- `onStartDateChange`: Date change handler
- `onPeriodChange`: Period change handler
- `onMetricChange`: Metric change handler
- `onHiddenSeriesChange`: Series visibility handler

#### ChartControls Component

Manages user interaction with these features:

- Metric selection via dropdown
- Period selection (1, 2, 3, 6, 9, 12 months)
- Custom date range selection
- Series visibility toggles
- Fixed-width layout prevention
- Left/right aligned control groups

Props:

- `selectedMetric`: Current metric
- `onMetricChange`: Metric change callback
- `startDate`: Current start date
- `onStartDateChange`: Date change callback
- `period`: Current period
- `onPeriodChange`: Period change callback
- `seriesKeys`: Available series
- `seriesVisibility`: Visibility state
- `onToggleSeries`: Toggle callback
- `chartConfig`: Styling configuration

### Data Architecture

- **Metric Configuration System**

  - Balance/Available/Daily Accrual: Line charts for trend visualization
  - Income/Expenses: Bar charts for transaction events
  - Zod schema validation for type safety
  - Extensible metric configuration using TypeScript discriminated unions

- **Data Schema**
  - Per-account daily balances with metrics
  - Global totals for aggregated view
  - Handles missing dates and empty datasets
  - Supports 30-365+ day ranges

### Technical Implementation

#### Chart Configuration

1. **Chart Type System**

   ```typescript
   type ProjectionMetric =
     | "balance"
     | "available"
     | "incomeReceived"
     | "expensesPaid";

   type MetricConfig = {
     label: string;
     shortLabel: string;
     chartType: "line" | "bar";
     description?: string;
   };
   ```

2. **Data Schema**

   ```typescript
   type DateBalance = {
     date: string; // ISO date string
     balance: number; // Account balance
     available: number; // Available amount
     incomeReceived: number;
     expensesPaid: number;
   };

   type AccountDailyBalances = {
     rowId: string; // Unique account identifier
     description: string;
     dates: DateBalance[];
   };
   ```

3. **Style Configuration**
   - Chart colors: `--chart-1` through `--chart-5`
   - Global line: `--foreground`
   - Theme integration: Uses CSS variables for colors
   - Required margin: 20px all sides for label spacing

#### Integration Points

1. **Data Transformation**

   - Transform raw API data in `useProjectionChartData` hook
   - Fill missing dates with zero values
   - Separate global totals from account data
   - Memoize transformations for performance

2. **Chart Customization**

   - Line Chart Settings

     ```typescript
     // Fixed curve settings - do not change
     <Line type="basis" dataKey={key} stroke={color} />
     ```

   - Label Configuration

     ```typescript
     // Date label formatting
     function formatXAxisLabel(
       value: string,
       chartData: ChartDataPoint[]
     ): string {
       // 8-12 labels for optimal spacing
       const targetLabels = Math.min(
         12,
         Math.max(4, Math.floor(chartData.length / 8))
       );
       const interval = Math.ceil(chartData.length / targetLabels);

       // Always show first, last, and interval dates
       if (
         index === 0 ||
         index === chartData.length - 1 ||
         index % interval === 0
       ) {
         return format(parseISO(value), "MMM dd");
       }
       return "";
     }
     ```

   - Title/Description Format

     ```typescript
     // Required format
     <CardTitle>{getChartTitle()}</CardTitle>
     <CardDescription>{getDateRangeDescription()}</CardDescription>
     ```

   - Fixed Settings
     - Label rotation: -45 degrees
     - Money formatting: Uses global `formatMoneyValue`
     - Min chart margin: { top: 20, right: 20, left: 20, bottom: 20 }

3. **Container Requirements**

   ```typescript
   // Required container structure
   <Card className="flex flex-col h-full pb-0">
     <CardHeader className="flex-shrink-0">
       {/* Title and description */}
     </CardHeader>
     <CardContent className="flex-1 flex flex-col p-0">
       <div
         className="flex-1 w-full min-h-0 px-6"
         style={{
           background: "linear-gradient(...)",
           minHeight: "400px",
         }}
       >
         {/* Chart content */}
       </div>
     </CardContent>
   </Card>
   ```

   Key Requirements:

   - Parent flex layout required
   - Minimum height: 400px
   - Container queries for responsiveness
   - Fixed control widths to prevent shifts
   - Gradient background for depth
   - Zero padding on card content

- **Responsive Design**

  - Minimum chart area of 400 pixels in height
  - Fixed -45 degree label rotation for consistent readability
  - Flexbox-based legend layout
  - Responsive control group layout

#### Technical Requirements

1. **Type Extensions**

   ```typescript
   // Add new metrics here
   type ProjectionMetric =
     | "balance"
     | "available"
     | "incomeReceived"
     | "expensesPaid";
   // | 'newMetric';  // Example extension point

   // Update API response type to match
   type DateBalance = {
     date: string;
     balance: number;
     available: number;
     incomeReceived: number;
     expensesPaid: number;
     // newMetric: number;  // Add corresponding field
   };
   ```

2. **Storage Integration**

   ```typescript
   // Key for local storage
   const PROJECTION_STORAGE_KEY = "pot-projections";

   // Stored data shape
   type ProjectionStorageData = {
     metric?: ProjectionMetric;
     period?: number;
     hiddenSeries?: string[];
   };
   ```

3. **CSS Requirements**

   ```css
   /* Required theme variables */
   :root {
     --chart-1: #color1; /* Primary series */
     --chart-2: #color2; /* Secondary series */
     --chart-3: #color3; /* Tertiary series */
     --chart-4: #color4; /* Quaternary series */
     --chart-5: #color5; /* Quinary series */
     --foreground: #color6; /* Global totals */
   }
   ```

4. **Tooltip Implementation**

   ```typescript
   // Required tooltip props
   type TooltipProps = {
     active?: boolean;
     payload?: {
       color?: string;
       dataKey?: string | number;
       value?: string | number | (string | number)[];
     }[];
     label?: string | number;
   };

   // Tooltip structure requirement
   <div className="rounded-lg border bg-background p-3 shadow-md">
     <div className="mb-2 font-medium text-foreground">
       {formatTooltipDate(label)}
     </div>
     <div className="space-y-1">{/* One entry per data point */}</div>
   </div>;
   ```

   Required Tooltip Features:

   - Date header with `formatTooltipDate`
   - Color indicators matching series
   - Entry per visible data point
   - Money value formatting with `formatMoneyValue`
   - Theme-aware background/text colors

### Extension Points

- **New Metrics**

  - Add to `ProjectionMetric` type union
  - Define in `PROJECTION_METRICS` with chart type
  - Implement in `DateValues` schema
  - Update API response type

- **New Chart Features**
  - Chart styling via CSS variables (--chart-1 through --chart-5)
  - Tooltip customization through ChartTooltip component
  - Series styling through chartConfig object
  - Custom formatters for axes and tooltips

#### Additional Technical Requirements

1. **Empty State Handling**

   ```typescript
   // Required checks before rendering
   const hasData = data && data.accounts.length > 0;
   const hasSeriesData = (key: string) =>
     data?.accounts.some((account) =>
       account.dates.some((d) => d[key as keyof DateBalance] !== 0)
     );

   // Empty state must preserve controls
   {
     !hasData && <NoProjectionData />;
   }
   ```

2. **Series Management**

   ```typescript
   // Required visibility state structure
   type SeriesVisibility = Record<string, boolean>;

   // Visibility toggle must update storage
   const toggleSeries = (key: string) => {
     const newHidden = hiddenSeries.includes(key)
       ? hiddenSeries.filter((k) => k !== key)
       : [...hiddenSeries, key];
     onHiddenSeriesChange(newHidden);
   };
   ```

3. **Data Validation Requirements**
   - All dates must be ISO format strings
   - Numeric values must be valid numbers
   - Account IDs must be unique
   - Date arrays must be same length across accounts
   - All metric fields must exist on each date entry

#### Performance Requirements

1. **Memory Optimization**

   ```typescript
   // Memoized calculations for chart data
   const { chartData, chartConfig } = useMemo(
     () => ({
       // Transform raw data to chart format
       chartData: transformData(data),
       // Build chart configuration
       chartConfig: buildChartConfig(data),
     }),
     [data]
   );

   // Container dimension caching
   const [dimensions, setDimensions] = useState(() => ({
     width: container.current?.clientWidth ?? 0,
     height: container.current?.clientHeight ?? 0,
   }));
   ```

2. **Event Handling**

   ```typescript
   // Efficient resize handling
   useEffect(() => {
     const handleResize = debounce(() => {
       if (!container.current) return;
       setDimensions({
         width: container.current.clientWidth,
         height: container.current.clientHeight,
       });
     }, 100);

     window.addEventListener("resize", handleResize);
     return () => window.removeEventListener("resize", handleResize);
   }, []);
   ```

#### Browser Compatibility

- **Modern Browser Support**:
  - Uses standard DOM APIs
  - Full flexbox layout support required
  - High contrast mode support via CSS custom properties

#### Accessibility Requirements

1. **ARIA Attributes**

   ```typescript
   // Required ARIA roles and labels
   <div role="group" aria-labelledby="legend-label">
     <span id="legend-label">Show:</span>
     <button
       aria-label={`${isVisible ? "Hide" : "Show"} ${config.label} series`}
       aria-pressed={isVisible}
     >
       {config.label}
     </button>
   </div>
   ```

2. **Keyboard Navigation**

   ```typescript
   // Focus management for controls
   <Button
     role="radio"
     aria-checked={isSelected}
     tabIndex={isSelected ? 0 : -1}
     onKeyDown={(e) => {
       if (e.key === "Enter" || e.key === " ") {
         onPeriodChange(opt.value);
       }
     }}
   >
     {opt.label}
   </Button>
   ```

3. **Screen Reader Support**
   - All charts must have descriptive titles
   - Data points need clear numeric values
   - Status messages for loading and errors
   - Announcements for data updates

#### Testing Requirements

1. **Visual Testing**

   - Visual regression testing for charts
   - Screenshot comparisons across viewport sizes
   - Color scheme validation for all themes

2. **Unit Tests**

   ```typescript
   // Data transformation tests
   test("should transform projection data correctly", () => {
     const { chartData } = useProjectionChartData(mockData, "balance");
     expect(chartData).toMatchSnapshot();
   });

   // Chart type selection tests
   test("should use correct chart type for metric", () => {
     expect(PROJECTION_METRICS["balance"].chartType).toBe("line");
     expect(PROJECTION_METRICS["incomeReceived"].chartType).toBe("bar");
   });
   ```

3. **Integration Tests**
   - API data flow validation
   - Storage persistence verification
   - Error handling scenarios

#### Known Limitations

- **Data Points**: Maximum determined by chart width
- **Chart Height**: Minimum 400px required
- **Label Rotation**: Fixed at -45 degrees
- **Browser Support**: Modern browsers only

Files and their responsibilities:

- `/features/projections/components/ProjectionChart.tsx`: Main chart component
- `/features/projections/components/ChartControls.tsx`: User interaction controls
- `/features/projections/hooks/useProjectionChartData.ts`: Data transformation
- `/features/projections/utils/chartHelpers.ts`: Formatting utilities
- `/data/projection.ts`: Type definitions and configuration

## Accounts Management

### Overview

The accounts feature provides comprehensive bank account management with the following capabilities:

#### Account Information Display

- **Account List View**
  - BSB and Account Number (formatted as XXX-XXX)
  - Description
  - Current Balance
  - Reserved Amount (funds set aside)
  - Accrued Funds (from expenses)
  - Daily Accrual Rate
  - Available Balance (Balance - Reserved - Accrued)
  - Account Status (Healthy/Low/Overdrawn)

#### Account Operations

1. **Create Account**

   ```typescript
   type CreateAccount = {
     bsb: string; // Format: XXX-XXX
     number: string; // Account number
     description: string;
     balance: number; // Initial balance
     reserved: number; // Reserved amount
   };
   ```

2. **Edit Account**

   ```typescript
   type EditAccount = {
     rowId: string; // Unique identifier
     etag: string; // For concurrency control
     description: string;
     balance: number;
     reserved: number;
   };
   ```

3. **Delete Account**
   - Validation prevents deletion if account has:
     - Associated expenses
     - Associated income
     - Active projections

### Implementation Details

#### State Management

1. **Account Summary Store**

   ```typescript
   type AccountsSummary = {
     totalBalance: number;
     totalReserved: number;
     totalAvailable: number;
     totalDailyAccrual: number;
     setSummary: (
       balance: number,
       reserved: number,
       available: number,
       dailyAccrual: number
     ) => void;
   };
   ```

2. **Account Filtering**
   ```typescript
   type UseAccountFilterOptions = {
     accounts: Account[];
     items: ItemWithAccount[];
     selectedAccountId: string | null;
     onAccountChange: (accountId: string | null) => void;
   };
   ```

#### Status Calculation

```typescript
const getAccountStatus = (balance: number, available: number) => {
  if (available < 0) return "overdrawn";
  if (available < balance * 0.1) return "low";
  return "active";
};
```

#### Account Form Validation

```typescript
const accountFormSchema = z.object({
  bsb: z.string().regex(/^\d{3}-\d{3}$/, "BSB must be in format XXX-XXX"),
  number: z.string().min(1, "Account number is required"),
  description: z.string().min(1, "Description is required"),
  balance: MoneyValueSchema,
  reserved: MoneyValueSchema,
});
```

### Technical Implementation

#### Account Filter Architecture

The account filtering system is built on a shared `useAccountFilter` hook that provides consistent filtering behavior across expenses and income management. This hook is the cornerstone of the filter implementation and works together with page-level state management:

1. **Filter Hook Implementation**

   ```typescript
   type UseAccountFilterOptions = {
     accounts: Account[]; // All available accounts
     items: ItemWithAccount[]; // Items to filter (expenses/income)
     selectedAccountId: string | null; // Current filter selection
     onAccountChange: (accountId: string | null) => void;
   };
   ```

   The filtering system combines:

   - Core filtering logic in the `useAccountFilter` hook
   - URL parameters in the page components for shareable filters
   - Local storage in the page components for persistent preferences

2. **State Management**

   ```typescript
   // Page-level state management (in ExpensesPage/IncomesPage)
   const urlAccountId = searchParams.get("accountId");
   const storedData = getStorageData();
   const isEditing = window.location.pathname.includes("/edit/");

   // When returning from edit mode, restore URL from storage
   useEffect(() => {
     if (!isEditing && !urlAccountId && storedData?.selectedAccountId) {
       const newSearchParams = new URLSearchParams();
       newSearchParams.set("accountId", storedData.selectedAccountId);
       setSearchParams(newSearchParams);
     }
   }, [isEditing, urlAccountId, storedData?.selectedAccountId]);
   ```

   Key features:

   - URL parameters for shareable filters
   - Local storage for persistent preferences
   - Edit mode handling to prevent URL updates during edits
   - Automatic filter restoration after edit completion

3. **Virtual Account Handling**

   ```typescript
   // Dynamic account list building with virtual accounts
   const accountsInItems = useMemo(() => {
     const uniqueAccountIds = new Set<string>();
     const accountsMap = new Map<string, Account>();
     let hasUnassignedItems = false;

     // Create a map of all accounts for quick lookup
     accounts.forEach((account) => {
       accountsMap.set(account.rowId.toString(), account);
     });

     // Find which accounts are actually used
     items.forEach((item) => {
       if (item.account?.rowId) {
         uniqueAccountIds.add(item.account.rowId.toString());
       } else {
         hasUnassignedItems = true;
       }
     });

     // Build list of accounts that have items
     const accountsInUse = Array.from(uniqueAccountIds)
       .map((id) => accountsMap.get(id))
       .filter((account): account is Account => account !== undefined)
       .sort((a, b) => a.description.localeCompare(b.description));

     // Add virtual "Not Assigned" account if needed
     if (hasUnassignedItems) {
       accountsInUse.unshift({
         rowId: "not-assigned",
         description: "Not Assigned",
         bsb: "",
         number: "",
         balance: 0,
         reserved: 0,
         totalExpenseAccrued: 0,
         dailyExpenseAccrual: 0,
         available: 0,
         linkedExpenses: 0,
         linkedIncomes: 0,
         etag: 0n,
       });
     }
     return accountsInUse;
   }, [items, accounts]);
   ```

   The system provides:

   - Efficient account lookup using Map and Set
   - Type-safe filtering with proper TypeScript type guards
   - Automatic inclusion of virtual "Not Assigned" account when needed
   - Alphabetically sorted accounts by description
   - Memory-efficient implementation using useMemo
   - Full Account type compatibility for the virtual account

4. **Edit Mode Handling**

   ```typescript
   // Page component implementation
   const isEditing = window.location.pathname.includes("/edit/");

   // Filter change handler with edit mode awareness
   const handleAccountChange = (accountId) => {
     if (accountId) {
       // Always update storage
       setStorageData({ selectedAccountId: accountId });

       // Only update URL if not editing
       if (!isEditing) {
         const newSearchParams = new URLSearchParams();
         newSearchParams.set("accountId", accountId);
         setSearchParams(newSearchParams);
       }
     } else {
       // Clear filter state
       setStorageData({ selectedAccountId: null });

       // Only update URL if not editing
       if (!isEditing) {
         setSearchParams(new URLSearchParams());
       }
     }
   };
   ```

   The system provides:

   - Simple edit mode detection via URL path
   - Consistent storage updates regardless of edit state
   - Deferred URL updates until edit completion
   - Clean separation between storage and URL state

5. **Auto-Validation System**

   ```typescript
   // Automatic filter validation
   useEffect(() => {
     if (selectedAccountId && accountsInItems.length > 0) {
       const isSelectedAccountPresent = accountsInItems.some(
         (account) => account.rowId.toString() === selectedAccountId
       );

       if (!isSelectedAccountPresent) {
         // Clear invalid filter selection
         onAccountChange(null);
       }
     }
   }, [selectedAccountId, accountsInItems]);
   ```

   Validation features:

   - Checks filter validity on data changes
   - Clears invalid selections automatically
   - Handles account deletion scenarios
   - Maintains consistent filter state

This implementation ensures:

- Consistent filtering behavior across features
- Reliable state persistence
- Proper handling of edge cases
- Type-safe implementation with TypeScript
- Clean separation of concerns

#### Edge Case Handling

The filter system handles several complex scenarios to ensure a robust user experience:

1. **Account Deletion**

   ```typescript
   // Auto-cleanup when filtered account is deleted
   useEffect(() => {
     if (selectedAccountId && accountsInItems.length > 0) {
       const isSelectedAccountPresent = accountsInItems.some(
         (account) => account.rowId.toString() === selectedAccountId
       );

       if (!isSelectedAccountPresent) {
         onAccountChange(null); // Clear invalid selection
       }
     }
   }, [selectedAccountId, accountsInItems]);
   ```

   - Automatically clears filter if selected account is deleted
   - Updates UI to show all items
   - Maintains URL and storage consistency

2. **Concurrent Edit Operations**

   ```typescript
   // Edit state management
   const isEditing = window.location.pathname.includes("/edit/");
   const urlAccountId = searchParams.get("accountId");
   const storedData = getStorageData();

   // Filter state resolution with edit priority
   const selectedAccountId = useMemo(
     () => (isEditing ? storedData?.selectedAccountId : urlAccountId) || null,
     [isEditing, urlAccountId, storedData]
   );
   ```

   - Prevents filter changes during active edits
   - Handles multiple concurrent edit sessions
   - Maintains separate filter state per edit
   - Resolves conflicts on save/cancel

3. **Edit Cancellation Recovery**

   ```typescript
   // URL state restoration after cancel
   useEffect(() => {
     if (!isEditing && storedData?.selectedAccountId) {
       const newSearchParams = new URLSearchParams();
       newSearchParams.set("accountId", storedData.selectedAccountId);
       setSearchParams(newSearchParams);
     }
   }, [isEditing, storedData?.selectedAccountId]);
   ```

   - Restores previous filter state
   - Handles browser navigation
   - Preserves user's filter context
   - Cleans up temporary state

4. **State Synchronization Timing**

   ```typescript
   // Storage and URL sync management
   const handleAccountChange = (accountId: string | null) => {
     // Always update storage immediately
     setStorageData({ selectedAccountId: accountId });

     // Defer URL updates if editing
     if (!isEditing) {
       const newSearchParams = new URLSearchParams();
       if (accountId) {
         newSearchParams.set("accountId", accountId);
       }
       setSearchParams(newSearchParams);
     }
   };
   ```

   - Immediate storage updates
   - Deferred URL changes
   - Batched state updates
   - Race condition prevention

5. **Filter State Validation**

   The system validates filter state at two levels:

   ```typescript
   // Hook-level validation
   useEffect(() => {
     if (selectedAccountId && accountsInItems.length > 0) {
       const isSelectedAccountPresent = accountsInItems.some(
         (account) => account.rowId.toString() === selectedAccountId
       );

       if (!isSelectedAccountPresent) {
         // Selected account no longer available, clear the filter
         onAccountChange(null);
       }
     }
   }, [selectedAccountId, accountsInItems, onAccountChange]);

   // Page-level validation for display
   const validatedSelectedAccountId = useMemo(() => {
     if (!initialAccountId) {
       return null;
     }

     const isValidAccount = accountsInItems.some(
       (account) => account.rowId.toString() === initialAccountId
     );

     return isValidAccount ? initialAccountId : null;
   }, [initialAccountId, accountsInItems]);
   ```

   Key validation features:

   - Automatic cleanup of invalid selections
   - Validation in both hook and page components
   - Proper type checking of account IDs
   - Safe handling of deleted accounts

These validation mechanisms ensure the filter system remains robust and user-friendly even in edge cases.

### Key Components

1. **AccountsPage**

   - Main accounts listing and management
   - Description filtering with state persistence:

     ```typescript
     // State for search term
     const [searchTerm, setSearchTerm] = useState<string>("");

     // Memoized description filtering
     const descriptionFilteredAccounts = useMemo(() => {
       if (!searchTerm.trim()) {
         return accounts;
       }

       return accounts.filter((account) =>
         account.description
           ?.toLowerCase()
           .includes(searchTerm.trim().toLowerCase())
       );
     }, [accounts, searchTerm]);

     // SearchInput component usage
     <SearchInput
       value={searchTerm}
       onChange={setSearchTerm}
       placeholder="Search by description..."
       ariaLabel="Search accounts by description"
       name="account-search"
     />;
     ```

   - Filter State Management:

     - Description search is maintained in component state
     - Real-time filtering as user types
     - Case-insensitive matching
     - Handles special characters and spaces
     - Resets on navigation/refresh

   - Integration with Navigation:

     - Preserves current search during account edits
     - Resets search when navigating away
     - Works with browser history navigation
     - Maintains filter during page refreshes

   - Real-time updates via React Query
   - Account status monitoring with live updates
   - Actions menu for CRUD operations

2. **AccountsOverview**

   - Dashboard widget showing account summaries
   - Real-time balance updates
   - Status indicators
   - Quick access to account details

3. **AccountForm**
   - Shared form component for create/edit
   - Real-time validation
   - Money value formatting
   - Error handling and display

### Feature Integration

1. **With Expenses**

   - Tracks expense accrual per account
   - Updates daily accrual rates
   - Maintains expense associations

2. **With Income**

   - Records expected income
   - Updates available balances
   - Maintains income associations

3. **With Projections**
   - Calculates future balances
   - Tracks financial health indicators
   - Projects cash flow

### Accessing Account Features

- Create a new account:

  - Click the "Add Account" button at the top of the accounts page
  - A slide-out sheet appears from the right with the account form

- Edit an existing account:
  - Click the "..." menu button in the account's row
  - Select "Edit" from the dropdown menu
  - A slide-out sheet appears from the right with the account form

### Error Handling

POT implements a comprehensive error handling system that manages errors effectively across both client and server sides:

#### Client-Side Error Types

The application implements a type-safe error hierarchy:

```typescript
// Base error types
const ErrorType = {
  Api: "Api",
  Network: "Network",
  Unexpected: "Unexpected",
} as const;

const ErrorCode = {
  Authentication: "Authentication Error",
  Validation: "Validation Error",
  NotFound: "Not Found",
  Conflict: "Conflict Error",
  Network: "Network Error",
  Unexpected: "Unexpected Error",
  Forbidden: "Forbidden Error",
} as const;

// Type-safe error classes
abstract class ApiError extends FailResultBase {
  constructor(code: ErrorCode, description: string) {
    super(ErrorType.Api, code, description);
  }
}
```

Specific error implementations:

- `NetworkError`: Connection and HTTP issues
- `AuthenticationError`: Login and authorization failures
- `ValidationError`: Form and input validation errors
- `NotFoundError`: Resource not found errors
- `ConflictError`: Concurrent modification conflicts
- `UnexpectedError`: Unhandled server errors
- `ForbiddenError`: Permission denied errors

#### API Error Response Handling

1. Error Response Type

   ```typescript
   type ApiErrorResponse = {
     type?: string;
     title?: string;
     status?: number;
     detail?: string;
     errors?: Array<{
       propertyName: string;
       errorCode: string;
       attemptedValue: string;
       errorMessage: string;
     }>;
     traceId?: string;
     correlationId?: string;
     instance?: string;
   };
   ```

2. Error Message Generation
   ```typescript
   // Example: Conflict error handling
   const getConflictMessage = (error: ApiErrorResponse): string => {
     if (error.errors?.length > 0) {
       return error.errors
         .map((err) => {
           if (err.propertyName.toLowerCase() === "etag") {
             return "A conflicting update has been performed by another user. Refresh and try again.";
           }
           return `The '${err.propertyName}' conflicts with another record that has the same value '${err.attemptedValue}'`;
         })
         .join("\n");
     }
     return error.detail ?? "A conflict error occurred";
   };
   ```

#### Server-Side Validation

The server uses FluentValidation for request validation:

1. Request Validation

   - Property-level validation with specific error codes
   - Custom validation rules with contextual data
   - ETag validation for concurrency control

2. Problem Details
   - RFC 7807 compliant problem details
   - Structured error responses with property details
   - Custom error states and metadata

#### Form Validation

The application implements type-safe form validation using Zod schemas:

1. **Base Value Schemas**

   ```typescript
   // Reusable money value validation
   const MoneyValueSchema = z
     .number({
       required_error: "This field is required",
     })
     .min(0, "Value must be 0 or greater");
   ```

2. **Type-Safe Form Schemas**

   ```typescript
   // Account form validation schema
   const accountFormSchema = z.object({
     bsb: z
       .string()
       .regex(/^\d{3}-\d{3}$/, "BSB must be in the format XXX-XXX"),
     number: z.string().min(1),
     description: z.string().min(1),
     balance: MoneyValueSchema,
     reserved: MoneyValueSchema,
   });

   // Type inference for form data
   type AccountFormData = z.infer<typeof accountFormSchema>;
   ```

3. **Domain Model Schemas**

   ```typescript
   // Base account schema
   const BaseAccountSchema = z.object({
     bsb: z.string(),
     number: z.string(),
     description: z.string(),
     balance: z.number(),
     reserved: z.number(),
   });

   // Identity schema for models
   const IdentitySchema = z.object({
     rowId: z.string(),
     etag: z.bigint(),
   });

   // Extended account schema
   const AccountSchema = BaseAccountSchema.extend({
     ...IdentitySchema.shape,
     totalExpenseAccrued: z.number(),
     dailyExpenseAccrual: z.number(),
     available: z.number(),
     linkedExpenses: z.number(),
     linkedIncomes: z.number(),
   });
   ```

4. **Operation-Specific Schemas**

   ```typescript
   // Expense accrual operation schema
   const AccrueExpensesSchema = z.object({
     rowId: z.string(),
     accrued: z.number(),
   });

   // Toggle exclusion operation schema
   const ToggleExcludeExpensesSchema = z.object({
     rowIds: z.array(z.string()),
     exclude: z.boolean(),
   });
   ```

Key Features:

- Strict type inference for form data
- Reusable validation schemas
- Custom error messages
- Operation-specific validation rules
- Domain model validation
- Integration with react-hook-form

3. **Server-Side Validation**

   The server uses FluentValidation with custom rules and validation contexts:

   a. **Base Validators**

   ```csharp
   internal sealed class RequestValidator : PotValidatorBase<Request>
   {
       public RequestValidator()
       {
           RuleFor(request => request.Description).IsNotEmpty();
           RuleFor(request => request.Amount).IsGreaterThanOrEqualTo(0.0d);
           RuleFor(request => request.AccountRowId).IsNotEmpty();
       }
   }
   ```

   b. **Custom Validation Rules**

   ```csharp
   this.CustomRuleFor(request => request.EndDate, (value, context) =>
   {
       if (value.HasValue)
       {
           var validationContext = context.GetContextData<Request, RequestValidationContext>();

           if (validationContext.NextDue > value.Value)
           {
               var failure = new ValidationFailure(
                   nameof(Request.EndDate),
                   "Cannot be earlier than the next due date",
                   value
               )
               {
                   ErrorCode = ErrorCodes.Invalid
               };

               context.AddFailure(failure);
           }
       }
   });
   ```

   c. **Frequency Validation**

   ```csharp
   this.CustomRuleFor(request => request.FrequencyCount, (value, context) =>
   {
       var validationContext = context.GetContextData<Request, RequestValidationContext>();

       if (validationContext.Frequency == Frequency.OneTime)
       {
           if (value != 0)
           {
               var failure = new ValidationFailure(
                   nameof(Request.FrequencyCount),
                   $"Must be zero when Frequency is {Frequency.OneTime.Name}",
                   value
               )
               {
                   ErrorCode = ErrorCodes.Invalid
               };

               context.AddFailure(failure);
           }
       }
       else
       {
           if (value < 1)
           {
               var failure = new ValidationFailure(
                   nameof(Request.FrequencyCount),
                   "Must be greater than zero",
                   value
               )
               {
                   ErrorCode = ErrorCodes.Invalid
               };

               context.AddFailure(failure);
           }
       }
   });
   ```

#### Local Storage

1. Type-Safe Storage Hook
   ```typescript
   type LocalStorageProps<T> = {
     key: string;
     initialValue?: T;
     onError?: (error: DisplayError) => void;
   };
   ```

````

2. Error Handling

   - Storage operation error catching
   - Type validation on retrieval
   - Error reporting via callbacks
   - Integration with global error handling

3. Feature-Specific Storage
   - Projection settings storage
   - Income/Expense filters storage
   - Authentication token storage
   - User preferences storage

#### Error Display Components

1. ErrorSheet Component
   ```typescript
   type SheetErrorProps = DisplayError & {
     onDismiss: () => void;
   };
   ```

   - Fixed-position error sheet at top of viewport
   - Supports multi-line error descriptions
   - Dismissible with X button
   - Styled with destructive theme color
   - Accessible with screen reader support
   - Responsive with max-width constraint

2. Toast System
   ```typescript
   type ErrorToastProps = {
     icon: LucideIcon;
     title: string;
     description: string;
   };
   ```

   - Icon-based error toasts with consistent styling
   - Customizable icons for different error types
   - Red color theme for error states
   - Clear title and description structure
   - Shared base IconToast component

#### Error Boundaries

React error boundaries catch and handle unexpected rendering errors:

- Prevents application crashes
- Provides fallback UI
- Uses `ErrorBoundary` component from `@/components/error/ErrorBoundary`
- Can be reset using `useResetErrorBoundary` hook

#### Context Error Management

1. Feature Contexts
   ```typescript
   type ContextErrorState = {
     error: DisplayError | null;
     setError: (error: DisplayError | null) => void;
   };
   ```
   - Centralized error state per feature
   - Error propagation to parent contexts
   - Error boundary integration
   - Error reset capabilities

#### Logging

The application includes error logging:

- Client-side errors logged with context
- Network errors with request details
- Validation failures with attempted values
- Unexpected errors with stack traces
- Storage operation errors with keys
- Form validation errors with field context

### ErrorProvider

The `ErrorProvider` is a centralized context for managing and displaying errors across the application. It simplifies error handling by providing a consistent interface for setting, retrieving, and clearing errors.

#### Key Features

- **Centralized Error State**: All errors are managed in a single context, reducing the need for local error state management.
- **Integration with ErrorSheet**: Automatically displays critical errors using the `ErrorSheet` component.
- **Type-Safe Error Management**: Ensures all errors conform to the `DisplayError` type.
- **Error Reset**: Provides methods to clear errors programmatically.

#### Usage

1. **Wrapping the Application**

   The `ErrorProvider` should wrap the root of your application to ensure all components have access to the error context:

   ```tsx
   import { ErrorProvider } from "@/components/feedback/ErrorProvider";

   function App() {
     return (
       <ErrorProvider>
         <YourAppComponents />
       </ErrorProvider>
     );
   }
   ```

2. **Accessing the Error Context**

   Use the `useErrorContext` hook to interact with the error state:

   ```tsx
   import { useErrorContext } from "@/components/feedback/ErrorProvider";

   function SomeComponent() {
     const { setError, clearError } = useErrorContext();

     const handleError = () => {
       setError({
         title: "An error occurred",
         description: "Something went wrong while processing your request."
       });
     };

     return (
       <button onClick={handleError}>Trigger Error</button>
     );
   }
   ```

3. **Displaying Errors**

   The `ErrorProvider` integrates seamlessly with the `ErrorSheet` component to display errors at the top of the viewport. Errors are automatically dismissed when cleared from the context.

## Expenses Management

### Overview

The expenses feature manages recurring and one-time expenses with comprehensive filtering and state persistence:

### Display and Filtering

1. **Expense List View**
   - Description and Amount
   - Category and Frequency type
   - Date information (Next Due, Start, End)
   - Associated account details
   - Status indicators

2. **Filter System**

   a. **Account Filtering**
   ```typescript
   // Local storage persisted state
   type ExpenseStorageData = {
     selectedAccountId: string | null;
   };

   // URL state synchronization
   useEffect(() => {
     if (!isEditing && !urlAccountId && storedData?.selectedAccountId) {
       const newSearchParams = new URLSearchParams();
       newSearchParams.set('accountId', storedData.selectedAccountId);
       setSearchParams(newSearchParams);
     }
   }, [isEditing, urlAccountId, storedData?.selectedAccountId]);
   ```

   b. **Description Search**
   ```typescript
   const filteredExpenses = useMemo(() => {
     return expenses.filter(expense =>
       expense.description
         .toLowerCase()
         .includes(searchTerm.toLowerCase())
     );
   }, [expenses, searchTerm]);
   ```

   c. **Filter State Management**
   - URL params for shareable filters
   - Local storage for persistent preferences
   - State reset on navigation
   - Edit mode filter preservation

### Operations

1. **Create Expense**
   - Form with validation
   - Account selection
   - Frequency configuration
   - Date validation rules

2. **Edit Expense**
   - Maintains filter state during edit
   - Returns to filtered view after save
   - Preserves account filter in URL
   - Updates local storage state

3. **Delete Expense**
   - Validation checks
   - Account balance updates
   - Projection recalculation

## Income Management

### Overview

The income feature manages recurring and one-time income sources with integrated filtering and state management:

### Display and Filtering

1. **Income List View**
   - Description and Amount
   - Category and Frequency type
   - Date information (Next Due, End)
   - Associated account details
   - Status indicators

2. **Filter Implementation**

   a. **Account Filter**
   ```typescript
   // Local storage structure
   type IncomeStorageData = {
     selectedAccountId: string | null;
   };

   // Shared account filtering hook usage
   const {
     accountsInItems,
     filteredItems: filteredIncomes,
     setSelectedAccountId: handleAccountChange,
   } = useAccountFilter<Income>({
     accounts,
     items: incomes,
     selectedAccountId: urlAccountId || storedData?.selectedAccountId || null,
     onAccountChange: accountId => {
       // Update storage
       setIncomeData({ selectedAccountId: accountId });

       // Update URL if not in edit mode
       if (!isEditing && accountId) {
         const newSearchParams = new URLSearchParams();
         newSearchParams.set('accountId', accountId);
         setSearchParams(newSearchParams);
       }
     },
   });
   ```

   b. **Description Search**
   ```typescript
   // Real-time search filtering
   const searchFiltered = useMemo(() => {
     if (!searchTerm) {
       return incomes;
     }

     return incomes.filter(income =>
       income.description
         .toLowerCase()
         .includes(searchTerm.toLowerCase())
     );
   }, [incomes, searchTerm]);
   ```

   c. **Filter State Handling**
   - URL parameters for shareable filters
   - Local storage for user preferences
   - Edit mode state preservation
   - Filter restoration after edit

### Operations

1. **Create Income**
   - Validated form inputs
   - Account selection
   - Frequency configuration
   - Date range validation

2. **Edit Income**
   - Maintains filter context
   - Preserves URL state
   - Updates local storage
   - Handles concurrent edits

3. **Delete Income**
   - Validation rules
   - Account updates
   - Projection recalculation

## Data Management

POT provides comprehensive data management capabilities for backing up and restoring your financial data:

### Exporting Data

- From the Maintenance page, click "Export Data"
- Choose a location to save the export file
- The exported file will be named: `pot-YYYY-MM-DD_HHMMSS.export`
- The export file contains:
  - All account information and balances
  - Expense definitions and schedules
  - Income definitions and schedules
  - All data is encrypted using RSA encryption

### Importing Data

1. **Preparing for Import**

   - Ensure you have a valid export file (`.export` extension)
   - The server must have the matching RSA private key
   - You can import data while the system has existing data

2. **Import Process**

   - Navigate to the Maintenance page
   - Click "Import Data"
   - Select your export file using the Browse button
   - Click "Import Data" to proceed
   - A success message will show the total number of items imported

3. **Import Validation**

   - The system checks for:
     - File integrity and encryption
     - Data format version compatibility
     - Duplicate account numbers
     - Valid expense/income configurations
   - Any validation errors will be displayed

4. **After Import**
   - The system will show a success message
   - New data will be immediately available
   - Projections will update automatically

### Data Security

- Export files are encrypted using RSA public/private key pairs
- Keys are configured in environment variables
- Export files can only be imported by servers with the matching private key
- Imported data is validated for integrity and correctness

# Quick Start Guide

## **Clone the repository**

```bash
git clone https://github.com/mjfreelancing/POT.git
cd POT
```

## **Run the application(s)**

### **Using Docker**

#### **Option 1 - Server Only**

Run the server in docker:

- Press `Ctrl+Shift+P` to open the Command Palette
- Select "Run Task" and choose "docker-start-pot-server-only"
- Start the client manually:

  - Within the terminal, navigate to `Source/Client/pot-react`
  - Start the client application:

    ```bash
    cd Source/Client/pot-react
    npm install
    npm run dev
    ```

- Open your browser at http://localhost:5175

#### **Option 2 - Client and Server**

Run the client and server in docker:

- Press `Ctrl+Shift+P` to open the Command Palette
- Select "Run Task" and choose "docker-start-pot-client-server"
- Open your browser at http://localhost:5175

### **Manually**

- Start the server found in the `Source/Server` folder:

  ```bash
  # Navigate to the server directory
  cd Source/Server

  # Restore dependencies
  dotnet restore pot.sln

  # Set up the development database (first time only)
  dotnet ef database update --project Pot.Data.Migrations

  # Run the server
  dotnet run --project Pot.AspNetCore
  ```

  The server will start on http://localhost:5242

- Start the client

  - Within the terminal, navigate to `Source/Client/pot-react`
  - Start the client application:

    ```bash
    cd Source/Client/pot-react
    npm install
    npm run dev
    ```

  - Open your browser at http://localhost:5175

# Navigation and Usage

## Application Structure

The POT application is organized into the following main sections:

- **Dashboard** - The landing page showing your overall financial status
- **Projections** - Financial projections based on your accounts and recurring transactions
- **Expenses** - Manage your recurring and one-time expenses
- **Income** - Track your income sources and recurring payments
- **Accounts** - Manage your bank accounts and track balances
- **Maintenance** - Export and import your financial data

## Accessibility Features

POT is built with accessibility in mind:

- Keyboard navigation support throughout the application
- ARIA attributes for screen readers
- Sufficient color contrast ratios for all UI elements
- Responsive design that works on various device sizes
- Focus management for modal dialogs and forms
- Error messages that are clear and descriptive

## Environment Configuration

POT uses environment files for configuration:

### Client Environment Variables

Located in `/Source/Client/pot-react/`:

- `.env` - Base environment variables for all environments
- `.env.development` - Development-specific settings
- `.env.production` - Production build settings

The base `.env` file contains the following environment variables:

```
# Timeout for API requests
VITE_API_TIMEOUT_MS=10000
```

The `.env.development` file contains options for connecting to the server when running locally as well as when running in docker, making it possible to easily switch between the two when applying updates to the server.

When connecting to the server running locally:

```
VITE_API_BASE_URL=http://localhost:5242/api
```

When connecting to the server running in docker:

```
VITE_API_BASE_URL=http://localhost:5241/api
```

The application uses a public/private key pair to encrypt exported data. The `.env.development` file includes the public key:

```
VITE_EXPORT_PUBLIC_KEY=<value here>
```

### Server Environment Variables

The server configuration is managed through environment files and appsettings.json files:

#### **Docker Configuration**

Located in Docker environment files:

- `/Source/Docker/.env` - Base settings
- `/Source/Docker/.env.development` - Development configuration
- `/Source/Docker/.env.production` - Production settings

```bash
# PostgreSQL configuration
POSTGRES_USER=<value>                 # Database user
POSTGRES_PASSWORD=<value>             # Database password

# RSA key for data encryption/decryption
RSA_PRIVATE_KEY=<value>              # Private key for decrypting exported data

# JWT Authentication (if not using default values)
JWT_ISSUER=<value>                   # JWT token issuer
JWT_AUDIENCE=<value>                 # JWT token audience
JWT_SECRET_KEY=<value>               # JWT signing key
```

#### **Local Configuration**

The common settings are located in `Source/Server/Pot.AspNetCore/appsettings.json`

```json
{
  "AllowedHosts": "*",
  "Database": {
    "Host": "localhost", // PostgreSQL server host
    "Username": "postgres", // Database user
    "Password": "password" // Database password
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

The development settings are located in `Source/Server/Pot.AspNetCore/appsettings.Development.json`

```json
{
  "Rsa": {
    "PrivateKey": "<value>" // Private key for decrypting exported data
  },
  "Jwt": {
    "Issuer": "<value>", // JWT token issuer
    "Audience": "<value>", // JWT token audience
    "SecretKey": "<value>" // JWT signing key
  }
}
```

# Scripts

## Development

- `npm run dev` - Start the development server on port 5175
  ```bash
  vite --port 5175
  ```

## Production

- `npm run build` - Build the application for production
  ```bash
  tsc -b && vite build
  ```
- `npm run preview` - Preview the production build locally
  ```bash
  vite preview
  ```

## Code Quality

- `npm run lint` - Check code for style issues
  ```bash
  eslint src/**/*.{ts,tsx}
  ```
- `npm run lint:fix` - Automatically fix linting issues
  ```bash
  eslint --fix src/**/*.{ts,tsx}
  ```
- `npm run lint:sort` - Fix and sort imports
  ```bash
  eslint --fix --fix-type layout,suggestion src/**/*.{ts,tsx}
  ```
- `npm run prettier` - Format code using Prettier
  ```bash
  prettier . --write
  ```

## Testing

- `npm run test` - Run unit tests
  ```bash
  vitest
  ```
- `npm run test:ui` - Run tests with UI, API server, and coverage reporting
  ```bash
  vitest --ui --api 9527 --coverage.enabled --coverage.provider=istanbul --coverage.all
  ```

## Type Checking

- `npm run type:check` - Verify TypeScript types
  ```bash
  tsc --noEmit
  ```

# Development Configuration

## Architecture and Technology Stack

POT is built with a modern web development stack:

- **Frontend**: React 18 with TypeScript, built using Vite
- **Backend**: ASP.NET Core API
- **Database**: PostgreSQL
- **Containerization**: Docker for both development and production
- **UI Framework**: Custom components based on shadcn/ui and TailwindCSS
- **State Management**: React Query for server state, React Context and Zustand for local state
- **Routing**: React Router v6

### State Management Architecture

POT uses a layered state management approach:

1. **Server State (React Query)**

   ```typescript
   // Query client configuration (main.tsx)
   const queryClient = new QueryClient({
     defaultOptions: {
       queries: {
         staleTime: 1000 * 60 * 5, // Data fresh for 5 minutes
         gcTime: 1000 * 60 * 30, // Cache for 30 minutes
       },
     },
   });
   ```

   Key features:

   - Automatic data caching and revalidation
   - Optimistic updates for mutations
   - Cache invalidation on data changes
   - Error and loading states

2. **Global UI State (Zustand)**

   ```typescript
   // Example: Dashboard summary state (accountsSummaryStore)
   type AccountsSummary = {
     totalBalance: number;
     totalReserved: number;
     totalAvailable: number;
     totalDailyAccrual: number;
     setSummary: (
       balance: number,
       reserved: number,
       available: number,
       dailyAccrual: number
     ) => void;
   };
   ```

   Usage cases:

   - Dashboard summaries and statistics
   - Application-wide UI state
   - Cross-component coordination
   - Persistent settings

3. **Component State (React Context)**

   ```typescript
   // Example: Theme context
   type ThemeProviderState = {
     theme: "dark" | "light" | "system";
     setTheme: (theme: Theme) => void;
   };

   const ThemeProviderContext = createContext<ThemeProviderState>(initialState);
   ```

   Implementation patterns:

   - Provider wraps feature-specific components
   - useContext hook for consuming state
   - State isolation for feature modules
   - Proper error handling for missing context

4. **Local Storage Integration**

   ```typescript
   // Type-safe storage hook
   const { getItem, setItem } = useLocalStorage<T>({
     key: STORAGE_KEY,
     initialValue,
     onError: handleError,
   });
   ```

   Storage patterns:

   - Theme preferences
   - User settings
   - Feature-specific data
   - Authentication tokens

## UI/UX Design

The application features a modern, responsive user interface with:

- Dark and light theme support with system preference detection
- Responsive design that works across different device sizes
- Collapsible sidebar navigation for better space utilization
- Consistent design language across all components
- Toast notifications for success/error feedback
- Loading indicators for all asynchronous operations

## TypeScript Configuration

The project uses a multi-tsconfig setup:

- `tsconfig.json` - Base configuration and path aliases
- `tsconfig.app.json` - Application-specific settings
- `tsconfig.node.json` - Node.js build settings (for Vite config)

The codebase strictly enforces TypeScript best practices:

- Uses types instead of interfaces
- No use of 'any' type allowed
- Strict type checking enabled

## Local Storage Implementation

The application uses a type-safe localStorage wrapper for persistent data storage across features:

### Core Storage Hook

- `/hooks/useLocalStorage.ts`: Type-safe wrapper with error handling
  ```typescript
  type LocalStorageProps<T> = {
    key: string;
    initialValue?: T;
    onError?: (error: DisplayError) => void;
  };
  ```

### Feature-Specific Storage

1. **Authentication Storage**

   - Key: 'pot-auth'
   - File: `/features/auth/hooks/useAuthStorage.ts`
   - Stored data:
     ```typescript
     type AuthTokens = {
       accessToken: string; // JWT access token
       refreshToken: string; // JWT refresh token
       expiresAt: number; // Token expiry timestamp
     };
     ```

2. **Projections Storage**

   - Key: 'pot-projections'
   - File: `/features/projections/hooks/useProjectionStorage.ts`
   - Stored data:
     ```typescript
     type ProjectionStorageData = {
       metric?: ProjectionMetric; // Selected chart metric
       period?: number; // Display period in months
       hiddenSeries?: string[]; // Hidden series identifiers
     };
     ```

3. **Expenses Storage**

   - Key: 'pot-expenses'
   - File: `/features/expenses/hooks/useExpenseStorage.ts`
   - Stored data:
     ```typescript
     type ExpenseStorageData = {
       selectedAccountId: string | null; // Selected account filter
     };
     ```

4. **Income Storage**
   - Key: 'pot-incomes'
   - File: `/features/incomes/hooks/useIncomeStorage.ts`
   - Stored data:
     ```typescript
     type IncomeStorageData = {
       selectedAccountId: string | null; // Selected account filter
     };
     ```

## Path Aliases

The `@/*` path alias is configured for importing from the `src` directory:

```typescript
import { Button } from "@/components/ui/button";
```

## ESLint Configuration

The project uses the new flat ESLint config with:

- TypeScript ESLint recommended rules
- React Hooks plugin
- React Refresh plugin
- Simple Import Sort plugin

Additional rules:

- Enforces type over interface
- Enforces import sorting
- Disables react-refresh warnings for constant exports
- Requires braces for all if statements

## Vite Configuration

The development server runs on port 5175 with support for:

- React Fast Refresh
- TailwindCSS
- Path aliases (@/\*)
- Environment variable management

## Testing

Vitest is configured for comprehensive testing:

- Run on port 9527 for UI mode
- Use Istanbul for coverage reporting
- Use JSDOM for DOM simulation
- Component testing with React Testing Library

# Security and Data Privacy

## Authentication Architecture

The authentication system in POT is designed to ensure reliable, race-condition-free authentication state across the entire application. It uses a layered approach to separate core token management from higher-level authentication features.

### Token Management Layer

The base layer handles secure token storage and validation:

```typescript
type TokenContextType = {
  tokens: AuthTokens | undefined;
  isAuthenticated: boolean;
  setTokens: (tokens: AuthTokens) => void;
};
```

TokenContext provides:
- Secure localStorage integration for token persistence
- Basic authentication state (isAuthenticated)
- Token manipulation methods
- Type-safe token management

### Authentication Layer

Built on top of TokenContext, AuthContext provides higher-level authentication features:

```typescript
type AuthContextType = {
  tokens: AuthTokens | undefined;
  userInfo: User | undefined;
  isAuthenticated: boolean;
  login: (tokens: AuthTokens) => void;
  logout: () => void;
};
```

Features include:
- User information management
- Login/logout operations
- Permission management
- Automatic token refresh
- Session maintenance

### Authentication Flow

1. **Application Initialization**
   ```typescript
   // main.tsx - Runs before React initialization
   setupAxiosDefaults();
   const tokenProvider = createAuthTokenProvider();
   setupAxiosInterceptors(tokenProvider);
   ```
   This ensures all API calls have proper authentication headers.

2. **Initial Load Flow**
   - TokenContext loads any stored tokens
   - Axios interceptors initialize with tokens
   - App mounts with authenticated state
   - User info automatically fetched if tokens exist

3. **Login Flow**
   - Credentials sent to `/login` endpoint
   - Response tokens stored in TokenContext
   - AuthContext updates authentication state
   - User info automatically fetched
   - Permissions updated in store

4. **Session Management**
   - Automatic token refresh before expiry
   - Failed requests queue during refresh
   - Requests retry after successful refresh
   - Session cleared on refresh failure

### Troubleshooting Authentication

Common symptoms and their likely causes:

1. **HTML Response Instead of JSON**
   - Check that axios interceptors are initialized in `main.tsx` before any React code
   - Verify token provider is properly set up
   - Ensure the request has proper auth headers

2. **Auth State Inconsistency**
   - Check localStorage for token presence
   - Verify TokenContext is mounted before AuthContext
   - Check React Query cache state for `/me` endpoint
   - Consider clearing React Query cache

3. **Token Refresh Issues**
   - Check token expiry calculation in refresh timer
   - Verify refresh token is being stored
   - Look for failed refresh requests in network tab
   - Check for queued requests during refresh

4. **Initial Load Auth Problems**
   - Verify initialization order in `main.tsx`
   - Check TokenContext state immediately after mount
   - Monitor `/me` request timing
   - Verify auth headers on initial requests

Remember:
- Always check network requests in dev tools
- Verify token presence in localStorage
- Monitor React Query cache state
- Check component mount order
- Inspect auth headers on requests

## Authentication and Authorization

POT implements a comprehensive security system combining JWT-based authentication with role-based authorization.

### Implementation Location

#### Client-Side Implementation

The client-side authentication uses React Context for global state management with the following key implementation details:

##### Component Architecture

- AuthProvider wraps the application root for global auth state
- AuthContext exposes tokens, authentication state, login/logout methods
- ProtectedRoute component enforces authentication on sensitive routes
- All components use the useAuth hook to access auth state

##### Token Management Implementation

- AuthTokens type defines access token, refresh token, and expiry
- TokenProvider interface manages token operations (get, refresh, clear)
- Type-safe localStorage wrapper for secure token storage
- Token refresh mechanism with automatic retry of failed requests

##### API Integration

- Axios interceptors automatically add Authorization headers
- 401 responses trigger token refresh flow
- Failed requests are queued and retried after token refresh
- Correlation IDs added to all auth-related requests

##### Centralized Logout

- Global logout manager handles application-wide logout
- Cleans up tokens, state, and redirects to login
- Registered callbacks for component cleanup

Key files and their responsibilities:

- `/Source/Client/pot-react/src/features/auth/` - Core authentication components and hooks
- `/Source/Client/pot-react/src/features/auth/AuthContext.tsx` - Global auth state management
- `/Source/Client/pot-react/src/features/auth/LoginPage.tsx` - Login implementation
- `/Source/Client/pot-react/src/api/hooks/useLogin.ts` - API integration for authentication

Error Handling:

- `/Source/Client/pot-react/src/api/errors/apiErrors.ts` - Centralized error types
- `/Source/Client/pot-react/src/components/feedback/ErrorSheet.tsx` - Error display component
- `/Source/Client/pot-react/src/lib/errors/` - Error utilities and type definitions

Storage and State:

- `/Source/Client/pot-react/src/hooks/useLocalStorage.ts` - Type-safe localStorage wrapper with error handling and logging
- Includes error handling and logging for storage operations
- Type safety for stored values
- Integration with application-wide error handling

API Integration:

- `/Source/Client/pot-react/src/api/hooks/useApi.ts` - Base API hook with error handling
- `/Source/Client/pot-react/src/api/client.ts` - Axios client configuration
- `/Source/Client/pot-react/src/api/interceptors/` - Request/response interceptors

#### Server-Side Implementation

- Authentication:

  - `/Source/Server/Pot.AspNetCore/Features/Auth/` - Authentication endpoints and handlers
  - `/Source/Server/Pot.App/Features/Auth/` - Core authentication business logic
  - `/Source/Server/Pot.AspNetCore/appsettings.json` - JWT configuration

- Authorization:
  - `/Source/Server/Pot.AspNetCore/Security/` - JWT token generation, validation, and permission handlers
  - `/Source/Server/Pot.App/Security/` - Password hashing and crypto utilities
  - `/Source/Server/Pot.Data/Security/` - Security-related database models and permission storage

### Authentication System

The authentication system is built around React Context with type-safe implementations:

1. **AuthContext Implementation**
   ```typescript
   type AuthContextType = {
     tokens: AuthTokens | undefined;
     isAuthenticated: boolean;
     login: (tokens: AuthTokens) => void;
     logout: () => void;
     error?: DisplayError;
   };

   function AuthProvider({ children }: { children: ReactNode }) {
     // Local storage integration
     const { getItem, setItem, removeItem } = useLocalStorage<AuthTokens>({
       key: AUTH_STORAGE_KEY,
       onError: setError,
     });

     // Memoized auth state
     const isAuthenticated = Boolean(tokens?.accessToken);

     // Login/logout handlers
     const login = useCallback((newTokens: AuthTokens) => {
       setItem(newTokens);
       setTokens(newTokens);
     }, [setItem]);

     const logout = useCallback(() => {
       removeItem();
       setTokens(undefined);
     }, [removeItem]);

     // Token refresh management
     useEffect(() => {
       if (tokens) {
         refreshTimerRef.current = createTokenRefreshTimer({
           currentTokens: tokens,
           onRefreshSuccess: login,
           onRefreshError: () => logout(),
         });
       }
     }, [tokens, login, logout]);

     return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
   }
   ```

2. **Token Management**
   ```typescript
   // Type-safe token structure
   type AuthTokens = {
     accessToken: string;    // JWT access token
     refreshToken: string;   // JWT refresh token
     expiresAt: number;     // Token expiry timestamp
   };

   // Token refresh timer
   type TokenRefreshConfig = {
     currentTokens: AuthTokens;
     onRefreshSuccess: (tokens: AuthTokens) => void;
     onRefreshError: () => void;
   };
   ```

3. **Token Refresh System**
   ```typescript
   type TokenRefreshConfig = {
     currentTokens: AuthTokens;
     onRefreshSuccess: (tokens: AuthTokens) => void;
     onRefreshError: (error: unknown) => void;
   };

   function createTokenRefreshTimer({
     currentTokens,
     onRefreshSuccess,
     onRefreshError,
   }: TokenRefreshConfig): TokenRefreshHandle {
     // Proactive refresh before expiry
     const refreshTimeMs = calculateRefreshTime(currentTokens.accessToken);

     // Token refresh logic
     const refresh = async () => {
       try {
         const response = await authClient.post<AuthTokens>(
           '/auth/refresh',
           { refreshToken: currentTokens.refreshToken },
           { headers: { Authorization: `Bearer ${currentTokens.accessToken}` } }
         );
         onRefreshSuccess(response.data);
       } catch (error) {
         onRefreshError(error);
       }
     };

     return {
       start: () => setTimeout(refresh, refreshTimeMs),
       stop: () => clearTimeout(timerId)
     };
   }
   ```

4. **Logout Management**
   ```typescript
   // Global logout manager
   const logoutManager = (() => {
     let logoutCallback: (() => void) | undefined;

     return {
       setLogoutCallback: (callback: () => void) => {
         logoutCallback = callback;
       },
       logout: () => {
         logoutCallback?.();
       },
     };
   })();
   ```

Key Features:
- Type-safe implementation
- Proactive token refresh
- Global logout handling
- Secure storage integration
- Automatic cleanup
- Error handling with types
- React Query integration

- Short-lived access tokens (60 minute expiry) for API authorization
- Refresh tokens with 30 day expiry
- Automatic token refresh via interceptors with queued request retry
- Secure token storage using type-safe localStorage wrapper
- Global authentication state management through React Context
- Token validation with strict issuer, audience, and signing key checks

#### Authentication Flow

1. Login Process

   - Credential validation and token generation
   - Secure token storage
   - State update and redirection

2. Protected Routes

   - Auth-protected route guards
   - Automatic redirect for unauthenticated users
   - Fresh authentication for sensitive operations

3. Token Lifecycle
   - Automatic refresh of expired tokens
   - Failed request retry with new tokens
   - Secure token cleanup on logout

### Authorization System

The authorization system uses a combination of JWT claims and database-backed permissions for secure access control, with a dynamic authorization policy provider pattern.

#### Authorization Pattern

1. **Dynamic Policy Provider**

   The `PermissionAuthorizationPolicyProvider` dynamically creates authorization policies from permission strings:

   ```csharp
   internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
   {
       public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
       {
           // Check for existing policy first
           var policy = await base.GetPolicyAsync(policyName);

           if (policy is not null)
           {
               return policy;
           }

           // Create a new policy with the permission requirement
           return new AuthorizationPolicyBuilder()
               .AddRequirements(new PermissionRequirement(policyName))
               .Build();
       }
   }
   ```

2. **Permission Handler**

   The `PermissionAuthorizationHandler` validates permissions against the user's assigned permissions:

   ```csharp
   internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
   {
       protected override async Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           PermissionRequirement requirement)
       {
           var userRowId = context.User.Claims
               .SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)
               ?.Value;

           if (!Guid.TryParse(userRowId, out var parsedUserRowId))
           {
               return;
           }

           // Get user's permissions from the service
           var permissions = await permissionService
               .GetPermissionsAsync(parsedUserRowId)
               .ConfigureAwait(false);

           // Succeed if user has the required permission
           if (permissions.Contains(requirement.Permission))
           {
               context.Succeed(requirement);
           }
       }
   }
   ```

3. **Endpoint Configuration**

   Endpoints that require authorization use the `RequireAuthorization()` method:

   - For general authentication (any valid user):
     ```csharp
     // Only requires a valid JWT token
     routeGroupBuilder
         .MapGet("/me", Me.Handler.Invoke)
         .RequireAuthorization("AuthenticatedUser")
     ```

   - For specific permissions:
     ```csharp
     // Requires the 'account:view' permission
     routeGroupBuilder
         .MapGet("/accounts", GetAll.Handler.Invoke)
         .RequireAuthorization("account:view")
     ```

4. **Policy Configuration**

   The authorization setup in the DI container:

   ```csharp
   builder.Services
       .AddAuthorization(options =>
       {
           // Add the "AuthenticatedUser" policy that only requires authentication
           options.AddPolicy("AuthenticatedUser", policy =>
               policy.RequireAuthenticatedUser());
       })
       .AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

   // Register the authorization handlers
   builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
   builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
   ```

This pattern allows for:
- Dynamic creation of authorization policies from permission strings
- Separation of authentication and permission checks
- Flexible permission management through the database
- Type-safe permission requirements
- Clear distinction between authentication-only and permission-required endpoints

#### Database Structure

The permission system uses four main entities:

1. `UserEntity`

   - Core user information
   - Links to roles via many-to-many relationship

2. `RoleEntity`

   - Named role definitions
   - Links to permissions via many-to-many relationship
   - Many-to-many relationship with users

3. `PermissionEntity`

   - Individual permission definitions
   - Stored as resource:action strings
   - Many-to-many relationship with roles

4. `SiteEntity`
   - Represents a tenant in the system
   - Users and accounts belong to sites

#### Authentication Flow

1. Request arrives with JWT token in Authorization header
2. `JwtSecurityTokenHandler.ValidateToken()` (see `JwtService`) validates the token using parameters configured by `JwtBearerOptionsSetup`:
   - Token lifetime validation
   - Issuer validation against configured value
   - Audience validation against configured value
   - Signature validation using configured signing key
3. JWT claims are extracted with MapInboundClaims=false to preserve original claim types
4. User identity is established from the subject claim (JwtRegisteredClaimNames.Sub)
5. Request proceeds to authorization check

#### Authorization Flow

When an endpoint requires authorization:

1. Request arrives with validated JWT token
2. Framework extracts user identity and claims
3. Authorization middleware checks for required permissions
4. PermissionAuthorizationPolicyProvider creates policy from permission string
5. PermissionAuthorizationHandler evaluates the permission
6. PermissionService loads actual permissions from database
7. Access is granted or denied based on permission match

#### Permission Model

1. Resource-based Permissions

   - Format: `resource:action` (e.g., `account:view`)
   - Granular access control per feature
   - Dynamic policy creation from permission strings

2. Role Structure

   - Admin Role: Full system access to all features
   - Viewer Role: Read-only access across all features (all :view permissions)
   - Custom roles: Can be defined with specific permission sets
   - Role inheritance: Users can have multiple roles
   - Permission aggregation: User's effective permissions are the union of all their roles' permissions

3. Permission Categories

   - Site Management (`site:manage`, `site:view`)
   - User Management (`user:manage`, `user:view`)
   - Account Management (`account:manage`, `account:view`)
   - Expense Management (`expense:manage`, `expense:view`)
   - Income Management (`income:manage`, `income:view`)

4. Endpoint Protection

   To require permissions on an endpoint:

   ```csharp
   routeGroupBuilder
       .MapGet(AccountsEndpoints.GetAll, GetAll.Handler.Invoke)
       .RequireAuthorization("account:view")
   ```

   The permission string automatically becomes a policy requirement through `PermissionAuthorizationPolicyProvider`.

5. Permission and Role Management

   Adding New Permissions:

   - Add permission to database through migration
   - Update role assignments as needed
   - Follow the resource:action naming pattern
   - Use lowercase consistently
   - Consider permission grouping with roles
   - Document the new permission

### Core Security Components

- JWT authentication via `JwtService` and `JwtBearerOptionsSetup`
- Permission-based authorization via `PermissionAuthorizationHandler`
- Role-based access control using database relationships
- Token management through `AuthContext` and `TokenProvider`
- Client-side route protection using `ProtectedRoute` component

### UI Permission Components

1. **PermissionGuard**

   Conditionally renders content based on permissions. Use when you want to completely hide content from unauthorized users.
   Supports both single and multiple permission checks.

   ```tsx
   // Single permission
   <PermissionGuard permission="account:manage">
     <RestrictedContent />
   </PermissionGuard>

   // Multiple permissions (all required)
   <PermissionGuard permission={["expense:manage", "expense:view"]}>
     <RestrictedContent />
   </PermissionGuard>
   ```

2. **WithPermission**

   Renders interactive elements in a disabled state when permissions are missing. Use for buttons, inputs, etc.
   to show functionality exists but is not available.

   ```tsx
   // Single permission
   <WithPermission permission="income:manage">
     <Button>Manage Income</Button>
   </WithPermission>

   // Multiple permissions (all required)
   <WithPermission permission={["account:manage", "account:view"]}>
     <Button>View and Manage Account</Button>
   </WithPermission>
   ```

3. **Permission Caching**

   For performance and UI stability, permissions can be cached at the component level to prevent flicker:

   ```tsx
   const permissionCache = React.useMemo(() => {
     return items.reduce((acc, item) => {
       if (item.permission) {
         acc[item.permission] = permissions.includes(item.permission);
       }
       return acc;
     }, {} as Record<string, boolean>);
   }, [items, permissions]);
   ```

### Implementation Guidelines

1. **Permission Implementation**

   - Use lowercase resource:action format for permission strings (e.g., `account:view`)
   - Add new permissions through database migrations
   - Update the permission documentation when adding new ones

2. **Role Implementation**
   - Use the many-to-many relationships in the database for role-permission assignments
   - Implement new roles through database migrations
   - Use `PermissionService` to load user permissions through role relationships

## Data Storage

POT stores all financial data in a PostgreSQL database:

- Account information is stored locally in the database
- No data is sent to external servers
- Import/export files use a proprietary, encrypted, format with data validation

## Security Considerations

- JWT-based authentication for API access
- Docker containers isolate application components
- Database access is restricted to the application
- No sensitive financial data in client-side storage
- All auth operations logged with correlation IDs
- Type-safe implementations throughout

## Project Structure

The POT application follows a modular architecture:

```
Source/
├── Client/                      # Frontend React application
│   └── pot-react/
│       ├── src/
│       │   ├── api/            # API integration
│       │   │   ├── errors/     # Error types and handling
│       │   │   ├── hooks/      # React Query hooks
│       │   │   └── interceptors/ # Axios interceptors
│       │   ├── assets/        # Static assets
│       │   ├── components/    # Shared components
│       │   │   ├── feedback/  # Error/loading components
│       │   │   ├── filters/   # Filter components
│       │   │   ├── input/     # Form inputs
│       │   │   ├── nav/       # Navigation components
│       │   │   ├── table/     # Table components
│       │   │   └── ui/        # Base UI components
│       │   ├── data/         # Type definitions & schemas
│       │   ├── features/     # Feature modules
│       │   │   ├── accounts/
│       │   │   ├── auth/
│       │   │   ├── dashboard/
│       │   │   ├── expenses/
│       │   │   ├── export/
│       │   │   ├── import/
│       │   │   ├── incomes/
│       │   │   └── projections/
│       │   ├── hooks/        # Shared hooks
│       │   ├── lib/          # Utilities
│       │   └── routes/       # Routing
│       └── tests/           # Test files
├── Docker/                  # Container configuration
│   ├── Client/             # Client container
│   ├── Postgres/           # Database container
│   ├── Server/             # Server container
│   └── Diagrams/           # Architecture diagrams
├── Server/                 # .NET backend
    ├── Pot.App/           # Business logic
    ├── Pot.AspNetCore/    # Web API
    ├── Pot.Data/          # Data access
    ├── Pot.Data.Migrations/ # Database migrations
    └── Pot.Shared/        # Shared code
│   └── Server/              # API server container setup
└── Server/                  # Backend .NET Core application
    ├── Pot.App/             # Core application logic
    ├── Pot.AspNetCore/      # API endpoints and controllers
    ├── Pot.Data/            # Data access and models
    ├── Pot.Data.Migrations/ # Database migrations
    └── Pot.Shared/          # Shared utilities and DTOs
```
````
