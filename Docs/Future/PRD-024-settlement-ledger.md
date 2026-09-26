# Settlement Ledger PRD

**Status:** Proposed (discovery draft)
**Priority:** Low
**Last Updated:** 2026-09-25
**Feature ID:** 024
**Scope:** Discovery only. Explore capturing a snapshot when an expense is paid or an income is received — the actual amount alongside the amount that was budgeted — and what that history could later do for projection accuracy, escalation and seasonality. This document records the problem, the intended value, and the considerations that must be resolved before any design is fixed. It commits to no schema, no API shape and no UX, and it is deliberately independent of [PRD-023](PRD-023-dynamic-accrual-calculation.md) — nothing there depends on this, and this may never be implemented. No part of the accrual derivation change may assume a ledger exists.
**Audience:** Future implementation agent(s), reviewers, and maintainers.

## Purpose

Decide whether POT should record what actually happened to each income and expense occurrence, so that future projections can be informed by observed amounts and behaviour rather than by repeating today's figures indefinitely — and, if so, how much of that has to be captured by the user versus inferred by the application.

## Problem Statement

Projections currently repeat a single point-in-time amount forever. `ProjectionsService` advances dates through `ExpenseRenewalCalculator` / `IncomeRenewalCalculator` and re-accrues each day, but `Renew` only ever moves `NextDue` and `AccrualStart` — `Amount` is never varied. A rent expense of $500/month is projected as $500/month for the full forecast window, no matter what has happened to it historically.

There is no history to correct that, in either direction:

1. **The amount series is destroyed as it is created.** `Amount` is mutated in place by the edit form, so when a subscription increases from $12 to $14 there is no record that it was ever $12. The information needed to infer an escalation rate — or even to know that a change happened at all — is discarded at the moment it is entered.
2. **Nothing records that an occurrence actually happened.** Renewal (`RenewExpensesService`, `RenewalMode.Overdue` to catch up to today, `RenewalMode.Future` to advance exactly one cycle) advances the schedule and is the closest thing to an acknowledgement. It captures no amount, no date of payment, and no distinction between "paid as budgeted", "paid a different amount" and "didn't pay". As a result there is no budget-vs-actual comparison anywhere in the product.
3. **Capture has no home.** When a user renews from the dashboard, the only available assumption is that the actual equals the budgeted amount — which is exactly right for a fixed subscription and exactly wrong for a utility bill, a grocery allowance or a variable-rate mortgage. There is no point in the current flow at which a user could say "this one was different".
4. **Balance is unexplained.** `Account.Balance` and `Account.Reserved` are user-entered figures. Nothing in the product can explain how they got to their current values, and nothing can reconcile them against what was supposed to happen.

The consequence is that the product's forecasts are structurally unable to learn: every projection assumes the present is the permanent future.

### What exists today (relevant context)

- Entities in `Pot.Data/Entities`: `Account`, `AccountAccrual`, `AuthSession`, `Expense`, `Income`, `OneTimePassword`, `Permission`, `Role`, `Setting`, `Site`, `User`. There is **no** history, occurrence or settlement entity — nothing anywhere records a past occurrence.
- `ExpenseEntity.Amount` / `IncomeEntity.Amount` are single mutable values. `ExpenseEntity.Accrued` is a derived allocation, not history, and is being retired by PRD-023.
- `NextDue` is the schedule cursor: user-entered on create/edit, advanced by renewal, and the sole basis for overdue. PRD-023 keeps it that way and explicitly does **not** derive it.
- Maintenance CSV export/import is versioned through `MetadataV1`–`MetadataV4`, with the importer already tolerant of ignored columns.
- Site-scoped global query filters apply to `Account`, `AccountAccrual`, `Expense`, `Income` and `Setting`; a ledger would be site-scoped financial data.

## Intended Value

Two distinct capabilities are bundled in the phrase "better projections", and they cost very different amounts of effort and user discipline. Separating them is the first thing this track must do.

| Capability                                                                                                         | Signal required                                                | Capture burden                                                           |
| ------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------- | ------------------------------------------------------------------------ |
| **Escalation / seasonality** — project rent rising, utilities swinging with the season, a subscription stepping up | The amount in force at each occurrence over time               | Low if captured at edit/renewal time; higher if only captured at payment |
| **Budget vs actual** — how closely reality tracks the plan, per item and per account                               | The actual amount (and whether the occurrence happened at all) | Requires per-occurrence capture, which is the opt-in burden              |

