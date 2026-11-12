---
id: ADR-0018
title: API hooks return results and one interceptor normalises every failure
status: accepted
date: 2025-05-03
area: frontend
related-prds: [PRD-0030]
---

# ADR-0018 (backfilled): API hooks return results and one interceptor normalises every failure

**Summary:** In the context of a client where every screen has to distinguish success from failure and
say something useful, facing the choice between letting each caller inspect the HTTP response and
returning a result, we decided that every API hook returns a result and that a single response
interceptor converts any failure into the client's error types, to achieve one error model and messages
that cannot contradict each other, accepting that the error taxonomy is central and must cover anything
a proxy can return.

## Context and Problem Statement

A failure can arrive as a Problem Details body, a proxy's HTML page, an empty body, a timeout or a
dropped connection. A screen that inspects responses itself has to handle all of those, and each screen
that does it slightly differently produces a different message for the same failure.

The server's contract helps but does not remove the problem: a gateway that never reaches the
application answers with no body at all. Transport has other cross-cutting behaviour too — base URL,
credentials, correlation identifier, token refresh — and if every screen configured it, they would
diverge.
How the interceptor reads a body, and which message each status shows, are set by
[PRD-0030](../prd/PRD-0030-problem-details-consolidation.md) (E1–E8). The alternatives below record what
each would have cost; the record does not show whether they were weighed before the choice.

## Decision Drivers

- A caller should be able to branch on a failure kind, not on a status code and a body shape.
- The same failure should read the same way wherever it is shown.
- Nothing should throw into a component's render path.

## Considered Options

1. Hooks return a result; one interceptor maps every failure into typed errors.
2. Hooks return the raw response and each caller handles errors.
3. Errors are thrown and caught by an error boundary per screen.
4. A discriminated union produced per endpoint.

## Decision Outcome

Chosen option: 1. A successful call produces a successful result carrying the response body, cast to the
hook's type but not validated; a failure produces a failing result carrying one of the client's error
types — bad request, authentication, forbidden, not found, method not allowed, conflict, rate limited,
server, unexpected, network. One interceptor reads whatever body arrived, reduces it to members it can
trust, and chooses the error type from the status. A second interceptor in front of it handles
authentication and, when a refresh fails, produces its own authentication failure — the one place where
two interceptors can decide a message.

Every call goes through one HTTP client, the default axios instance configured once at start-up with
these interceptors, so every call inherits the same transport behaviour. A second instance exists only
for the token refresh, which must not run through the authentication interceptor that depends on it; it
shares the correlation and normalising interceptors.

The root error boundary exists for the failures that no interceptor sees — a render error — and is not
the general mechanism.

### Consequences

- Good, because a screen has one thing to check and one set of types to switch on.
- Good, because a body that is missing, an HTML error page and a malformed document all become a typed
  failure rather than an exception in a component.
- Good, because the message for a given failure is decided once.
- Good, because transport behaviour — credentials, correlation identifier, refresh, error normalisation —
  is defined once and inherited by every call.
- Bad, because the error taxonomy is a central assumption: a status it does not classify falls to a
  generic error, and adding a status is a change in one place that every screen inherits.
- Bad, because the interceptor's reduction of a body drops members it does not recognise, so a new
  server field is invisible until the client is told about it.
- Bad, because a failure carries only a type, a code and one description: a `400`'s messages are joined
  into one string and their field names dropped, so a form cannot place a server message beside its
  field, and a `401`, `403`, `404`, `405` or `429` always shows fixed client copy, whatever `detail` the
  server sent.
- Bad, because a result never rejects, so the query library treats every failure as success: `useGet`
  resolves a failure as a `FailResult` value, its retries never run (the `retry: false` on `useMe` has no
  effect), `isError` is never set, and a failed read is held as fresh data for the stale time
  ([ADR-0017](ADR-0017-server-state-lives-in-a-tanstack-query-cache-with-explicit-invalidation.md)), so remounting a
  screen shows the cached failure rather than asking again.
- Bad, because a second axios instance is needed for authentication, which is a wrinkle every reader
  encounters.

### Confirmation

- Test: the interceptor fixtures assert each status maps to its error type, including a response with no
  body and one with a body that is not Problem Details.
- Review: a screen that inspects a status code or a response body directly is a defect; the normalising
  interceptor is the only place that reads them, the authentication interceptor's refresh failure aside.

## Pros and Cons of the Options

### Result plus one interceptor (chosen)

- Good: one model, no exceptions in render, and a single place to extend.
- Bad: a central taxonomy, and reduction that can hide new fields.

### Raw responses per caller

- Good: each screen decides exactly what it needs.
- Bad: every screen re-implements the mapping, and they drift.

### Thrown errors with boundaries

- Good: familiar React idiom.
- Bad: an expected refusal becomes an exception, and a boundary catches a render error and an API failure
  the same way.

### Union per endpoint

- Good: precise per operation.
- Bad: a type per endpoint and a mapping per endpoint, for a taxonomy that is already status-driven.

## More Information

- The server contract these errors are derived from is a separate decision.
- The client is configured in `src/main.tsx` and `src/api/axios.ts`; the refresh instance is
  `authClient`.
- Where a failure is shown — a sheet, a toast, or inline beside a field — is a presentation rule, not part
  of this decision.
