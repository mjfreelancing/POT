# Expense Accrual Policy Modes Plan

Feature ID: 003
Date created: 2026-04-03
Scope: Implemented and validated.
Audience: Future implementation agent(s), reviewers, and maintainers.

## Objective

Reduce accidental daily-need inflation and simplify expense setup by making accrual behavior server-driven and policy-based, while preserving an optional first-cycle start-date override for advanced cases.

## Problem Statement (Confirmed)

Current model assumes accrual-driven allocation for all expenses and requires explicit accrual start dates.

1. Some expenses should not pre-accrue.

- Especially for one-time expenses, users may prefer taking the full due-date hit rather than contributing to daily need.

2. First-cycle setup can require manual date backfilling.

- Users currently adjust accrual start to a previous virtual due date to avoid temporary inflation.

3. Accrual concepts are not obvious for many users.

- Current UX exposes a low-level date control that many users do not understand or need.

4. Server should remain source of truth.

- Frontend-only date logic risks drift from server behavior.

## PRD Status

- Stage: Complete
- Code changes: Complete
- Last updated: 2026-04-06

Implementation reference:

- Stable and dynamic accrual semantics are documented in [Source/Server/IMPLEMENTATION.md](../../Source/Server/IMPLEMENTATION.md).

## Decisions Confirmed In Discussion

1. Existing data migration strategy for new policy field

- Migration 1: add nullable accrual policy column.
- Migration 2: backfill policy values.
  - One-time expenses: None.
  - All other expenses: Automatic.
- Migration 3: make accrual policy non-nullable.

2. Default policy on new create

- Confirmed: one-time defaults to None.
- Confirmed: recurring defaults to Automatic.

3. Server-driven functional behavior first

- UI/UX layout discussion is intentionally deferred until functional semantics are finalized.

4. Policy simplification

- Replace three-mode draft (Automatic/Manual/None) with two modes (Automatic/None).
- Manual is treated as an optional start-date override within Automatic.

## Current State Summary (Verified)

1. Expense contracts require a non-null accrual start date.

