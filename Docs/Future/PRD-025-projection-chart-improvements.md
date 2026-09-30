# Projection Chart Improvements PRD

**Status:** Planning
**Priority:** Medium
**Last Updated:** 2026-09-30
**Feature ID:** 025
**Scope:** The projections chart's filter options, read-out and help text. Replaces the chart's two balance metrics with one `Balance` metric plus three `Include` switches — `Reserved`, `Accruals`, `Arrears` — marks the period's low and high in the plot, relabels the series-toggle group from `Show:` to `Series`, and gives every filter-bar control a tooltip. It also reduces the projections payload to the base balance plus the components the switches subtract — the netting of the day's accrual stays on the server — so the chart composes the balances it plots instead of reading a derived one.
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
- `Source/Server/Pot.App/Features/Projections/Models/DateProjection.cs` — the response carries `date`, `balance`, `available`, `dailyAccrual`, `incomeReceived`, `expensesPaid`, `expenseItems`, `incomeItems`. The loop already computes `accrued`, `arrears`, `reserved` and `accrualSettledByPayments` per day (`DateProjectionValues`), none of which reaches the response. The client's `DateValues` mirrors the response's eight members exactly. `available` is the response's one derived member: `MapToDateBalanceAvailable` (`Source/Server/Pot.App/Features/Projections/ProjectionsService.cs`) composes it as `balance − reserved − accrued − arrears + accrualSettledByPayments`, so those components are subtracted server-side and carried nowhere else. Its readers are the `available` metric entry R1 retires, and the App and integration assertions that pin the composition. `IProjectionsService` has a single consumer (`Pot.AspNetCore/Features/Projections/Get/Handler.cs`), so the projections endpoint is the whole of the change.

## Proposed Solution

Candidate requirements; nothing is implemented.

- **R1 — One balance metric.** Merge `Account Balances` and `Available Balances` into a single metric: the metric select keeps the `Account Balances` entry and loses `Available Balances`. The label stays `Account Balances` because the entry sits on the projections page, so its context is already a projected balance. The other entries (`Projection Accruals`, `Incomes`, `Expenses`) are unchanged. The response member goes with the entry (R6): the metric and the payload field are retired together, so no reader can select a figure the payload no longer derives.
- **R2 — Three switches.** Add `Include Reserved`, `Include Accruals` and `Include Arrears` to the filter bar, beneath the metric select. Each switch, when on, subtracts its component from the plotted balance. The switch row renders only while `Account Balances` is the selected metric; under `Projection Accruals`, `Incomes` or `Expenses` there is no balance for a switch to subtract from, so the switches are hidden.
- **R3 — Defaults.** `Include Reserved` off, `Include Accruals` off, `Include Arrears` **on** — so a first load with no remembered state plots the balance net of past-due debt.
- **R4 — Persistence.** The three switch states are remembered exactly as the metric, start date and period already are (`useProjectionStorage`). A previously stored `'available'` metric resolves to `Balance` with all three switches on, so a returning reader keeps the reading they had — now composed from the payload's components rather than read from the payload's own `available` member.
- **R5 — One scope for all series.** The switches apply to every visible series — each account line and the combined total alike. There is no per-series switch state.
- **R6 — The payload carries the balance and its components, and nothing derived.** Expose the per-day components the switches subtract — `reserved`, `arrears`, and the accrual net of the day's assumed payments — on the projection response, for each account and for the global series, and remove `available`, the response's one derived member. The payload states the base `balance` and those components, and every balance the chart plots is the client's to compose; the retired expression survives only as the client's all-three-switches-on case. The netted accrual is resolved by the server: netting it needs each paid row's own accrual, and the payload publishes those rows as amounts only.
- **R7 — Min/max read-out.** Mark the extremes of the plotted line over the period the chart is displaying as two horizontal reference lines at the **visible aggregate** — the lowest and highest value across every series the reader currently has visible, hidden series excluded. Each line spans the plot's full width and carries its value as a label at the plot's left edge, inside the plot, on an opaque chip so the grid and the curves do not cut through the text. Each label carries the value only: with several accounts the aggregate extreme may sit on no single account, so naming one would usually misattribute it. The lines are thin and low-contrast, so they read as annotations rather than as another series. They measure the plotted values, so they follow the switches. Nothing is rendered when the chart has no data to plot — the same condition that shows its no-data state, which covers a window with no days and one whose values are all zero — or when every series is hidden, and a low equal to the high renders one line with one label. The read-out belongs to the line metrics only: under `Incomes` or `Expenses` the bars are anchored at zero, so the minimum is meaningless and the maximum is a per-day magnitude in a different unit.
- **R8 — Legend group label.** Relabel the series-toggle group from `Show:` to `Series`. Presentation-only: the toggles, their accessible names, their behaviour and the `aria-labelledby` wiring are unchanged.
- **R9 — Baseline unchanged.** With all three switches off, every series is identical to today's `Account Balances`, and the other metrics are untouched.
- **R10 — Filter-bar help text.** Add a tooltip to every filter-bar control that has none today — the `View:` metric select, the `From:` date picker, the `Period:` presets and the custom-month input, and each series toggle in the `Series` group. The three `Include` switches each state the component they deduct, since `accruals` and `arrears` are model terms rather than everyday ones. Copy stays short: the shared tooltip primitive balances wrapped lines, so a hint that needs two lines can read as if it wrapped early. Help text is additive to a control's accessible name rather than a replacement for it: the metric select keeps its existing `aria-label` (`Select chart metric to display`), and each hint is attached as the control's accessible description through `aria-describedby`, as the table column hints already are (PRD-023 §8).

