# Projection Chart Improvements PRD

**Status:** Complete
**Priority:** Medium
**Last Updated:** 2026-10-03
**Feature ID:** 025
**Scope:** The projections chart's filter bar and read-out. Replaces the chart's two balance metrics with one `Account Balances` metric plus three `Include` deductions — `Reserved`, `Accruals`, `Arrears` — offered through one compact control, marks the period's low and high in the plot, and relabels the series legend from `Show:` to `Accounts` with the combined total shown separately. The period becomes a `1–12 month` picker, and on small screens the whole filter bar collapses behind a single `Options` disclosure. It also reduces the projections payload to the base balance plus the components the deductions subtract (the netting of the day's accrual stays on the server), so the chart composes the balances it plots instead of reading a derived one. As a scope extension (R11) it also deletes the client's expired legacy storage-key purge. A planned tooltip layer for the filter bar was dropped (see _Deferred_). _(Note, 2026-10-04: PRD-028 supersedes the combined total's semantics — the series becomes `Total (Selected Accounts)`, the sum of the legend-visible accounts, and the server's `global` aggregate is removed.)_
**Audience:** Future implementation agent(s), reviewers, and maintainers.
**Sequencing:** Depends on PRD-023 (`Arrears` and the assumed-payment basis of the `balance` line), which the projection code already implements.

## Purpose

Let a reader ask the balance question in one place, instead of choosing between two metrics whose difference they have to work out: one balance line, three deductions offered through one control that states its own basis, a read-out of the period's extremes, and a legend that names the accounts and keeps the combined total separate.

## Problem Statement

1. **Two balance metrics make the reader do the arithmetic.** `Account Balances` is the cash line; `Available Balances` subtracts `Reserved` and the obligations. Neither shows a balance net of just the past-due debt, the difference between them is not explained anywhere, and the obligation is only visible as the gap between two lines.
2. **The period has no read-out.** Reading the window's extremes means scanning the axis by eye, which gets harder as accounts are added or series are toggled.
3. **The series toggles do not name what they filter, and the total hides among the accounts.** They sit under a `Show:` label that states the action rather than the subject, and the combined `Total (All Accounts)` sits in the same group as the accounts it summarises.
4. **The filter bar does not hold up across widths.** The period presets are the widest element and push the bar into ragged, misaligned rows at intermediate widths, while on phones the bar consumes vertical space the chart needs.

## Current Behaviour

Verified against the code on 2026-10-01.

**Client** (`Source/Client/pot-react/src`)

