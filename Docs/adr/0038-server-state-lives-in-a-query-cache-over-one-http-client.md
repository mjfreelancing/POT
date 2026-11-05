---
id: ADR-0038
title: Server state lives in TanStack Query over one configured HTTP client
status: accepted
date: 2025-03-23
area: frontend
---

# ADR-0038 (backfilled): Server state lives in TanStack Query over one configured HTTP client

**Summary:** In the context of a client whose screens all read and write the same server resources,
facing the choice between hand-managed fetching and a query cache with explicit invalidation, we decided
that every server read and write goes through TanStack Query hooks over a single axios instance, to
achieve one place for caching, refetching and invalidation and one place for transport behaviour,
accepting that invalidation is a dependency graph the developer must keep accurate.

## Context and Problem Statement

The same account list is shown on more than one screen, and a change on one screen must be visible on the
others. Fetched data also has to survive navigation, refetch when stale, and not refetch on every mount.

Transport has its own cross-cutting behaviour: base URL, credentials, correlation identifier, token
refresh, and turning a failure into the shape the interface uses. If every screen configured that itself,
they would diverge.

## Decision Drivers

- One cache, so two screens showing the same data cannot disagree.
- One transport configuration, so a change to headers or retry behaviour happens once.
- A change that alters server data must invalidate what depends on it, explicitly.

## Considered Options

1. TanStack Query over one axios instance, with explicit invalidation.
2. `useEffect` and local state per screen.
3. A Redux store holding server data.
4. One axios instance without a query layer, refetching on mount.

## Decision Outcome

Chosen option: 1. Server data is read through query hooks and changed through mutation hooks, keyed by
resource. The client sets a five-minute stale time and a thirty-minute retention, so a screen shows what it
has and refetches only when the data is older than that, or when a mutation invalidates it. Transport is one
axios instance with its interceptors; a second instance exists only for the authentication calls that must
not run through the interceptor that depends on them.

Invalidation is explicit: an operation declares which query keys it affects, so a change to the accounts
list refreshes what derives from accounts and nothing else.

### Consequences

- Good, because caching, refetching and invalidation are the library's behaviour rather than each screen's
  invention.
- Good, because transport behaviour — credentials, correlation identifier, refresh, error normalisation —
  is defined once and inherited by every call.
- Good, because a mutation's effect on cached reads is stated where the mutation is written, so it can be
  reviewed.
- Bad, because retries are the library's default rather than a decision: a failing query is retried, and
  only one hook opts out, so a throttled or forbidden read is repeated before the user sees it.
- Bad, because the invalidation graph is maintained by hand: a mutation that forgets a dependent key leaves
  a stale screen, and nothing detects it.
- Bad, because query keys are strings with structure, so a typo is a silent miss rather than a compile
  error.
- Bad, because a second axios instance is needed for authentication, which is a wrinkle every reader
  encounters.

### Confirmation

- Test: the invalidation helper and the mutation hooks are tested with the query client substituted, so the
  assertions are about which keys a mutation invalidates rather than about refetching from a real cache.
- Gap: no test runs a mutation against a real cache and observes a dependent query refetch.
- Review: a new mutation names the keys it invalidates; a screen that fetches with `useEffect` instead of a
  query hook is a defect.

## Pros and Cons of the Options

### Query cache over one client (chosen)

- Good: shared cache, shared transport, explicit invalidation.
- Bad: a graph to maintain, and keys as strings.

### Local state per screen

- Good: nothing to learn.
- Bad: two screens showing the same data hold two copies that disagree, and each screen re-implements
  loading, error and retry.

### Redux for server data

- Good: one store, and devtools.
- Bad: caching, staleness and refetching are written by hand, which is most of what the query library does.

### Axios alone, refetch on mount

- Good: the smallest dependency.
- Bad: every navigation refetches, and no screen can reuse another's data.

## More Information

- The return shape of the API hooks, and the one interceptor that normalises failures, are separate
  decisions.
- Browser storage is used for preferences, not for server data, which is a separate decision.
