# Dynamic Accrual Calculation PRD

**Status:** Planned
**Priority:** Medium
**Last Updated:** 2026-09-26
**Feature ID:** 023
**Scope:** Replace persisted expense-accrual state with on-demand calculation. One accrual engine becomes the single authority for `TotalExpenseAccrued`, `DailyExpenseAccrual`, `StableExpenseAccrual` and per-expense `Accrued`, evaluated for an as-of date whenever values are read (`/accounts`, `/expenses`, `/projections`). Covers the server read paths, the removal of the accrual command surface and dirty/status machinery, the client surfaces that consume it (dashboard cards and overview, accounts table, expenses `Accrued` column, accrual quick actions and status context), the append-only migration path that drops the persisted columns, and the maintenance import/export implications. `NextDue`, renewal semantics and the overdue rule are unchanged; the projection engine's outputs change only where an occurrence's settlement state is mis-measured today (§3.1). The calculation is expressed over explicit input and output models so that no entity is mutated and no derived value can be persisted.
**Audience:** Future implementation agent(s), reviewers, and maintainers.

## Purpose

Make expense accrual a pure function of `(expenses, asOfDate)` evaluated at read time, so there is no stale accrual state to refresh, no dirty-flag or last-accrued bookkeeping, no manual "accrue" step, and exactly one derivation of "available" across the dashboard, accounts table, expenses list and projections.

## Scope

### In Scope