### How the switches compose

The base line is the projection's `Balance` value (`StartingBalance + IncomeReceived − ExpensesPaid`), which assumes every bill is paid on its due date and every income received on its. At the read date it is therefore the account's **entered balance, plus the income assumed received that day and less the expenses assumed paid that day** — not the raw entered balance whenever something falls due or is received today. Each switch subtracts one component:

| Include Reserved | Include Accruals | Include Arrears | Plotted value                                  | Equivalent to                       |
| ---------------- | ---------------- | --------------- | ---------------------------------------------- | ----------------------------------- |
| off              | off              | off             | `balance`                                      | today's `Account Balances` metric   |
| off              | off              | **on**          | `balance − arrears`                            | **the default**                     |
| off              | on               | off             | `balance − unpaidAccrual`                      | a reading no metric shows today     |
| off              | on               | on              | `balance − unpaidAccrual − arrears`            | `Available` plus `reserved`         |
| on               | on               | on              | `balance − reserved − unpaidAccrual − arrears` | today's `Available Balances` metric |

The remaining combinations follow the same rule. `unpaidAccrual` is the day's accrual minus the part its assumed payments already cover: a bill due today is accrued in full **and** already deducted from the base line (a 100 bill due today reads `Balance = 900` on a 1000 account), so subtracting the raw accrual would count it twice. The server already nets this inside today's `Available`; that member leaves the response (R6), and R6 exposes the same netted figure so the client only subtracts.

Because the base already nets that day's due bills, a bill due today is deducted from **every** combination — it is simply not owned by any switch. The account's own balance, as shown by the accounts read, is a different number: it does not assume the payment.

### What R6 removes, and what no consumer can be asked to derive

**The concern.** `available` is the only member of the projection response the server derives, and its derivation is not a single subtraction: it re-adds the part of the day's accrual that the day's own assumed payments settle. A bill due today is accrued in full **and** already deducted from `balance`, so subtracting the raw accrual would count it twice. R6 can therefore be read as handing that correction to the client — as if every consumer, now and later, had to reproduce the add-back, the accrual gate, the payment attribution and the arrears walk before it could state a balance. That would be a poor contract: it exports the model's hardest arithmetic to code that cannot see the model, and makes consumers responsible for a rule the payload exists to express.

**The reality.** The add-back never crosses the boundary. It is not a step the client applies; it is the _definition_ of one published number. Writing `Settled` for the payment-settled accrual (`AccrualSettledByPayments`):