The escalation signal is largely obtainable from amount _changes_ rather than from payments, because the useful information is "the figure moved from X to Y on date D". The budget-vs-actual signal genuinely requires recording what was paid or received. A PRD for this feature should state which of the two it is optimising for; trying to serve both by default is what makes the capture burden unacceptable.

## Considerations To Explore

### 1. Capture UX — the main risk

- **Where does capture happen?** Renewal on the dashboard is the natural hook for recurring items, because the user is already asserting "this cycle has passed". Alternatives include a row action on the expenses/incomes lists, and a prompt when viewing a projection date that has occurrences on it.
- **One occurrence or many?** `RenewalMode.Overdue` can advance several cycles in a single call, so "renew" is not a one-to-one mapping with "I paid this occurrence". The pairing rule has to be defined, not assumed.
- **Default with confirmation, not entry.** For the common fixed-subscription case, "actual = budgeted, confirm" should be one action. Any flow that demands a typed amount for a $12 subscription will not be used.
- **Variable items need a real amount.** Utilities, groceries, fuel and similar need a genuine figure without friction.
- **Corrections.** If a captured row is wrong, can it be edited or deleted, and does that affect anything already shown?
- **One-time expenses.** A one-time expense is expected to be paid and then deleted; capture must decide what happens at that boundary.
- **Silence vs prompt.** Users who never record anything must experience no nagging, and the app must not degrade to a "needs attention" state if the feature is opt-in.

### 2. Opt-in model

- **Per user or per site?** Site-scoped storage is the natural fit with the existing query filters, but the _choice_ to capture is personal — a shared household site would then share one history. This needs an explicit decision.
- **Discoverability.** An opt-in feature that is invisible is never adopted; the value has to be demonstrated the moment it is enabled.
- **Empty-history tolerance.** Every consumer must behave exactly as today when there is no history at all, and must not treat sparse history as meaningful data.
- **No core dependency.** No due date, overdue state, accrual value or projection output may depend on the ledger. Any dependency would create a second source of truth for something already derived (see PRD-023, and the caution below).

### 3. Relationship to renewal and `NextDue`

The decision already taken — and it should be restated in this document when it is written as a design — is that the ledger is **not** the schedule cursor. `NextDue` remains user-entered and renewal-driven, and overdue remains `NextDue < today`.

The overlap that must be resolved regardless: renewal is today's de-facto acknowledgement of an occurrence, so a settlement capture that happens at payment time either absorbs renewal, accompanies it, or is unrelated to it. Two actions for one real-world event would be poor UX; conflating them risks changing what renewal means. Related open question: should captured actuals ever influence the projected amount for future cycles, and if so, at item level or category level?

### 4. Data shape (considerations only — no schema is committed here)

Recorded because these are cheap to get right at table-creation time and expensive to retrofit:

- Site-local occurrence date, consistent with `NextDue` / `EndDate` semantics and mindful of PRD-018's hardcoded time zone.
- The budgeted amount **in force at that occurrence** as a snapshot, alongside the actual amount. The budgeted snapshot is what makes an amount series possible without a separate history mechanism.
- When the row was recorded and by whom; a source discriminator (manual today, import/feed later).
- Currency, once PRD-018 lands.
- An idempotency key over `(item, kind, occurrence date)` so repeat renewals or retries cannot duplicate.
- Item reference strategy: live foreign key versus a soft `RowId` plus a description snapshot, which decides what survives a rename or a delete.
- Whether the ledger participates in the account/site/user deletion flows (PRD-022) by cascade or by retention, and what the retention rule is.

### 5. Analytics and how (or whether) they reach projections

- What is computed: per-item variance, escalation rate, seasonality, cadence drift (late or skipped occurrences), and confidence based on how much history exists.
- How it is presented: a history view per item, an account-level summary, or surfaced only as a better projected amount.
- How it feeds forecasts: an explicit per-item escalation rate, a category-level trend, or a statistical fit over history. Explicitly consider the failure mode of over-fitting very short or very sparse series.
- Whether the first release ships capture with **no** consumer at all. This is defensible: a time series only starts being useful once it starts being recorded, so capture-only has standalone value.

### 6. Backfill, import/export and migration

- No backfill is required, because history legitimately starts when the feature is enabled. Consider offering voluntary entry of past amounts (for example the last few occurrences, or "when did this price last change?") to make the analytics useful sooner.
- Maintenance CSV: whether captured rows are exported/imported at all, and if so, the metadata package version that carries them (`MetadataV4` → `V5` pattern from PRD-021).
- Existing users have no history: every consumer must treat an empty ledger as "no information", never as "nothing happened" — which is the same requirement as empty-history tolerance above, viewed from the data side.

### 7. Product guardrails