- Refactoring `AccrueExpenseCalculator` into a pure calculation over explicit input models that returns an immutable accrual view, so no entity is mutated and no derived value can reach the database.
- Introducing the models that feed and return it: an immutable per-expense input record, an immutable per-account view, and the repository projection that produces the input without materialising expense graphs.
- Moving the accrual arithmetic that lives in `Pot.Data/Extensions/ExpenseEntityExtensions.cs` into the calculation, so all accrual maths has one home.
- Server read paths: `GET /api/accounts`, `GET /api/accounts/{id}`, `GET /api/expenses` and `GET /api/projections` all obtaining accrual values from that one engine for the same as-of date.
- Removal of the accrual command surface: `POST /api/accruals/accrue-expenses`, `AccrueExpensesService`, `AccrualDirtyStateManager`'s dirty/clean responsibilities, the accrual half of `/api/accruals/status`, and the accrual-relevant account accrual storage.
- Client removal: the "Accrue Expenses" quick action and the combined "Renew & Accrue" action (removed entirely, since the two separate renew actions already cover everything that would remain), plus the accrual status context state and its refresh triggers.
- Schema changes via new, append-only EF migrations (the existing `AddAccountAccrual` / `BackfillAccountAccrual` migrations are not touched).
- Maintenance import/export handling for the affected columns.
- The projection accrual basis for un-settled occurrences (§3.1), including the deliberate change to payment-day and overdue `Available` values.
- Documentation updates to match the changed behaviour: `Docs/ARCHITECTURE.md` (the dual-metric accrual description), `Docs/FEATURES.md` (the accrual and quick-action entries), `Docs/USER-GUIDE/` (`Accounts.md`, `Dashboard.md`, `Projections.md`, and the documentation plan's available-calculation section) and `Source/Server/IMPLEMENTATION.md` (the stable/dynamic accrual semantics referenced by PRD-003).

### Out of Scope

- `NextDue` semantics. It remains the user-entered value from create/edit, advanced only by renewal, and remains the single source of "overdue" (`NextDue < today`).
- Renewal behaviour and `RenewalMode.Overdue`/`Future` semantics, and the renewal half of `/api/accruals/status`. Renewal's _implementation_ does become a pure calculation so the projection loop carries no mutable simulation state, but that is an internal change decided under the model design in §1 (OD-08); the behaviour it produces for the user is unchanged.
- The projection engine's calculation rules, forecasting window, or metric set.
- A settlement/actuals ledger. That is a separate, independent track — see [PRD-024](PRD-024-settlement-ledger.md).
- Site locale/currency (PRD-018) and deletion flows (PRD-022), beyond noting the interactions recorded below.

## Problem Statement

Expense accrual results are persisted, so they are stale the moment anything they depend on changes — and they depend on two different clocks.

| Clock    | Trigger                                                                                                  | Effect on persisted accrual                                     |
| -------- | -------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| Event    | Expense create / accrual-impacting update / reassignment / exclusion toggle / renewal / impactful delete | The account's accrual aggregate no longer reflects its expenses |
| Calendar | A local day boundary passes                                                                              | The daily accrual ramp moves on regardless of any user action   |

The calendar clock is the binding one: an account's status goes stale **every day with no edits at all**, which is why the dashboard carries a permanent "accruals are pending" state and a manual "Accrue Expenses" quick action. An edit-time hook cannot remove that step; only deriving the values (or automating the trigger) can.

Persisting a derived value also brings its own costs:

1. **Write amplification and etag churn.** An accrual pass rewrites every expense whose `Accrued` changed, plus the account row and the account accrual row. `DbContextBase.OnBeforeSave` stamps a fresh `Etag` on each `Added`/`Modified` entry, so a "refresh" of derived data invalidates concurrency tokens on unrelated rows. PRD-009 explicitly designed around this churn.
2. **Two answers to one question, on two different bases.** `Available = Balance - Reserved - TotalExpenseAccrued` is exposed from `/accounts`, while `ProjectionsService` computes `Available = balance - reserved - accrued + expensesPaid`. Beyond staleness, the two measure different things: `/accounts` accrues a full obligation for an occurrence that is due or overdue and not yet acknowledged, while the projection advances the schedule before measuring and therefore reports the next cycle's partial accrual instead. The disagreement is structural, not merely temporal, and it is widest exactly when the obligation matters most.
3. **Duplicate implementation of the same rules.** The accrual rules (policy modes, accrual-start handling, one-time boundaries, stable vs daily contribution) live in one calculator, but the _decision of when they are applied and whether the result is current_ is spread across the dormant dirty-state manager, the status specification, the command service and the client's refresh triggers.
4. **Invalidation machinery with failure modes.** Every mechanism proposed to keep the persisted value fresh — accrue on each mutation, accrue at login, accrue on a schedule, write-during-GET — is an invalidation strategy for a value the application can already compute. Each adds a trigger, and the scheduled/login variants add a new failure domain (missed window, duplicate execution per replica, auth-path coupling) for a result that is idempotent per `(account, asOfDate)`.

### Identified defect: the projection loop measures after advancing the schedule

`Source/Server/IMPLEMENTATION.md` documents the accrual rules: on the due date `Accrued` is set to the full `Amount` regardless of elapsed days, and a recurring expense then begins accruing for the next period from that date. `MapToDateBalanceAvailable` depends on the first rule — it adds `ExpensesPaid` back "because the accrued amount already considers the expense total".

`ProjectionsService` breaks that combination for any item due today or already overdue, because it calls `Renew(Overdue, date)` **before** `AccrueExpenses(...)`. Renewal advances `NextDue` past the measurement date, so the accrual takes the next period's branch and reports a partial amount — zero on the day the item is due — instead of the full amount the add-back assumes. For a 1000 balance and a 100 monthly bill due today:

| Measurement                                     | `Accrued` at the read date                            | `Balance` | `Available` |
| ----------------------------------------------- | ----------------------------------------------------- | --------- | ----------- |
| Projection (current)                            | 0.00 — next period, zero days elapsed                 | 900       | **1000**    |
| Documented rule, and the dashboard accrual path | 100.00 — full amount for the occurrence being settled | 900       | **900**     |

The visible consequence is that `Available` exceeds `Balance` on every payment day by the total of that day's bills, which also masks a temporary overdraft on the day it exists; the figure corrects itself the next day, so it presents as a one-day spike rather than a persistent error. The same ordering understates an un-settled overdue occurrence for the same reason (§3.1). `ProjectionsServiceFixture` encodes the current result (`"accrued(0, just reset) + expensesPaid(900) = 5000"`), so no test currently flags it.

The projection engine demonstrates the alternative: `ProjectionsService` renews overdue items and accrues each day of the forecast **in memory, without persisting anything**, for every account and every day in the window. It never reads the persisted accrual columns. The persisted values therefore exist only to serve the non-projection surfaces, and are a stale copy of what those surfaces could compute.

## Current Behaviour

### Persisted accrual state

| Item                                                                                         | Location                                                                    | Notes                                     |
| -------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- | ----------------------------------------- |
| `Account.TotalExpenseAccrued`, `Account.DailyExpenseAccrual`, `Account.StableExpenseAccrual` | `Pot.Data/Entities/AccountEntity.cs`                                        | Written only by the accrual pass          |
| `Expense.Accrued`                                                                            | `Pot.Data/Entities/ExpenseEntity.cs`                                        | Per-expense allocation; also exported     |
| `AccountAccrual` (`AccountId`, `LastAccruedDate`, `AccruedIsDirty`)                          | `Pot.Data/Entities/AccountAccrualEntity.cs`                                 | One row per account; the staleness record |
| Schema history                                                                               | `20260421112007_AddAccountAccrual`, `20260421113608_BackfillAccountAccrual` | Append-only; not modified by this feature |

### Who writes it

- `Pot.App/Features/Accruals/AccrueExpenses/AccrueExpensesService.cs` — loads each account and all of its expenses, calls `IAccrueExpenseCalculator.AccrueExpenses(account, expenses, today)`, then `SetAccountCleanAsync(accountId, today)`, then saves. It does **not** renew.
- `Pot.App/Calculators/AccrueExpenseCalculator.cs` — mutates `account.TotalExpenseAccrued` / `DailyExpenseAccrual` / `StableExpenseAccrual` and each `expense.Accrued`, resetting them first. Consumes `DailyAccrual()`, `DailyBalance()`, `DaysFromAccrualStart()`, `Balance()` from `Pot.Data/Extensions/ExpenseEntityExtensions.cs`.
- `Pot.App/Concerns/Accruals/AccrualDirtyStateManager.cs` — `GetAccountsRequiringRecalc(before, after, asOfDate)` decides which accounts an impacting change affects; `SetAccountsDirtyAsync(...)` flags them; `SetAccountCleanAsync(...)` clears the flag and stamps `LastAccruedDate`.
- Marking paths: `Features/Expenses/{Create,Update,Delete,ToggleExclude,Renew}` services inject the dirty-state manager (or the account accrual repository, for delete). `UpdateExpenseService` compares `ExpenseAccrualState` before/after; metadata-only edits (`Description`, `Note`) and ended one-time items deliberately do not mark dirty (PRD-009).

### Staleness rule and status

`Pot.Data/Specifications/AccountAccrualSpecifications.cs` → `RequiresAccrualUpdate(asOfDate)`; `AccountAccrualRepository.GetRequiredAccountAccrualsAsync(...)` projects it to account `RowId`s. An account requires accrual when `AccruedIsDirty`, or `LastAccruedDate` is null, or `LastAccruedDate < asOfDate` (PRD-009, Q3).

`Pot.App/Features/Accruals/Status/AccrualsStatusService.cs` returns `ExpenseRenewalsRequired`, `IncomeRenewalsRequired` and `AccountAccrualsRequired`.

### Consumers of the persisted values

| Consumer                                                                                 | Surface                                                                                                                                                              |
| ---------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Pot.App/Features/Accounts/Get/Models/Output.cs`, `.../Accounts/GetAll/Models/Output.cs` | `Available => Balance - Reserved - TotalExpenseAccrued`, plus the three aggregates                                                                                   |
| Dashboard                                                                                | `AccountsOverview.tsx` sums `dailyExpenseAccrual` and `stableExpenseAccrual` for the summary metrics; `AccountCard.tsx` shows `available` and `stableExpenseAccrual` |
| Accounts page                                                                            | `AccountsTable.tsx` ("Total Accrued", "Stable Accrual"), `AccountMobileCard.tsx`                                                                                     |
| Expenses page                                                                            | `ExpensesTable.tsx` ("Accrued" column)                                                                                                                               |
| Maintenance export                                                                       | `Features/Maintenance/Export/Accounts/AccountsExporter.cs`, `.../Export/Expenses/ExpensesExporter.cs`                                                                |

`Pot.Data/Repositories/Accounts/AccountRepository.cs` reads accounts with **linked counts only** (`GetAllAccountsWithLinkedCountsAsync`, `GetAccountWithLinkedCountsOrDefaultAsync`), so it currently never loads the expenses an on-demand accrual would need.

### The parallel, already-derived path

`Pot.App/Features/Projections/ProjectionsService.cs` iterates days from today, calling `_expenseRenewalCalculator.Renew(expenses, RenewalMode.Overdue, date)` and `_accrueExpenseCalculator.AccrueExpenses(account, expenses, date)` per day, then maps `Available = balance - item.Reserved - item.Accrued + item.ExpensesPaid`. `Pot.Data/Repositories/Projections/ProjectionsRepository.cs` includes each account's non-excluded incomes and expenses. `PotDbContext` runs with a no-tracking default and the request never calls `SaveAsync`, so nothing the projection path computes is persisted — which is why the chart is unaffected by whether accruals were run.

### Client orchestration

`src/features/dashboard/contexts/AccrualsContext.tsx` polls `GET /api/accruals/status` (mount, window focus via `useWindowFocus`), exposing `expenseRenewals`, `incomeRenewals` and `accountAccruals`. `AccrueAccountExpensesAction.tsx` posts `/accruals/accrue-expenses` for the required accounts, then invalidates the status and `['projections']`. `RenewAccrueAllAction.tsx` renews expenses, renews incomes, then accrues, in that order. `QuickActions.tsx` renders both under permission guards. Cache dependencies live in `src/concerns/cache/cacheInvalidation.ts` (`expenses → accounts → projections`).

## Proposed Design

### 1. One calculation over explicit models, evaluated on read

#### 1.1 Input and output models

The calculation takes an explicit input model instead of entities, and returns an immutable view instead of writing into them (shapes indicative; naming to be settled in implementation):

```text
ExpenseAccrualInput                       // immutable record, one per expense
  RowId                                   // identity, so per-expense results can be attributed
  AccountId, ExcludeFromCalcs
  AccrualStart, NextDue, EndDate
  AccrualPolicy, Frequency, FrequencyCount, Amount

AccountAccrualView                        // immutable record, one per account
  TotalExpenseAccrued
  DailyExpenseAccrual
  StableExpenseAccrual
  Expenses: [ { RowId, Accrued } ]        // detail shape only; see 1.4
```

All four outputs are still produced: the three aggregates and per-expense `Accrued` for `/accounts` and `/expenses`, and `DailyExpenseAccrual` for the projection response's `dailyAccrual` metric and its chart series. Only the accounts response stops exposing the daily field (§8) — do not remove the metric from the calculation.

`Pot.App/Concerns/Accruals/Models/ExpenseAccrualState.cs` already carries every input field except `RowId`, and its current purpose — the before/after comparison used to mark accounts dirty — is removed by this feature. It is therefore **repurposed and renamed** (`ExpenseAccrualInput`), with `RowId` added, rather than replaced by a new type.

`AccountEntity` leaves the calculation signature entirely. The calculator currently both reads and writes it; the three account aggregates become results, and `Balance`/`Reserved` stay in the mapping layer where `Available` is composed.

#### 1.2 Immutable input versus a mutable model

| Option                                           | Shape                                                                 | Assessment                                                                                                                                                                                                                                                                                                                                                                                                                    |
| ------------------------------------------------ | --------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A. Mutate entities (today)                       | `AccrueExpenses(AccountEntity, IEnumerable<ExpenseEntity>, DateOnly)` | **Rejected.** `PotDbContext` defaults to no-tracking, but `WithTracking()` is reference-counted per request, so a tracking scope opened by any repository enables it for all reads in that request. A mutated tracked entity is then `Modified`, and `DbContextBase.OnBeforeSave` stamps a fresh `Etag` on it during whatever save the request performs. Persisting a derived value must be impossible, not merely unintended |
| B. Mutable input model                           | The same shape over a DTO instead of an entity                        | Removes the entity hazard, but leaves "what changed in this iteration" implicit, makes a snapshot unsafe to share between consumers, and gives up `with`-expression variation. Viable **only** if the projection loop's snapshot cost is measured to matter (see 1.5)                                                                                                                                                         |
| C. Immutable record input, immutable view output | `Calculate(input, asOfDate)`                                          | **Settled** (OD-13). The property this feature exists to create — derived values can never be written back — becomes a property of the types. Records also give value equality (already relied on in this area for before/after comparison) and `with`-based variation for tests and later what-if analysis                                                                                                                   |

#### 1.3 Where the models live

| Option                          | Placement                                                                                                                              | Assessment                                                                                                                                                                                                                                                 |
| ------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Single shared record            | A new `Models` folder in `Pot.Shared`, referenced by `Pot.Data` (as the EF projection target) and `Pot.App` (as the calculation input) | **Settled** (OD-09). One type, no mapper, no dependency inversion: `Pot.Data` already references `Pot.Shared`, and `Pot.App` references both. The record _is_ the accrual-relevant fact set for an expense, so sharing it is cohesion rather than coupling |
| Single record in the data layer | `Pot.Data/Repositories/<Area>/Dtos`, following `AccountWithLinkedCounts`                                                               | Same mechanics and consistent with an existing precedent; slightly odd for the calculation's contract to be owned by the persistence layer                                                                                                                 |
| Two types plus a mapper         | A narrow EF projection DTO in `Pot.Data` and the calculation input in `Pot.App`                                                        | The calculator owns its own contract and the projection can evolve independently, at the cost of a mapper and a drift risk between two shapes that coincide by nature                                                                                      |

The existence of `AccountWithLinkedCounts` (`Pot.Data/Repositories/Accounts/Dtos`) establishes that narrow, EF-projectable read models are an accepted pattern here. Whichever placement is chosen, the repository must project only the fields listed in 1.1 rather than materialising expense graphs.

#### 1.4 Two shapes, one implementation

`/expenses` needs `Accrued` per row; `/accounts` and `/projections` need only the three aggregates. A per-expense collection allocated per account per day would be pure waste in the projection loop, so the calculation exposes two entry points over one private implementation:

```text
CalculateAccountTotals(input, asOfDate)             -> three aggregates only
CalculateAccountWithExpenseDetail(input, asOfDate)  -> three aggregates plus per-expense Accrued
```

The detail shape is the contract of the two expense read endpoints (`GET /api/expenses`, `GET /api/expenses/{id}`), which expose per-row `Accrued` for the expenses table's column (OD-07). It is deliberately **not** produced for the projection window: per-item accrued across every day of the window would multiply the detail allocation by the window length for a figure nothing renders today. If a projection surface ever needs it, it is computed **on demand for the single date that surface is showing**, not across the series.

#### 1.5 The projection loop

The loop is the one place where the input strategy carries a cost, because the schedule moves as it walks days: it currently renews _and_ accrues by mutating the same objects, 365 times. The load-bearing detail is _when_ the schedule is read: the due check and the accrual both measure the persisted (measurement) cursor, and only then is the forecast cursor advanced. Reading the already-advanced state is what produced the defect in the Problem Statement.

| Option                                    | Mechanism                                                                                                                                                                            | Assessment                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| P1. Per-day immutable snapshot            | Re-read (and re-project) the measurement input on each forecast day instead of holding it fixed                                                                                      | Simple contract, 365 × M mappings, and an ordering hazard that only tests catch                                                                                                                                                                                                                                                                                                                                                                                |
| P2. Read-only contract over mutable state | Simulation objects implement a get-only `IExpenseAccrualInput`; renewal mutates them, the calculation can only read                                                                  | No snapshots, no allocation, and the type system enforces read-only inputs. Under the settled semantics it must also expose the measurement cursor separately from the advancing forecast cursor, which a read-only view of the live state cannot do                                                                                                                                                                                                           |
| P3. Pure renewal plus a value-type cursor | Immutable facts per expense plus a small `readonly record struct` cursor (`NextDue`, `AccrualStart`); renewal folds the cursor and the calculation takes `(facts, cursor, asOfDate)` | **Settled** (OD-08). No mutation anywhere in the loop, no meaningful allocation (a two-date value type), the ordering hazard disappears because the cursor is derived rather than read, and renewal becomes as testable as accrual. Requires pure counterparts for `ExpenseRenewalCalculator`/`IncomeRenewalCalculator`; the renewal _command_ path still writes the computed cursor back to the entity, because a schedule advance is a fact worth persisting |

Under P3 the loop carries two cursors per expense: the **measurement cursor** (the persisted one, which only the user's renewal moves) and the **forecast cursor** (advanced as the loop walks days). The measurement cursor governs the read date — the day whose settlement state is being measured — where the due check and the accrual both read it, so an un-acknowledged occurrence is counted in full instead of being released by the loop's own catch-up (OD-01). From the following day the assumed payment has been made — the same assumption that reduces the forecast balance — so the forecast cursor governs and the occurrence is released rather than counted a second time (OD-11).

Because the accrual basis changes for un-settled occurrences, projection outputs are **not** byte-for-byte identical to today's: payment-day and overdue `Accrued`/`Available` change by design (§Problem Statement, §3.1), and the projection fixtures are updated deliberately (see Tests and Validation). Any other difference is a defect in the refactor.

#### 1.6 Rules preserved

The existing rules are preserved exactly: `AccrualPolicy.None` short-circuits, `AccrualStart` gating, the one-time due-date boundary, the stable-metric fixed-denominator rule for one-time items and the end-date gate for recurring items, and the "due today is fully accrued but not paid" treatment. The as-of date comes from `ITimeProvider.GetLocalDateNow()`. The calculation is the only place accrual arithmetic lives, and it is shared by every consumer.

### 2. Read paths consume the view

| Path                                          | Change                                                                                                                                                                                                                                                                   |
| --------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `GET /api/accounts`, `GET /api/accounts/{id}` | The repository projects the accrual-relevant expense fields into the input record (§1.3) and the calculation produces the aggregates; the existing output members are populated from them. `Available` keeps its current formula and therefore keeps its current meaning |
| `GET /api/expenses`                           | Populate the per-row `Accrued` from the detail shape (§1.4) for the same as-of date                                                                                                                                                                                      |
| `GET /api/projections`                        | Adopt the model in §1.5 and call the same calculation on the same measurement cursor. Outputs change only where settlement state was previously mis-measured (§3.1); the fixtures are updated deliberately to guard against any other difference                         |

What is now shared across the read paths is the accrual basis, not the balance: the chart still applies its own assumed payments inside the window, so its `Balance` — and therefore `Available` — can differ from the card's user-maintained balance. Neither `Balance` nor `Reserved` is an accrual input: both stay where the account response is composed. Response shapes change in exactly one place: the accounts response stops exposing `dailyExpenseAccrual` (OD-06, §8).

### 3. `NextDue` and renewal do not change

`NextDue` stays exactly what it is today: the value the user enters on create/edit, advanced only by renewal, and the sole basis for "overdue" (`NextDue < today`). Renewal behaviour is unaffected; whether its implementation also becomes a pure calculation is settled in §1.5 (OD-08). The renewal-derived signals that depend on `NextDue` — overdue/due-today/due-soon/ended badges, dashboard overview windows, the bulk renew confirmation breakdown, and `BudgetReminderService` — all continue to behave as they do now, because nothing in this feature moves the cursor.

#### 3.1 Worked examples: the same bill in four settlement states

The examples use the same inputs throughout: account `Balance = 1000`, `Reserved = 0`, no incomes, and one expense of 100 with `Frequency.Months` and `AccrualPolicy.Automatic` on a 31-day cycle. The read date is day 0, and `Available = Balance - Reserved - Accrued + ExpensesPaid`.

"Settled" means the user has renewed (marked the occurrence paid); it is the only settlement record the product has today, so these four states are distinguished by the cursor, not by the movement of money.

| Settlement state on the read date     | Cursor (`NextDue`, `AccrualStart`)     | Counted as paid in the window | Current: `Accrued` → `Available` | Option C: `Accrued` → `Available` |
| ------------------------------------- | -------------------------------------- | ----------------------------- | -------------------------------- | --------------------------------- |
| Paid a day early — settled on day −1  | `day 31`, `day −1`                     | none                          | 3.13 → **996.87**                | 3.13 → **996.87**                 |
| Paid on the day — settled on day 0    | `day 31`, `day 0`                      | none                          | 0.00 → **1000.00**               | 0.00 → **1000.00**                |
| Not yet paid, still due today         | `day 0` (accrual start not used)       | 100, assumed by the window    | 0.00 → **1000.00**               | 100.00 → **900.00**               |
| Paid a day late, not yet acknowledged | `day −1`, then `day −1` after catch-up | none                          | 3.23 → **996.77**                | 100.00 → **900.00**               |

1. **The settled states are unaffected.** When the cursor has already been advanced there is nothing to catch up, so the current code and option C agree: `Available` is the balance less the accrual ramping towards the next occurrence.
2. **The un-settled states are the defect in numbers.** Today the chart barely distinguishes settled from un-settled — 0.00 or 3.23 of accrual where the obligation is the full 100 — which is why clearing a bill appears to change so little.
3. **"Paid late" and "not paid at all" are the same input.** With no payment record the product cannot tell them apart, so both hold the same obligation until the user acknowledges one of them. This is the gap PRD-024 exists to close.
4. **One obligation per un-settled item, not one per missed cycle.** The obligation is a single `Amount`, matching the existing accrual rule, even when the renewal catch-up would skip several cycles (OD-12). POT forecasts available funds on the basis that income is received and expenses are paid on time; it is not a full financial-tracking application, so counting every skipped cycle would be modelling arrears rather than projecting a balance.
5. **Day 1 and beyond.** The un-settled obligation is held on the read date only (OD-11, settled). The window assumes the payment was made — the same assumption that produced the 900 balance above — so holding the obligation any longer would subtract the same 100 twice: the balance has already lost it, and the accrual would take it again.
6. **The accounts page's "Total Accrued" column moves with the same rule.** For an un-settled item it now shows the full obligation (100.00 in row 3 rather than today's 0.00, and 100.00 in row 4 rather than 3.23), so the column, the account's `Available` and the chart report the same number. That is a visible change on that column and is intentional.

### 4. Removed surfaces

Server:

- `POST /api/accruals/accrue-expenses` (`Pot.AspNetCore/Features/Accruals/AccrualsEndpoints.cs` and its handler/request/mapping), `IAccrueExpensesService` / `AccrueExpensesService`.
- `AccountAccrualsRequired` from the status output and `GetRequiredAccountAccrualsAsync`; the renewal halves of the status endpoint are retained.
- `IAccrualDirtyStateManager` and its dirty/clean responsibilities. The before/after comparison built on `ExpenseAccrualState` and the `GetAccountsRequiringRecalc` calls in the expense mutation services become dead and are removed; the renewal services simply stop flagging. The record type itself is repurposed rather than deleted (see below).
- `AccountAccrualEntity` and `AccountAccrualSpecifications`.
- The three aggregate columns on `AccountEntity` and `Accrued` on `ExpenseEntity` (see migration approach below).

Repurposed rather than removed:

- `ExpenseAccrualState` becomes `ExpenseAccrualInput` (with `RowId` added), as described in §1.1. Its before/after comparison call sites are removed with the dirty machinery; the type itself is the calculation's input.
- The accrual arithmetic in `Pot.Data/Extensions/ExpenseEntityExtensions.cs` (`DailyAccrual`, `DailyBalance`, `DaysFromAccrualStart`, `Balance`) moves into the calculation. Those helpers have no other consumers, so the move is contained; `IsDueOnDate` and `DaysDueFrom`, used by the projection loop and reminders, are unaffected.

Client:

- `AccrueAccountExpensesAction.tsx` is removed with the accrual command surface.
- `RenewAccrueAllAction.tsx` is removed altogether rather than reduced to a renew action: with the accrual leg gone it would only renew expenses and incomes, which `RenewExpensesAction.tsx` and `RenewIncomesAction.tsx` already do separately. The combined `renew-accrue-all` label, hint text and permission guard go with it, and `QuickActions.tsx` keeps the two renew actions.
- `accountAccruals` and its refresh path from `AccrualsContext.tsx`; `useApiAccrueAccountExpenses`; the accrual members of `src/data/accruals.ts`. The renewals-required parts of the status context stay, as renewal remains a real user action.
- No change is needed to `useAccountFilter.ts`/account types: they already carry `totalExpenseAccrued`, `dailyExpenseAccrual` and `stableExpenseAccrual`.

### 5. Schema and migration approach

EF migration files are append-only, so the existing accrual migrations remain in sequence and are **not** edited or removed. Dropping the storage is a new migration.

Decision: two-step rollout (OD-14):

1. **Stop reading and writing.** Ship the derived read paths and the removal of the command surface, leaving the columns and table in place. Behaviour change only; trivially reversible.
2. **Drop.** A later migration (for example `DropAccountAccrualAndAggregates`) removes `AccountAccrual` and the four columns.

Step 2 is safe from a data-loss perspective because the values are reproducible from source data. Two operational notes:

- PRD-013 gates startup on applied migrations, so a destructive migration is a deploy-time event: replicas still serving during a rolling deploy must no longer depend on the dropped columns, which step 1 guarantees.
- `DbContextBase.DisableCascadeDelete` forces `Restrict` on every FK, so the `AccountAccrual → Account` FK is part of the delete ordering. If this feature lands before PRD-022, that PRD's sweep of `AccountAccrual` disappears.

### 6. Maintenance import/export

`AccountsExporter` writes the three aggregates and `ExpensesExporter` writes `Accrued`; both importers already ignore those columns (the assignments are commented out), so neither value round-trips today.

Decision: the derived columns are no longer exported. They would have to be computed at export time to be written at all, they are reproducible from the exported rows, and nothing consumes them on import. `AccountData` / `ExpenseData` (export) and `AccountCsvRow` / `ExpenseCsvRow` with their interfaces (import) lose the corresponding members together, and the narrower header set is verified to round-trip.

Decision: the maintenance package version stays at **v4**. It was introduced by PRD-021 on 2026-09-24 and has no consumers in the field, so there are no v4 exports whose meaning needs protecting and the column-set change can be made in place instead of following PRD-021's version-bump discipline. If v4 is ever consumed, that discipline applies again: a changed column set bumps the version so superseded exports are rejected rather than misread.

### 7. Performance and caching

**Decision: no cache.** Derived values are computed on read. This section records why, and the complete design a future agent would need if measurement ever says otherwise.

`GET /api/accounts` gains one projection query (the account row plus the accrual-relevant expense fields and the version aggregate described below) and a per-account pure calculation.

`GET /api/projections` is unchanged and already the heavier path: it renews and accrues for every account on every day of the window, and it ships today without a cache. That is the scale check for the new work — until the accounts path is measured to be worse than a path that is 365 times larger, a cache would be solving a problem that does not exist.

The projection loop remains the hot path; §1.5's settled P3 (OD-08) keeps it allocation-free apart from the small per-day view records, and the per-expense detail shape is opt-in (§1.4). Repository shape is settled by §1.3: the query projects the input fields per expense rather than loading expense graphs, and for the read paths the input is built once per request.

#### If a cache is ever warranted

A future agent should implement exactly this much and no less. Anything simpler is wrong in a way that will not be visible until a user sees a stale number.

**The cached value has three clocks.** An accrual value changes when the date rolls over, when the rules change, or when the data changes; the key has to carry all three.

| Clock    | Example change                                                | Key component                                                                                          |
| -------- | ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| Calendar | the site-local date rolls over                                | `asOfDate` (from `ITimeProvider`, per PRD-018) — a new day is a new key, so nothing needs invalidating |
| Rules    | the accrual arithmetic changes in a deploy                    | `derivationVersion`                                                                                    |
| Data     | an expense is created, edited, deleted, reassigned or toggled | `dataVersion` (per account)                                                                            |

Key: `(accountId, asOfDate, derivationVersion, dataVersion)`.

- **`derivationVersion`** — an assembly/build-derived value (for example `AssemblyInformationalVersion`) is the zero-maintenance choice; a deploy that changes nothing about accruals causes harmless misses. A hand-maintained constant is more precise but fails silently: a missed bump serves values computed by superseded rules, which is worse than having no cache at all.
- **`dataVersion`** — `(maxExpenseEtag, expenseCount)` over the account's expenses, projected in the same accounts query (`account.Expenses.Max(expense => expense.Etag)` and `account.Expenses.Count`). `DbContextBase.OnBeforeSave` re-stamps `Etag` on every added or modified row, so an edit or a creation raises the max, a deletion changes the count (or lowers the max when the deleted row held it), and a reassignment changes both accounts. The account's **own** `Etag` is not usable here: once this PRD removes the accrual command, an expense edit no longer modifies the account row.
- **Cache the view, not `Available`.** `Balance` and `Reserved` are not accrual inputs and are composed where the account response is built (§2), so account-row edits — balance and reserved changes — never need to touch the cache.

**In-process or distributed.** Under this key an in-process `IMemoryCache` is _correct_ on every replica, because the key already contains everything that determines the value: replicas cannot disagree, they only duplicate the work (N× computation, N× memory, each warmed independently). Measured by database load rather than replica CPU, an in-process cache therefore achieves nothing in a horizontally scaled deployment. If the goal is to stop replicas querying the database for the same values, the cache must be distributed (`IDistributedCache`, for example Redis) — which is a new operational dependency for a feature that currently has none. Either way it must be cache-aside with graceful degradation: a miss, a store outage or a deserialisation failure computes the value as today and never fails the request.

**Lifetime and eviction.** Because the key changes whenever the value would, entries are never reused after a change, so time-based expiry is only about reclaiming space. Bound it deliberately: an account accumulates roughly one entry per (day, data version) pair, so size to `accounts × (edits per day + 1)`, or expire entries at the end of the site-local day plus a margin. Apply a size limit so an unusual day cannot exhaust a replica.

**What not to do:**

- A TTL-only cache without the version components briefly serves values computed from superseded data or rules — the exact staleness this PRD removes.
- Eviction call sites in the mutation services instead of a key component is easier to write but is the dirty-flag pattern again, with the same easily-missed paths (reassignment, delete, toggle, renew) that PRD-009 catalogued.
- Caching `Available` instead of the view adds account-row edits as a fourth input for no benefit.

**Multi-tenancy.** `accountId` is globally unique, so no site component is needed while the key is built on account ids; add one only if a future key becomes site-scoped.

Whatever is built should be validated by asserting that cached and freshly computed values agree, and that each data mutation produces a different key.

### 8. Client effects

- The dashboard no longer shows an "accruals pending" state, because there is nothing to accrue. Numbers are current whenever the accounts query is current, and `expenses → accounts → projections` invalidation already covers edits.
- Quick Actions keeps `RenewExpensesAction` and `RenewIncomesAction`; the combined action is removed rather than reduced (§4).
- No accrual-specific loading or error state is required; the existing accounts query loading/error handling covers the new computation.
- `dailyExpenseAccrual` is dropped from the accounts response and from `src/data/account.ts`, together with the client surface that only existed to carry it: the `totalDailyAccrual` member of `useAccountsSummary.ts` (written by `AccountsOverview` and never read), the placeholder entry in `useAccountFilter.ts`, the account factory and the two tests that reference it (`tests/data/account.test.ts`, `tests/features/dashboard/stores/useAccountsSummary.test.ts`). No rendered value changes. The calculation still produces the metric — only the accounts response loses the field (§1.1).

### 9. Post-implementation follow-ups

Deliberately deferred until the change is implemented and validated, and listed here so none is dropped:

- [ ] **Add a supersession note to PRD-003 and PRD-009** (OD-05), once this PRD is completely implemented and validated (§3.1 scenarios and the projection regression suite passing in production). The note states what each document still contributes — PRD-003's policy semantics, PRD-009's boundary conventions — and what this PRD supersedes: the persisted accrual state and the dirty/last-accrued status mechanics. Do not annotate before validation, and do not rewrite those documents.
- [ ] **Ship the drop migration** — the settled step 2 of the two-step rollout in §5 (OD-14) — then remove this item.
- [ ] **Complete the documentation updates** listed in Scope: `Docs/ARCHITECTURE.md`, `Docs/FEATURES.md`, the `Docs/USER-GUIDE/` pages and `Source/Server/IMPLEMENTATION.md`.
- [ ] **Update this feature's index rows** — 023's own row, and the 003/009 rows touched by the supersession note — per the `Docs/Future/README.md` completion convention.

## Open Decisions

### Behavioural decisions

| ID    | Decision                                                                                                                                                                                                                                                                              | Notes                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OD-01 | Derived accrual measures the persisted, un-acknowledged cursor, and the projection must not release an un-acknowledged occurrence by advancing its schedule before measuring it. **Settled:** option C (§3.1)                                                                         | Option C, decided in review. The earlier framing of a chart-versus-list "inconsistency" was wrong: the real problem is two measurement bases, and the projection one contradicted the documented due-date rule                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| OD-02 | Maintenance export columns and package version. **Settled:** the derived columns are no longer exported and the package stays at v4 (§6)                                                                                                                                              | See §6                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| OD-03 | Combined renew UX. **Settled:** the combined action is removed entirely and `RenewExpensesAction` / `RenewIncomesAction` remain (§4)                                                                                                                                                  | The two separate renew actions already cover everything that would remain                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| OD-04 | Caching the derived values. **Settled:** no cache — compute on read, and revisit only on measured evidence (§7)                                                                                                                                                                       | A correct cache needs a three-clock key (date, rules, data) and, for horizontal scaling, a distributed store — more machinery than the computation it avoids at POT's scale, so the full design is retained in §7 for a future agent                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| OD-05 | Supersession notes for PRD-003 and PRD-009. **Settled:** add a short status note to each once this PRD is completely implemented and validated, tracked as a checklist item in §9                                                                                                     | Still valid inputs to the engine: their policy-mode semantics and boundary conventions. Superseded by this PRD: the persisted accrual state and the dirty/last-accrued status mechanics. Completed documents are not rewritten — the note records what changed and what still applies                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| OD-06 | API response shapes for the derived values. **Settled:** keep `available`, `stableExpenseAccrual`, `totalExpenseAccrued` and per-expense `accrued`; drop `dailyExpenseAccrual` from the accounts response, along with its dead client store member (§8)                               | Every retained field backs a rendered value: `available` (which also drives the account status badges), `stableExpenseAccrual` (the "Daily Need" figure in four places), `totalExpenseAccrued` (one sortable column) and per-expense `accrued` (the per-item column). `dailyExpenseAccrual` has no reader at all — it is summed into `totalDailyAccrual`, which nothing selects. The calculation still produces the daily metric because the projection response consumes it                                                                                                                                                                                                                                                                                                                                                  |
| OD-07 | Should per-expense accrued values be exposed only where the UI needs them (expenses list) or on all expense reads? **Settled:** on both expense read endpoints (`GET /api/expenses`, `GET /api/expenses/{id}`), not narrowed to the list; no other endpoint changes shape to carry it | The value is a by-product of the same calculation, and the canonical expense payload already backs the expenses table, the mobile card, the dashboard overview (which receives `accrued` through `useApiGetAllExpenses` and does not render it) and the account filter's derived list — so narrowing it to the list would mean a second contract, or a flag, for a figure those surfaces may legitimately show. Create/update/delete/toggle/renew responses do not carry it and should not: cache invalidation already refreshes the list after a save, so the field would have no reader. The one surface where it would be informative but is not exposed is the projection expense detail, because it would materialise the detail shape across every day of the window; that stays on demand for the selected date (§1.4) |

### Model design decisions (from §1)

| ID    | Decision                                                                                                                                                                                                                                  | Notes                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OD-08 | Projection-loop input strategy: per-day immutable snapshot (P1), read-only contract over mutable state (P2), or pure renewal with a value-type cursor (P3)? **Settled:** P3 (§1.5)                                                        | No mutation in the loop, no meaningful allocation, and the accrual-against-stale-dates hazard disappears because the cursor is derived rather than read. P3 also implies pure counterparts for `ExpenseRenewalCalculator`/`IncomeRenewalCalculator`, with the renewal command still persisting the computed cursor, and it is what makes the two-cursor split (measurement vs forecast) that OD-01 depends on structural rather than ordering-sensitive |
| OD-09 | Where do the input and view models live: one shared record in `Pot.Shared`, one record in `Pot.Data` DTOs, or a `Pot.Data` projection DTO plus a `Pot.App` input record and mapper? **Settled:** one shared record in `Pot.Shared` (§1.3) | One shape, no mapper, no drift, no dependency inversion: `Pot.Data` already references `Pot.Shared` and `Pot.App` references both. The record _is_ the accrual-relevant fact set for an expense, so sharing it is cohesion rather than coupling. Placement confirmed in review under this repository's conventions; `AccountWithLinkedCounts` remains the precedent for repository-specific DTOs, which this model deliberately is not                  |
| OD-10 | Is the per-expense detail shape a separate entry point, or should one entry point always return it? **Settled:** two entry points over one private implementation (§1.4)                                                                  | The aggregate shape stays the default so the projection loop never allocates per-expense detail that nothing reads; the detail shape is explicit and consumed by the two expense read endpoints (OD-07)                                                                                                                                                                                                                                                 |
| OD-13 | Is the calculation expressed over immutable records (option C), a mutable input model (option B), or the entities (option A)? **Settled:** option C — immutable record input, immutable view output (§1.2)                                | The same property OD-08's immutable facts rely on: the guarantee that a derived value can never be written back is expressed by the types rather than by the absence of a tracking scope somewhere in the request. Records also give value equality and `with`-based variation for tests and later what-if analysis                                                                                                                                     |

### Migration and rollout decisions

| ID    | Decision                                                                                                                                               | Notes                                                                                                                                                                                                                                                                       |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OD-14 | Migration rollout: a single destructive migration, or stop reading/writing first and drop in a later migration? **Settled:** the two-step rollout (§5) | Removing the values is safe because they are reproducible from source data, but under PRD-013's startup migration gate a destructive migration is a deploy-time event, so step 1 (stop reading and writing) must ship before step 2 (drop) however close together they land |

### Projection settlement semantics

| ID    | Decision                                                                                                                                                                                | Notes                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OD-11 | How long is an un-acknowledged occurrence held inside the forecast window: the read date only (C1), or until the user acknowledges it (C2)? **Settled:** C1 — the read date only (§3.1) | §3.1 note 5. The window assumes income is received and expenses are paid on time, so the payment is assumed made on its due date and the occurrence is released. Holding it instead (C2) subtracts the same amount a second time: the forecast balance has already absorbed the payment while the accrual would keep taking it every day, leaving the account looking 100 worse off than the balance the same forecast produced. C1 also needs no extra forecast state, and it does not weaken the read-date distinction this PRD exists to introduce — settled and un-settled still differ by 100 on day 0 (900 against 1000), because that is measured against the persisted cursor (OD-01) |
| OD-12 | Should an un-settled obligation be one `Amount`, or one per cycle the renewal catch-up would skip? **Settled:** one `Amount` (§3.1 note 4)                                              | §3.1 note 4. Matches the existing accrual rule. The renewal catch-up does know how many cycles are outstanding, so per-cycle counting is implementable, but it would model arrears: POT forecasts available funds on the basis that income is received and expenses are paid on time and is not a full financial-tracking application. Counting what was actually owed is the settlement track's job (PRD-024), not this engine's                                                                                                                                                                                                                                                             |

## Design Notes (Rationale and Rejected Alternatives)

**Why not "accrue automatically after each expense change" (the original idea).** It addresses only the event clock. The calendar clock still marks every account stale each day, so the user would still face a pending state daily — the exact irritation the idea was meant to remove. It also reintroduces the write amplification and etag churn of §Problem 1 on the mutation path, re-runs a whole-account recalculation per edit (undoing the batching the manual action was originally added for), and leaves the two derivations of `Available` in place.

**Why not "accrue on login".** Idempotency is free (`RequiresAccrualUpdate` makes a second login in a day a read-only no-op), but it couples a write to the authentication path, still writes and re-stamps etags once per user per day, and does not help a session left open across midnight.

**Why not a scheduled worker.** It covers users who never open the app, but adds a new scheduling surface with the same failure classes already tracked for the existing workers (per-replica execution, missed windows on restart, catch-up), for a value that is idempotent and cheap to compute on read. It is the most machinery for the least additional correctness.

**Why not "accrue if stale inside a GET".** A write inside a read is impure: it mutates `Etag`s, surprises caching and concurrency, and fails against read replicas. If materialisation is ever required again, a version-keyed cache (§7) is the shape that does not have these properties.

**Why the schedule cursor is _not_ derived.** An earlier draft proposed deriving `NextDue` too, treating it as a recurrence anchor and computing the current due date on read. That would erase the overdue concept: renewal is currently the only record that an occurrence has been acknowledged, and overdue drives badges (`badgeStyles`, `tableRowUtils`), `getDaysDue` status derivation, dashboard overview windows, the bulk renew breakdown (`Overdue` vs `Future`) and `BudgetReminderService`. Deriving the cursor would have required a persisted acknowledgement fact and migration seeding to preserve behaviour — a much larger change, and one that belongs with the settlement track (PRD-024) rather than here.

**Why an immutable record input rather than a mutable model, and rather than the entities.** The rule this feature is built on is that a derived value must never be persisted. Expressed against entities, that rule survives only because the projection path happens to run no-tracking; a single `WithTracking()` scope opened elsewhere in the same request would turn an accrual mutation into an `Etag`-stamping `Modified` row on the next save. A mutable DTO removes the entity hazard but keeps the mutation idiom, leaves "what changed this iteration" implicit, and makes a shared snapshot unsafe. Immutable records make the rule structural, and `with`-expression variation is exactly what a later date-varied or what-if analysis (PRD-024) would want.

**Why one shared input type rather than a data-layer DTO plus an app mapping (OD-09).** The field set _is_ the accrual-relevant fact set for an expense, and every consumer needs exactly it, so a single shared record gives one shape, no mapper and no drift, with no dependency inversion. The nearest precedent (`AccountWithLinkedCounts`) bundles an entity with counts and exists for one repository's needs; this model is narrower and is not repository-specific, which is why it does not need to live beside a repository.

**Why the detailed view is a separate entry point (OD-10).** The projection loop runs the calculation for every account on every day of the window. A per-expense collection materialised on each of those calls would allocate for data that nothing reads, so the aggregate shape is the default and the detail shape is explicit.

**Why per-expense accrued is not narrowed to the expenses list.** It is already on the canonical expense payload, which serves the table, the mobile card, the dashboard overview and the account filter's derived list; narrowing it would add a second contract, or a flag, for a value those surfaces may legitimately render, and would save nothing server-side because the per-row figure is a by-product of the calculation the list requires anyway. The write responses stay without it because they have no reader, and the projection detail stays without it because producing per-item accrued for every day of the window would multiply the detail shape's cost by the window length rather than compute it once for the date on screen.

**Why renewal is also a pure calculation under P3 (OD-08).** Renewal and accrual read the same schedule through different lenses: renewal advances it, accrual measures it. Expressing renewal as a fold over an immutable cursor removes the ordering hazard in the projection loop (accruing against pre-renewal dates), makes the renewal rules unit-testable without entities, and costs a two-date value type per step rather than an allocation. Persistence is unaffected: the renewal command still writes the computed cursor back to the entity, because a schedule advance is a fact, not a derived value.

**Why the projection must measure before it advances.** `Source/Server/IMPLEMENTATION.md` states that on the due date `Accrued` is the full `Amount`, and `MapToDateBalanceAvailable` relies on that when it adds `ExpensesPaid` back. An implementation that advances the schedule first — as today's loop does — measures the next period instead, which is what broke the add-back and produced the payment-day `Available` overshoot. Keeping a measurement cursor separate from the forecast cursor makes the documented rule structural rather than a comment the ordering silently contradicts; option C is therefore a restoration of documented behaviour in the projection path, not a new convention.

**Why the un-acknowledged obligation is released after the read date (OD-11).** The window assumes income is received and expenses are paid on time, so its day-0 balance already has the payment taken out of it — that is where the 900 comes from. Holding the obligation for the rest of the window would then subtract the same amount a second time against that balance, showing an account 100 worse off for a payment the forecast itself assumed was made. Releasing it keeps the projection internally consistent, while the read date still separates settled from un-settled (900 against 1000), which is the whole point of measuring the persisted cursor (OD-01). The two views are not in conflict: the account card reads the user's own maintained balance and holds the obligation until renewal, and the chart assumes the payment and moves on.

**Why one `Amount` rather than one per skipped cycle (OD-12).** POT forecasts available funds on the basis that income is received and expenses are paid on time; it is not a full financial-tracking application. An item un-renewed for three months therefore represents a single outstanding obligation of one `Amount`, not three months of arrears. Modelling arrears needs the payment history PRD-024 exists to capture, and it would change the meaning of the account aggregates rather than only the projection window — a product decision, not a calculation one.

**Why the projection engine is the reference.** It already recomputes renewal and accrual per day, per account, without persisting, and is validated by `Pot.App.Tests/Features/Projections/ProjectionsServiceFixture.cs`. This feature generalises that proven path rather than inventing a new one; the projection fixtures remain the guard that the shared engine changes nothing beyond the settlement basis decided in §3.1.

## Tests and Validation

- **Calculator unit tests** — `Pot.App.Tests/Calculators/AccrueExpenseCalculatorFixture.cs` reworked against the pure API over `(facts, cursor, asOfDate)`, including `AccrualPolicy.None`, accrual-start gating, one-time boundaries, stable-metric denominators and end-date gates. No entity graph or `DbContext` setup is required, which is the testability dividend of the model change.
- **Model and ordering tests** — the renewal fold (multi-cycle `Overdue` catch-up, single-cycle `Future`, end-date clamping, `AccrualStart` selection per policy) plus the ordering guarantee the settled semantics depend on: the due check **and** the accrual both read the measurement cursor, and the forecast cursor is advanced only afterwards, so an un-acknowledged occurrence is never released by the loop's own catch-up.
- **Settlement scenarios** — the four states in §3.1 become explicit test cases on the same inputs (`Balance` 1000, `Reserved` 0, one 100 monthly expense on a 31-day cycle), asserting `Accrued` and `Available` at the read date and across the following days:
  - **paid a day early** (settled on day −1): `3.13` → `996.87`, unchanged from today;
  - **paid on the day** (settled on day 0): `0.00` → `1000.00`, unchanged from today;
  - **un-settled, due today**: `100.00` → `900.00`, replacing today's `0.00` → `1000.00` (the payment-day overshoot);
  - **un-settled, one day overdue**: `100.00` → `900.00`, replacing today's `3.23` → `996.77`.
    Add the two boundary assertions the defect implies: `Available` never exceeds `Balance` on a payment day, and a temporary overdraft is not masked (with `Balance = 0` and a 100 bill due today, `Available` is `−100.00`, not `0.00`). The pure-calculation cases belong in `AccrueExpenseCalculatorFixture`; the window behaviour belongs in `ProjectionsServiceFixture`, including the day-1 case that the un-settled obligation is released once the window assumes the payment (OD-11).
- **Read-model tests** — new fixtures asserting the account accrual view (`TotalExpenseAccrued`, `DailyExpenseAccrual`, `StableExpenseAccrual`, per-expense `Accrued`) for representative expense mixes.
- **Projection regression** — `Pot.App.Tests/Features/Projections/ProjectionsServiceFixture.cs` gains the window-level cases from the settlement scenarios above; every other assertion must pass unchanged, and any further diff is a defect in the refactor.
- **Retired or moved fixtures** — `Pot.App.Tests/Features/Accruals/{Status/AccrualsStatusServiceFixture,AccrueExpenses/AccrueExpensesServiceFixture}.cs`, `Pot.App.Tests/Concerns/Accruals/AccrualDirtyStateManagerFixture.cs` and `Pot.Data.Tests/Specifications/AccountAccrualSpecificationsFixture.cs` are removed with their production counterparts; `Pot.Data.Tests/Extensions/ExpenseEntityExtensionsFixture.cs` retires with the helpers, whose arithmetic is then covered through the calculator.
- **Integration** — assert `/api/accounts` and `/api/expenses` values reflect an expense edit immediately, with no intervening accrual call; confirm `/api/accruals/status` returns only renewal requirements; confirm the maintenance export path (`Pot.AspNetCore.Integration.Tests/Features/Maintenance/Export/ExportFixture.cs`) reflects the narrower column set and still round-trips through the importer at v4; assert through the projections endpoint that `Available` does not exceed `Balance` on a payment day.
- **Client** — dashboard/accounts/expenses component and E2E coverage for the removed accrual surfaces, and confirmation that account values update after an expense edit without any accrual interaction.

## Non-Goals

- Any settlement, actuals, or occurrence-history store. See [PRD-024 — Settlement Ledger](PRD-024-settlement-ledger.md). POT is a forecasting tool rather than a full financial-tracking application: it projects available funds on the basis that income is received and expenses are paid on time, so nothing here counts what was actually paid or how far behind a schedule has fallen.
- Changing what "available" means, or the formulas that produce it.
- Changing accrual policy modes (PRD-003) or the accrual boundary conventions (PRD-009) beyond moving where and when they are applied.

## Related Documents

- [PRD-003 — Expense Accrual Policy Modes](PRD-003-expense-accrual-policy-modes.md): policy semantics (`Automatic` / `None`, optional accrual-start override) remain the engine's inputs.
- [PRD-009 — Accrual Dirty Flag Rules and UI Status](PRD-009-accrual-dirty-flag-rules-and-ui-status-prd.md): the dirty/status persistence model this feature retires, and the ended-boundary conventions the engine keeps.
- [PRD-013 — DB Readiness Health Check and Worker Startup Gate](PRD-013-db-readiness-health-check-and-worker-startup-gate.md): migrations are gated at startup, which shapes the two-step drop.
- [PRD-018 — Site Locale and Time Zone Settings](PRD-018-site-locale-settings.md): the as-of date and site-local date semantics this feature relies on.
- [PRD-022 — User and Site Deletion](PRD-022-user-and-site-deletion.md): its `AccountAccrual` sweep disappears if this lands first.
- [PRD-024 — Settlement Ledger](PRD-024-settlement-ledger.md): the independent, later track that captures actual amounts.
