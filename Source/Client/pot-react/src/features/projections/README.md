# Projections Chart Feature

This directory contains the implementation of the account balance projections chart feature.

## Components

### `ProjectionChart`

The main chart component that displays account balance projections over time with fully responsive sizing.

**Features:**

- Multiple line series (one per account + global total)
- Interactive legend with series visibility toggle
- **Smart Default Visibility**: Accounts with empty dates arrays are hidden by default
- Dynamic date formatting based on data range
- Currency formatting using the app's `formatMoneyValue` utility
- **Fully Responsive Design**: Automatically adapts to container size and window resizing
- Empty state handling
- Visual indicators for accounts with no data
- **Dynamic Sizing Management**: Uses container measurement for optimal chart sizing

**Props:**

- `data`: Projection data from the API
- `title`: Optional chart title (default: "Account Balance Projections")
- `curveType`: Optional curve type for line smoothing (default: "basis")

## Responsive Sizing System

The `ProjectionChart` component implements a sophisticated responsive sizing system that eliminates scroll bars and maximizes chart visibility:

### Container Measurement Strategy

- **useRef Hook**: Uses `containerRef` to reference the chart container DOM element
- **Dynamic Dimensions**: Measures actual container `clientWidth` and `clientHeight`
- **Real-time Updates**: Automatically recalculates dimensions on window resize events
- **Minimum Constraints**: Enforces minimum size of 300x300 pixels for usability

### Size Calculation Details

```typescript
const [dimensions, setDimensions] = useState({ width: 800, height: 400 });

useEffect(() => {
  const updateDimensions = () => {
    if (containerRef.current) {
      const { clientWidth, clientHeight } = containerRef.current;
      setDimensions({
        width: Math.max(300, clientWidth - 40), // Account for padding
        height: Math.max(300, clientHeight - 20), // Account for margins
      });
    }
  };

  updateDimensions();
  window.addEventListener('resize', updateDimensions);
  return () => window.removeEventListener('resize', updateDimensions);
}, []);
```

### Layout Structure

- **Flex Container**: Uses CSS flexbox (`flex flex-col h-full`) for predictable layout
- **Fixed Header**: Card header is `flex-shrink-0` to prevent compression
- **Flexible Content**: Chart area uses `flex-1` to fill remaining space
- **Overflow Prevention**: `min-h-0` on chart container prevents flex overflow issues
- **Container Reference**: Chart container div includes `ref={containerRef}` for measurement

### Benefits

- **No Scroll Bars**: Chart dimensions precisely match available space
- **True Responsiveness**: Adapts to all screen sizes and window resizing
- **Consistent Aspect Ratio**: Maintains proper proportions across devices
- **Performance Optimized**: Only recalculates on actual resize events
- **Cross-Browser Compatible**: Uses standard DOM measurement APIs

### `NoProjectionData`

Component displayed when there's no data available to chart.

## Hooks

### `useProjectionChartData`

Custom hook that transforms raw projection data into chart-ready format.

**Returns:**

- `chartData`: Array of data points for the chart
- `chartConfig`: Configuration object for styling and labels
- `seriesKeys`: Array of series identifiers
- `hasData`: Boolean indicating if there's meaningful data to display

## Utilities

### `chartHelpers.ts`

Helper functions for chart formatting:

- `formatXAxisLabel`: Dynamic date label formatting based on data range
- `formatTooltipDate`: Full date formatting for tooltips
- `getStrokeDashArray`: Line style patterns for different series
- `getStrokeWidth`: Line width based on series type

## Usage

```tsx
import { ProjectionChart } from '@/features/projections/components';

function ProjectionsPage() {
  const { data: result } = useApiGetProjection();

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      {result?.success && (
        <div className="p-6 flex-1">
          <ProjectionChart data={result.value} />
        </div>
      )}
    </div>
  );
}
```

### Full-Screen Layout Pattern

The `ProjectionsPage` implements a full-screen layout pattern:

- **Screen Height Container**: Uses `h-screen` to fill the viewport
- **Flex Column Layout**: `flex flex-col` for vertical stacking
- **Flexible Chart Area**: `flex-1` allows chart to use all available space
- **Gradient Background**: Subtle gradient from background to muted colors
- **Responsive Padding**: `p-6` provides consistent spacing around content

This pattern ensures the chart automatically fills the available screen space without causing overflow or scroll bars.

## Styling

The chart uses shadcn/ui chart components with predefined CSS variables and responsive design principles:

- `--chart-1` through `--chart-5` for series colors
- `--foreground` for the global total line
- **Flexbox Layout**: Uses CSS flexbox for predictable responsive behavior
- **Tailwind CSS Classes**: Leverages utility classes for consistent spacing and sizing
- **Container Queries**: Responds to parent container size rather than viewport size
- **Card Component Structure**: Built on shadcn/ui Card components for consistent styling

### Responsive Breakpoints

The chart adapts to different screen sizes through:

- **Minimum Dimensions**: 300x300 pixels minimum for readability
- **Dynamic Scaling**: Automatically adjusts to container size
- **Legend Wrapping**: Interactive legend wraps on smaller screens
- **Axis Label Rotation**: X-axis labels rotate for better fit on narrow displays

## Custom Tooltip Implementation

The chart features a custom tooltip implementation that provides enhanced readability and visual hierarchy.

### Problem with Default ChartTooltipContent

The initial implementation used shadcn/ui's `ChartTooltipContent` component:

```tsx
<ChartTooltip
  content={
    <ChartTooltipContent
      labelFormatter={value => formatTooltipDate(value as string)}
      formatter={(value, name) => [
        formatMoneyValue(value as number),
        chartConfig[name as string]?.label || name,
      ]}
    />
  }
/>
```