```
Available = Balance − Reserved − Accrued − Arrears + Settled
          = Balance − Reserved − (Accrued − Settled) − Arrears
          = Balance − Reserved − unpaidAccrual − Arrears
```

`unpaidAccrual` is evaluated by the accrual engine, row by row, where the row facts and the schedule cursor live — the same place `Accrued` and `Settled` are already resolved today. The client is handed the result and subtracts it. It never sees `Accrued`, never sees `Settled`, and never needs to know which rows settled what: the whole correction is spent before the payload is built.

**What a consumer receives.** Three numbers it may subtract, and nothing else — `reserved`, `arrears` and `unpaidAccrual`. Its rule is `balance − [reserved] − [unpaidAccrual] − [arrears]`, each bracket present or absent according to its switch. That is arithmetic rather than domain knowledge, and it is the same rule whatever the consumer is.

**What no consumer can be asked for.** The tempting alternative is to publish more raw material and let consumers derive the rest. It does not work:

| Quantity        | Derivable by a consumer? | Why                                                                                                                                    |
| --------------- | ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------- |
| `reserved`      | Yes                      | a stored account field, validated `≥ 0` on create and update                                                                           |
| `arrears`       | **No**                   | needs the schedule walked backwards from the persisted `NextDue`, counting every un-settled past-due occurrence, per row               |
| `unpaidAccrual` | **No**                   | needs each due row's own accrued amount; `expenseItems` carry `rowId`, `description` and `amount` only, and `Accrued` is not published |
| `balance`       | **No**                   | a running forecast, folded day by day through renewal and the assumed-payment basis                                                    |

So the components a switch subtracts must be published whether or not R6 removes `available` — that is forced by the switches, not by the removal. And once they are published, `available` is _by construction_ `balance − reserved − unpaidAccrual − arrears`: a member composed entirely of published members, carrying no content of its own. R6 removes it for that reason alone, not to move work to the client.

**Where the safety property goes.** `Available ≤ Balance` is the assertion that pinned the add-back to the rows that actually settled it. It does not need `available` to exist, because it decomposes into component non-negativity — every term the balance nets out is `≥ 0` by construction, so their sum can never drive the balance below itself:

```
Reserved ≥ 0  ∧  unpaidAccrual ≥ 0  ∧  Arrears ≥ 0   ⟹   Available ≤ Balance


where:
 ∧ and
 ⟹ implies
```

All three hold structurally. `reserved` is validated non-negative on create and update; `arrears` counts whole occurrences and cannot go below zero; and `Settled` sums the accruals of a _subset_ of the rows that make up `Accrued`, so it can never exceed it and `unpaidAccrual` can never go below zero. The property therefore moves onto the components — asserted server-side against the payload — rather than being lost with the derived member.

**What the removal buys.** One member per fact, and an endpoint that states values instead of answering one question. The retired expression is reproduced exactly by the client's all-three-switches-on case, so nothing readable today becomes unreadable; and a later export or view composes whatever balance it needs from the same four numbers instead of being handed a balance shaped for the accounts question. Nothing is asked of a consumer that the model alone can answer.

## Non-Goals

- **No change to the settlement model.** `Arrears` stays held for the whole window and is never debited from `Balance`; a bill due today stays assumed paid.
- **No change to the assumed-payment basis.** The base line stays the entered balance adjusted by that day's assumed movements — bills assumed paid, income assumed received; the accounts read keeps showing the real balance.
- **No new series.** No dedicated arrears or accrual line.
- **No change to the accounts or expenses reads, or to the maintenance export.** The accounts read keeps its own `available` — `Balance − Reserved − TotalCommitted`, with no assumed payments — a different figure from the projection chart's retired member, so the word survives this PRD in one place only.
- **No derived member on the projections payload.** `available` is removed rather than deprecated (R6); the endpoint carries the balance and the components a reader composes from, not a balance it answers with.
- **No second Y axis.** The extremes are annotated in the plot; no right-hand scale is added.
- **No per-series switch state.**
- **No budgeting, actuals or seasonality work.** That is [PRD 024 — Settlement Ledger](PRD-024-settlement-ledger.md)'s territory.
- **No schema change.**

