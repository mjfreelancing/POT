# Projection Chart Improvements PRD

**Status:** Proposed (discovery draft)
**Priority:** Medium
**Last Updated:** 2026-09-28
**Feature ID:** 025
**Scope:** The projections chart's filter options and read-out. Replaces the chart's two balance metrics with one `Balance` metric plus three `Include` switches — `Reserved`, `Accruals`, `Arrears` — adds a min/max read-out for the period shown, and relabels the series-toggle group from `Show:` to `Series`.
**Audience:** Future implementation agent(s), reviewers, and maintainers.
**Sequencing:** Depends on PRD-023 (`Arrears` and the assumed-payment basis of the `balance` line), which the projection code already implements.

## Purpose

Let a reader ask the balance question in one place, instead of choosing between two metrics whose difference they have to work out. One `Balance` line with three switches that state their own deductions; a read-out of the period's extremes; and a series-toggle group that names what it filters.

## Problem Statement

**Two balance metrics make the reader do the arithmetic.** `Account Balances` is the cash line; `Available Balances` subtracts `Reserved` and the obligations. Neither shows a balance net of just the past-due debt, the difference between them is not explained anywhere, and the obligation is only visible as the gap between two lines.

**The period has no read-out.** Reading the window's extremes means scanning the axis by eye, which gets harder as accounts are added or series are toggled.

**The series toggles do not name what they filter.** They sit under a `Show:` label that states the action rather than the subject, unlike their siblings in the filter bar (`From:`, `Period:`).

## Current Behaviour

- `Source/Client/pot-react/src/data/projection.ts` — `ProjectionMetric` is `'balance' | 'available' | 'dailyAccrual' | 'incomeReceived' | 'expensesPaid'`; `PROJECTION_METRICS` maps each to a title and filter label (`Account Balances`, `Available Balances`, `Projection Accruals`, `Incomes`, `Expenses`). Period presets and `DEFAULT_PROJECTION_PERIOD` live here too.
- `Source/Client/pot-react/src/features/projections/components/ChartControls.tsx` — the filter bar (metric select, start-date picker, period presets and custom month input, per-series visibility toggles) and the legend row, whose toggles are introduced by a static `Show:` label (`span#legend-label`) wired through `aria-labelledby`.
- `Source/Client/pot-react/src/features/projections/ProjectionsPage.tsx` and `useProjectionStorage` — metric, period and start date are page state, persisted client-side. The period and start date bound the request, so the payload window is the period shown.
- `Source/Client/pot-react/src/features/projections/hooks/useProjectionChartData.ts` — one point per date from `data.global`, each point reading `dateBalance[metric]` per account plus a `global` total. This is the single seam where a metric becomes a plotted value.
- `Source/Server/Pot.App/Features/Projections/Models/DateProjection.cs` — the response carries `date`, `balance`, `available`, `dailyAccrual`, `incomeReceived`, `expensesPaid`, `expenseItems`, `incomeItems`. The loop already computes `accrued`, `arrears`, `reserved` and `accrualSettledByPayments` per day (`DateProjectionValues`), none of which reaches the response. The client's `DateValues` mirrors the response's eight members exactly.

## Proposed Solution

Candidate requirements; nothing is implemented.

- **R1 — One balance metric.** Merge `Account Balances` and `Available Balances` into a single metric: the metric select keeps the `Account Balances` entry and loses `Available Balances`. The label stays `Account Balances` because the entry sits on the projections page, so its context is already a projected balance. The other entries (`Projection Accruals`, `Incomes`, `Expenses`) are unchanged.
- **R2 — Three switches.** Add `Include Reserved`, `Include Accruals` and `Include Arrears` to the filter bar, beneath the metric select. Each switch, when on, subtracts its component from the plotted balance.
- **R3 — Defaults.** `Include Reserved` off, `Include Accruals` off, `Include Arrears` **on** — so a first load with no remembered state plots the balance net of past-due debt.
- **R4 — Persistence.** The three switch states are remembered exactly as the metric, start date and period already are (`useProjectionStorage`). A previously stored `'available'` metric resolves to `Balance` with all three switches on, so a returning reader keeps the reading they had.
- **R5 — One scope for all series.** The switches apply to every visible series — each account line and the combined total alike. There is no per-series switch state.
- **R6 — Components on the payload.** Expose the per-day components the switches subtract — `reserved`, `arrears`, and the accrual net of the day's assumed payments — on the projection response, for each account and for the global series.
- **R7 — Min/max read-out.** Report the minimum and maximum of the plotted line over the period the chart is displaying.
- **R8 — Legend group label.** Relabel the series-toggle group from `Show:` to `Series`. Presentation-only: the toggles, their accessible names, their behaviour and the `aria-labelledby` wiring are unchanged.
- **R9 — Baseline unchanged.** With all three switches off, every series is identical to today's `Account Balances`, and the other metrics are untouched.

### How the switches compose

The base line is the projection's `Balance` value (`StartingBalance + IncomeReceived − ExpensesPaid`), which assumes every bill is paid on its due date and every income received on its. At the read date it is therefore the account's **entered balance, plus the income assumed received that day and less the expenses assumed paid that day** — not the raw entered balance whenever something falls due or is received today. Each switch subtracts one component:

