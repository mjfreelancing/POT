# Accrual Dirty State Architecture PRD

- Feature ID: 009
- Date created: 2026-04-18
- Status: Implemented
- Priority: High
- Audience: Server and client maintainers, reviewers, and future implementation agents

## Objective

Implement account-level accrual status architecture that:

1. Removes false UI notices.
2. Preserves daily staleness behavior.
3. Avoids account etag churn from dirty-state writes.
4. Simplifies status querying and operational reasoning.

## Business Problem

Expense-level dirty tracking created precision and maintenance issues:

1. Dirty status was distributed across many expense records.
2. Account-level status checks depended on scanning expense predicates.
3. Mutation paths (especially reassignment/delete edge cases) were harder to reason about consistently.
4. Expense-level status fields increased model and workflow complexity without being the true decision boundary for account status.

## Definitions

### Accrual-Impacting

For this PRD, "accrual-impacting" means a change that affects an expense's accrual-state projection inputs.

1. Projection inputs are: `AccountId`, `ExcludeFromCalcs`, `AccrualStart`, `NextDue`, `EndDate`, `AccrualPolicy`, `Frequency`, `FrequencyCount`, and `Amount`.
2. If an expense was excluded before and remains excluded after the change, the change is treated as non-impacting for dirty-state updates.
3. Ended detection for dirty-impact logic is one-time only and uses strict boundary `EndDate < asOfDate`.
4. This rule applies consistently to both update recalc suppression and delete impact checks.

## Questions and Decisions

### Q1: What is the authoritative source of accrual status?

Decision:

1. Use one account accrual row per account as the source of truth.

### Q2: Keep or remove expense-level status columns?

Decision:

1. Remove expense columns `AccruedIsDirty` and `LastAccruedUpdate`.

### Q3: What is the account status rule?

Decision:

An account requires accrual update when:

1. `AccruedIsDirty == true`, or
2. `LastAccruedDate == null`, or
3. `LastAccruedDate < asOfDate`

### Q4: Which mutations must mark status dirty?

Decision:

1. Create expense marks dirty.
2. Accrual-impacting update marks dirty.
3. Reassignment marks both old and new account dirty.
4. Accrual-impacting toggle exclusion marks dirty.
5. Renew marks dirty.
6. Delete marks dirty only when impactful and not last expense.
7. Last-expense delete removes the account accrual row.

### Q5: Which updates are explicitly non-impacting?

Decision:

1. Metadata-only updates do not mark dirty.
2. Current non-impacting fields are `Description` and `Note`.

### Q6: Should account entity include a dirty flag column?

Decision:

1. No. Dirty state remains in dedicated account accrual storage.

### Q7: What implementation pattern is used?

Decision:

1. Service-first orchestration boundary for dirty/clear behavior.
2. Repository-backed account accrual data access.
3. Idempotent dirty transitions.

## Current State Summary

Current production behavior is account-level and implemented through account accrual state.

1. Status source of truth is account accrual state, not expense fields.
2. Expense status columns were removed from the expense model and database.
3. Write paths mark account accrual state dirty when changes are accrual-impacting.
4. Successful accrual clears dirty state and stamps the account accrual date.
5. Last-expense deletion removes the account accrual row for that account.
6. Maintenance import/export metadata is versioned to support the expense schema change.

## Exact Implementation Details

### Data Model

`AccountAccrual` fields:

1. `AccountId` (`int`, unique FK to `Account.Id`)
2. `LastAccruedDate` (`DateOnly?`)
3. `AccruedIsDirty` (`bool`, default true)

Behavioral contract:

1. One account accrual row per account when accrual tracking is active.
2. Row may be removed when the account has no remaining expenses.

### Mutation Semantics Matrix

| Event                                 | Implemented state update                                                                                                                                                                                               |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Expense create                        | Mark account `AccruedIsDirty = true`.                                                                                                                                                                                  |
| Expense update (impacting fields)     | Mark account `AccruedIsDirty = true`, except when both before/after states are one-time and both satisfy `EndDate < asOfDate` (no dirty mark).                                                                         |
| Expense update (account reassignment) | Mark old account and new account `AccruedIsDirty = true`.                                                                                                                                                              |
| Expense update (metadata-only fields) | No change to account accrual state.                                                                                                                                                                                    |
| Toggle exclusion                      | Mark account `AccruedIsDirty = true` only when accrual-impacting.                                                                                                                                                      |
| Renew expense                         | Mark account `AccruedIsDirty = true`.                                                                                                                                                                                  |
| Delete expense                        | If deleted expense is accrual-impacting and other expenses remain, mark dirty. If last expense, remove account accrual row and do not mark dirty. Dirty impact uses one-time strict-ended cutoff `EndDate < asOfDate`. |
| Accrue account (success)              | Set `AccruedIsDirty = false`; set `LastAccruedDate = asOfDate`.                                                                                                                                                        |
| Status check                          | Account requires update when dirty, never accrued, or last accrued before as-of date.                                                                                                                                  |