## Open Questions

Raised in review (2026-09-30) against R1–R10 as written. Each item either contradicts a requirement, leaves a decision to the implementer that the PRD should make, or names a surface the requirements do not reach. None rejects the design. Numbering continues from the Resolved Questions below, so item 8 follows item 7 and no item number is shared between the two lists. Numbers are stable references: items are not renumbered when one is settled or withdrawn, so gaps are expected.

### Decisions required before implementation

8. **Which switch state does a returning reader with a stored `'balance'` metric get?** R3 makes `Include Arrears` default **on**, R9 pins all-off as today's `Account Balances`, and R4 migrates only a stored `'available'`. A reader who previously had `'balance'` stored — the current `DEFAULT_PROJECTION_METRIC` — has a remembered metric and **no** remembered switch state, which R3's "first load with no remembered state" does not describe. Three readings are possible: defaults (off/off/on, so the line changes silently for every returning reader), all-off (their reading is preserved, but the arrears default then applies only to a genuinely empty store), or a stored reading per metric that treats a missing switch state as on. R3 and R4 need to state which.
9. **Is the migrated metric coerced on read or written back?** R4 says a stored `'available'` "resolves to" `Balance` with all three switches on but does not say whether the stored record is rewritten. `useProjectionStorage` persists `metric`, `period`, `startDate` and `hiddenSeries` into both `sessionStorage` and `localStorage`, and after R1 the `ProjectionMetric` type no longer contains `'available'` — so a read-time coercion leaves an out-of-contract value on disk indefinitely, and a cast back into state would carry it. Decide: coerce on every read, or migrate once and write back (and in which of the two stores).
10. **Does the read-out render under `Projection Accruals`?** R7 says "the read-out belongs to the line metrics only", but its stated reason is only about bars: "the maximum is a per-day magnitude in a different unit". `dailyAccrual` is a **line** metric whose values are per-day magnitudes, so the same objection applies to it. Either the rule is "line metrics" and `Projection Accruals` gets the read-out, or the rule should be "the balance metric only". As written, R7's rule and its justification disagree.
11. **How are the values formatted, and what happens when the labels collide?** R7 fixes the lines' placement and appearance but names no formatter — the client already has `formatMoneyValue` — and only handles the exact-equality case. A low and high that are close but unequal put two chips at the same left edge, and a chip can also meet the Y-axis, the zero reference line, or the first data point. Say which formatter the labels use and what the collision rules are.
12. **What are the new payload members called, and what is the client's shape?** R6 states the values to publish and the member to remove but never names the new members — `unpaidAccrual` appears only as prose — nor whether they land on the same per-day object as `balance`, nor how the global values relate to the accounts'. It also does not state the client-side change: `DateValues` currently mirrors the response's eight members exactly and `available` is a required member, so removing it is a compile-breaking change to a shared type, and the `ProjectionMetric` union loses its `'available'` entry. The contract should be written out, since this is a breaking response change.
13. **Which vocabulary do the switches and their hints use?** `Include Accruals` subtracts the **netted** accrual (`unpaidAccrual`), not the `Accrued` figure the accounts table shows, so a reader who has learned `Accrued` from the accounts page may read the switch as subtracting something else. PRD-023 §8 settled the accounts vocabulary as `Accrued`, `Arrears` and `Committed`, and Resolved Question 7 says each switch "names the component it deducts". Decide whether the switch labels and hints follow the accounts terms (`Accrued` rather than `Accruals`) and whether the hint states that the figure is net of the day's payments.
14. **What copy do the tooltips carry, and which component delivers them?** R10 names the controls to hint and says each `Include` switch states the component it deducts, but supplies no text for any of them. It also appeals to "the shared tooltip primitive" without saying which component that is: `components/table/ColumnHeaderHint.tsx` is the only existing hint wrapper and lives under `table/`, while the shared primitive is `components/ui/tooltip.tsx`, with `TooltipProvider` mounted in `App.tsx`. Decide whether a new shared hint component is added or the table one is generalised — a shared-helper placement decision the repository instructions require confirming — and whether the copy is implementer-authored with assertions on the mechanism rather than the wording, as PRD-023 §8 did for the table hints.
15. **Where does the switch row sit on mobile?** R2 places the switches "beneath the metric select", which is a desktop statement: `ChartControls.tsx` branches on `isMobile` and collapses the filter bar on small screens. State where the row renders on mobile and whether it participates in the existing collapse, so the requirement does not quietly assume one layout.