- `data/projection.ts` — `ProjectionMetric` is `'balance' | 'available' | 'dailyAccrual' | 'incomeReceived' | 'expensesPaid'`. `PROJECTION_METRICS` maps each to a title, a filter label and a chart type (`Account Balances`, `Available Balances`, `Projection Accruals`, `Incomes`, `Expenses`; the first three are lines, the last two bars). `DEFAULT_PROJECTION_METRIC` is `'balance'`. `DateValues` mirrors the response's eight members exactly, `available` being required.
- `features/projections/components/ChartControls.tsx` — a compact labelled row holds `View` (the metric select, `aria-label` `Select chart metric to display`), `From` (the date picker), `Period` (a `Select` over `1 month … 12 months`) and `Include` (a `Popover` trigger whose label states the current basis, e.g. `Balance less: Arrears`, opening the three switches). A second row holds the `Accounts` legend: one toggle per account, then `Total (All Accounts)` after a divider (it is not an account). From `md` up everything is shown inline; on small screens the whole bar collapses behind a single full-width `Options` / `Hide options` button. Nothing in the bar has a tooltip (deferred).
- `features/projections/ProjectionsPage.tsx` — metric, period, start date and hidden series are page state, persisted through `useProjectionStorage`. The page **always requests 12 months** from the start date (`apiEndDate = startDate + 12 months − 1 day`); the period only filters the returned points client-side in `ProjectionChart.tsx`. The payload window is therefore _not_ the period shown.
- `features/projections/hooks/useProjectionStorage.ts` — persists `{ startDate, metric, period, hiddenSeries }` to both `sessionStorage` and `localStorage` under one user-scoped key. Read order is session, then local, then defaults, filling any missing field from the defaults with `??`. A local-only read is copied into session storage as-is. Every write goes through `updateStorage`, which spreads the current read result and overrides one key.
- `features/projections/hooks/useProjectionChartData.ts` — one point per date from `data.global`, each reading `dateBalance[metric]` per account plus a `global` value. This is the single seam where a metric becomes a plotted value. `hasData` is computed over the whole payload, not the displayed period, and ignores series visibility.
- `features/projections/components/ProjectionChart.tsx` — already computes the visible aggregate range (`getVisibleValueRange`: min and max over the displayed period's points and the visible series) to place the Y-axis domain, which pads it by 10%. It renders a zero `ReferenceLine` only when the domain spans zero. The chart tooltip reads the plotted series. The income/expense detail sheets serve only the `incomeReceived` and `expensesPaid` metrics.
- Hint machinery — `components/feedback/Hint.tsx` (moved from `components/table/ColumnHeaderHint.tsx` in this work) wraps a trigger in a local `TooltipProvider` (`delayDuration={0}`) and renders `TooltipContent` with `max-w-sm text-wrap`. `TooltipProvider` is also mounted in `App.tsx`. `components/ui/switch.tsx` (Radix) exists and is used by the expense, income and budget-reminder forms. `EnrichedDatePicker` renders its own trigger button and exposes no hint prop.

**Server** (`Source/Server`)

- `Pot.App/Features/Projections/Models/DateProjection.cs` — the published day: `Date`, `Balance`, `Available`, `DailyAccrual`, `IncomeReceived`, `ExpensesPaid`, `ExpenseItems`, `IncomeItems`. The API returns it unchanged (`Pot.AspNetCore/Features/Projections/Get/Response.cs` passes `Output.Accounts` and `Output.Global` through).
- `DateProjectionValues` (internal) already carries `StartingBalance`, `Accrued`, `Arrears`, `Reserved` and `AccrualSettledByPayments` per account per day and for the global series (the global values are the per-account sums). None of the last four reaches the response.
- `ProjectionsService.MapToDateBalanceAvailable` computes `Balance = StartingBalance + IncomeReceived − ExpensesPaid` and `Available = Balance − Reserved − Accrued − Arrears + AccrualSettledByPayments`. `Available` is the response's one derived member, and its only readers are the `available` metric entry and the server tests that pin it.
- `Arrears` is measured once on the read date and held for every day of the window (`state.HoldArrears`); `Reserved` is the account's stored value.

## Requirements

- **R1 — One balance metric.** Remove `Available Balances` from the metric select. The remaining `balance` entry keeps its key and its label and chart title `Account Balances`; its context is already the projections page. The other entries (`Projection Accruals`, `Incomes`, `Expenses`) are unchanged. `'available'` leaves the `ProjectionMetric` union and the `available` payload member is retired with it (R6), so no reader can select a figure the payload no longer derives.
- **R2 — Three `Include` deductions, offered through one control.** A single `Include` control renders only while `Account Balances` is the selected metric; under the other three metrics there is no balance to subtract from, so it is not rendered, and its state is retained while hidden (it lives in the persisted `include` record). Its label states the current basis at rest — `Balance less: Arrears` with the defaults, `Balance` when all three are off — and opening it reveals three switches labelled `Reserved`, `Accruals` and `Arrears` (accessible names `Include Reserved`, `Include Accruals`, `Include Arrears`). Each switch, when on, subtracts its component from the plotted balance: `Reserved` subtracts `reserved`, `Arrears` subtracts `arrears`, and `Accruals` subtracts `unpaidAccrual`, **not** the raw `Accrued` shown on the accounts page. `unpaidAccrual` is the day's accrual net of the part already covered by that day's assumed payments, because the base `balance` has already had those payments deducted; subtracting the raw accrual would deduct the same bill twice. For a bill due today and assumed paid, `unpaidAccrual` is therefore `0` (the bill is already in `balance`), and switching `Accruals` on changes nothing for that bill. The basis label is always visible at rest, so the meaning never depends on a hover tooltip.
- **R3 — Defaults.** `Reserved` off, `Accruals` off, `Arrears` **on**, so a first load plots the balance net of past-due debt.
- **R4 — Persistence.** The three states are persisted in `useProjectionStorage` beside the metric, period and start date, as one `include` object of three booleans. Validation is per field and always falls back to the default rather than migrating: a stored `metric` that is not a current `PROJECTION_METRICS` key (for example the retired `'available'`) resolves to the default metric, and a missing or malformed `include` (or any non-boolean member) resolves to the default for that member (off/off/on, A-01). The check applies to the session read and the local read alike, and the local-to-session seeding copy writes the validated record. Because `updateStorage` spreads the validated read, the next write of any setting leaves both stores holding valid values, so there is no migration code to remove later. A returning reader who had `Available Balances` selected therefore sees the default view; that change in behaviour is accepted.
- **R5 — One scope for all series.** The switches apply to every visible series — each account line and the combined total alike. There is no per-series switch state.
- **R6 — The payload carries the balance and its components, and nothing derived.** Each day on each account, and on the global series, exposes `reserved`, `arrears` and `unpaidAccrual` alongside `balance`, and `available` is removed. The contract is in _Payload contract_ below.
- **R7 — Min/max read-out.** Mark the extremes of the plotted values as two horizontal reference lines, at the **visible aggregate**: the lowest and highest value across every visible series over the displayed period (hidden series excluded). This is the range `getVisibleValueRange` already computes, reused as-is. That function is a closure inside `ProjectionChart`, so the read-out uses it in place rather than extracting it.
  - Each line spans the plot's full width and carries its value as plain text at the plot's left edge, inside the plot, styled to match the Y-axis labels, with a background-coloured halo so it masks whatever it crosses. The high value sits above its line and the low value below its own, so the two extend away from each other and cannot collide however close the values are. Since no plotted value lies above the high or below the low, the value can never cover data. Where it crosses the grid or the zero line the halo simply masks it. A low equal to the high renders one line with one value, above the line.
  - Labels carry the value only, formatted with `formatMoneyValue` (as the Y axis and chart tooltip do). With several accounts the aggregate extreme may sit on no single account, so naming one would usually misattribute it.
  - The lines are thinner and lower-contrast than the `6 6` zero line and no heavier than the `3 3` grid, so they read as annotations rather than as another series. They measure the plotted values, so they follow the switches.
  - Nothing is rendered when the chart is in its no-data state (`hasData` false), when every series is hidden, or under a bar metric. The read-out belongs to **line** metrics, which are `Account Balances` and `Projection Accruals` (A-02).
- **R8 — `Accounts` legend, with the total separate.** Relabel the series-toggle group from `Show:` to `Accounts`, and render `Total (All Accounts)` as its own toggle outside the group (after a divider at `sm` and above), since it is a combined total rather than an account. The per-series toggles keep their accessible names (`Hide <name> account on chart` / `Show <name> account on chart`) and their `aria-pressed` state, and the group keeps its `aria-labelledby` wiring. The chips wrap rather than scroll horizontally, and long account names truncate.
- **R9 — Baseline unchanged.** With all three switches off, every series is identical to today's `Account Balances`, and the other metrics are untouched.
- **R10 — Filter-bar help text (deferred).** A tooltip layer for the filter bar was planned and dropped on 2026-10-03; see _Deferred_ below. Nothing in the bar is hover-only: the `Include` control states its basis in its own label (R2).
- **R11 — Remove the legacy storage-key purge (scope extension).** Not part of the chart work; included because the projections hook is being reworked anyway and the change is mechanical (A-10). Accepted as scope in this PRD rather than a separate one; implement it as its own commit so it can be reverted independently. Delete `concerns/storage/storageMigration.ts` and its test (`tests/concerns/storage/storageMigration.test.ts`), the `purgeLegacyStorageKeys` and `LegacyStorageKey` exports in `concerns/storage/index.ts` and `concerns/index.ts`, and all seven call sites, together with their `TEMPORARY` comments, now-unused imports and the `if (userId)` wrappers that exist only for the purge:
  - `useProjectionStorage` (`pot-projections`), `useAccountStorage` (`pot-accounts`), `useDashboardStorage` (`pot-dashboard`), `useExpenseStorage` (`pot-expenses`), `useIncomeStorage` (`pot-incomes`);
  - `stores/useUserStore.ts` (`pot-user`, a module-level call);
  - `components/theme/ThemeProvider.tsx` (`pot-ui-theme`, including `LEGACY_THEME_STORAGE_KEY`; `readThemeFromStorage` then only reads the provided key, and its comment drops the "removing legacy keys" clause).

  Behaviour of the scoped keys is unchanged. The tests that exist only to assert the purge go with it: the `legacy key cleanup` block in `useProjectionStorage.test.ts` and its equivalents in the accounts, dashboard, expenses and incomes storage tests, `tests/stores/useUserStore.storageMigration.test.ts`, and the purge cases in `ThemeProvider.test.tsx` (`purges legacy pot-ui-theme key…`; the `does not purge the env-scoped global theme key` case stays as a check that the scoped key is read, renamed accordingly).

- **R12 — Period as a 1–12 month picker.** Replace the `1/2/3/6/9/12` presets and the `Custom` month input with one single-choice control offering every whole month from `1` to `12`. The page always fetches a 12-month window, so `1–12` is the complete domain: `12` shows the whole fetched window and `1` trims it to the first month. The control shows the current value (`6 months`, `1 month`) and its accessible name is `Select chart period in months`. Selection still drives the existing `period` state and persistence (R4) and still trims client-side only; it does not refetch.
- **R13 — Small-screen `Options` disclosure.** On small screens the entire filter bar — the four facets and the `Accounts` legend — collapses behind one full-width `Options` / `Hide options` button, so the bar is a single button until the reader opens it. From `md` up the controls are always shown inline. The four facets share one row only from `lg`: from `md` to `lg` they lay out as a 2×2 grid (`View` / `From`, then `Period` / `Include`), because the sidebar takes 16rem from `md` up and the single row needs roughly 1010px of content. Nothing hover-dependent is lost when the disclosure is closed, because the `Include` trigger states the basis in its own label (R2).
- **R14 — Empty state when every series is hidden.** When the payload has data but every legend toggle is off (each account and the combined total), the plot is replaced by a centred message — `No account selected` / `Select at least one account to display the chart.` — rather than an empty set of axes. The no-data state (`hasData` false) keeps its own `No data available` wording. Both states share the existing empty-state component, which takes the title and description as props.

### Payload contract

Server — `DateProjection` (per account and global, same shape):

| Member          | Change  | Meaning                                                                                                    |
| --------------- | ------- | ---------------------------------------------------------------------------------------------------------- |
| `balance`       | kept    | `StartingBalance + IncomeReceived − ExpensesPaid`                                                          |
| `reserved`      | **new** | the account's stored reserved amount (global: the sum)                                                     |
| `arrears`       | **new** | past-due obligation measured on the read date and held for every day of the window (global: the sum)       |
| `unpaidAccrual` | **new** | `Accrued − AccrualSettledByPayments`: the day's accrual net of the part its assumed payments already cover |
| `available`     | removed | by construction `balance − reserved − unpaidAccrual − arrears`                                             |

`dailyAccrual`, `incomeReceived`, `expensesPaid`, `expenseItems` and `incomeItems` are unchanged. The global members are the per-account sums the loop already holds, so `unpaidAccrual` for the global series is the sum of the accounts' because the netting is linear. The accounts read and the expenses read are untouched.

Client — `DateValues` drops `available` and gains `reserved`, `arrears` and `unpaidAccrual`; `ProjectionMetric` drops `'available'`. `data/projection.ts` gains `ProjectionInclude` (`{ reserved, accruals, arrears }`, all booleans) and `DEFAULT_PROJECTION_INCLUDE` (off/off/on). `useProjectionChartData` takes the include state as a third argument and, for the `balance` metric only, plots `balance − (reserved) − (unpaidAccrual) − (arrears)` with each bracket present when its switch is on. The global point composes from the global payload members, which the server sums, so it equals the sum of the composed account values. Every other metric reads its member directly as today.

### Help text (deferred)

The tooltip layer was dropped on 2026-10-03 (R10, D-7); the `Include` control states its basis in its own label instead. The indicative copy below is kept for when hints are picked up again:

| Control        | Copy                                                                                             |
| -------------- | ------------------------------------------------------------------------------------------------ |
| `View`         | Choose what the chart plots.                                                                     |
| `From`         | First day shown. Today or later.                                                                 |
| `Period`       | How many of the 12 fetched months to show.                                                       |
| Account toggle | Show or hide this series on the chart.                                                           |
| `Reserved`     | Subtract each account's reserved amount.                                                         |
| `Accruals`     | Subtract what is being set aside for upcoming bills. A bill due today is already in the balance. |
| `Arrears`      | Subtract bills already past due that have not been renewed.                                      |

## How the switches compose

**What `Accruals` subtracts.** The switch subtracts `unpaidAccrual` (accrual net of the day's assumed payments), never the raw `Accrued`. With a 1000 account and a 70 bill due today, `balance` is already `930`, raw `Accrued` is `70`, and `unpaidAccrual` is `0`; the `Accruals` switch therefore leaves day 0 at `930`. It only starts to subtract once the bill is no longer due on the day being measured (day 1: `10`).

The base line is the projection's `balance` (`StartingBalance + IncomeReceived − ExpensesPaid`), which assumes every bill is paid on its due date and every income received on its. At the read date it is therefore the account's **entered balance, plus the income assumed received that day and less the expenses assumed paid that day** — not the raw entered balance whenever something falls due or is received today. Each switch subtracts one component:

| Reserved | Accruals | Arrears | Plotted value                                  | Equivalent to                       |
| -------- | -------- | ------- | ---------------------------------------------- | ----------------------------------- |
| off      | off      | off     | `balance`                                      | today's `Account Balances` metric   |
| off      | off      | **on**  | `balance − arrears`                            | **the default**                     |
| off      | on       | off     | `balance − unpaidAccrual`                      | a reading no metric shows today     |
| off      | on       | on      | `balance − unpaidAccrual − arrears`            | `Available` plus `reserved`         |
| on       | on       | on      | `balance − reserved − unpaidAccrual − arrears` | today's `Available Balances` metric |

The remaining combinations follow the same rule. A bill due today is accrued in full **and** already deducted from `balance` (a 100 bill due today reads `balance = 900` on a 1000 account), so subtracting the raw accrual would count it twice. `unpaidAccrual` is the accrual with that double-count removed, so a bill due today is deducted from **every** combination and is owned by no switch. The account's own balance on the accounts read is a different number: it does not assume the payment.

### Worked example: checking for double-counting

Inputs: account balance `1000`; `Reserved` `100`; no incomes; one expense of `70` on a 7-day cycle (`Frequency.Weeks`, count 1) with `AccrualPolicy.Automatic`, so it accrues `10` a day. Values follow the calculator and projection loop as they stand (PRD-023): each day is measured against the cursor at the start of the day, then that day's renewal is folded in.

#### Scenario A — bill due today on day 0, not yet paid; due again on day 7

On day 0 the persisted cursor is `NextDue = day 0`. The bill is not renewed, so the accounts page still shows the full `1000` and counts the bill as `Accrued = 70`. The chart assumes it is paid on its due date.

| Day | Start-of-day cycle      | `balance` | raw `Accrued` | settled by the day's payment | `unpaidAccrual` | `arrears` |
| --- | ----------------------- | --------- | ------------- | ---------------------------- | --------------- | --------- |
| 0   | due today, assumed paid | 930       | 70            | 70                           | **0**           | 0         |
| 1   | cycle day 1 of 7        | 930       | 10            | 0                            | 10              | 0         |
| 2   | cycle day 2 of 7        | 930       | 20            | 0                            | 20              | 0         |
| 3   | cycle day 3 of 7        | 930       | 30            | 0                            | 30              | 0         |
| 4   | cycle day 4 of 7        | 930       | 40            | 0                            | 40              | 0         |
| 5   | cycle day 5 of 7        | 930       | 50            | 0                            | 50              | 0         |
| 6   | cycle day 6 of 7        | 930       | 60            | 0                            | 60              | 0         |
| 7   | due today, assumed paid | 860       | 70            | 70                           | **0**           | 0         |

Plotted value per switch combination. `Arrears` is `0` throughout, so each column covers the combination with `Arrears` on and off.

| Day | Nothing, or `Arrears` only (default) | `Accruals` | `Reserved` | `Reserved` + `Accruals` (all on) |
| --- | ------------------------------------ | ---------- | ---------- | -------------------------------- |
| 0   | 930                                  | 930        | 830        | **830**                          |
| 1   | 930                                  | 920        | 830        | 820                              |
| 2   | 930                                  | 910        | 830        | 810                              |
| 3   | 930                                  | 900        | 830        | 800                              |
| 4   | 930                                  | 890        | 830        | 790                              |
| 5   | 930                                  | 880        | 830        | 780                              |
| 6   | 930                                  | 870        | 830        | 770                              |
| 7   | 860                                  | 860        | 760        | 760                              |

How to read it:

- **No double count.** The `Accruals` column falls by exactly `10` every day, `930 → 860`, including across the payment on day 7 (`870 → 860`). That is what a steady `10` a day set aside should produce: by day 7 the `70` is fully set aside and the payment takes it from the balance, so the payment does not change what is spendable. Subtracting the raw `Accrued` (`70`) on day 0 instead of `unpaidAccrual` (`0`) would deduct the day-0 bill twice.
- **Day 0 matches the accounts page.** With everything on the chart reads `830`; the accounts page computes `Balance − Reserved − TotalCommitted = 1000 − 100 − 70 = 830`. The two views start from different balances (`1000` against `930` after the assumed payment) but agree on what is spendable.
- **The default reads the balance after the assumed payment.** With `Arrears` the only switch on, nothing is owed, so the line is the forecast balance: `930`, then `860` once the day-7 bill is paid.
- **`Reserved` is an independent, constant `100`** in every column, so it cannot interact with the accrual.

#### Scenario B — the same account with `50` of arrears

The same inputs, plus a second expense: a one-time `50` bill that fell due yesterday and has not been settled. It carries `arrears = 50`, accrues nothing, and is not due on any day of the window, so it is never assumed paid and `balance` is unchanged from scenario A. The "accrues nothing" premise is confirmed against the real service by characterisation fixtures before implementation starts. Arrears is measured on the read date and held for every day.

Each cell shows `amount => plotted value`, where the plotted value is `balance − amount` for that switch on its own.

| Day | `balance` | `Reserved` | `Accruals` (`unpaidAccrual`) | `Arrears` |
| --- | --------- | ---------- | ---------------------------- | --------- |
| 0   | 930       | 100 => 830 | 0 => 930                     | 50 => 880 |
| 1   | 930       | 100 => 830 | 10 => 920                    | 50 => 880 |
| 2   | 930       | 100 => 830 | 20 => 910                    | 50 => 880 |
| 3   | 930       | 100 => 830 | 30 => 900                    | 50 => 880 |
| 4   | 930       | 100 => 830 | 40 => 890                    | 50 => 880 |
| 5   | 930       | 100 => 830 | 50 => 880                    | 50 => 880 |
| 6   | 930       | 100 => 830 | 60 => 870                    | 50 => 880 |
| 7   | 860       | 100 => 760 | 0 => 860                     | 50 => 810 |

How to read it:

- **Each switch deducts its own amount once.** `Reserved` is a constant `100` and `Arrears` a constant `50`; only `Accruals` moves, by `10` a day. With several switches on, the amounts add: day 0 with all three on is `930 − 100 − 0 − 50 = 780`, and day 6 is `930 − 100 − 60 − 50 = 720`.
- **`Accruals` is `0` on the payment days (0 and 7).** The bill is already deducted from `balance` on those days, so `unpaidAccrual` is `0` rather than the raw `70`.
- **`Arrears` never overlaps `Accruals`.** The `50` is a past-due occurrence; the `Accruals` amounts belong to the cycle in progress of the `70` bill. They come from different occurrences and are never in the same figure.
- **`Arrears` is not part of `balance`.** It is subtracted on every day, including after the day-7 payment, and is never added back or released.

#### Conclusion

Neither scenario double-counts. The only place a double count could arise is a bill that is both taken out of `balance` and accrued on its due day, and `unpaidAccrual` removes exactly that overlap. `arrears` cannot overlap with it, because an occurrence is arrears only when its due date is strictly before the read date, so the same occurrence is never in both. Both worked examples are carried into the tests (see Tests).

### Background: why the netting stays on the server

_Rationale only; this section specifies no behaviour beyond R6._ The projection response no longer carries `available`. It is described here as the **retired** member, because its derivation is what `unpaidAccrual` now publishes.

The retired `available` was the only derived member, and its derivation was not one subtraction: it re-added the part of the day's accrual that the day's own assumed payments settle. R6 does not hand that correction to the client. The add-back is the _definition_ of one published number. With `Settled` for `AccrualSettledByPayments`, the retired expression is:

```
retired available = balance − reserved − Accrued − arrears + Settled
                  = balance − reserved − (Accrued − Settled) − arrears
                  = balance − reserved − unpaidAccrual − arrears
```

`unpaidAccrual` is evaluated by the accrual engine row by row, where the row facts and schedule cursor live. The client never sees `Accrued` or `Settled`; it subtracts three numbers.

| Quantity        | Derivable by a consumer? | Why                                                                                                            |
| --------------- | ------------------------ | -------------------------------------------------------------------------------------------------------------- |
| `reserved`      | Yes                      | a stored account field, validated `≥ 0` on create and update                                                   |
| `arrears`       | **No**                   | needs the schedule walked from the persisted `NextDue`, counting every un-settled past-due occurrence, per row |
| `unpaidAccrual` | **No**                   | needs each due row's own accrued amount; `expenseItems` carry `rowId`, `description` and `amount` only         |
| `balance`       | **No**                   | a running forecast, folded day by day through renewal and the assumed-payment basis                            |

The switches force the three components to be published. Once they are, the retired `available` is composed entirely of published members and carries no content of its own, which is the only reason it is removed.

**Where the safety property goes.** The invariant the retired member satisfied, `available ≤ balance`, becomes component non-negativity, because every term the balance nets out is `≥ 0` by construction:

```
reserved ≥ 0  ∧  unpaidAccrual ≥ 0  ∧  arrears ≥ 0   ⟹   (balance − reserved − unpaidAccrual − arrears) ≤ balance
```

`reserved` is validated non-negative; `arrears` counts whole occurrences; `Settled` sums the accruals of a subset of the rows making up `Accrued`, so `unpaidAccrual` cannot go below zero. The property is asserted server-side against the payload's components (see Tests).

## Decisions

Settled by the user or in review before this rewrite.

| ID   | Decision                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D-1  | **Min/max scope is the visible aggregate** — one pair for every visible series, hidden series excluded, which is the range the Y-axis domain already uses. Per-series extremes duplicate the hover tooltip and make a table rather than a glance; including hidden series would contradict the axis and legend.                                                                                                                                                                                                                                                                       |
| D-2  | **Min/max placement is full-width reference lines labelled at the plot's left edge.** Rejected: a chart-header pair (too far from the curve), a second Y-axis rail (an axis-shaped legend that implies the series are measured against it; an honest rail duplicates the left axis), extreme markers (collide with overlapping series and the zero line), a footer, tooltip-only (no use on touch), and a floating text block.                                                                                                                                                        |
| D-3  | **Min/max edge cases hide or dedupe.** No read-out for an empty window or all series hidden; one line and one label when low equals high. A `—` or `$0` would assert a balance the projection does not have.                                                                                                                                                                                                                                                                                                                                                                          |
| D-4  | **The switches are hidden under the non-balance metrics.** A switch subtracts from a balance; under the other metrics it has nothing to act on.                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| D-5  | **No basis statement on the chart plot.** The `Include` control states the basis in its own label (`Balance less: Arrears` / `Balance`, A-11), the real bank balance stays on the accounts read rather than being restated here, and the plot carries only the read-out (R7).                                                                                                                                                                                                                                                                                                         |
| D-6  | **Only the chart tooltip applies.** There is no detail panel for the balance metric, since the detail sheets serve `Incomes` and `Expenses`. The tooltip reads the plotted series, so it reports the switched values.                                                                                                                                                                                                                                                                                                                                                                 |
| D-7  | **Hints are deferred** (2026-10-03). The planned tooltip layer (one per switch plus short hints on the other filter-bar controls) is not built; the `Include` control states its basis in its label (R2, R10). See _Deferred_.                                                                                                                                                                                                                                                                                                                                                        |
| D-8  | **The full switch set is kept** although three readings carry the story. The reader composes the question rather than choosing from approved readings, and `Include Reserved` lets the all-on case reproduce today's `Available Balances` exactly. Effort was explicitly not the deciding factor (user decision 2026-09-30).                                                                                                                                                                                                                                                          |
| D-9  | **`available` is removed rather than kept beside the components** (R6). Keeping it is cheapest, but leaves two implementations of one figure and a member whose every input is already published.                                                                                                                                                                                                                                                                                                                                                                                     |
| D-10 | **The shared hint component is the existing `ColumnHeaderHint`, moved and renamed.** `components/table/ColumnHeaderHint.tsx` becomes `components/feedback/Hint.tsx` (props and behaviour unchanged, including the `max-w-sm text-wrap` content), and its call sites in `DataTableColumnHeader.tsx` and `dataTableColumnFactories.tsx` are repointed. A second wrapper was rejected because it would duplicate a 20-line component and its wrap handling. The component is in place and used by the table column headers; the filter-bar hints it was prepared for are deferred (D-7). |

## Assumptions

Decisions made while rewriting this document from its open questions, each resolved from the current code. They are for review; change any of them and the requirement it names moves with it.

| ID   | Assumption                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Requirement  |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ |
| A-01 | **A stored record with no switch state gets the defaults (off/off/on).** That is the existing rule in `useProjectionStorage`, which fills every missing field from the defaults, and it means a returning reader with the old stored `'balance'` sees the default reading. The alternative — preserving their old all-off reading — would make the arrears default apply only to an empty store, which is almost nobody. The accepted consequence is that the line changes for returning readers whose accounts have arrears; an up-to-date account reads the same either way.          | R3, R4       |
| A-02 | **The read-out applies to line metrics, including `Projection Accruals`.** R7's stated reason for exclusion was about bars anchored at zero; `dailyAccrual` is a line in the same money unit whose peak is meaningful. The rule is therefore `chartType === 'line'`, matching the branch the chart already takes.                                                                                                                                                                                                                                                                       | R7           |
| A-03 | **The whole filter bar collapses behind one `Options` disclosure on small screens** (updated 2026-10-03). The bar holds four facets plus the accounts legend; at phone widths that consumes vertical space the chart needs, so `View`, `From`, `Period`, `Include` and the `Accounts` legend sit behind a full-width `Options` / `Hide options` button, and `md` up shows them inline. The basis of the plotted balance stays discoverable because the `Include` trigger states it in its own label. This supersedes the earlier "the switch row is always visible on mobile" decision. | R2, R12, R13 |
| A-04 | **The switch label stays `Accruals`** (from the requirements and index row), although the accounts vocabulary is `Accrued` (PRD-023 §8). The figure it subtracts differs from the accounts `Accrued` only on a payment day, and the select already uses "Accruals" (`Projection Accruals`). The control's basis label carries the distinction (a bill due today is already in the balance) rather than a different word. The payload member is `unpaidAccrual`.                                                                                                                         | R2, R6       |
| A-05 | **`metric` identity is unchanged.** The code key stays `'balance'`; the filter label and chart title stay `Account Balances`. The requirements use `Account Balances` for the metric and `Balance` only for the plotted-value formula. The index row in `Docs/Future/README.md` is corrected to match.                                                                                                                                                                                                                                                                                  | R1           |
| A-06 | **The no-data gate is the existing `hasData`, computed from the composed values.** It is evaluated over the whole payload, not the displayed period, and ignores visibility today; this PRD does not change that, but it must be fed the plotted (switched) values so a window whose plotted values are all zero shows the no-data state.                                                                                                                                                                                                                                               | R7           |
| A-07 | **Non-negativity of `unpaidAccrual` is asserted with a small tolerance (1e-9), and production does not clamp it.** The two sums are accumulated over different row subsets in different orders, so an account whose rows are all due can come out a few ulps below zero. A clamp would hide a genuine unbounded add-back, which the assertion exists to catch.                                                                                                                                                                                                                          | Tests        |
| A-08 | **The server fixtures keep their numbers.** `ProjectionsServiceFixture` (36 `Available` mentions) gets a local helper that composes `balance − reserved − unpaidAccrual − arrears` from the published members, so every pinned value and the comments that explain it survive unchanged. `AvailableInvariantFixture` is rewritten against the components (composed value plus non-negativity) and renamed accordingly.                                                                                                                                                                  | Tests        |
| A-09 | **Client fixtures are edited, not retired.** The files that hard-code `'available'` or `available:` are listed under Tests so the change is scoped rather than discovered.                                                                                                                                                                                                                                                                                                                                                                                                              | Tests        |
| A-10 | **The legacy storage-key purge is deleted everywhere (R11).** `purgeLegacyStorageKeys` is marked `TEMPORARY` and only ever removes pre-scoping flat keys (introduced 2026-05-03, commit `c641e6e7`, about five months ago); it never reads them, so nothing is migrated and removal cannot lose data. Its own note says to delete the helper once enough time has passed, which is long since true. The cost of skipping it is stale flat keys left in browsers that have not opened the app since; accepted, since those values are unused.                                            | R11          |
| A-11 | **The basis is stated by the `Include` trigger label, not a separate caption** (updated 2026-10-03). Radix tooltips open on hover and focus, not touch, and the tooltip layer is deferred, so the control states its own basis at rest (`Balance less: Arrears` / `Balance`). The earlier separate touch caption is retired with it.                                                                                                                                                                                                                                                    | R2           |

## Open Questions

None. Every question raised against the earlier draft is now settled, as a decision or an assumption above.

## Deferred

- **Filter-bar tooltips (R10).** Dropped on 2026-10-03. The shared `Hint` component (D-10) is ready and used by the table column headers; the indicative copy under _Help text (deferred)_ is kept for when this is picked up. The `Include` control's basis label covers the touch case that motivated the caption.
- **`All` / `None` legend actions.** Not wanted (2026-10-03); revisit only if account lists grow.
- **"N of 12 months" hint on the period.** Not wanted (2026-10-03); the chart's own date-range subtitle states the window.

## Deployment

Client and server ship together, so removing `available` from the projections response needs no compatibility period.

## Non-Goals

- **No change to the settlement model.** `Arrears` stays held for the whole window and is never debited from `Balance`; a bill due today stays assumed paid.
- **No change to the assumed-payment basis.** The base line stays the entered balance adjusted by that day's assumed movements; the accounts read keeps showing the real balance.
- **No new series.** No dedicated arrears or accrual line.
- **No change to the accounts or expenses reads, or to the maintenance export.** `available` is not removed from the application, only from the projections response. `GET /api/accounts` and `GET /api/accounts/{id}` keep their own `available` (`Balance − Reserved − TotalCommitted`, computed on the entered balance with no assumed payments), which backs the accounts table's `Available` column, the account cards and their status badges, and the dashboard total. It is a different figure from the projection member retired here and is untouched.
- **No derived member on the projections payload.** `available` is removed rather than deprecated.
- **No second Y axis** and **no per-series switch state.**
- **No budgeting, actuals or seasonality work.** That is [PRD-024 — Settlement Ledger](PRD-024-settlement-ledger.md)'s territory.
- **No schema change.**

## Tests

- **App (payload)** — the response carries `reserved`, `arrears` and `unpaidAccrual` on every emitted day, per account and for the global series; `unpaidAccrual` excludes the part a due-today bill's assumed payment covers and a non-accruing row settles nothing (A-08); the global value is the sum of the accounts'; the response no longer carries `available`.
- **App (components)** — every published component is non-negative on every emitted day, per account and global: `reserved ≥ 0`, `arrears ≥ 0`, `unpaidAccrual ≥ −1e-9` (A-07). This is `Available ≤ Balance` re-homed onto the components, and catches an unbounded add-back.
- **App (existing fixture)** — `ProjectionsServiceFixture`'s `Available` assertions move to the local composing helper (A-08); no expected value changes.
- **App (double-count scenarios)** — the two worked examples under _How the switches compose_ become fixtures on the same inputs (`balance` 1000, `Reserved` 100, a 70 weekly bill): scenario A and scenario B (the same account with a past-due one-time 50 bill) across days 0–7, asserting the published `balance`, `reserved`, `unpaidAccrual` and `arrears` per day. The client chart-data test then asserts the composed series for every switch combination against the tables, including that the `Accruals` series falls by exactly 10 a day across both payment days.
- **Integration** — `AvailableInvariantFixture` (renamed, A-08) asserts through `/api/projections` that the members are present and `available` is absent, that the composed value on a payment day equals `balance` for an accruing bill and for `AccrualPolicy.None`, and that no component is negative.
- **Client (chart data)** — each switch subtracts only its own component; all three on reproduces the retired `available`, pinned against a fixture captured before the removal; all off equals today's `Account Balances`; the default is off/off/on; the global series equals the sum of the accounts; other metrics are untouched.
- **Client (storage)** — a stored `'available'` (or any unknown metric) loads the default metric and default switches from session storage and from local storage, and the next write leaves neither store holding it; a record with no `include`, or with a non-boolean member, loads the default for that member (A-01); valid stored values are returned unchanged.
- **Client (filter bar)** — the `Period` picker offers every month `1`–`12` and shows the current value (`6 months`; singular for `1`); the `Include` control states the basis (`Balance less: Arrears`, `Balance` when none are active) and reveals the three switches with accessible names; the control is absent under the other three metrics and keeps its state while hidden; the bar collapses behind the `Options` disclosure on small screens and is always shown from `md` up. This replaces the earlier "renders without expanding the filters" and "caption reflects state" assertions (A-03, A-11).
- **Client (component)** — the three switches apply to every series and persist across a revisit; the legend renders one toggle per account, with `Total (All Accounts)` as a separate toggle, and the chips wrap rather than scroll (R8).
- **Client (min/max)** — (jsdom renders a 0×0 plot because the shared `ResizeObserver` mock never reports a size; the test file removes that global so Recharts keeps its 320×200 initial dimension) the value renders at the visible low and high; hidden series do not move them; nothing renders in the no-data state, with every series hidden, or under a bar metric; a low equal to the high renders one line and one value; the read-out renders under `Projection Accruals` (A-02) and follows the switches. Label offsets and theme contrast are left to the manual pass.
- **Client (help text)** — deferred with R10/D-7; nothing to assert until hints are built.
- **Client (legend)** — the group reads `Accounts`; `Total (All Accounts)` is a separate toggle outside the group; the toggles keep their accessible names (`Hide <name> account on chart` / `Show <name> account on chart`) and `aria-pressed` state.
- **Client (empty state)** — with data present but every series hidden in the legend, the plot is replaced by the `No account selected` message and no chart is rendered; the no-data case keeps its own `No data available` wording.
- **E2E** — toggling each switch moves the plotted balance by that component alone.
- **Client (R11)** — the client type-check and the full client suite pass with the purge deleted; nothing imports `storageMigration`, and the remaining storage-hook and theme tests still pass unchanged apart from the removed purge cases.

**Client fixtures and tests that hard-code `'available'`** (A-09): `tests/data/projection.test.ts` (asserts the `PROJECTION_METRICS` key list exactly); `tests/shared/factories/projectionFactory.ts` (sets `available` on every `DateValues`, and needs the three new members); `tests/features/projections/hooks/useProjectionStorage.test.ts` (stores and seeds `'available'` throughout); `tests/features/projections/ProjectionsPage.test.tsx` and `tests/integration/CoreFlows.ProjectionFlow.test.tsx` (each calls `onMetricChange('available')` and seeds `metric: 'available'`).

## Documentation

- `Docs/USER-GUIDE/Projections.md` — the metric table was updated to the four current metrics, and the `Available Balances Interpretation` section was replaced by an `Account Balances And The Include Deductions` section covering the three `Include` deductions and their defaults, the read-out, the `Accounts` legend (with the separate total) and the `Options` collapse. Its statement that the page always requests a 12-month window was unchanged.
- `Source/Server/DEVELOPER.md` (line ~5086) and `Source/Server/IMPLEMENTATION.md` (line ~119, and the record step) — both were updated to state that the projections read publishes `balance`, `reserved`, `arrears` and `unpaidAccrual`, and that the chart composes the value it plots.
- **PRD-023 §2 and §8** — §2's chart `Available` form and §8's "gains no field of its own" each took a dated note recording the retirement of the derived projection member.
- `Docs/Future/README.md` — the 025 row's wording was aligned to A-05 and its date updated.

## Design Notes

- **Why merge rather than add a modifier.** Two balance metrics made the reader work out their difference; one line plus three explicit switches states it, and removes the question of which metric a modifier belongs to.
- **Why the metric keeps the label `Account Balances`.** Its context is the projections page, so it already reads as a projected balance; renaming would cost familiarity for no clarity gain.
- **Why arrears defaults on.** An account that is up to date reads the same either way, so the default changes only what an account that is behind sees first — the case this feature exists for. The accepted consequence is that the first-load line can sit below the real bank balance; the `Include` control states the basis in its own label (`Balance less: Arrears`).
- **Why the read-out reuses the visible-aggregate range.** The chart already computes it on every render to place the Y domain, so the read-out adds no state or computation, and the lines can never fall outside the domain.
- **Why the lines are marked in the plot and kept though near-redundant.** A header pair sits a card's width from the curve. With the domain padded by 10%, the curves already sit close to the low and high lines, so the line's value is as the anchor that attaches the exact figure to the right height.
- **Why the lines look unlike the other dashes.** The plot already carries the `3 3` grid and the `6 6` zero line, so the low and high lines are thinner and lower-contrast rather than a third competing dash pattern.
- **Why the read-out hides rather than showing a placeholder.** A `—` or `$0` reads as a zero balance, which an empty window does not support.
- **Why the group is labelled `Accounts`, with the total separate.** The label names the subject the reader toggles (their accounts), and moving `Total (All Accounts)` out of the group keeps it from reading as one of them.
- **Why the period is a 1–12 picker.** The window is always 12 months, so the period is just a number of months to show; one picker covers the whole domain and removes the presets-plus-`Custom` machine (and the widest element in the bar).
- **Why the facets and legend collapse together on phones.** Four facets plus a legend consume the height the chart needs; one `Options` button returns it, and the `Include` label keeps the basis visible whether the disclosure is open or closed.
- **Why the facets wait for `lg` to share one row.** The sidebar takes 16rem from `md` up, so the row of four only has room from `lg`; below it they pair up two by two, and every control in the band uses the same default size so nothing looks mismatched.
- **Why `Include` is one control rather than three inline switches.** It is the only metric-conditional group; one labelled control states the basis at rest, shrinks the row, and keeps the three switch names for assistive tech.
- **Why the accrual switch needs a netted figure.** Without it every payment day counts the bill paid that day twice. The netting stays on the server because it needs each paid row's accrual, which the payload does not carry.
- **Why a due-today bill is owned by no switch.** The base line is already net of it, so no combination can attribute it.
- **Rejected: a dedicated arrears series.** It adds legend weight without answering a question the switches and the accounts/expenses tables do not already answer.
- **Rejected: a single `Include Committed` switch** (`accrued + arrears`). Simpler vocabulary, but it cannot be turned off in parts, so it loses the arrears-only reading the default relies on.
- **Rejected: keeping `available` beside the components.** It saves test churn (no App test moves, no client type is touched), but it publishes a member whose every input is already published and leaves two implementations of one figure. The churn is a one-off cost against a permanent contract (D-9).
- **Rejected: one `Include Arrears` option and no metric merge.** The PRD's original shape and by far the smallest change. It keeps the two-metric pair whose difference the reader has to work out and gives up five of the eight readings. Effort was explicitly not the deciding factor (user decision 2026-09-30).
