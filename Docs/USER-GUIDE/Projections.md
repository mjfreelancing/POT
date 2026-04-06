# Financial Projections

## Overview

Projections help you inspect future balance movement and drill into income/expense event days.
The chart supports account-level filtering while preserving a global total series.

## Available to

- Site Owners
- Admins
- Viewers

## Key Features

- Date-range projection view (chart and tooltip)
- Account series visibility toggles
- Total series (`Total (All Accounts)`) for all-account context
- Day detail sheet for income and expense bars

## Income/Expense Bar Interaction Rules

These rules apply to both income and expense bar metrics.

1. Hover behavior

- Hover an individual account bar: tooltip shows that account value only.
- Hover the total bar (`Total (All Accounts)`): tooltip shows all account values for that date, including accounts that are currently hidden in legend filters.

2. Click behavior

- Click an individual account bar: detail sheet is scoped to that account's items.
- Click the total bar: detail sheet shows all accounts/items for that date.

3. Detail sheet totals

- `Total Income` / `Total Expenses` always reflect the full scoped set for the clicked context.
- `Filtered Total` appears only when some rows are hidden in the detail sheet context.

4. Item count label semantics

- Show `N items` when all relevant rows are visible.
- Show `X of N items` when the current view is a subset (for example, account-scoped details against a larger day total).

This is intentional and helps distinguish:

- what is currently visible (`X`)
- what exists on that date in total (`N`)

## Step-by-Step Guide

1. Open the Projections page and select an income or expense metric.
2. Hover a bar to inspect date values in the tooltip.
3. Click an account bar to inspect only that account's items.
4. Click the total bar to inspect all account items for the date.
5. In the detail sheet, read the item badge:

- `N items` means fully expanded for that context.
- `X of N items` means the view is intentionally scoped.

## Permission-Based Features

### For All Users

- View projection chart
- Hover bars for tooltip breakdown
- Open day-level detail sheets

### For Site Owners & Admins

- Same projections interactions as viewers

Note: projections interactions are read-only across roles.

## Tips & Best Practices

- Use account-bar click when reconciling one account.
- Use total-bar click when auditing all items due/received on a date.
- If item counts look unexpected, compare `Filtered Total` and the `X of N items` badge to confirm current scope.

---

[Back to User Guide](README.md)
