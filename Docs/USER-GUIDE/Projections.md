# Financial Projections

## Overview

Projections shows date-based forecast values for each account and a combined Total (All Accounts) series.
It supports both trend metrics (line charts) and event metrics (bar charts), with filters for metric, start date, period, and per-series visibility, plus an Include control that subtracts standing obligations from the plotted balance.

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
  - Period (a 1 to 12 month picker)
  - Include (Reserved, Accruals and Arrears deductions; shown only for Account Balances)
  - Accounts (series visibility toggles, with the combined total separate)
- Chart area:
  - Line chart for trend metrics, with the period's low and high marked
  - Bar chart for event metrics
- Right-side detail sheet:
  - Opens only for bar metrics when a bar is clicked

## Metric Types And Chart Types

The View selector supports four metrics.

| Metric label in View selector | Internal metric key | Chart type |
| ----------------------------- | ------------------- | ---------- |
| Account Balances              | balance             | Line       |
| Operational Accruals          | dailyAccrual        | Line       |
| Income                        | incomeReceived      | Bar        |
| Expenses                      | expensesPaid        | Bar        |

Line metrics show trends over time.
Bar metrics show per-date event amounts and support opening details.

### Operational Accruals Interpretation

Operational Accruals is the date-sensitive accrual metric used by projection simulation. It is expected to vary as due dates approach and as periods renew.

It is derived from the expense schedules for each day of the window, so it always reflects the current schedules, amounts and accrual policies.

This is different from Dashboard Daily Need:

- Operational Accruals: dynamic event-date metric for simulation behavior.
- Daily Need: stable long-run funding guidance for daily planning.

Do not interpret short-term movement in Operational Accruals as a change in your long-run Daily Need unless underlying obligations changed.

### Account Balances And The Include Deductions

`Account Balances` plots each account's forecast balance: the entered balance plus the income assumed received and less the expenses assumed paid on each day.

The `Include` control subtracts standing obligations from that line. It appears only while `Account Balances` is selected, and its label states the current basis (`Balance less: Arrears` with the defaults, or `Balance` when nothing is subtracted). Open it to toggle three deductions:

- **Reserved** — the account's reserved amount (funds set aside, such as an emergency fund).
- **Accruals** — what is being set aside for upcoming bills, net of the amounts the day's assumed payments already cover. A bill due today is the current bill: it is already deducted from the balance, so it is not subtracted a second time by this toggle.
- **Arrears** — one billed amount for every past-due, un-settled occurrence. Arrears is held for every day of the window, because the forecast makes no assumption about when you catch up, and it is not deducted from the underlying balance line.

Defaults: `Reserved` off, `Accruals` off, `Arrears` on, so a first load plots the balance net of past-due debt. The three toggles apply to every visible series — each account and the combined total alike.

With all three off the line is the plain forecast balance. With all three on it is the balance less reserved, the net accrual and arrears, which is the most conservative reading. The individual combinations let you isolate one obligation at a time.

Because the chart assumes the window's payments happen on their due dates, these figures can differ from the `Available` and `Committed` columns on the accounts page, which are measured on the entered balance without those assumptions.

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

- One picker offering every whole month from 1 to 12 (`6 months`, `1 month`).
- The control shows the current value; its accessible name is `Select chart period in months`.
- 12 shows the whole fetched window and 1 trims it to the first month.
- Controls the rendered time window from the selected start date; the chart updates immediately and does not refetch.

### Accounts (series visibility)

- The group is labelled `Accounts` and holds one toggle per account.
- `Total (All Accounts)` is a separate toggle beside the group, because it is a combined total rather than an account.
- Hidden series are removed from chart rendering.
- The Y-axis domain, and the low/high read-out, are recalculated from visible series only.

## Mobile Layout

- On small screens the whole filter bar — View, From, Period, the Include control and the Accounts legend — collapses behind a single full-width `Options` / `Hide options` button.
- From the medium breakpoint up the controls are always shown inline.
- The `Include` control states the current basis in its own label, so the plotted basis is clear whether the disclosure is open or closed.

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
- The period's visible low and high are marked as two horizontal reference lines, each labelled with its value at the left of the plot. The low label sits below its line and the high label above its own.
- The values are the lowest and highest across every visible series in the displayed period, so hiding a series can move them.
- The read-out is shown for line metrics only, and is hidden when there is no data or every series is hidden.

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
  - No data available card is shown when there are no non-zero values for the selected metric in the returned projection dataset.
  - When data exists but every toggle in the Accounts legend is off (including `Total (All Accounts)`), the plot is replaced by a `No account selected` message.

## Persistence Behavior

Projection preferences are stored under the authenticated user's scoped projections key.

Storage behavior:

- The page reads `sessionStorage` first, then falls back to `localStorage`, then defaults.
- If a tab has no session entry yet but local storage has saved projection state, that saved state seeds the tab and is copied into `sessionStorage`.
- Updates mirror the full projection state to both stores so a fresh tab can start from the most recently used settings while already-open tabs can continue independently.

Persisted values:

- metric
- period
- include (the Reserved, Accruals and Arrears toggles)
- hiddenSeries
- startDate when explicitly set to a non-today date

Period persistence rules:

- The selected 1 to 12 month period is persisted as `period`.
- On revisit, the previously selected period value is restored.

Start date persistence rules:

- If the user sets start date to today, startDate is removed from storage.
- When startDate is removed, the remaining projection preferences are retained in both stores.
- If a stored start date is before today on load, it is removed and replaced with today in page state.

## Step-By-Step Usage

1. Open Projections page.
2. Select a View metric.
3. Set From date.
4. Set Period by choosing a month from 1 to 12.
5. For Account Balances, open Include and choose which of Reserved, Accruals and Arrears to subtract.
6. Use the Accounts toggles to hide or reveal account series (and the separate Total (All Accounts) toggle).
7. Hover chart points or bars to inspect values.
8. For Income or Expenses metrics, click a bar to open date details.
9. In the detail sheet:

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