### Ambiguities and internal inconsistencies

17. **`Balance` or `Account Balances`?** The header Scope and R3/R4 call it the `Balance` metric, R1 keeps the filter label `Account Balances`, and the `Docs/Future/README.md` index row says "one `Balance` metric". State whether the metric's identity changes at all — the code key is already `'balance'` — whether the filter label and the chart title stay `Account Balances`, and which of the two names the requirements and the index row should use.

### Surfaces the requirements do not cover

23. **Client fixtures and tests that hard-code `'available'`.** R1 and R6 remove the metric and the member, but no requirement names the client surfaces that carry them: `tests/data/projection.test.ts` asserts the `PROJECTION_METRICS` key list exactly, `tests/shared/factories/projectionFactory.ts` sets `available` on every `DateValues` fixture (a required member today), `tests/features/projections/hooks/useProjectionStorage.test.ts` stores and seeds `'available'`, and `tests/features/projections/ProjectionsPage.test.tsx` and `tests/integration/CoreFlows.ProjectionFlow.test.tsx` each call `onMetricChange('available')`. Name them so the change is scoped rather than discovered.
24. **The server fixtures that assert `Available`.** `Source/Server/Pot.AspNetCore.Integration.Tests/Features/Projections/AvailableInvariantFixture.cs` asserts the member through the HTTP boundary (`paymentDay.Available…`, `AssertAvailableNeverExceedsBalance`), and `Pot.App.Tests/Features/Projections/ProjectionsServiceFixture.cs` carries roughly thirty `Available` assertions with comments encoding the composition. R6's design note says the App assertions "follow it to the client's all-three-switches-on case", but these are server-side fixtures and cannot move to the client. State what each becomes: rewritten against the components, or deleted.

## Resolved Questions

All seven questions carried by the discovery draft are settled (2026-09-30). They are kept here because each one bounds a requirement above. The Open Questions above continue this numbering from 8, so no item number is shared between the two lists.

1. **Min/max scope — the visible aggregate.** One pair covering every visible series, hidden series excluded, which is the range the chart already computes for its Y-axis domain. Per-series extremes were rejected: they duplicate what the tooltip reports for the hovered day, and one pair per account is a table rather than a glance. Including hidden series was rejected because the axis and the legend both ignore them, so the read-out would contradict the rest of the chart.
2. **Min/max placement — reference lines in the plot, labelled on the line.** Two full-width horizontal lines mark the visible low and high, each labelled with its value at the left edge of the plot. The chart header was rejected as too far from the data — the reader has to carry the number back to the curve. A **second Y-axis rail** was rejected even though the charting library supports one: a rail carrying only two annotations is an axis-shaped legend, and an axis is a strong claim that the series beside it are measured against it, so readers would take the account lines as a second measurement. The only honest rail — the same domain and ticks as the left axis — duplicates the axis already on screen. Also rejected: markers on the extremes (they collide with overlapping series and the zero reference line), a footer (below the scrollable plot area, competing with the legend), tooltip-only (hover-only, so no use on touch), and a floating text block in the plot (a second overlay beside the tooltip, and it drops the anchor that ties the value to its height).
3. **Min/max edge cases — hide, or dedupe.** No read-out when the window is empty or every series is hidden; one line and one label rather than two coincident ones when the low and high are equal, which covers both a flat line and a one-point window. A `—` or a `$0` was rejected because either asserts a balance the projection does not have.
4. **Non-balance metrics — the switches are hidden (R2).** The rule is R2's; what is recorded here is why it holds. A switch subtracts a component from a balance, so under the other metrics it has nothing to act on.
5. **Basis statement — not required.** The basis in question is which components the line has netted out, not the real bank balance; that figure stays on the accounts read and is not restated on the chart. The switch row states the basis by showing it, and each switch explains its own deduction (R10), so no header or chart-level statement is added.
6. **Tooltips and detail panels — only the chart tooltip applies.** There is no detail panel for the balance metric: the details sheets serve `Incomes` and `Expenses`, which are exactly the metrics under which the switches are hidden (4). The single chart tooltip reads the plotted series, so it reports the switched values for the day hovered — the same numbers the line draws.
7. **Switch help text — one hint per switch** (R10), each naming the component it deducts, with the same short hints extended to the filter-bar controls that have none.

