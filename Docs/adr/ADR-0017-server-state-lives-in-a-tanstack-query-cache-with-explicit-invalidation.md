---
id: ADR-0017
title: Server state lives in a TanStack Query cache with explicit invalidation
status: accepted
date: 2025-03-23
area: frontend
---

# ADR-0017 (backfilled): Server state lives in a TanStack Query cache with explicit invalidation

**Summary:** In the context of a client whose screens all read and write the same server resources,
facing the choice between hand-managed fetching and a query cache with explicit invalidation, we decided
that every server read and write goes through TanStack Query hooks, and that a mutation states what it
invalidates, to achieve one place for caching, refetching and invalidation, accepting that invalidation is
a dependency graph the developer must keep accurate.

## Context and Problem Statement

The same account list is shown on more than one screen, and a change on one screen must be visible on the
others. Fetched data also has to survive navigation, refetch when stale, and not refetch on every mount.
The alternatives below record what each would have cost; the record does not show whether they were
weighed before the choice.

## Decision Drivers

- One cache, so two screens showing the same data cannot disagree.
- Data should survive navigation and refetch only when stale, not on every mount.
- A change that alters server data must invalidate what depends on it, explicitly.

## Considered Options

1. TanStack Query, with explicit invalidation.
2. `useEffect` and local state per screen.
3. A Redux store holding server data.
4. Plain HTTP calls without a query layer, refetching on mount.

## Decision Outcome

Chosen option: 1. Server data is read through query hooks and changed through mutation hooks, keyed by
resource. A screen shows what the cache holds and refetches only when the data is stale or a mutation
invalidates it.

Invalidation is explicit: a mutation names the keys it changed, and one dependency table adds the keys
that derive from them, so a dependency between resources is declared once rather than at every mutation.

### Consequences

- Good, because caching, refetching and invalidation are the library's behaviour rather than each screen's
  invention.
- Good, because a mutation's effect on cached reads is stated where the mutation is written, so it can be
  reviewed.
- Bad, because the hooks' result shape defeats the library's failure handling: a failed read is cached as
  a success, so retries never run. See
  [ADR-0018](ADR-0018-api-hooks-return-results-and-one-interceptor-normalises-failures.md).
- Bad, because the invalidation graph is maintained by hand: a mutation that forgets a dependent key leaves
  a stale screen, and nothing detects it.
- Bad, because the keys a query is stored under are untyped `string[]` while invalidation accepts a typed
  union, so a query whose key is outside the union (`settings`, `roles`) can be invalidated only by a direct
  call, and a mismatch between the two is a silent miss rather than a compile error.

### Confirmation

- Test: `tests/concerns/cache/cacheInvalidation.test.ts` asserts the dependency expansion against a mocked
  query client. `tests/features/users/delete/hooks/useDeleteUser.test.tsx`,
  `tests/features/approvals/components/PendingApprovalMobileCard.test.tsx` and
  `tests/features/users/UsersPage.test.tsx` assert the keys a mutation invalidates, with
  `useCacheInvalidation` mocked; the account, expense and income mutation hooks have no such fixture.
- Instruction: `.github/instructions/client.instructions.md` (State and data flow).
- Gap: no test runs a mutation against a real cache and observes a dependent query refetch.
- Gap: a few call sites bypass the dependency table and call the query client directly: the dashboard
  renew actions for projections, and the budget reminders form for settings.
- Review: a new mutation names the keys it invalidates; a screen that fetches with `useEffect` instead of a
  query hook is a defect.

## Pros and Cons of the Options

### Query cache (chosen)

- Good: shared cache, and explicit invalidation.
- Bad: a graph to maintain, and keys as strings.

### Local state per screen

- Good: nothing to learn.
- Bad: two screens showing the same data hold two copies that disagree, and each screen re-implements
  loading, error and retry.

### Redux for server data

- Good: one store, and devtools.
- Bad: caching, staleness and refetching are written by hand, which is most of what the query library does.

### Plain HTTP calls, refetch on mount

- Good: the smallest dependency.
- Bad: every navigation refetches, and no screen can reuse another's data.

## More Information

- The client sets a five-minute stale time and a thirty-minute retention.
- The dependency table is `src/concerns/cache/cacheInvalidation.ts`: expenses and incomes invalidate
  accounts, and accounts invalidate projections.
- The return shape of the API hooks, the one configured HTTP client and the interceptor that normalises
  failures are [ADR-0018](ADR-0018-api-hooks-return-results-and-one-interceptor-normalises-failures.md).
- Browser storage is used for preferences, not for server data, which is a separate decision.
