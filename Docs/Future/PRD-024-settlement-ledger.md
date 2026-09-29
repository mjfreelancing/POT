# Settlement Ledger PRD

**Status:** Proposed (discovery draft)
**Priority:** Low
**Last Updated:** 2026-09-27
**Feature ID:** 024
**Scope:** Discovery only. Record what was actually paid or received against each expense and income occurrence, alongside the budgeted amount that was in force — a **budget-vs-actual history record** for later reporting and analysis. This document records the problem, the intended value, and the considerations that must be resolved before any design is fixed. It commits to no schema, no API shape and no UX, and it is deliberately independent of [PRD-023](PRD-023-dynamic-accrual-calculation.md) — nothing there depends on this, and this may never be implemented. No part of the accrual derivation change may assume a ledger exists.

**Explicitly out of scope:** anything that _computes_ from the record. The ledger is an append-only history table whose size grows with usage and whose older rows may be purged, so it must never become an input to a calculation, projection, accrual, due date or overdue state. Escalation/seasonality analytics and feeding actuals into projections are retained only as [Possible Future Considerations](#possible-future-considerations).

**Audience:** Future implementation agent(s), reviewers, and maintainers.

## Purpose

Decide whether POT should record what actually happened to each income and expense occurrence — the actual amount paid or received, set against the budgeted amount for that occurrence — so the product holds a truthful **budget-vs-actual history** that later reporting and analysis can use.

The aim is deliberately narrow: **capture and retain the record**. Nothing in this track changes a projection, an accrual, a due date or an overdue state, and no calculation may read the record (see [History-Record Guardrails](#history-record-guardrails)).

## Problem Statement

Each item carries a single point-in-time amount that is overwritten in place, and nothing records what was planned or what actually happened at any occurrence. `ProjectionsService` advances dates through `ExpenseRenewalCalculator` / `IncomeRenewalCalculator` and re-accrues each day, but `Renew` only ever moves `NextDue` and `AccrualStart` — `Amount` is never varied — so a rent expense of $500/month leaves no trace of what it was before the last edit.

There is no history in either direction:

1. **The amount series is destroyed as it is created.** `Amount` is mutated in place by the edit form, so when a subscription increases from $12 to $14 there is no record that it was ever $12. Capturing the budgeted amount in force at each occurrence is therefore a prerequisite for any budget-vs-actual comparison across time, and the only reason the figure has to be snapshotted rather than read from the current row.
2. **Nothing records that an occurrence actually happened.** Renewal (`RenewExpensesService`, `RenewalMode.Overdue` to catch up to today, `RenewalMode.Future` to advance exactly one cycle) advances the schedule and is the closest thing to an acknowledgement. It captures no amount, no date of payment, and no distinction between "paid as budgeted", "paid a different amount" and "didn't pay". As a result there is no budget-vs-actual comparison anywhere in the product.
3. **Capture has no home.** When a user renews from the dashboard, the only available assumption is that the actual equals the budgeted amount — which is exactly right for a fixed subscription and exactly wrong for a utility bill, a grocery allowance or a variable-rate mortgage. There is no point in the current flow at which a user could say "this one was different".
4. **Balance is unexplained.** `Account.Balance` and `Account.Reserved` are user-entered figures. Nothing in the product can explain how they got to their current values. (Reconciliation is **not** part of this track — see [Possible Future Considerations](#possible-future-considerations).)

The consequence is that the product holds no record of what actually happened: every occurrence is overwritten by the next, and there is nothing to report on or compare against later.

### What exists today (relevant context)

- Entities in `Pot.Data/Entities`: `Account`, `AccountAccrual`, `AuthSession`, `Expense`, `Income`, `OneTimePassword`, `Permission`, `Role`, `Setting`, `Site`, `User`. There is **no** history, occurrence or settlement entity — nothing anywhere records a past occurrence.
- `ExpenseEntity.Amount` / `IncomeEntity.Amount` are single mutable values. `ExpenseEntity.Accrued` is a derived allocation, not history, and is being retired by PRD-023.
- `NextDue` is the schedule cursor: user-entered on create/edit, advanced by renewal, and the sole basis for overdue. PRD-023 keeps it that way and explicitly does **not** derive it.
- Maintenance CSV export/import is versioned through `MetadataV1`–`MetadataV4`, with the importer already tolerant of ignored columns.
- Site-scoped global query filters apply to `Account`, `AccountAccrual`, `Expense`, `Income` and `Setting`; a ledger would be site-scoped financial data.

> **Note (2026-09-27).** Three entries above have moved since this was written, all through [PRD-023](PRD-023-dynamic-accrual-calculation.md): `AccountAccrual` is no longer an entity (first bullet), `ExpenseEntity.Accrued` is no longer stored because accrual is now computed on read — the retirement the second bullet anticipated has landed — and the global query filters therefore apply to `Account`, `Expense`, `Income` and `Setting` only (last bullet). Nothing else in the list changed, and none of it affects this track: a ledger would still be site-scoped financial data with no history entity in the model today.

## Intended Value

This track serves **one** capability: **budget vs actual** — how closely reality tracks the plan, per item and per account — captured as a retained history record.

| Capability                                                 | Signal required                                                | Capture burden                                              |
| ---------------------------------------------------------- | -------------------------------------------------------------- | ----------------------------------------------------------- |
| **Budget vs actual** — how closely reality tracks the plan | The actual amount (and whether the occurrence happened at all) | Requires per-occurrence capture, which is the opt-in burden |

The budgeted amount **in force at that occurrence** is captured alongside the actual, because it is what makes a comparison meaningful and what preserves an amount series the edit form otherwise destroys.

The related capability **escalation / seasonality** (projecting rent rising, utilities swinging with the season, a subscription stepping up) draws on the same captured amount series but exists to change _future forecasts_, so it is deliberately excluded here and retained under [Possible Future Considerations](#possible-future-considerations). Keeping it out is what keeps the capture burden acceptable.

## History-Record Guardrails

The ledger is a **history table, not a computation source**. Two properties force that boundary:

- **It grows without bound.** One row per settled occurrence per item is the natural shape, so the table's size tracks usage over years rather than the size of the working set.
- **It may be purged.** Older rows are a natural retention target. Anything that computed from the table would change its answer the day a purge ran.

Therefore:

- No projection, accrual, due date, overdue state or account balance may be derived from the ledger.
- The ledger must never become the schedule cursor. `NextDue` stays user-entered and renewal-driven (see §3).
- Reporting over the ledger is read-only and must tolerate missing, sparse or partially purged history.
- If a capability ever needs this data for a calculation, it must be copied into purpose-built, bounded storage rather than computed in place from the growing table.

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

The overlap that must be resolved regardless: renewal is today's de-facto acknowledgement of an occurrence, so a settlement capture that happens at payment time either absorbs renewal, accompanies it, or is unrelated to it. Two actions for one real-world event would be poor UX; conflating them risks changing what renewal means. Whether captured actuals may later influence projected amounts is **out of scope** for this track and retained under [Possible Future Considerations](#possible-future-considerations).

### 4. Data shape (considerations only — no schema is committed here)

Recorded because these are cheap to get right at table-creation time and expensive to retrofit:

- Site-local occurrence date, consistent with `NextDue` / `EndDate` semantics and mindful of PRD-018's hardcoded time zone.
- The budgeted amount **in force at that occurrence** as a snapshot, alongside the actual amount. The budgeted snapshot is what makes an amount series possible without a separate history mechanism.
- When the row was recorded and by whom; a source discriminator (manual today, import/feed later).
- Currency, once PRD-018 lands.
- An idempotency key over `(item, kind, occurrence date)` so repeat renewals or retries cannot duplicate.
- Item reference strategy: live foreign key versus a soft `RowId` plus a description snapshot, which decides what survives a rename or a delete.
- Whether the ledger participates in the account/site/user deletion flows (PRD-022) by cascade or by retention, and what the retention rule is.

### 5. Reporting and how the history is read

- Capture ships with **no consumer at all** in the first increment. This is defensible: a time series only starts being useful once it starts being recorded, so capture-only has standalone value — and it keeps the first release clear of the calculation boundary.
- Read-only reporting over the history (per-item variance, cadence drift, confidence based on history depth) is the intended later consumer.
- Anything that would _compute_ from the ledger is out of scope (see [History-Record Guardrails](#history-record-guardrails)). Escalation and projection influence are retained under [Possible Future Considerations](#possible-future-considerations).

### 6. Backfill, import/export and migration

- No backfill is required, because history legitimately starts when the feature is enabled. Consider offering voluntary entry of past amounts (for example the last few occurrences, or "when did this price last change?") to make the analytics useful sooner.
- Maintenance CSV: whether captured rows are exported/imported at all, and if so, the metadata package version that carries them (`MetadataV4` → `V5` pattern from PRD-021).
- Existing users have no history: every consumer must treat an empty ledger as "no information", never as "nothing happened" — which is the same requirement as empty-history tolerance above, viewed from the data side.

### 7. Product guardrails

- POT is projection-first. The ledger is a **history record for reporting and analysis**; it is not a transaction register, an accrual source or a projection input, and it should not grow into any of those.
- Do not derive anything from the ledger. No due date, overdue state, accrual value, balance or projection output may read it (see [History-Record Guardrails](#history-record-guardrails)).
- Do not attempt balance reconciliation in the first iteration. `Balance` and `Reserved` remain user-entered.

## Possible Future Considerations

Recorded so they are not lost, and explicitly **not** part of this track. None may be implemented without a separate decision, because each one either changes a projection or turns the ledger into a calculation input.

- **Escalation and seasonality.** Using the captured amount series to project rent rises, seasonal utility swings or a subscription stepping up. The signal is largely obtainable from amount _changes_ rather than from payments.
- **Feeding actuals into projections.** Letting observed amounts change the projected amount for future cycles, at item or category granularity. Over-fitting short or sparse series is the main failure mode.
- **Budget-vs-actual analytics.** Per-item variance, cadence drift (late or skipped occurrences) and confidence based on history depth — read-only reporting over the history, subject to the guardrails above.
- **Balance reconciliation.** Explaining how `Balance` and `Reserved` reached their current values.
- **Retention and purge.** Defining how long history is kept and how purging interacts with reporting.

## Open Decisions

| ID    | Decision                                                                                                                          | Notes                                                                                                                                                              |
| ----- | --------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| OD-01 | Which capability is the target: escalation/seasonality, budget-vs-actual, or both? **Settled:** budget-vs-actual only             | Escalation/seasonality moved to [Possible Future Considerations](#possible-future-considerations); the capture burden is acceptable for only one capability        |
| OD-02 | Is amount-change history (captured when a user edits an amount) an adequate, cheaper alternative or adjunct to payment capture?   | **Deferred** with escalation/seasonality — it is an escalation mechanism, not a budget-vs-actual one                                                               |
| OD-03 | Opt-in at user level or site level, and how is it discovered and enabled?                                                         | Storage is site-scoped; the choice is personal                                                                                                                     |
| OD-04 | Where does capture live: renewal flow, list row action, projection date detail, or a combination?                                 | Renewal's many-cycle advance complicates a one-to-one mapping                                                                                                      |
| OD-05 | Does "record payment" absorb, accompany or ignore renewal?                                                                        | Overlapping acknowledgement semantics; decide before either surface is built                                                                                       |
| OD-06 | Are captured actuals allowed to change projected amounts, and at what granularity?                                                | **Deferred** and explicitly out of scope: the ledger is not a calculation source. Retained under [Possible Future Considerations](#possible-future-considerations) |
| OD-07 | Item reference and deletion semantics (live FK vs soft reference; cascade vs retention under PRD-022)                             | Determines what survives an item or site deletion                                                                                                                  |
| OD-08 | Is the ledger part of the maintenance export/import package, and if so under which metadata version?                              | Follow the PRD-021 version-bump precedent if yes                                                                                                                   |
| OD-09 | Should capture ship with no consumer as a first increment?                                                                        | Time series accrue value only once recording starts; consistent with a history-only scope                                                                          |
| OD-10 | Is this feature wanted at all?                                                                                                    | It is explicitly allowed to be a documented idea that is never implemented                                                                                         |
| OD-11 | Retention and purge policy for the history table. **Settled direction:** older rows may be purged, so nothing may compute from it | See [History-Record Guardrails](#history-record-guardrails)                                                                                                        |

## Design Notes (Rationale and Rejected Alternatives)

**Rejected: appending only at renewal.** Renewal captures no amount and cannot distinguish "paid as budgeted" from "paid differently", which is precisely the information the feature exists to capture. Renewal remains the most natural _prompt_ for capture, but it is not sufficient as the capture itself.

**Rejected: inferring escalation from `Amount` alone.** Because the edit form mutates `Amount` in place, the previous value is gone. There is nothing to infer from, which is why an amount snapshot at occurrence time (or an amount-change history) is a prerequisite for the escalation use case.

**Rejected: making the ledger the schedule cursor.** Deriving `NextDue` from settled occurrences would erase the overdue concept — renewal is currently the only record of acknowledgement, and overdue drives badges, status derivation, dashboard overview windows, the bulk renew breakdown and budget reminder emails. It would also require migration seeding to preserve current behaviour for existing users. The cursor stays where it is.

**Rejected: publishing schema in PRD-023.** The accrual derivation change must stand alone and must not assume a ledger. Mixing the two would make the simpler, immediately valuable change contingent on a feature that may never be built, and would leak an unapproved data model into an approved design. PRD-023 references this document only as a non-goal.

**Deliberately deferred: analytics design.** Trend, seasonality, confidence modelling and any use of actuals in forecasts are worthwhile but should be designed against real captured data, and against the [History-Record Guardrails](#history-record-guardrails), not up front.

**Rejected: using the ledger as a calculation source.** Its size grows with usage and older rows may be purged, so any calculation that read it would change its answer the day a purge ran. Reporting over it is read-only and tolerates missing history.

**Rejected: deriving arrears or overdue state from the ledger.** Arrears is derived from the schedule — frequency plus the persisted `NextDue` — not from payment history; [PRD-023](PRD-023-dynamic-accrual-calculation.md) does exactly that without a ledger. The ledger's later value is to make that derivation _trustworthy_ (it can distinguish "paid but not renewed" from "genuinely missed"), not to supply it.

## Related Documents

- [PRD-023 — Dynamic Accrual Calculation](PRD-023-dynamic-accrual-calculation.md): retires the persisted accrual aggregates; independent of this track and must not depend on it.
- [PRD-003 — Expense Accrual Policy Modes](PRD-003-expense-accrual-policy-modes.md): policy semantics the accrual engine keeps.
- [PRD-009 — Accrual Dirty Flag Rules and UI Status](PRD-009-accrual-dirty-flag-rules-and-ui-status-prd.md): the persistence and status model PRD-023 retires.
- [PRD-018 — Site Locale and Time Zone Settings](PRD-018-site-locale-settings.md): site-local dates and currency for any captured row.
- [PRD-019 — Email Outbox and Durable Dispatch](PRD-019-email-outbox-durable-dispatch.md): the repository's precedent for durable, idempotent, site-local-occurrence records.
- [PRD-021 — Remove Account BSB and Account Number](PRD-021-remove-account-bsb-and-number.md): the maintenance-package metadata versioning precedent.
- [PRD-022 — User and Site Deletion](PRD-022-user-and-site-deletion.md): deletion flows any retained history must participate in.