## Tests

- **App (payload)** — the response carries `reserved`, `arrears` and the netted accrual on every emitted day, per account and for the global series; the netted accrual excludes the part a due-today bill's assumed payment covers; the global figure is the sum of the accounts'; and the response no longer carries `available`.
- **App (components)** — every published component is non-negative on every emitted day, per account and for the global series: `reserved ≥ 0`, `arrears ≥ 0`, `unpaidAccrual ≥ 0`. This is the `Available ≤ Balance` property re-homed from the removed derived member, and it is the assertion that catches an unbounded add-back, where the payment-settled accrual comes to exceed the accrual it is part of.
- **Client (chart data)** — each switch subtracts only its own component; all three on reproduces the retired `available` value, pinned against a fixture captured before the removal; all off equals today's `Account Balances`; the default is off/off/on; the global series is the sum rather than a re-computation.
- **Client (component)** — the three switches render with accessible names, persist across a revisit, apply to every series, and are absent under the other three metrics; a stored `'available'` metric loads as `Balance` with all three on.
- **Client (min/max)** — a line with a labelled value is rendered for the visible low and high; hidden series do not move them; nothing renders when the window is empty or every series is hidden; a low equal to the high renders one line and one label; nothing renders under the bar metrics.
- **Client (help text)** — every filter-bar control and every `Include` switch exposes a tooltip.
- **Client (legend label)** — the group reads `Series`; the toggles keep their existing accessible names.
- **Integration / E2E** — the projections payload carries the new members and no `available`, and toggling each switch moves the plotted balance by that component alone.

## Documentation

The change retires a metric and a payload member that are described outside the code, so the documentation moves with it:

- `Docs/USER-GUIDE/Projections.md` — the metric table (`Account Balances`, `Available Balances`, `Projection Accruals`, `Incomes`, `Expenses`) and the `Available Balances Interpretation` section describe the retired metric; the page needs the merged metric, the three switches and their defaults, the read-out, and the `Series` group label.
- `Source/Server/DEVELOPER.md` and `Source/Server/IMPLEMENTATION.md` — both state that the projection chart composes its own `Available` from the running forecast balance, which R6 removes.
- **PRD-023 §2 and §8** — this PRD makes §2's chart `Available` form and §8's "the chart gains no field of its own" stale, so each takes a dated note naming this PRD.

## Design Notes

