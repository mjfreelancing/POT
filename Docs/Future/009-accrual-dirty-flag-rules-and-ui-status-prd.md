# Accrual Dirty State Architecture PRD

- Feature ID: 009
- Date created: 2026-04-18
- Status: Planning
- Priority: High
- Audience: Server and client maintainers, reviewers, and future implementation agents.

## Objective

Define and implement an account-level accrual status architecture that:

1. Removes false UI notices.
2. Preserves daily staleness behavior.
3. Avoids account etag churn from dirty-state writes.
4. Simplifies status querying and operational reasoning.

## Business Problem

Current accrual status is inferred from expense-level fields and mixed write paths. This causes imprecision and maintenance overhead.

1. Non-accrual expense edits can trigger dirty status.
2. Some mutation paths (especially delete and account reassignment impact) are harder to reason about consistently.
3. Status reads are driven by expense-level predicates rather than explicit account-level accrual state.

## Current State Summary

The existing implementation is expense-driven:

1. Expense dirty writes are set in create, update, toggle exclusion, and renewal.
2. Expense dirty is cleared during accrual calculation.
3. Status currently treats an account as requiring accrual when any related expense satisfies:
   - `AccruedIsDirty == true`, or
   - `LastAccruedUpdate == null`, or
   - `LastAccruedUpdate < asOfDate`.

Key references:

- [Source/Server/Pot.App/Features/Expenses/Create/CreateExpenseService.cs](../../Source/Server/Pot.App/Features/Expenses/Create/CreateExpenseService.cs#L60)
- [Source/Server/Pot.App/Features/Expenses/Update/UpdateExpenseService.cs](../../Source/Server/Pot.App/Features/Expenses/Update/UpdateExpenseService.cs#L108)
- [Source/Server/Pot.App/Features/Expenses/ToggleExclude/ExcludeExpensesService.cs](../../Source/Server/Pot.App/Features/Expenses/ToggleExclude/ExcludeExpensesService.cs#L47)
- [Source/Server/Pot.App/Calculators/ExpenseRenewalCalculator.cs](../../Source/Server/Pot.App/Calculators/ExpenseRenewalCalculator.cs#L46)
- [Source/Server/Pot.App/Calculators/AccrueExpenseCalculator.cs](../../Source/Server/Pot.App/Calculators/AccrueExpenseCalculator.cs#L42)
- [Source/Server/Pot.Data/Specifications/ExpenseSpecifications.cs](../../Source/Server/Pot.Data/Specifications/ExpenseSpecifications.cs#L21)
- [Source/Server/Pot.Data/Repositories/Expenses/ExpenseRepository.cs](../../Source/Server/Pot.Data/Repositories/Expenses/ExpenseRepository.cs#L65)

## Decision Log

### Decision 1: Target architecture

Adopt a dedicated account accrual state table (one row per account) as the source of truth for status.

Rationale:

1. Supports direct account-level status checks.
2. Avoids writing dirty state to the account entity itself, reducing etag/concurrency side effects.
3. Removes dependency on expense-level dirty columns for status.

### Decision 2: Expense columns to remove

Plan to remove expense-level accrual status columns after migration/cutover:

1. `Expense.AccruedIsDirty`
2. `Expense.LastAccruedUpdate`

### Decision 3: Required account accrual state fields

Required table fields:

1. `AccountRowId` (PK/FK)
2. `LastAccruedDate` (`DateOnly`, nullable)
3. `AccruedIsDirty` (`bool`)

Explicitly not required for correctness:

1. Dirty reason flags
2. Dirty timestamp fields

### Decision 4: Status rule

An account requires accrual update when:

1. `AccruedIsDirty == true`, or
2. `LastAccruedDate == null`, or
3. `LastAccruedDate < asOfDate`

This preserves daily staleness behavior without requiring a daily background job.

### Decision 5: Mutation semantics

On accrual-impacting changes, mark affected account(s) dirty.

Idempotency rule:

1. If an account is already dirty, no additional state transition is required beyond ensuring `AccruedIsDirty` remains true.

2. Create expense: mark target account dirty.
3. Update expense:
   - If change is accrual-impacting, mark account dirty.
   - If account changes, mark both old and new accounts dirty.

4. Toggle exclusion: mark account dirty only when the expense is accrual-impacting.
5. Renew expense: mark account dirty.
6. Delete expense: mark account dirty only when the deleted expense is accrual-impacting.

For this PRD, accrual-impacting includes:

1. `AccrualPolicy != None`
2. The expense would otherwise participate in accrual outcomes under the active accrual rules.

On successful accrual for an account:

1. Set `AccruedIsDirty = false`
2. Set `LastAccruedDate = asOfDate`

### Decision 6: Metadata-only updates

Metadata-only updates should not mark account accrual state dirty.

Initial non-impacting fields:

1. `Description`
2. `Note`

### Decision 7: Account dirty column on Account entity

Do not add an accrual dirty flag column to the account entity.

Rationale:

1. Avoid account etag churn from frequent expense writes.
2. Keep dirty-state writes isolated to dedicated accrual state storage.

## Data Model

Proposed logical table (name to be confirmed during implementation design):

1. `AccountAccrual`
2. Columns:
   - `AccountRowId` (`Guid`, PK, FK to `Account.RowId`)
   - `LastAccruedDate` (`DateOnly`, nullable)
   - `AccruedIsDirty` (`bool`, non-null)

Behavioral contract:

1. Exactly one row per account.
2. Row is created/maintained as part of account lifecycle and accrual workflows.

## Functional Rules Matrix

| Event                                 | Required state update                                                                    |
| ------------------------------------- | ---------------------------------------------------------------------------------------- |
| Expense create                        | Mark account `AccruedIsDirty = true`.                                                    |
| Expense update (impacting fields)     | Mark account `AccruedIsDirty = true`.                                                    |
| Expense update (account reassignment) | Mark old account and new account `AccruedIsDirty = true`.                                |
| Expense update (metadata-only fields) | No change to account accrual state.                                                      |
| Toggle exclusion                      | Mark account `AccruedIsDirty = true` only when the expense is accrual-impacting.         |
| Renew expense                         | Mark account `AccruedIsDirty = true`.                                                    |
| Delete expense                        | Mark account `AccruedIsDirty = true` only when the deleted expense is accrual-impacting. |
| Accrue account (success)              | Set `AccruedIsDirty = false`; set `LastAccruedDate = asOfDate`.                          |
| Status check                          | Account requires update when dirty, never accrued, or last accrued before as-of date.    |

## Recommended Delivery Sequence

### Phase 1: Schema and compatibility

1. Add `AccountAccrual` table and indexes.
2. Backfill one row per existing account.
3. Initialize backfill values using a safety-first approach:

- Set `AccruedIsDirty = true` for all existing accounts.
- Set `LastAccruedDate = null` for all existing accounts.

This forces a full recalculation pass after rollout and helps rectify any pre-existing inconsistent accrual state.

### Phase 2: Dual-write / compatibility window

1. Update accrual-impacting write paths to maintain `AccountAccrual`.
2. Keep existing expense fields temporarily for safe rollout.
3. Add parity checks in tests to ensure account-state rule matches current behavior.
4. Execute an implementation-pattern review checkpoint after migration and entity updates are available, before finalizing the HOW pattern.

### Phase 3: Status read cutover

1. Switch accrual status queries to account-state table.
2. Validate dashboard behavior and actions against expected scenarios.

### Phase 4: Cleanup

1. Remove expense-level status columns:

- `AccruedIsDirty`
- `LastAccruedUpdate`

2. Remove obsolete expense-level status logic and tests.
3. Finalize and simplify documentation.

## Test Plan Requirements

### Unit and service coverage

1. Mutation-to-dirty mapping tests per event type.
2. Account reassignment tests covering dual-account dirty updates.
3. Metadata-only update tests confirming no dirty transition.
4. Accrual success tests confirming dirty clear and date update.

### Data/repository coverage

1. Account status rule tests for all three status conditions.
2. Query tests for account-set status retrieval.

### Integration coverage

1. Endpoint-level status behavior (`/accruals/status`) under mixed account scenarios.
2. End-to-end flows for create/update/delete/toggle/renew followed by status refresh.

## Risks and Mitigations

1. Risk: Behavior drift during migration.
   - Mitigation: phased rollout with dual-write and parity tests.

2. Risk: Incorrect backfill values.
   - Mitigation: migration validation scripts and targeted fixtures.

3. Risk: Missed dirty updates in edge mutation paths.
   - Mitigation: explicit mutation matrix coverage and regression tests.

## Open Question

Implementation HOW remains open:

1. Should accrual-impact detection be implemented as:

- a dedicated boolean method,
- a decision/specification object, or
- another pattern?

Constraint for this decision:

1. The implementation must support precise accrual-impact detection, including account reassignment, while keeping code ownership clear and testable.

### Proposal For Further Review (Deferred Until Post-Migration Entities Exist)

To avoid losing current architectural guidance while deferring a final commitment too early, the following proposal is retained for post-migration review:

1. Preferred candidate pattern to review first:
   - Decision/specification object for accrual-impact analysis.
   - Dedicated mutation service for `AccountAccrual` state transitions.
   - Feature services orchestrate both components.
2. Why this is the leading candidate:
   - Centralized rule logic (single source of truth).
   - Clear separation of decision logic and state mutation.
   - Strong unit-testability at analyzer, mutation-service, and orchestration levels.
3. Why final decision is deferred:
   - Entity/repository surface will change during migration.
   - Final method signatures and transaction boundaries should be validated against actual post-migration model shape.
4. Required outcome of the review checkpoint:
   - Confirm or reject the hybrid candidate.
   - Publish final HOW choice and concrete class/interface signatures.
   - Update this PRD decision log with the selected implementation pattern.

### Detailed Specialist Review Capture (Preserve For Post-Migration Decision)

This section captures the detailed architecture review requested in discussion so design intent is not lost before implementation begins.

#### Candidate Pattern Comparison

1. Pattern A: Simple boolean utility methods
   - Shape:
     - `IsExpenseAccrualImpacting(expense) -> bool`
     - `HasAccrualImpactingChange(before, after) -> bool`
   - Strengths:
     - Minimal code and low ceremony.
     - Very easy to unit test as pure functions.
     - Fast to introduce during migration.
   - Risks:
     - Rule usage can drift across mutation paths.
     - No structured output for multi-account effects.
     - Harder to prove all services consistently apply the same logic.
   - Assessment:
     - Useful as a temporary step, weaker as final architecture.

2. Pattern B: Decision/specification object with structured result
   - Shape:
     - `AnalyzeUpdate(before, after) -> AccrualImpactResult`
     - `AnalyzeDelete(expense) -> AccrualImpactResult`
     - `AnalyzeToggleExclusion(expense) -> AccrualImpactResult`
   - Strengths:
     - Single source of truth for rules.
     - Supports account reassignment and multi-account marking naturally.
     - Highly testable and reviewable.
   - Risks:
     - Services must still call and apply results correctly.
   - Assessment:
     - Strong candidate.

3. Pattern C: Command/event heavy domain orchestration
   - Shape:
     - command handlers/events for dirty-state transitions.
   - Strengths:
     - Strong decoupling and audit/event extensibility.
   - Risks:
     - High indirection and boilerplate for current scope.
     - Harder debugging and increased implementation overhead.
   - Assessment:
     - Not preferred for this feature scope.

4. Pattern D: Hybrid (recommended candidate for review)
   - Shape:
     - Specification/decision analyzer for rule evaluation.
     - Dedicated mutation service for `AccountAccrual` state changes.
     - Feature services orchestrate analyzer + mutation service.
   - Strengths:
     - Clear responsibilities and low ceremony.
     - Strong unit and integration testability.
     - Precise handling of reassignment, delete, toggle conditions.
     - Supports idempotency and transactional clarity.
   - Risks:
     - Requires disciplined service usage and review gates.
   - Assessment:
     - Leading candidate for final HOW decision.

#### Pattern Scoring Snapshot (Specialist Summary)

| Pattern                                              | Unit testability             | Service testability                            | Implementation complexity | Migration friendliness | Edge-case miss risk |
| ---------------------------------------------------- | ---------------------------- | ---------------------------------------------- | ------------------------- | ---------------------- | ------------------- |
| Pattern A: boolean utility                           | Very high for pure functions | Moderate (usage consistency harder to enforce) | Low                       | High                   | High                |
| Pattern B: decision/specification object             | Very high                    | High                                           | Medium                    | High                   | Medium              |
| Pattern C: command/event orchestration               | High                         | Medium                                         | High                      | Medium-low             | Medium-high         |
| Pattern D: hybrid (specification + mutation service) | Very high                    | Very high                                      | Medium                    | Very high              | Low                 |

Specialist interpretation:

1. Pattern A is simple but easier to apply inconsistently.
2. Pattern B is strong but still requires clean orchestration discipline.
3. Pattern C is likely over-scoped for this feature.
4. Pattern D offers the best balance of precision, maintainability, and testability.

#### Suggested Placement (If Hybrid Is Confirmed)

1. Rule analyzer (application layer)
   - `Pot.App/Features/Expenses/Accrual/AccrualImpactSpecification.cs`
2. Analyzer output model
   - `Pot.App/Features/Expenses/Accrual/Models/AccrualImpactResult.cs`
3. Account accrual mutation service
   - `Pot.App/Features/Accounts/Accrual/AccountAccrualMutationService.cs`
4. Account accrual entity and repository contracts
   - `Pot.Data/Entities/AccountAccrual.cs`
   - `Pot.Data/Repositories/...` (account-accrual repository abstraction + implementation)

#### Recommended Component Contract (Draft For Review)

1. Analyzer result model
   - `AccrualImpactResult`
     - `IsImpacting : bool`
     - `AccountRowIdsToMark : Guid[]`
     - `Reason : string` (diagnostic/logging; optional for correctness)

2. Analyzer interface
   - `IsExpenseAccrualImpacting(expense)`
   - `AnalyzeUpdate(before, after)`
   - `AnalyzeDelete(expense)`
   - `AnalyzeToggleExclusion(expense)`

3. Mutation service interface
   - `MarkAccountsDirtyAsync(accountRowIds, reason, cancellationToken)`
   - `ClearAccrualDirtyAsync(accountRowId, asOfDate, cancellationToken)`

4. Repository requirements
   - Get account-accrual row by account id.
   - Batch get by account ids.
   - Add/update/save with idempotent behavior.

#### Suggested Orchestration Flow (Pseudo)

1. Update expense (impacting fields)
   - Snapshot original state.
   - Apply input changes.
   - Analyze impact.
   - If impacting, mark returned account ids dirty.
   - Persist.

2. Update expense (metadata-only)
   - Snapshot original state.
   - Apply input changes.
   - Analyze impact.
   - If not impacting, skip dirty updates.
   - Persist.

3. Update expense (account reassignment)
   - Snapshot original state with old account.
   - Apply new account.
   - Analyze impact.
   - Mark both old and new accounts dirty.
   - Persist.

4. Delete expense
   - Analyze deleted expense impact before removal.
   - If impacting, mark account dirty.
   - Delete and persist.

5. Toggle exclusion
   - Toggle flag.
   - Analyze toggle impact.
   - Mark account dirty only when accrual-impacting condition is true.
   - Persist.

6. Accrue success
   - On successful accrual completion for an account:
     - `AccruedIsDirty = false`
     - `LastAccruedDate = asOfDate`

#### Transaction and Idempotency Guidance

1. Dirty-state transitions are idempotent.
   - If already dirty, no additional state transition is required.

2. Keep mutation and dirty updates in aligned transaction boundaries where practical.
   - Avoid scenarios where expense writes commit but accrual-state updates fail (or vice versa).

3. Reassignment is a two-account concern.
   - Always mark both old and new account rows when reassignment is accrual-impacting.

4. Missing account-accrual row handling
   - Preferred: row exists for every account by lifecycle guarantee.
   - Fallback: create row lazily and proceed safely.

#### Comprehensive Test Matrix (For Later Implementation)

1. Analyzer unit tests
   - Impacting updates: amount, next due, accrual window, frequency, frequency count, policy, exclusion.
   - Non-impacting updates: description, note.
   - Toggle exclusion with policy None vs non-None.
   - Delete with accrual-impacting vs non-impacting expense.
   - Reassignment marks both accounts.

2. Mutation service unit tests
   - Marks clean account dirty.
   - No-op/idempotent when already dirty.
   - Clears dirty and sets `LastAccruedDate` on success.
   - Handles missing account-accrual row per lifecycle decision.
   - Batch behavior for multi-account updates.

3. Service orchestration tests
   - Update path calls analyzer and applies returned account ids.
   - Metadata-only updates do not dirty accounts.
   - Delete/toggle conditional behavior enforced.
   - Reassignment marks both old/new accounts.

4. Repository/data tests
   - Account-level status rule across all three conditions:
     - dirty true
     - never accrued
     - stale date
   - Query behavior for account sets.

5. Integration tests
   - End-to-end status behavior via `/accruals/status` for mixed account scenarios.
   - Create/update/delete/toggle/renew then status refresh assertions.
   - Migration parity checks during dual-write window.

#### Acceptance Criteria For Post-Migration HOW Review

The selected HOW pattern must demonstrate:

1. Full coverage of the functional rules matrix.
2. Explicit support for reassignment and conditional toggle/delete behavior.
3. Unit-testability of rule logic without database dependencies.
4. Idempotent dirty-state updates.
5. Safe transactional behavior for write + dirty-state transitions.
6. Clear ownership boundaries that align with repository layering.

## Out of Scope

1. Changing accrual math formulas.
2. Client UX redesign beyond status correctness.

## References

- [Source/Server/Pot.App/Features/Accruals/Status/AccrualsStatusService.cs](../../Source/Server/Pot.App/Features/Accruals/Status/AccrualsStatusService.cs)
- [Source/Server/Pot.App/Features/Accruals/AccrueExpenses/AccrueExpensesService.cs](../../Source/Server/Pot.App/Features/Accruals/AccrueExpenses/AccrueExpensesService.cs)
- [Source/Client/pot-react/src/features/dashboard/contexts/AccrualsContext.tsx](../../Source/Client/pot-react/src/features/dashboard/contexts/AccrualsContext.tsx)
- [Source/Client/pot-react/src/api/hooks/useAccrualsStatus.ts](../../Source/Client/pot-react/src/api/hooks/useAccrualsStatus.ts)
- [Docs/Future/README.md](README.md)
