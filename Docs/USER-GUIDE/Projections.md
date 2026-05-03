# Financial Projections

## Overview

Projections shows date-based forecast values for each account and a combined Total (All Accounts) series.
It supports both trend metrics (line charts) and event metrics (bar charts), with filters for metric, start date, period, and per-series visibility.

The page is read-only and designed for analysis.

## Available to

- Site Owners
- Admins
- Viewers

## What The Page Displays

- Header: Projections
- Chart title: changes with selected metric
- Chart subtitle: selected date range and day count
- Filter controls:
  - View (metric)
  - From (start date)
  - Period (preset month buttons plus Custom month input)
  - Show (series visibility toggles)
- Chart area:
  - Line chart for trend metrics
  - Bar chart for event metrics
- Right-side detail sheet:
  - Opens only for bar metrics when a bar is clicked

## Metric Types And Chart Types

The View selector supports five metrics.

| Metric label in View selector | Internal metric key | Chart type |
| ----------------------------- | ------------------- | ---------- |
| Account Balances              | balance             | Line       |
| Available Balances            | available           | Line       |
| Projection Accruals           | dailyAccrual        | Line       |
| Income                        | incomeReceived      | Bar        |
| Expenses                      | expensesPaid        | Bar        |

Line metrics show trends over time.
Bar metrics show per-date event amounts and support opening details.

### Projection Accruals Interpretation

Projection Accruals is the operational, date-sensitive accrual metric used by projection simulation. It is expected to vary as due dates approach and as periods renew.

This is different from Dashboard Daily Need:

- Projection Accruals: dynamic event-date metric for simulation behavior.
- Daily Need: stable long-run funding guidance for daily planning.

Do not interpret short-term movement in Projection Accruals as a change in your long-run Daily Need unless underlying obligations changed.

## Date Window And Period Logic

The page always requests a 12-month data window from the selected start date.

- API request range:
  - Start: selected start date
  - End: start date plus 12 months minus 1 day

The Period filter controls only what is rendered in the chart view.

- Example:
  - Start date is May 1
  - Period is 3 months
  - Chart shows May 1 through July 31
  - API still fetched a full 12-month window from May 1

## Filters And Controls

### View (metric)

- Changes chart title, chart type, and value field used from projection data.

### From (start date)

- Uses date picker control.
- Cannot select dates earlier than today.
- If stored start date becomes earlier than today (for example after leaving the app open overnight), it is automatically corrected to today.

### Period

- Preset buttons: 1 mo, 2 mo, 3 mo, 6 mo, 9 mo, 12 mo.
- Custom option:
  - Select Custom to enter a month value manually.
  - Allowed range is 1 to 12 months.
  - Unit label is shown as `mo`.
- The chart updates immediately when the custom value changes.
- Controls the rendered time window from the selected start date.

### Show (series visibility)

- One toggle per account plus one for Total (All Accounts).
- Hidden series are removed from chart rendering.
- Y-axis domain is recalculated from visible series only.

## Mobile Layout

- On mobile, date and period controls are inside a collapsible Filters section.
- In the Period control, choosing Custom shows an inline month input in the same control block.
- The legend row (Show toggles) is also shown only when that section is expanded.

## Tooltip Behavior

### Line metrics

- Standard multi-series tooltip behavior from visible series payload.
- Hidden series are not displayed.

### Bar metrics

- Hovering an individual account bar shows that bar's account value for the date.
- Hovering the Total (All Accounts) bar shows all account values for that date in the tooltip legend.
- In Total (All Accounts) hover mode, hidden accounts are still included in the tooltip legend.

## Bar Click And Details Behavior

The detail sheet appears only when metric is Income or Expenses and the user clicks a bar.

### Click individual account bar

- Sheet date is set to clicked bar date.
- Items are scoped to clicked account only.
- Item count badge uses full-date denominator from all accounts for that date.
  - Example: 2 of 5 items

### Click Total (All Accounts) bar

- Sheet date is set to clicked bar date.
- Items include all accounts for that date.
- Hidden or filtered-out accounts are still included in the details panel in this mode.

### Detail totals and counts

- Total Income or Total Expenses:
  - Sum of all items in the current sheet scope.
- Filtered Total:
  - Shown only when currently visible rows are a subset of sheet scope.
- Item badge:
  - N items when full set is visible
  - X of N items when subset is visible

### Accounts section in detail sheet

- Shown only when there are visible items.
- Lists account names with color dots from chart config.

## Line Chart Axis Behavior

- Y-axis is symmetric around visible min and max with margin.
- A zero reference line appears only when visible domain spans both negative and positive values.

## Bar Chart Axis Behavior

- Y-axis starts at zero.
- Domain scales upward from visible max with margin.

## Loading, Error, And Empty States

- Loading:
  - Overlay shown while projections query is loading or refetching.
- Error:
  - Error sheet shown for either storage errors or API errors.
  - Storage and API errors are handled as separate states.
- Empty chart data:
  - No Data Available card is shown when there are no non-zero values for the selected metric in the returned projection dataset.

## Persistence Behavior

Projection preferences are stored under the authenticated user's scoped projections key.

Storage behavior:

- The page reads `sessionStorage` first, then falls back to `localStorage`, then defaults.
- If a tab has no session entry yet but local storage has saved projection state, that saved state seeds the tab and is copied into `sessionStorage`.
- Updates mirror the full projection state to both stores so a fresh tab can start from the most recently used settings while already-open tabs can continue independently.

Persisted values:

- metric
- period
- hiddenSeries
- startDate when explicitly set to a non-today date

Period persistence rules:

- Both preset and custom period values are persisted as `period`.
- On revisit, the previously selected period value is restored.

Start date persistence rules:

- If the user sets start date to today, startDate is removed from storage.
- When startDate is removed, the remaining projection preferences are retained in both stores.
- If a stored start date is before today on load, it is removed and replaced with today in page state.

## Step-By-Step Usage

1. Open Projections page.
2. Select a View metric.
3. Set From date.
4. Set Period:

- choose a preset month button, or
- choose Custom and enter months (1-12).

5. Use Show toggles to hide or reveal account series.
6. Hover chart points or bars to inspect values.
7. For Income or Expenses metrics, click a bar to open date details.
8. In the detail sheet:

- use totals to compare full scope versus filtered visibility
- use item badge to understand subset versus full-day totals

## Permission-Based Features

### For All Users

- All projections interactions are available:
  - filter controls
  - tooltips
  - details sheet for bar metrics

### For Site Owners And Admins

- Same projections behavior as viewers.

## Tips And Validation Checks

- If a bar detail appears to have fewer rows than expected, compare item badge and Filtered Total.
- Use Total (All Accounts) bar click for complete date audit across accounts.
- Use account bar click for account-specific reconciliation.
- If start date seems to reset, check whether previously saved date is now earlier than today.

---

[Back to User Guide](README.md)