- Entity: [Source/Server/Pot.Data/Entities/ExpenseEntity.cs](../../Source/Server/Pot.Data/Entities/ExpenseEntity.cs#L19)
- API create request: [Source/Server/Pot.AspNetCore/Features/Expenses/Create/Request.cs](../../Source/Server/Pot.AspNetCore/Features/Expenses/Create/Request.cs#L12)
- API update request: [Source/Server/Pot.AspNetCore/Features/Expenses/Update/Request.cs](../../Source/Server/Pot.AspNetCore/Features/Expenses/Update/Request.cs#L18)

2. Accrual and stable daily metrics are date-driven.

- Calculator: [Source/Server/Pot.App/Calculators/AccrueExpenseCalculator.cs](../../Source/Server/Pot.App/Calculators/AccrueExpenseCalculator.cs)
- Expense date math helpers: [Source/Server/Pot.Data/Extensions/ExpenseEntityExtensions.cs](../../Source/Server/Pot.Data/Extensions/ExpenseEntityExtensions.cs)

3. Renewal mutates accrual start every cycle.

- Renewal logic: [Source/Server/Pot.App/Calculators/ExpenseRenewalCalculator.cs](../../Source/Server/Pot.App/Calculators/ExpenseRenewalCalculator.cs)

4. Projections renew first, then accrue.

- Ordering: [Source/Server/Pot.App/Features/Projections/ProjectionsService.cs](../../Source/Server/Pot.App/Features/Projections/ProjectionsService.cs#L86)

5. Frontend currently exposes explicit accrual start in create/edit expense forms.

- Form: [Source/Client/pot-react/src/features/expenses/components/ExpenseForm.tsx](../../Source/Client/pot-react/src/features/expenses/components/ExpenseForm.tsx)

## Candidate Policy Model

Proposed expense-level policy enum (server-defined and persisted):

1. Automatic

- Server computes and maintains recurring-cycle accrual behavior.
- Intended default for most users.
- User may provide an optional accrual start override on create/update to influence the first active cycle.
- After renewal, behavior continues automatically.

2. None

- No accrual contribution to daily/stable accrual metrics.
- Expense is treated as pay-on-due for projection debits only.

## Baseline Architecture Direction (Current Preference)

1. Add policy enum to expense model and API contracts.

- Policy remains explicit and inspectable.
- Renewal/write paths can set accrual start deterministically based on policy.

2. Use nullable accrual start in API/frontend request contracts.

- Client should not send sentinel date values for non-editable modes.
- For Automatic and None, accrual start may be omitted/null.
- For Automatic, when accrual start is provided, server validates the date using the same optional-field pattern used by End Date (validate only when present).
- Server derives canonical persisted value when missing.
- Server must validate the request has no date when None.
- For None, canonical persisted `AccrualStart` should be null once the database column is nullable.

3. Keep server as source of truth for derived dates.

- Frontend submits intent (policy + optional override date).
- Server computes canonical values and returns canonical state.
- Frontend should update local form/list state from server response after save.

4. None mode must be explicit in accrual calculation paths.

- Date alignment alone is insufficient for recurring items because recurring stable accrual currently does not depend on accrual start.
- None mode should short-circuit both daily and stable accrual contributions.

## Important Clarification from Discussion

Policy still likely needs to be known by server renewal/update logic.

- Even if date drives accrual calculations, server needs policy intent to decide whether and how accrual start should advance per cycle.
- This supports deterministic behavior without frontend date rules.

## Initial UX Direction

1. Reposition fields around schedule intent.

- Keep Next Due and End Date together.
- Introduce Accrual Policy control.

2. Keep Accrual Start visible with mode-based interactivity.

- Show as editable override for Automatic.
- Keep visible but disabled for None to avoid layout jump.
- Null value display is acceptable and aligns with existing optional-date UX behavior.

3. Keep UX server-aligned.

- Avoid frontend-only derivation that can diverge from backend behavior.

## UX Decision Matrix (Policy + Date)

This table isolates the agreed UX and contract combinations for `Accrual Policy` and `Accrual Start`.

| Accrual Policy | `AccrualStart` Input | Date Requirement | UI Behavior                             | Server Result                                                              |
| -------------- | -------------------- | ---------------- | --------------------------------------- | -------------------------------------------------------------------------- |
| Automatic      | Date provided        | Optional         | Editable date input                     | Uses provided date as first-cycle override.                                |
| Automatic      | Empty/null           | Optional         | Editable date input (may be left empty) | Server computes canonical date (current server rule: defaults to `today`). |
| None           | Empty/null           | Required empty   | Date shown but disabled                 | Persists `AccrualStart = null`; accrual contribution remains disabled.     |
| None           | Date provided        | Invalid          | UI should prevent entry                 | Request is rejected by validation (`None` accepts null only).              |

Quick guide after server canonicalization is applied (current modes only):

- Empty `AccrualStart` in persisted/table data implies policy is `None`.
- Date value in persisted/table data implies policy is `Automatic`.
- This interpretation is valid for current modes (`Automatic`, `None`) and current server canonicalization rules.

## Functional Rule Table (Implementation Contract)

This table defines server behavior for create, update, renewal, and accrual paths.

| Operation              | Policy    | Input AccrualStart | Server Canonicalization                                                                                                 | DailyExpenseAccrual Contribution             | StableExpenseAccrual Contribution                   | Notes                                                                                             |
| ---------------------- | --------- | ------------------ | ----------------------------------------------------------------------------------------------------------------------- | -------------------------------------------- | --------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Create                 | Automatic | null               | Set to server-derived default start for first cycle (initially `today`; can be overridden by future policy refinement). | Calculated using existing date-driven logic. | Calculated using existing recurring stable logic.   | Response returns canonical stored value.                                                          |
| Create                 | Automatic | date               | Use provided date as first-cycle override; persist canonical date.                                                      | Calculated using existing date-driven logic. | Calculated using existing recurring stable logic.   | Override affects first cycle only.                                                                |
| Create                 | None      | null               | Persist `AccrualStart = null` and policy=None.                                                                          | 0                                            | 0                                                   | Expense still debits projected/account balance on due date. Reject request if a date is supplied. |
| Update                 | Automatic | null               | Recompute canonical start using server rules.                                                                           | Calculated using existing date-driven logic. | Calculated using existing recurring stable logic.   | Use response as source of truth in UI.                                                            |
| Update                 | Automatic | date               | Persist provided first-cycle override date.                                                                             | Calculated using existing date-driven logic. | Calculated using existing recurring stable logic.   | Renewal resumes automatic advancement.                                                            |
| Update                 | None      | null               | Persist policy=None and `AccrualStart = null`.                                                                          | 0                                            | 0                                                   | Reject request if a date is supplied.                                                             |
| Renew (Future/Overdue) | Automatic | n/a                | Existing renewal progression applies; accrual start advanced by server recurrence rules.                                | Calculated as today from renewed dates.      | Calculated as today from renewed frequency average. | No special mode branching beyond policy gate.                                                     |
| Renew (Future/Overdue) | None      | n/a                | NextDue still advances by recurrence rules; accrual contribution remains disabled; `AccrualStart` stays null.           | 0                                            | 0                                                   | Preserves scheduling/pay-on-due without pre-funding.                                              |
| AccrueExpenses runtime | Automatic | n/a                | Use existing accrual/start/frequency paths.                                                                             | Existing behavior.                           | Existing behavior.                                  | No change except nullable request normalization before persistence.                               |
| AccrueExpenses runtime | None      | n/a                | Explicit policy short-circuit in accrual calculator path.                                                               | 0                                            | 0                                                   | Required because recurring stable accrual is not start-date-driven today.                         |

### Operational Clarifications

1. None means no accrual: both daily and stable expense accrual contributions are zero on all dates.
2. None does not mean ignore expense: due-date debit and scheduling still apply via NextDue/renewal.
3. Automatic date override is a setup control, not a separate long-lived mode.
4. Create/update endpoints should return canonical persisted values so the UI can refresh without local derivation.
5. Validation contract: Automatic accepts null or date (date validated only when provided); None accepts null only.

## Decisions Status

No open decisions currently tracked.

### Confirmed Decisions

1. None mode accrual behavior: daily and stable accrual contributions remain zero on all dates, including renewal cycles; account balance still debits on due date according to existing scheduling/renewal rules.
2. Automatic override behavior: override applies to first cycle only; renewal then follows automatic progression.
3. API response contract: create/update return canonical persisted values for UI refresh.
4. Request validation contract: Automatic accepts null or date (validated when provided); None accepts null only.
5. None-mode canonicalization decision: persist `AccrualStart = null`; renewal does not need to advance it for None mode.
6. Terminology decision: use "Accrual Policy".
7. UI/UX workflow decision: implement server-side policy and nullable contract changes first, then refine UI/UX in real time during implementation.

## Risks And Mitigations

1. Behavior drift between frontend and backend.

- Mitigation: server computes canonical accrual start; client displays returned values.

2. Ambiguity around None mode if policy is not represented server-side.

- Mitigation: persist explicit policy enum per expense.

3. Contract impact from nullable accrual start in API requests.

- Mitigation: keep server canonicalization explicit and update validators/mappings/tests together.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Existing planning docs:
  - [Docs/Future/001-per-session-login-support.md](001-per-session-login-support.md)
  - [Docs/Future/002-server-login-logout-session-architecture.md](002-server-login-logout-session-architecture.md)
