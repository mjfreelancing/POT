# POT Implementation Documentation

This document provides detailed technical documentation for the POT (Personal Organization Tool) codebase, explaining complex business logic, calculations, and architectural patterns.

---

## Table of Contents

- [Pot.App Project](#potapp-project)
  - [AccrualCalculator](#accrualcalculator)
  - [Renewal Calculators](#renewal-calculators)
  - [ProjectionsService](#projectionsservice)

---

## Pot.App Project

This section documents the core application logic layer (`Pot.App`), including calculators, services, and business rules for financial projections.

---

## AccrualCalculator

**Location:** `Pot.App/Calculators/AccrualCalculator.cs`

### Purpose

Calculates expense accruals for an as-of date by measuring how much of each expense has accumulated (but not yet been paid) from its accrual start date, and how much of the schedule is already past due.

Nothing is persisted and no entity is mutated: the calculator takes the accrual-relevant facts plus the schedule cursor for each expense and returns an immutable `AccountAccrualView`. Every accrual-bearing read (`/api/accounts`, `/api/accounts/{id}`, `/api/expenses`, `/api/projections`) obtains its values from this one engine for the same as-of date, so the values read are always the current ones.

The accrual system provides a realistic view of "available" funds by accounting for expenses that are gradually building up but haven't been paid yet, and by carrying the cycles already behind as arrears. This prevents overspending by showing how much money is truly available after considering upcoming and past-due obligations.

### Core Concepts

#### 1. Accrual Period

Each expense has an `AccrualStart` date and a `NextDue` date. The expense gradually accumulates from start to due date.

#### 2. Daily Accrual Rate

When an expense period begins, we calculate a fixed daily rate:

```
DailyAccrual = Amount / (days from AccrualStart to NextDue)
```

**Example:** $300 expense over 30 days = $10/day

This rate remains constant throughout the accrual period.

#### 3. Accumulated Accrual

The amount accrued so far:

```
Accrued = DailyAccrual × (days from AccrualStart to currentDate)
```

**Example:** After 15 days, Accrued = $10/day × 15 days = $150

On the due date, `Accrued` is set to the full `Amount` regardless of how many days have passed.

### Due Date Behavior

When `currentDate == NextDue`:

- `Accrued` is set to the full `Amount` (regardless of how many days have passed)
- This represents the expense being "fully due" even if not yet paid
- For projections, we consider the expense "paid" on this date (it gets subtracted from Balance)
- For recurring expenses: we calculate the `DailyAccrual` rate for the NEXT period
- For one-time expenses: no further accrual happens after the due date

### Renewal Logic (Recurring Expenses)

When a recurring expense is due, it immediately begins accruing for the next period:

1. The `ExpenseRenewalCalculator` advances `NextDue` to the next occurrence
2. `AccrualStart` is set to the old `NextDue` (the date it was just paid)
3. We calculate `dailyAccrual` for the new period: `Amount / daysToNextDue`
4. That rate contributes to the account's `DailyExpenseAccrual` total (the dynamic projection metric), which the next read derives from the advanced cursor rather than from a stored value

**Example:** Monthly rent of $1200 due Jan 31:

- On Jan 31: `Accrued = $1200` (full amount), expense is "paid"
- `NextDue` advances to Feb 28 (28 days away - see [Month-end drift](#month-end-drift) under [Renewal Calculators](#renewal-calculators))
- New `dailyAccrual = $1200 / 28 = $42.86/day`
- On Feb 1: `Accrued = $42.86` (1 day × $42.86/day)

⚠️ **Note:** This expense will remain on the 28th for all subsequent months due to month-end drift (see [Renewal Calculators](#renewal-calculators)).

### One-Time Expense Handling

One-time expenses (`Frequency.OneTime`) do not renew:

- They accrue from `AccrualStart` to `NextDue` like any other expense
- On the due date, `Accrued = full Amount`
- After the due date, no further accrual occurs (`DailyExpenseAccrual` is NOT updated)
- In practice, one-time expenses should be deleted after they're paid, or excluded from future calculations

### Account Totals

The calculator returns five account-level amounts:

1. **TotalExpenseAccrued:** Sum of each expense's accrual for the cycle in progress (what has accumulated so far)
2. **TotalArrears:** One `Amount` for every past-due, un-settled occurrence. This follows the schedule rather than the accrual policy, so an `AccrualPolicy.None` row or one with a null `AccrualStart` still carries it.
3. **TotalCommitted:** `TotalExpenseAccrued + TotalArrears` - the headline obligation, rendered as the `Committed` column on the accounts table
4. **DailyExpenseAccrual:** Sum of all daily accrual rates (total rate of accrual across all expenses; the dynamic projection metric)
5. **StableExpenseAccrual:** Sum of stable per-expense daily contributions (long-run daily funding requirement)

The accounts read composes Available from the committed total:

```
Available = Balance - Reserved - TotalCommitted
          = Balance - Reserved - TotalExpenseAccrued - TotalArrears
```

The projections read no longer publishes a derived `Available`; it returns the running forecast `balance` and the components the chart subtracts (`reserved`, `arrears` and `unpaidAccrual`, where `unpaidAccrual` is the day's accrual net of the payments its own rows settle), and the client composes `balance - reserved - unpaidAccrual - arrears`. See [CRITICAL Formula - Available Funds](#critical-formula---available-funds) for the derivation.

A bill due today is the current bill: it accrues in full and is **not** counted as arrears.

### StableExpenseAccrual

`StableExpenseAccrual` is intentionally different from `DailyExpenseAccrual`:

- `DailyExpenseAccrual` is operational and date-sensitive.
- `StableExpenseAccrual` is long-run and intended for stable funding guidance.

Current implementation behavior:

1. Policy gate: expenses with `AccrualPolicy.None` contribute 0 to accrued, daily, and stable totals.
2. One-time expenses: contribute before due date only, using fixed period days (`NextDue - AccrualStart`).
3. Recurring expenses: contribute using average period days via `Frequency.GetAverageDaysToNext(FrequencyCount)`.
4. Recurring end-date rule: contribution stops after `EndDate` (inclusive on end date itself).

### Payment-Day Convention

For projection and accrual semantics, expense due date is treated as payment date.

1. Due-day expense amount is reflected in balance movement for that date.
2. Due-day accrued handling uses full due amount semantics in the current cycle.
3. Recurring obligations then continue into the next cycle via renewal rules.

This keeps due-date cashflow behavior explicit and aligned with available-balance calculations.

### Near-Term Coverage Interpretation

Operationally, short-term obligations can create immediate funding pressure that is separate from the long-run stable daily target.

When documenting or presenting behavior, distinguish:

1. One-off near-term catch-up requirement.
2. Ongoing stable daily funding requirement.

### Domain Glossary Mapping

Use the following terms consistently across code comments and docs:

- `TotalExpenseAccrued`: accrued obligations.
- `TotalArrears`: past-due obligations (one `Amount` per missed, un-settled cycle).
- `TotalCommitted`: accrued plus past-due obligations; labelled `Committed` in the accounts table.
- `DailyExpenseAccrual`: marginal accrual rate (dynamic operational metric).
- `StableExpenseAccrual`: daily funding rate (stable planning metric).
- `Available`: available balance (spendable amount after the reserved and committed amounts).

### Rounding

All accrual calculations use:

```csharp
Math.Round(value, 2, MidpointRounding.AwayFromZero)
```

This ensures consistent financial rounding to 2 decimal places.

### Expense Sorting

Expenses are sorted by `NextDue` (ascending) before processing. While not critical in the current implementation, this ordering would be important if future enhancements prevent accounts from going negative (expenses due sooner would be processed first).

### Excluded Expenses

Expenses with `ExcludeFromCalcs = true` are absent from the accounts and projection queries, so they contribute nothing to any account total. They still appear in the expense-read detail shape with `Accrued = 0` and `Arrears = 0`, because the expenses list renders a value for every row.

Expenses with `AccrualPolicy.None` contribute 0 to `TotalExpenseAccrued`, `DailyExpenseAccrual` and `StableExpenseAccrual`, but they still carry arrears when their schedule is behind.

---

## Renewal Calculators

**Locations:** `Pot.App/Calculators/ExpenseRenewalCalculator.cs` and `IncomeRenewalCalculator.cs`, with the renewal rules in the pure `ExpenseRenewalFold` and `IncomeRenewalFold`.

Expenses and income renew by the same rules, so they are documented once. The calculator adapts the entity graph to the fold and writes the folded cursor back, because a schedule advance is a fact worth persisting; the fold holds the rules and mutates nothing.

### Purpose

Advance a recurring item to its next due date, simulating the passage of time as items are settled and rescheduled. Renewal moves the schedule only: it changes no balance, and for an expense it moves the accrual cursor rather than computing an accrual.

### Expenses and income differ in three ways

| Aspect            | Expense                                                                                         | Income                      |
| ----------------- | ----------------------------------------------------------------------------------------------- | --------------------------- |
| Schedule carried  | `NextDue` and `AccrualStart`                                                                    | `NextDue` only              |
| Accrual start     | The period boundary just entered for `AccrualPolicy.Automatic`, `null` for `AccrualPolicy.None` | None - income never accrues |
| Projection effect | The amount is subtracted on the due date                                                        | The amount is added         |

Frequency, end dates, the timing rule, one-time items and excluded items behave identically.

### Renewal modes

Both calculators take a `RenewalMode`:

- **`RenewalMode.Future`** advances an item exactly once, so a user can settle a future occurrence early - marking March rent as paid in February, or an expected March salary as received. A renewed expense's `AccrualStart` becomes the as-of date, because the period it has just entered began when it was settled.
- **`RenewalMode.Overdue`** advances repeatedly while the next due date is on or before the as-of date. It is the mode the projection loop uses, and the one a manual renew uses to catch up.

### Frequency calculation

Days to the next occurrence come from `FrequencyExtensions.GetDaysToNext`:

- **Days:** `FrequencyCount` days (every 3 days = 3)
- **Weeks:** `FrequencyCount × 7` days (bi-weekly = 2 × 7 = 14)
- **Months:** `DateOnly.AddMonths(frequencyCount)`
- **EndOfMonth:** `DateOnly.AddMonths(frequencyCount)`, then the last day of that month (Jan 31 → Feb 28 → Mar 31 → Apr 30), so it does not suffer the drift below
- **Years:** `DateOnly.AddYears(frequencyCount)`
- **OneTime:** has no next occurrence and is rejected before this point

For example, `Weeks` × 2 is bi-weekly, `Months` × 1 monthly, `Months` × 3 quarterly, `Months` × 6 semi-annually and `Years` × 1 annually.

### Month-end drift

A monthly item due on the 29th, 30th or 31st that renews through a shorter month lands on the last valid day of that month and never returns to its original day:

1. **Jan 31** → Feb 28 (February has 28 days in a non-leap year)
2. **Feb 28** → Mar 28 (adding a month from the 28th keeps the 28th)
3. **Mar 28** → Apr 28, and every later month
4. **Stuck on the 28th** - it never returns to the 31st

`DateOnly.AddMonths` clamps to the last valid day, and every later addition starts from the clamped date. An item starting on the 30th drifts the same way, and one starting on the 29th drifts to the 28th in a non-leap year. A user who needs the original day must correct the next due date once the drift happens; `EndOfMonth` is the frequency for a schedule that should stay on the month end.

Covered by `Should_Handle_Month_End_Dates_Starting_Jan_31_With_Multiple_Renewals` and `Should_Handle_Month_End_31st_Renewing_Twice_Through_February`, in both the expense and income fixtures.

### Leap-year drift

A yearly item due on 29 February drifts to 28 February at the first non-leap year and never returns, even in a later leap year:

1. **29 Feb 2024** → 28 Feb 2025 (2025 has no 29 February)
2. **28 Feb 2025** → 28 Feb 2026 (adding a year from the 28th keeps the 28th)
3. **28 Feb 2027** → 28 Feb 2028, a leap year that still stays on the 28th

`DateOnly.AddYears` clamps for the same reason, and later additions start from the clamped date. Covered by `Should_Handle_Leap_Year_In_Yearly_Frequency` in both the expense and income fixtures.

### Timing rule: an item due on the as-of date is renewed

`RenewalMode.Overdue` advances every occurrence on or before the as-of date, and that includes one due exactly on it. What the caller must not do is fold the renewal before settling that occurrence: the projection loop measures the day's due rows against the cursor as it stands at the start of the day and folds the renewal in afterwards (`AccountProjectionState`), so the payment is counted on its own date and the schedule then moves on. A caller that folds first would measure the item against the next period.

For a monthly rent due Jan 31, the projection reads:

- Jan 30: `NextDue = Jan 31`, nothing is due and nothing advances
- Jan 31: the rent is paid, then `NextDue` advances to Feb 28
- Feb 1: `NextDue = Feb 28`, nothing is due

Covered by `Should_Advance_Expense_Due_Today`, `Should_Advance_Income_Due_Today` and the fold fixtures.

### End dates

An item may carry an optional `EndDate`. Once `NextDue` has reached it the item does not renew, and a step that would land beyond it is not taken, so the schedule stops at the last occurrence inside the window:

- The renewal is refused outright when `NextDue >= EndDate`
- Inside the loop, an advance to a date after `EndDate` breaks out and the item keeps its current `NextDue`

For a monthly subscription ending March 31 that started on Jan 31: February renews it to Mar 28, and the March renewal would land on Apr 28, so it stops at `NextDue = Mar 28`. The item is still paid on that date; it simply never advances again. It also shows the [month-end drift](#month-end-drift) above - the original 31st became the 28th.

### One-time and excluded items

An item with `Frequency.OneTime`, or one with `ExcludeFromCalcs` set, does not renew; the fold returns the cursor unchanged. A one-time item is expected to be removed once it is settled, or left for the other calculations to ignore. An excluded item takes no part in projections or accruals, so its schedule never needs to move.

### Loop behaviour

`RenewalMode.Overdue` loops until the next due date is past the as-of date. It is a loop rather than a single step because an item can be several occurrences behind - a weekly expense three weeks overdue - and because a projection walks a long window, in which a monthly item renews twelve times over a year of forecast days. The calculator is called for each projected day, so the schedule advances as the window walks and each balance is updated as items are settled on their due dates. `RenewalMode.Future` never loops.

### Accrual after renewal (expenses)

Renewal writes the schedule and nothing else: `NextDue` advances, and `AccrualStart` becomes the period boundary the item has just entered, or `null` when `AccrualPolicy` is `None`. Renewal moves the cursor the accrual calculation reads, so the next read reports the new cycle from the advanced schedule.

---

## ProjectionsService

**Location:** `Pot.App/Features/Projections/ProjectionsService.cs`

### Purpose

Orchestrates financial projections by simulating day-by-day account balance changes based on scheduled income and expenses, providing a realistic forecast of future financial positions.

The `ProjectionsService` generates forward-looking financial projections by:

1. Starting from current account balances
2. Applying scheduled income and expenses for each projected day
3. Tracking expense accruals to show "available" funds (accounting for upcoming obligations)
4. Aggregating individual account projections into a global view

This provides users with a realistic picture of their future financial position, helping them make informed decisions about spending and saving.

### Calculation Flow

For each day in the projection period:

1. **Identify due items:** Find expenses and income due on this date
2. **Calculate totals:** Sum income received and expenses paid
3. **Accrue expenses:** Derive accumulated expenses, arrears and daily accrual rates from the start-of-day cursor
4. **Renew items:** Advance recurring expenses/income to their next due dates
5. **Update balance:** Apply income and expenses to account balance
6. **Record projection:** Store date, balance, reserved, arrears, unpaidAccrual, and transaction details

`DailyAccrual` in projection output is the dynamic operational metric. It is expected to vary during a cycle.

### CRITICAL Formula - Available Funds

The "Available" amount shows how much money is truly available for spending after accounting for upcoming obligations. The formula is:

```
Available = Balance - Reserved - Accrued - Arrears + ExpensesPaid
```

> **Update (2026-10-03):** the projections read no longer publishes a derived `Available`. It publishes the running forecast `balance` and the components the client subtracts — `reserved`, `arrears` and `unpaidAccrual` (`Accrued` less the accrual the day's assumed payments settle) — and the chart composes `balance - reserved - unpaidAccrual - arrears`. The derivation below still explains why the settled part of the day's accrual is added back. The accounts read keeps its own `Available` (`Balance - Reserved - TotalCommitted`) and is unchanged.

#### Breaking Down Each Component

- **Balance:** Current account balance INCLUDING the effect of expenses paid today (if any). When an expense is due, we subtract it from Balance to reflect the "payment".

- **Reserved:** Funds set aside and not available for spending (e.g., emergency fund, savings goals). This is a fixed amount per account.

- **Accrued:** Total of all accrued values, representing how much of future expenses has accumulated but not yet been paid. This INCLUDES expenses due today (which accrue their full `Amount`).

- **Arrears:** One `Amount` for every occurrence whose due date has passed un-settled. It is held on every day of the window rather than released like the cycle in progress, and it is not deducted from the forecast `Balance`, so it lowers `Available` only.

- **ExpensesPaid:** Total expenses paid TODAY, counted only for the rows that contributed their full amount to `Accrued`. This is ADDED BACK to prevent double-counting.

#### WHY Add Back ExpensesPaid?

This is the most subtle part of the formula. On a day when an expense is due:

1. The expense is "paid", so we subtract it from Balance:

   ```csharp
   account.Balance -= expense.Amount
   ```

2. The expense is fully accrued (`Accrued = Amount`) because it's due today

3. If we calculate Available as: `Balance - Reserved - Accrued`, we would DOUBLE-COUNT the expense:
   - Once in the reduced Balance (expense was subtracted)
   - Once in the Accrued amount (expense is fully accrued)

4. By adding back `ExpensesPaid`, we correct for this double-counting:
   ```
   Available = (Balance - expense) - Reserved - (Accrued including expense) + expense
   Available = Balance - Reserved - Accrued (the expense cancels out)
   ```

#### EXAMPLE - Available Calculation

**Scenario:** Account with $5000 balance, $1000 reserved, $800 rent due today

**Before rent payment:**

- Balance: $5000
- Accrued: $800 (rent is fully accrued)
- Available: $5000 - $1000 - $800 = $3200

**On rent payment day (AFTER payment is processed):**

- Balance: $4200 (rent subtracted)
- Accrued: $800 (still shows full amount because expense hasn't been "renewed" yet)
- ExpensesPaid: $800
- Available WITHOUT adding back: $4200 - $1000 - $800 = $2400 — WRONG! Double-counted rent
- Available WITH adding back: $4200 - $1000 - $800 + $800 = $3200 — CORRECT! Same as before payment

The Available amount should remain constant on the payment day, just like in reality - paying a bill you've been saving for doesn't change how much discretionary money you have.

### Pre-Start Days

Projections can start in the future (e.g., project starting next week). To ensure accuracy:

1. Calculate "pre-start days" from today to the start date
2. Process all days from today through the projection period
3. Only record projections for days on or after the start date
4. This ensures income/expenses between now and start date are properly accounted for

**Example:** Today is Jan 15, projection starts Jan 20, forecast 30 days:

- Process days: Jan 15-Feb 18 (5 pre-start + 30 projection)
- Record days: Jan 20-Feb 18 (30 days)
- This captures any income/expenses due Jan 15-19 that affect Jan 20's starting balance

### Due Date Logic

An expense or income is considered due on a specific date if:

```csharp
entity.NextDue == date && (entity.EndDate ?? DateOnly.MaxValue) >= date
```

This means:

- `NextDue` must exactly match the current date
- The item must not have reached its `EndDate` (if specified)

Items are processed (balance updated) on their due date, THEN renewed to the next occurrence. This order is critical for accurate projections.

### Balance Updates

After processing all due items for a day, the account balance is updated:

```csharp
account.Balance += incomeReceived - expensesPaid
```

This modified balance is used as the starting balance for the next day's projection.

### Combined Total

The service publishes only the per-account projections; it no longer computes a global (all-accounts) series.

> **Update (2026-10-04):** the chart's combined series is now `Total (Selected Accounts)`. The client composes it by summing the accounts currently shown in the legend, for the selected metric, so the response carries no `global` member.

### Why Projection DailyAccrual Varies

Variation is expected in the current implementation:

1. Due-day transitions switch an expense from current-cycle remaining-balance behavior to next-cycle period behavior.
2. Calendar-driven period lengths vary for monthly and yearly schedules.
3. Mixed frequencies and staggered due dates produce a moving aggregate.

Use this metric for operational simulation interpretation, not as a stable daily set-aside target.

### Transaction Details

Each projected day includes:

- **ExpenseItems:** List of expenses paid (with description and amount)
- **IncomeItems:** List of income received (with description and amount)

This allows users to see exactly what transactions occur on each date, not just totals.

### Calculator Integration

The service orchestrates three calculators:

1. **ExpenseRenewalCalculator:** Advances recurring expenses to next due dates using `RenewalMode.Overdue`
2. **IncomeRenewalCalculator:** Advances recurring income to next due dates using `RenewalMode.Overdue`
3. **AccrualCalculator:** Derives the day's accrued, arrears and committed figures for each account

These are called in sequence for each projected day. Renewal advances in-memory cursors and the accrual calculation reads them; no accrual value is written back to the database.

**Note:** ProjectionsService always uses `Overdue` mode because it's simulating the passage of time day by day. The `Future` mode is used by the Renew services when users manually mark future items as paid/received early.

### Excluded Items

The repository only returns income/expenses where `ExcludeFromCalcs = false`. Items marked as excluded are not considered in projections at all.

### Projection Period

The projection period is specified by:

- **StartDate:** First day to include in projection results
- **DaysForecast:** Number of days to project from StartDate

**Example:** StartDate = Jan 20, DaysForecast = 30 produces projections from Jan 20 to Feb 18.

### Data Flow

1. Repository loads accounts with their income/expenses (excludes `ExcludeFromCalcs = true`)
2. Service creates dictionaries to track account expenses and income
3. For each day, service processes all accounts:
   - Find due items
   - Apply renewals
   - Calculate accruals
   - Update balances
   - Record projections
4. After all days processed, return the per-account projections (there is no global series; the client sums the selected accounts)

### Stateful Processing

The projection process is stateful:

- Account balances are modified as we progress through days
- Expense `NextDue` dates advance as they're renewed
- Income `NextDue` dates advance as they're renewed
- The day's `accrued`, `arrears` and `unpaidAccrual` are derived on each day from the current cursor; no accrual value is written back

⚠️ **Warning:** This means running projections modifies the entity data so do not persist it back to the repository. Cloned models could be used but there is no need for that at this time.

### Validation

The service validates that `StartDate` is not before today:

```csharp
Throw<InvalidOperationException>.When(localDate > startDate, "Projections cannot start earlier than today")
```

This prevents attempting to project the past, which would produce incorrect results since historical transactions aren't captured in the projection system.

---
