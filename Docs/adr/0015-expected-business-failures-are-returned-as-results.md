---
id: ADR-0015
title: Expected business failures are returned as results, not thrown as exceptions
status: accepted
date: 2025-01-24
area: backend
---

# ADR-0015 (backfilled): Expected business failures are returned as results, not thrown as exceptions

**Summary:** In the context of application services whose failures are ordinary outcomes — a missing
record, a rule refusing the change, a stale tag — facing the choice between signalling them by throwing
and returning them, we decided that a service returns an enriched result that is either a value or a
typed error, to achieve failure paths that read as flow and map to status codes in one place, accepting
that a caller must handle the result rather than let an exception propagate.

## Context and Problem Statement

Most failures in this application are not exceptional. A user deleting a record another user already
deleted, an update whose tag is stale, a name already taken in the same site: these happen in normal
use and are part of the feature's behaviour, not a fault.

The handler still has to turn each into an HTTP response, and the client still has to tell them apart.
If they arrive as exceptions, the only way to know which is which is to catch and inspect, either in
every handler or in one global handler that must recognise each type — and control flow that is not
visible in the method's signature.

## Decision Drivers

- A failure a feature defines should be visible where the feature is written.
- The mapping from failure to status and error code should exist once, not per handler.
- Faults — a bug, an unavailable dependency — should stay distinguishable from refusals.

## Considered Options

1. Return an enriched result carrying either a value or a typed error.
2. Throw a domain exception per failure and convert centrally.
3. Return `null` or `bool` and let the handler decide what went wrong.
4. Return a tuple of value and error message.

## Decision Outcome

Chosen option: 1. Service methods return `EnrichedResult<Output>`: a success carries the value, a
failure carries an `ApiErrorBase` — an `ApiBasicError`, an `ApiDetailError` that also names the field, or
an `ApiDetailErrorCollection` — whose `ErrorType` names the kind, with an error code and a user-safe
message. The error does not carry a status. Handlers check the result and convert it with
`ToProblemDetails()`; the conversion from error kind to status code exists once, in
`ProblemTypes.ToStatusCode`.

Faults remain exceptions and reach the exception handler, so "the caller may not do this" and "the
server broke" never look the same. Two exception families are refusals rather than faults and are
converted centrally: a unique, reference or concurrency failure from the database becomes a `409`
(`DatabaseExceptionHandler`), and an unreadable request a `400` (`BadRequestExceptionHandler`).

### Consequences

- Good, because every failure a feature can produce is listed in the code that produces it, rather than
  discovered by reading catch blocks.
- Good, because status codes and error codes come from one mapping, so a new feature inherits them
  rather than inventing them.
- Good, because for the refusals a feature defines the distinction from a fault is structural: one is a
  value, the other is an exception. A refusal the database discovers — a race past an "already exists"
  check — still arrives as an exception and is converted by the database exception handler.
- Bad, because every call site must handle the result; an ignored result is a silent success path that
  does nothing.
- Bad, because a failure is slightly more work to construct than a `throw`.
- Bad, because a nested service call must be unwrapped and re-wrapped as it passes a failure upwards,
  which is visible ceremony in the code.

### Confirmation

- Test: the application-layer service fixtures assert the failing result for each refusal and the
  successful value otherwise.
- Test: `ApiDetailErrorFactoryFixture` pins the error type, code, property name and message each failure
  kind produces, and `EnrichedErrorExtensionsFixture` pins the status each kind maps to.
- Review: a `throw` inside a service for a condition the feature defines is a defect; a fault is not.

## Pros and Cons of the Options

### Result values (chosen)

- Good: failure is part of the signature, so a caller can see it has a result to handle, though nothing
  forces it to.
- Bad: propagation is explicit at each layer.

### Domain exceptions

- Good: call sites stay simple, and a `throw` reads naturally.
- Bad: the set of failures is invisible from the signature, and a catch block that misses one turns a
  refusal into a `500`.

### `null` or `bool`

- Good: the least ceremony.
- Bad: loses the reason, so the handler has to reconstruct it from state it no longer has.

### Value and message tuple

- Good: no new type.
- Bad: a message is not a code: it cannot be branched on reliably, translated, or tested without
  matching prose.

## More Information

- How these failures and faults are written on the wire is
  [ADR-0014](0014-every-failure-is-written-as-a-problem-details-document.md).
- The cost of raising and catching exceptions was a further reason to keep them for faults, and the
  server guide lists performance among its reasons; no measurement of that cost is recorded.