| Include Reserved | Include Accruals | Include Arrears | Plotted value                                  | Equivalent to                       |
| ---------------- | ---------------- | --------------- | ---------------------------------------------- | ----------------------------------- |
| off              | off              | off             | `balance`                                      | today's `Account Balances` metric   |
| off              | off              | **on**          | `balance − arrears`                            | **the default**                     |
| off              | on               | off             | `balance − unpaidAccrual`                      | a reading no metric shows today     |
| off              | on               | on              | `balance − unpaidAccrual − arrears`            | `Available` plus `reserved`         |
| on               | on               | on              | `balance − reserved − unpaidAccrual − arrears` | today's `Available Balances` metric |

The remaining combinations follow the same rule. `unpaidAccrual` is the day's accrual minus the part its assumed payments already cover: a bill due today is accrued in full **and** already deducted from the base line (a 100 bill due today reads `Balance = 900` on a 1000 account), so subtracting the raw accrual would count it twice. The server already nets this for `Available`; R6 exposes the netted figure so the client only subtracts.

Because the base already nets that day's due bills, a bill due today is deducted from **every** combination — it is simply not owned by any switch. The account's own balance, as shown by the accounts read, is a different number: it does not assume the payment.

## Non-Goals

- **No change to the settlement model.** `Arrears` stays held for the whole window and is never debited from `Balance`; a bill due today stays assumed paid.
- **No change to the assumed-payment basis.** The base line stays the entered balance adjusted by that day's assumed movements — bills assumed paid, income assumed received; the accounts read keeps showing the real balance.
- **No new series.** No dedicated arrears or accrual line.
- **No change to the accounts or expenses reads, or to the maintenance export.**
- **No per-series switch state.**
- **No budgeting, actuals or seasonality work.** That is [PRD 024 — Settlement Ledger](PRD-024-settlement-ledger.md)'s territory.
- **No schema change.**

## Open Questions

1. **Min/max scope** — the extremes of each series, or only the combined total; hidden series included, or visible only?
2. **Min/max placement** — chart header, chart footer, markers on the extremes, detail panel, or tooltip only?
3. **Min/max edge cases** — what to show when the window is empty or holds a single point.
4. **Non-balance metrics** — with `Projection Accruals`, `Incomes` or `Expenses` selected, are the switches hidden, shown-but-disabled, or inert?
5. **Basis statement** — the default includes arrears, so the line can sit below the real bank balance. Should the chart or its header state the active basis, or is the switch row enough?
6. **Tooltips and detail panels** — do they follow the switches, or keep reporting raw values?
7. **Switch help text** — what each switch says about what it deducts, since `accruals` and `arrears` are model terms.

## Tests

- **App (payload)** — the response carries `reserved`, `arrears` and the netted accrual on every emitted day, per account and for the global series; the netted accrual excludes the part a due-today bill's assumed payment covers; the global figure is the sum of the accounts'.
- **Client (chart data)** — each switch subtracts only its own component; all three on equals today's `Available Balances`; all off equals today's `Account Balances`; the default is off/off/on; the global series is the sum rather than a re-computation.
- **Client (component)** — the three switches render with accessible names, persist across a revisit, and apply to every series; a stored `'available'` metric loads as `Balance` with all three on.
- **Client (min/max)** — once scope and placement are settled: the extremes over the period shown, the treatment of hidden series, and the empty/single-point behaviour.
- **Client (legend label)** — the group reads `Series`; the toggles keep their existing accessible names.
- **Integration / E2E** — the projections payload includes the new members, and toggling each switch moves the plotted balance by that component alone.

## Design Notes

- **Why merge rather than add a modifier.** Two balance metrics made the reader work out their difference; one line plus three explicit switches states it instead, and removes the question of which metric a modifier belongs to.
- **Why the metric keeps the label `Account Balances`.** Its context is the projections page, so the entry already reads as a projected balance; renaming it would cost familiarity for no clarity gain.
- **Why arrears defaults on.** An account that is up to date reads the same either way, so the default only changes what an account that is behind sees first — the case this feature exists for. The accepted consequence is that the first-load line can sit below the real bank balance, which is why open question 5 exists.
- **Why the accrual switch needs a netted figure.** See "How the switches compose": without it, every payment day counts the bill paid that day twice.
- **Why a due-today bill is not owned by a switch.** The base line is already net of it, so no combination can attribute it: the accrual switch carries only the accrual its assumed payments have not already cancelled.
- **Rejected: a dedicated arrears series.** It adds legend weight without answering a question the switches and the accounts/expenses tables do not already answer.
- **Rejected: a single `Include Committed` switch** (`accrued + arrears`). Simpler vocabulary, but it cannot be turned off in parts, so it loses the arrears-only reading the default relies on.
- **Why the group is labelled `Series`.** The group holds the per-account lines and the combined total, so `Accounts` would mis-describe one of its own entries; `Series` covers both and matches the toggles' accessible names.