### Date Boundary Conventions

1. Dirty-impact ended logic in `AccrualDirtyStateManager` is centralized in one private helper accepting `Frequency`, `EndDate`, and `asOfDate`.
2. Ended means one-time and strict boundary `EndDate < asOfDate`.
3. Equality boundary (`EndDate == asOfDate`) is still active (not ended).
4. The same rule is used by both `GetAccountsRequiringRecalc` and `IsExpenseDeletionImpactful`.
5. Renewal eligibility is governed by renewal-specific logic and is intentionally separate from this dirty-impact ended rule.
6. Client ended-status semantics align with strict boundary (`daysDue < 0`), so due today is not ended until the next day.

### Account Status Read Path

1. Status reads use account accrual repository queries.
2. Expense renewals and income renewals are still included in overall status output.
3. Account accrual required flag comes from account-level rule evaluation.

### Import/Export and Metadata Versioning

1. Maintenance metadata current version is v3.
2. v3 reflects removal of expense fields `AccruedIsDirty` and `LastAccruedUpdate`.
3. Expense CSV shape in v3 aligns to the post-removal expense schema.

### Completion Scope

1. Account accrual table/repository support.
2. Backfill and lifecycle handling.
3. Write-path integration across create/update/delete/toggle/renew.
4. Clear/stamp on accrual success.
5. Status read cutover to account accrual source.
6. Removal of expense-level status fields from runtime model and schema.
7. Metadata/import/export version update to v3.

## Open Questions

1. None currently identified for this feature scope.

## Out of Scope

1. Changing accrual math formulas.
2. Client UX redesign beyond status correctness.

## References

- [Source/Server/Pot.Data/Entities/AccountAccrualEntity.cs](../../Source/Server/Pot.Data/Entities/AccountAccrualEntity.cs)
- [Source/Server/Pot.Data/Repositories/AccountAccrual/AccountAccrualRepository.cs](../../Source/Server/Pot.Data/Repositories/AccountAccrual/AccountAccrualRepository.cs)
- [Source/Server/Pot.App/Concerns/Accruals/AccrualDirtyStateManager.cs](../../Source/Server/Pot.App/Concerns/Accruals/AccrualDirtyStateManager.cs)
- [Source/Server/Pot.App/Features/Accruals/Status/AccrualsStatusService.cs](../../Source/Server/Pot.App/Features/Accruals/Status/AccrualsStatusService.cs)
- [Source/Server/Pot.App/Features/Accruals/AccrueExpenses/AccrueExpensesService.cs](../../Source/Server/Pot.App/Features/Accruals/AccrueExpenses/AccrueExpensesService.cs)
- [Source/Server/Pot.App/Features/Expenses/Delete/DeleteExpenseService.cs](../../Source/Server/Pot.App/Features/Expenses/Delete/DeleteExpenseService.cs)
- [Source/Server/Pot.Data/Migrations/20260424150721_RemoveExpenseAccruedTracking.cs](../../Source/Server/Pot.Data/Migrations/20260424150721_RemoveExpenseAccruedTracking.cs)
- [Source/Server/Pot.App/Features/Maintenance/Metadata/Models/MetadataBase.cs](../../Source/Server/Pot.App/Features/Maintenance/Metadata/Models/MetadataBase.cs)
- [Source/Client/pot-react/src/features/dashboard/contexts/AccrualsContext.tsx](../../Source/Client/pot-react/src/features/dashboard/contexts/AccrualsContext.tsx)
- [Source/Client/pot-react/src/api/hooks/useAccrualsStatus.ts](../../Source/Client/pot-react/src/api/hooks/useAccrualsStatus.ts)
- [Source/Server/Pot.App.Tests/Concerns/Accruals/AccrualDirtyStateManagerFixture.cs](../../Source/Server/Pot.App.Tests/Concerns/Accruals/AccrualDirtyStateManagerFixture.cs)
