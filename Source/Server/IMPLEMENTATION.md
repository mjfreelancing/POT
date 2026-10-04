# POT Implementation Documentation

This document provides detailed technical documentation for the POT (Personal Organization Tool) codebase, explaining complex business logic, calculations, and architectural patterns.

---

## Table of Contents

- [Pot.App Project](#potapp-project)
  - [AccrualCalculator](#accrualcalculator)
  - [ExpenseRenewalCalculator](#expenserenewalcalculator)
  - [IncomeRenewalCalculator](#incomerenewalcalculator)
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
Accrued = DailyAccrual � (days from AccrualStart to currentDate)
```

**Example:** After 15 days, Accrued = $10/day � 15 days = $150

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
- `NextDue` advances to Feb 28 (28 days away - see [Month-End Date Behavior](#month-end-date-behavior) in ExpenseRenewalCalculator)
- New `dailyAccrual = $1200 / 28 = $42.86/day`
- On Feb 1: `Accrued = $42.86` (1 day � $42.86/day)

⚠️ **Note:** This expense will remain on the 28th for all subsequent months due to month-end date drift (see [ExpenseRenewalCalculator](#expenserenewalcalculator) documentation).

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

## ExpenseRenewalCalculator

**Location:** `Pot.App/Calculators/ExpenseRenewalCalculator.cs`

### Purpose

Advances recurring expenses to their next due date, simulating the passage of time as expenses are "paid" and renewed for the next billing period.

The renewal calculator is responsible for moving recurring expenses forward in time. When an expense becomes due, it needs to be "paid" and then scheduled for the next occurrence. This calculator handles the scheduling logic without modifying balances or accruals (those are handled separately).

### Core Logic

The renewal calculator accepts a `RenewalMode` parameter that determines the renewal behavior:

**Future Mode** (for advancing future items to mark as paid early):

- Advances each expense exactly ONCE to its next period
- Does not use a while loop - single advancement only
- Used when user wants to manually mark future items as "paid early"
- Example: Marking March rent as paid in February

**Overdue Mode** (for catching up overdue items or projections):

- Advances expenses repeatedly until caught up with `asOfDate`
- Uses a while loop: `while (nextDue <= asOfDate)`
- Used for automatic renewal of overdue items and for projections
- Example: Advancing weekly expense that's 3 weeks overdue

For each expense (regardless of mode):

1. Calculate days to next occurrence based on `Frequency` and `FrequencyCount`
2. Add those days to `NextDue` to get the new due date
3. Update `AccrualStart` to the old `NextDue` (expense starts accruing from when it was paid)
4. For Overdue mode: Repeat until `NextDue` is in the future (beyond `asOfDate`)

### Frequency Calculation

The number of days to add is determined by `Frequency` and `FrequencyCount`:

- **Days:** `FrequencyCount` days (e.g., every 3 days = 3)
- **Weeks:** `FrequencyCount � 7` days (e.g., bi-weekly = 2 � 7 = 14)
- **Months:** Uses .NET's `DateOnly.AddMonths()` - see [Month-End Date Behavior](#month-end-date-behavior) below
- **Years:** Uses .NET's `DateOnly.AddYears()` - see [Leap Year Date Behavior](#leap-year-date-behavior-yearly-frequency) below

**Examples:**

- `Frequency.Weeks`, `FrequencyCount=2`: Every 2 weeks (bi-weekly)
- `Frequency.Months`, `FrequencyCount=1`: Monthly
- `Frequency.Months`, `FrequencyCount=3`: Quarterly
- `Frequency.Months`, `FrequencyCount=6`: Semi-annually
- `Frequency.Years`, `FrequencyCount=1`: Annually

### Month-End Date Behavior

⚠️ **IMPORTANT:** Monthly frequencies exhibit date drift behavior due to .NET's `AddMonths` implementation.

**The Problem:**
When an expense due on the **29th, 30th, or 31st** renews through a shorter month (like February), the date permanently drifts to the last valid day of that month and **never returns to the original day**.

**Example: Expense due on January 31st**

1. **Jan 31** ? Feb 28 (Feb only has 28 days in non-leap years)
2. **Feb 28** ? Mar 28 (AddMonths from 28th stays on 28th)
3. **Mar 28** ? Apr 28 (continues on 28th)
4. **Forever stuck on 28th** - will never return to 31st

**Why This Happens:**

- `FrequencyExtensions.GetDaysToNext()` uses `DateOnly.AddMonths(frequencyCount)`
- When adding months to Jan 31, .NET returns Feb 28 (the last valid day)
- Subsequent additions are from Feb 28, so the date stays on 28th

**Real-World Impact:**

- An expense starting on Jan 31 will be due on the 28th for all subsequent months (except Jan)
- An expense starting on Jan 30 drifts to Feb 28, then stays on 28th
- An expense starting on Jan 29 drifts to Feb 28 in non-leap years, then stays on 28th

**Tested and Documented:**
This behavior is validated by comprehensive tests:

- `ExpenseRenewalCalculatorFixture.Should_Handle_Month_End_Dates_Starting_Jan_31_With_Multiple_Renewals`
- `ExpenseRenewalCalculatorFixture.Should_Handle_Month_End_31st_Renewing_Twice_Through_February`

**User Guidance:**
Users should be aware that monthly expenses/income starting on the 29th-31st will shift to the 28th after February. The user will need to manually update the next due date when this occurs.

### Leap Year Date Behavior (Yearly Frequency)

⚠️ **IMPORTANT:** Yearly frequencies exhibit date drift behavior when renewing from leap year dates (Feb 29).

**The Problem:**
When an expense due on **February 29th** (leap year) renews to a non-leap year, the date permanently drifts to February 28th and **never returns to February 29th** even in subsequent leap years.

**Example: Expense due on February 29, 2024 (leap year)**

1. **Feb 29, 2024** ? Feb 28, 2025 (2025 is not a leap year, so Feb 29 doesn't exist)
2. **Feb 28, 2025** ? Feb 28, 2026 (AddYears from 28th stays on 28th)
3. **Feb 28, 2026** ? Feb 28, 2027 (continues on 28th)
4. **Feb 28, 2027** ? Feb 28, 2028 (2028 IS a leap year, but stays on 28th)
5. **Forever stuck on Feb 28** - will never return to Feb 29, even in leap years

**Why This Happens:**

- `FrequencyExtensions.GetDaysToNext()` uses `DateOnly.AddYears(frequencyCount)`
- When adding years to Feb 29, 2024, .NET returns Feb 28, 2025 (the last valid day in non-leap years)
- Subsequent additions are from Feb 28, so the date stays on 28th permanently

**Real-World Impact:**

- An expense starting on Feb 29 will be due on Feb 28 for all subsequent years (including leap years)

**Tested and Documented:**
This behavior is validated by tests:

- `ExpenseRenewalCalculatorFixture.Should_Handle_Leap_Year_In_Yearly_Frequency`
- `IncomeRenewalCalculatorFixture.Should_Handle_Leap_Year_In_Yearly_Frequency`

**User Guidance:**
Users should be aware that yearly expenses/income starting on Feb 29 will permanently shift to Feb 28 after the first renewal. The user will need to manually update the next due date when this occurs.

### ⚠️ CRITICAL Timing Rule

**Expenses are NOT renewed if `NextDue == asOfDate`**

The while loop condition is: `while (nextDue <= asOfDate)`

But expenses due EXACTLY on `asOfDate` are skipped because:

- They are considered "still due" on that date
- For projections, they need to be paid (subtracted from Balance) on that date
- If we advanced them before processing the payment, the projection would be incorrect
- They will be renewed on the NEXT day's processing

**Example timeline for monthly rent due Jan 31:**

- Jan 30 processing: `NextDue = Jan 31` (future), no renewal
- Jan 31 processing: `NextDue = Jan 31` (today), **SKIP renewal** (expense is paid on this date)
- Feb 1 processing: `NextDue = Jan 31` (past), **NOW advance** to Feb 28

### End Date Handling

Expenses can have an optional `EndDate`. When present:

- Renewal stops if `NextDue` would advance beyond `EndDate`
- Check: `if (nextDue <= endDate)` before updating `NextDue`
- Once `NextDue >= EndDate`, the expense is effectively "finished" and won't renew again

**Example:** Monthly subscription ending March 31, originally starting on Jan 31

- Feb 28: renews to Mar 28 (within end date) - **already drifted from 31st to 28th**
- Mar 28: would renew to Apr 28, but Apr 28 > Mar 31, so **NO renewal**
- Expense remains at `NextDue = Mar 28`, will be paid but not renewed

⚠️ **Note:** This expense originally started on the 31st but drifted to the 28th after February (see [Month-End Date Behavior](#month-end-date-behavior) above).

### One-Time Expenses

Expenses with `Frequency.OneTime` are explicitly skipped:

```csharp
if (expense.Frequency == Frequency.OneTime) continue;
```

One-time expenses should not renew. After they're paid on their `NextDue` date, they should be deleted from the system, or excluded from future calculations (handled by other parts of the application).

### Excluded Expenses

Expenses with `ExcludeFromCalcs = true` are skipped:

```csharp
if (expense.ExcludeFromCalcs) continue;
```

These expenses are excluded from projections and accruals, so they don't need renewal logic.

### Accrual State After Renewal

When an expense is renewed:

1. `AccrualStart` is updated to the old `NextDue`
2. `NextDue` is advanced to the next occurrence

Nothing else is written for accrual purposes. Renewal moves the cursor the accrual calculation reads, so the next read reports the new cycle from the advanced schedule.

### Projection Usage

During financial projections, this calculator is called for each projected day:

1. Process all expenses due on or before current projection date
2. Advance any that are past due
3. This simulates the passage of time day by day
4. Each account's balance is updated as expenses are "paid" on their due dates

### Loop Behavior

The while loop (used in **Overdue mode only**) handles cases where multiple renewals are needed:

```csharp
while (nextDue <= asOfDate)
```

This can happen when:

- The system hasn't been updated in a long time (e.g., weekly expense that's 3 weeks overdue)
- Projections span long periods (e.g., projecting 365 days means monthly expenses renew 12 times)

The loop continues until `NextDue` is in the future relative to `asOfDate`.

**Note:** Future mode does NOT use a while loop - it advances exactly once.

---

## IncomeRenewalCalculator

**Location:** `Pot.App/Calculators/IncomeRenewalCalculator.cs`

### Purpose

Advances recurring income sources to their next due date, simulating the passage of time as income is received and scheduled for the next payment period.

The income renewal calculator is the counterpart to `ExpenseRenewalCalculator`, handling the scheduling of recurring income (salary, dividends, interest, etc.). When income becomes due, it needs to be renewed so it is scheduled for the next occurrence.

### Core Logic

The renewal calculator accepts a `RenewalMode` parameter that determines the renewal behavior:

**Future Mode** (for advancing future items to mark as received early):

- Advances each income exactly ONCE to its next period
- Does not use a while loop - single advancement only
- Used when user wants to manually mark future items as "received early"
- Example: Marking expected March salary as received in February

**Overdue Mode** (for catching up overdue items or projections):

- Advances income repeatedly until caught up with `asOfDate`
- Uses a while loop: `while (nextDue <= asOfDate)`
- Used for automatic renewal of overdue items and for projections
- Example: Advancing monthly salary that's 3 months overdue

For each income (regardless of mode):

1. Calculate days to next occurrence based on `Frequency` and `FrequencyCount`
2. Add those days to `NextDue` to get the new due date
3. For Overdue mode: Repeat until `NextDue` is in the future (beyond `asOfDate`)

### Key Difference from Expenses

Unlike expenses, income does NOT have:

- `AccrualStart` (income doesn't gradually accrue like expenses)
- `Accrued` tracking (no need to track partial income before it's received)
- `AccruedIsDirty` flag (no accrual recalculation needed)

Income is simpler: it either has been received (past due dates) or will be received (future due dates). There's no gradual accumulation or "available funds" adjustment needed for income.

### Frequency Calculation

Same as `ExpenseRenewalCalculator`, days to add are determined by `Frequency` and `FrequencyCount`:

- **Days:** `FrequencyCount` days
- **Weeks:** `FrequencyCount � 7` days (e.g., bi-weekly salary = 2 � 7 = 14)
- **Months:** Uses .NET's `DateOnly.AddMonths()` - see [Month-End Date Behavior](#month-end-date-behavior-1) below
- **Years:** Uses .NET's `DateOnly.AddYears()` - see [Leap Year Date Behavior](#leap-year-date-behavior-yearly-frequency-1) below

**Examples:**

- `Frequency.Weeks`, `FrequencyCount=2`: Bi-weekly paycheck (every 2 weeks)
- `Frequency.Months`, `FrequencyCount=1`: Monthly salary
- `Frequency.Months`, `FrequencyCount=3`: Quarterly dividend
- `Frequency.Years`, `FrequencyCount=1`: Annual salary review or bonus

### Month-End Date Behavior

⚠️ **IMPORTANT:** Monthly frequencies exhibit date drift behavior due to .NET's `AddMonths` implementation.

This is identical to the behavior in `ExpenseRenewalCalculator`. When income due on the **29th, 30th, or 31st** renews through a shorter month (like February), the date permanently drifts to the last valid day of that month and **never returns to the original day**.

**Example: Salary paid on January 31st**

1. **Jan 31** ? Feb 28 (Feb only has 28 days in non-leap years)
2. **Feb 28** ? Mar 28 (AddMonths from 28th stays on 28th)
3. **Mar 28** ? Apr 28 (continues on 28th)
4. **Forever stuck on 28th** - will never return to 31st

**Tested and Documented:**
This behavior is validated by tests:

- `IncomeRenewalCalculatorFixture.Should_Handle_Month_End_Dates_Starting_Jan_31_With_Multiple_Renewals`
- `IncomeRenewalCalculatorFixture.Should_Handle_Month_End_31st_Renewing_Twice_Through_February`

See the [Month-End Date Behavior](#month-end-date-behavior) section in [ExpenseRenewalCalculator](#expenserenewalcalculator) documentation for full details.

### Leap Year Date Behavior (Yearly Frequency)

⚠️ **IMPORTANT:** Yearly frequencies exhibit date drift behavior when renewing from leap year dates (Feb 29).

**The Problem:**
When income due on **February 29th** (leap year) renews to a non-leap year, the date permanently drifts to February 28th and **never returns to February 29th** even in subsequent leap years.

**Example: Annual bonus paid on February 29, 2024 (leap year)**

1. **Feb 29, 2024** ? Feb 28, 2025 (2025 is not a leap year, so Feb 29 doesn't exist)
2. **Feb 28, 2025** ? Feb 28, 2026 (AddYears from 28th stays on 28th)
3. **Feb 28, 2026** ? Feb 28, 2027 (continues on 28th)
4. **Feb 28, 2027** ? Feb 28, 2028 (2028 IS a leap year, but stays on 28th)
5. **Forever stuck on Feb 28** - will never return to Feb 29, even in leap years

**Why This Happens:**

- `FrequencyExtensions.GetDaysToNext()` uses `DateOnly.AddYears(frequencyCount)`
- When adding years to Feb 29, 2024, .NET returns Feb 28, 2025 (the last valid day in non-leap years)
- Subsequent additions are from Feb 28, so the date stays on 28th permanently

**Real-World Impact:**

- Income starting on Feb 29 will be received on Feb 28 for all subsequent years (including leap years)

**Tested and Documented:**
This behavior is validated by tests:

- `IncomeRenewalCalculatorFixture.Should_Handle_Leap_Year_In_Yearly_Frequency`

**User Guidance:**
Users should be aware that yearly income starting on Feb 29 will permanently shift to Feb 28 after the first renewal. The user will need to manually update the next due date when this occurs.

### ⚠️ CRITICAL Timing Rule

**Income is NOT renewed if `NextDue == asOfDate`**

The while loop condition is: `while (nextDue <= asOfDate)`

But income due EXACTLY on `asOfDate` is skipped because:

- It is considered "still due" on that date
- For projections, it needs to be received (added to Balance) on that date
- If we advanced it before processing the receipt, the projection would be incorrect
- It will be renewed on the NEXT day's processing

**Example timeline for bi-weekly salary due Jan 17:**

- Jan 16 processing: `NextDue = Jan 17` (future), no renewal
- Jan 17 processing: `NextDue = Jan 17` (today), **SKIP renewal** (income is received on this date)
- Jan 18 processing: `NextDue = Jan 17` (past), **NOW advance** to Jan 31 (14 days later)

### End Date Handling

Income can have an optional `EndDate` (e.g., contract work ending on a specific date):

- Renewal stops if `NextDue` would advance beyond `EndDate`
- Check: `if (nextDue <= endDate)` before updating `NextDue`
- Once `NextDue >= EndDate`, the income is effectively "finished" and won't renew again

**Example:** Contract work ending March 31, paid monthly on the 31st

- Jan 31: received, renews to Feb 28 (within end date) - **date drifts to 28th**
- Feb 28: received, renews to Mar 28 (within end date) - **stays on 28th**
- Mar 28: received, would renew to Apr 28, but Apr 28 > Mar 31, so **NO renewal**
- Income remains at `NextDue = Mar 28`, will be received but not renewed

⚠️ **Note:** This example demonstrates the month-end date drift - income starting on 31st permanently shifts to 28th after February (see [Month-End Date Behavior](#month-end-date-behavior-1) above).

### One-Time Income

Income with `Frequency.OneTime` is explicitly skipped:

```csharp
if (income.Frequency == Frequency.OneTime) continue;
```

One-time income (e.g., bonuses, tax refunds, gifts) should not renew. After it's received on its `NextDue` date, it should be deleted from the system, or excluded from future calculations (handled by other parts of the application).

### Excluded Income

Income with `ExcludeFromCalcs = true` is skipped:

```csharp
if (income.ExcludeFromCalcs) continue;
```

These income sources are excluded from projections, so they don't need renewal logic.

### Projection Usage

During financial projections, this calculator is called for each projected day:

1. Process all income due on or before current projection date
2. Advance any that are past due to their next occurrence
3. This simulates the passage of time day by day
4. Each account's balance is updated as income is "received" on due dates

### Loop Behavior

The while loop (used in **Overdue mode only**) handles cases where multiple renewals are needed:

```csharp
while (nextDue <= asOfDate)
```

This can happen when:

- The system hasn't been updated in a long time (e.g., monthly salary that's 3 months overdue)
- Projections span long periods (e.g., projecting 365 days means monthly income renews 12 times)

The loop continues until `NextDue` is in the future relative to `asOfDate`.

**Note:** Future mode does NOT use a while loop - it advances exactly once.

### Symmetry with Expenses

This calculator mirrors the logic of `ExpenseRenewalCalculator` but for income. The key structural differences are:

- No `AccrualStart` updates (income doesn't accrue)
- No `AccruedIsDirty` flag (no accrual system for income)
- Otherwise, the timing rules, frequency handling, and end date logic are identical

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
- Available WITHOUT adding back: $4200 - $1000 - $800 = $2400 ? WRONG! Double-counted rent
- Available WITH adding back: $4200 - $1000 - $800 + $800 = $3200 ? CORRECT! Same as before payment

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
4. After all days processed, return account projections and global projections

### Stateful Processing

The projection process is stateful:

- Account balances are modified as we progress through days
- Expense `NextDue` dates advance as they're renewed
- Income `NextDue` dates advance as they're renewed
- Expense `Accrued` amounts update daily

⚠️ **Warning:** This means running projections modifies the entity data so do not persist it back to the repository. Cloned models could be used but there is no need for that at this time.

### Validation

The service validates that `StartDate` is not before today:

```csharp
Throw<InvalidOperationException>.When(localDate > startDate, "Projections cannot start earlier than today")
```

This prevents attempting to project the past, which would produce incorrect results since historical transactions aren't captured in the projection system.

---