- **Why merge rather than add a modifier.** Two balance metrics made the reader work out their difference; one line plus three explicit switches states it instead, and removes the question of which metric a modifier belongs to.
- **Why the metric keeps the label `Account Balances`.** Its context is the projections page, so the entry already reads as a projected balance; renaming it would cost familiarity for no clarity gain.
- **Why arrears defaults on.** An account that is up to date reads the same either way, so the default only changes what an account that is behind sees first — the case this feature exists for. The accepted consequence is that the first-load line can sit below the real bank balance; the switch row and its hints state that basis, and the real balance stays on the accounts read rather than being restated on the chart (question 5).
- **Why the min/max reuses the visible-aggregate range.** The chart already computes exactly that range on every render to place its Y-axis domain, so the read-out adds no state, no computation and no new interaction — only the two lines and their formatting. Sharing the range also guarantees the lines can never fall outside the domain.
- **Why the level is marked in the plot rather than in the header.** A header pair sits a card's width away from the curve it describes, so the reader has to carry the number back to the chart; a line drawn at the value needs no such translation.
- **Why the lines are kept though they are near-redundant.** With the domain padded by 10%, the top and bottom curves already sit close to the low and high lines, so the line itself carries little information — it is the anchor that attaches the exact value to the right height.
- **Why there is no second axis.** A rail carrying only the two values is an axis-shaped legend; it lends the annotation the authority of a scale and invites readers to treat the account lines as measured against it. An honest rail — the same domain and ticks as the left axis — would duplicate the axis already on screen.
- **Why the lines look unlike the other dashes.** The plot already carries the `3 3` grid and the `6 6` zero line, so the low and high lines are thinner and lower-contrast: a different kind of object rather than a third dash pattern competing with the only semantic baseline.
- **Why the read-out hides rather than showing a placeholder.** A `—` or a `$0` reads as a balance of zero, which an empty window does not support; showing nothing claims nothing.
- **Why the accrual switch needs a netted figure.** See "How the switches compose": without it, every payment day counts the bill paid that day twice. The netting stays on the server even though the subtraction moves to the client — it needs each paid row's accrual, which the payload does not carry.
- **Why a due-today bill is not owned by a switch.** The base line is already net of it, so no combination can attribute it: the accrual switch carries only the accrual its assumed payments have not already cancelled.
- **Rejected: a dedicated arrears series.** It adds legend weight without answering a question the switches and the accounts/expenses tables do not already answer.
- **Rejected: a single `Include Committed` switch** (`accrued + arrears`). Simpler vocabulary, but it cannot be turned off in parts, so it loses the arrears-only reading the default relies on.
- **Why the group is labelled `Series`.** The group holds the per-account lines and the combined total, so `Accounts` would mis-describe one of its own entries; `Series` covers both and matches the toggles' accessible names.
- **Why the derived member is removed rather than kept.** `available` was the only member that composed the others, so the switches now express that calculation for themselves; keeping it would leave two implementations of one figure and a payload that answers a question instead of carrying the values it is built from. Removing it also lets the same endpoint serve readers the chart does not anticipate — an export or another view takes `balance`, `reserved`, `arrears` and the netted accrual and derives what it needs, instead of being handed a balance that only answers the accounts question. What it moves is verification, not arithmetic: the App assertions that pin the composition follow it to the client's all-three-switches-on case, and the `Available ≤ Balance` invariant is re-homed onto the components, where it is the stronger statement because it names the term that could misbehave. The add-back itself does not move — see "What R6 removes, and what no consumer can be asked to derive".
- **Rejected: keeping `available` beside the components.** It is the cheapest option — no App test moves, no client type is touched — but it leaves two implementations of one figure, one server-side and one on the client's all-three-on case, and it publishes a member whose every input is already published. Retaining a value a consumer can compute exactly, from members already in the payload, is the duplicate the removal exists to avoid; the saving is test churn, which is a one-off cost against a permanent contract.
- **Rejected: leaving the metrics alone and adding one `Include Arrears` option.** This was the PRD's original shape and is by far the smallest change: no metric merge, no stored-state migration, no switch row, and only `arrears` added to the payload. It still reaches the reading the feature exists for. It was rejected because it keeps the two-metric pair whose difference the reader has to work out, and because it gives up five of the eight readings. **Effort was explicitly not the deciding factor (user decision 2026-09-30): the flexibility the switches give the reader outweighs the mechanism they cost.**
- **Why the full switch set survives although three readings carry the story.** All off, arrears-only and all-on each correspond to a metric or a default the reader already knows; the other five combinations have no story of their own. They are kept anyway. The point of the switches is that the reader composes the question rather than choosing from a list of approved readings, so a set with combinations withheld would be an arbitrary shortlist dressed as a control. `Include Reserved` earns its place twice over: `Reserved` is a figure the reader sets deliberately on the account, and it is what lets the all-on case reproduce today's `Available Balances` metric exactly.