**Issues with this approach:**

- **Poor Visual Hierarchy**: All text appeared in the same color and weight
- **Cramped Layout**: Dollar values and account names were squeezed together
- **Limited Styling Control**: Default component provided minimal customization options
- **Readability Problems**: White-on-white text with no visual separation between data points

### Custom Tooltip Solution

The implementation was replaced with a fully custom tooltip that provides:

```tsx
<ChartTooltip
  content={({ active, payload, label }) => {
    if (!active || !payload?.length) return null;

    return (
      <div className="rounded-lg border bg-background p-3 shadow-md">
        <div className="mb-2 font-medium text-foreground">
          {formatTooltipDate(label as string)}
        </div>
        <div className="space-y-1">
          {payload.map((entry, index) => (
            <div
              key={index}
              className="flex items-center justify-between gap-3"
            >
              <div className="flex items-center gap-2">
                <div
                  className="h-3 w-3 rounded-full"
                  style={{ backgroundColor: entry.color }}
                />
                <span className="text-sm text-muted-foreground">
                  {chartConfig[entry.dataKey as string]?.label || entry.dataKey}
                </span>
              </div>
              <span className="font-semibold text-foreground">
                {formatMoneyValue(entry.value as number)}
              </span>
            </div>
          ))}
        </div>
      </div>
    );
  }}
/>
```

**Improvements achieved:**

- **Clear Visual Hierarchy**: Date header, muted account names, bold dollar amounts
- **Color Indicators**: Small colored circles matching line series colors
- **Proper Spacing**: Generous gaps and padding for better readability
- **Professional Layout**: Clean alignment with flexbox and justify-between
- **Enhanced Contrast**: Different text weights and colors for easy scanning
- **Consistent Styling**: Matches the overall design system and theme

## Curve Types

The chart supports multiple curve types through the `curveType` prop, allowing you to customize how lines are drawn between data points. The type is automatically extracted from Recharts Line component props for type safety.

### Available Curve Types:

#### **Smooth Curves** (Best for Financial Data):

- **`"basis"`** _(Default)_ - Smooth B-spline curves that create flowing lines perfect for financial projections
- **`"natural"`** - Natural cubic splines that pass through all data points with smooth transitions
- **`"monotone"`** - Monotonic cubic interpolation that preserves monotonicity and avoids overshooting

#### **Linear Curves**:

- **`"linear"`** - Straight lines between data points (sharp, precise)
- **`"linearClosed"`** - Linear interpolation with closed path (connects end to start)

#### **Step Functions** (Good for Discrete Data):

- **`"step"`** - Step function with vertical lines at midpoints
- **`"stepBefore"`** - Step function with vertical line before each point
- **`"stepAfter"`** - Step function with vertical line after each point

#### **Advanced Curves**:

- **`"basisClosed"`** - B-spline with closed path
- **`"basisOpen"`** - B-spline without connecting start/end points
- **`"monotoneX"`** - Monotonic interpolation preserving X-axis monotonicity
- **`"monotoneY"`** - Monotonic interpolation preserving Y-axis monotonicity

#### **Bump/Cardinal Curves**:

- **`"bump"`** - Bump interpolation (smooth curves with controlled curvature)
- **`"bumpX"`** - Bump interpolation emphasizing X-axis direction
- **`"bumpY"`** - Bump interpolation emphasizing Y-axis direction

### Usage Examples:

```tsx
// Smooth, elegant curves (recommended for financial data)
<ProjectionChart data={result.value} curveType="basis" />

// Sharp, precise lines for exact data representation
<ProjectionChart data={result.value} curveType="linear" />

// Step function for discrete financial events
<ProjectionChart data={result.value} curveType="stepAfter" />

// Natural smooth curves
<ProjectionChart data={result.value} curveType="natural" />
```

### Recommendations for Financial Data:

- **`"basis"`** - Best overall choice for account projections (smooth, professional)
- **`"natural"`** - Good for trend analysis where smooth curves are important
- **`"linear"`** - Use when precise data representation is critical
- **`"stepAfter"`** - Good for showing discrete financial transactions or events
- **`"monotone"`** - Ideal when you need smoothness but want to preserve data trends exactly

## Data Requirements

The chart expects data in the `Projection` format:

- `accounts`: Array of account daily balances
- `global`: Array of global daily balances
- Each balance entry has `date` (ISO string) and `balance` (number)

The chart handles:

- Missing dates (filled with 0 values)
- Empty account data arrays
- Date ranges from 30 to 365+ days

## Technical Implementation Details

### Performance Optimizations

- **Memoized Calculations**: Chart data transformation is memoized in `useProjectionChartData`
- **Efficient Re-renders**: Only updates dimensions on actual resize events
- **DOM Measurement Caching**: Stores container dimensions in state to avoid repeated DOM queries
- **Event Cleanup**: Properly removes resize listeners on component unmount

### Browser Compatibility

- **Modern Flexbox**: Uses CSS flexbox features supported in all modern browsers
- **Standard APIs**: Relies on standard DOM measurement APIs (`clientWidth`, `clientHeight`)
- **Graceful Degradation**: Falls back to default dimensions if container measurement fails
- **TypeScript Safety**: Full type safety for all props and data structures

### Accessibility Features

- **Screen Reader Support**: Proper ARIA labels and descriptions
- **Keyboard Navigation**: Interactive legend buttons are keyboard accessible
- **High Contrast**: Supports high contrast mode through CSS custom properties
- **Responsive Text**: Chart labels scale appropriately for different screen sizes