- POT is projection-first and explicitly not historical budgeting. The ledger exists to improve forward forecasts; it is not a transaction register, and it should not grow into one.
- Do not attempt balance reconciliation in the first iteration. `Balance` and `Reserved` remain user-entered.
- Do not let the ledger become the schedule cursor, or a source of due dates, overdue state or accruals. That is the one drift that would undo PRD-023's single-derivation property.

## Open Decisions

| ID    | Decision                                                                                                                        | Notes                                                                                                              |
| ----- | ------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| OD-01 | Which capability is the target: escalation/seasonality, budget-vs-actual, or both?                                              | Determines everything else, including whether per-occurrence capture is needed at all                              |
| OD-02 | Is amount-change history (captured when a user edits an amount) an adequate, cheaper alternative or adjunct to payment capture? | If escalation is the primary goal, this may deliver most of the value for a fraction of the effort and user burden |
| OD-03 | Opt-in at user level or site level, and how is it discovered and enabled?                                                       | Storage is site-scoped; the choice is personal                                                                     |
| OD-04 | Where does capture live: renewal flow, list row action, projection date detail, or a combination?                               | Renewal's many-cycle advance complicates a one-to-one mapping                                                      |
| OD-05 | Does "record payment" absorb, accompany or ignore renewal?                                                                      | Overlapping acknowledgement semantics; decide before either surface is built                                       |
| OD-06 | Are captured actuals allowed to change projected amounts, and at what granularity?                                              | Item-level escalation vs category-level trend                                                                      |
| OD-07 | Item reference and deletion semantics (live FK vs soft reference; cascade vs retention under PRD-022)                           | Determines what survives an item or site deletion                                                                  |
| OD-08 | Is the ledger part of the maintenance export/import package, and if so under which metadata version?                            | Follow the PRD-021 version-bump precedent if yes                                                                   |
| OD-09 | Should capture ship with no consumer as a first increment?                                                                      | Time series accrue value only once recording starts                                                                |
| OD-10 | Is this feature wanted at all?                                                                                                  | It is explicitly allowed to be a documented idea that is never implemented                                         |

## Design Notes (Rationale and Rejected Alternatives)

**Rejected: appending only at renewal.** Renewal captures no amount and cannot distinguish "paid as budgeted" from "paid differently", which is precisely the information the feature exists to capture. Renewal remains the most natural _prompt_ for capture, but it is not sufficient as the capture itself.

**Rejected: inferring escalation from `Amount` alone.** Because the edit form mutates `Amount` in place, the previous value is gone. There is nothing to infer from, which is why an amount snapshot at occurrence time (or an amount-change history) is a prerequisite for the escalation use case.

**Rejected: making the ledger the schedule cursor.** Deriving `NextDue` from settled occurrences would erase the overdue concept — renewal is currently the only record of acknowledgement, and overdue drives badges, status derivation, dashboard overview windows, the bulk renew breakdown and budget reminder emails. It would also require migration seeding to preserve current behaviour for existing users. The cursor stays where it is.

**Rejected: publishing schema in PRD-023.** The accrual derivation change must stand alone and must not assume a ledger. Mixing the two would make the simpler, immediately valuable change contingent on a feature that may never be built, and would leak an unapproved data model into an approved design. PRD-023 references this document only as a non-goal.

**Deliberately deferred: analytics design.** Trend, seasonality and confidence modelling are worthwhile but should be designed against real captured data, not up front.

## Related Documents

- [PRD-023 — Dynamic Accrual Calculation](PRD-023-dynamic-accrual-calculation.md): retires the persisted accrual aggregates; independent of this track and must not depend on it.
- [PRD-003 — Expense Accrual Policy Modes](PRD-003-expense-accrual-policy-modes.md): policy semantics the accrual engine keeps.
- [PRD-009 — Accrual Dirty Flag Rules and UI Status](PRD-009-accrual-dirty-flag-rules-and-ui-status-prd.md): the persistence and status model PRD-023 retires.
- [PRD-018 — Site Locale and Time Zone Settings](PRD-018-site-locale-settings.md): site-local dates and currency for any captured row.
- [PRD-019 — Email Outbox and Durable Dispatch](PRD-019-email-outbox-durable-dispatch.md): the repository's precedent for durable, idempotent, site-local-occurrence records.
- [PRD-021 — Remove Account BSB and Account Number](PRD-021-remove-account-bsb-and-number.md): the maintenance-package metadata versioning precedent.
- [PRD-022 — User and Site Deletion](PRD-022-user-and-site-deletion.md): deletion flows any retained history must participate in.
