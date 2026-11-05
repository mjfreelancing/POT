---
id: ADR-0044
title: Validation failures answer 400 with an errors map, never 422
status: accepted
date: 2026-10-16
area: backend
related-prds: [PRD-0030]
---

# ADR-0044 (backfilled): Validation failures answer 400 with an errors map, never 422

**Summary:** In the context of a client that must show a message beside the field that caused it,
facing the choice between `400 Bad Request` and `422 Unprocessable Entity` for a request whose shape is
wrong, we decided that every validation failure answers `400` carrying an `errors` map keyed by field,
to achieve one status the client can rely on for "fix what you sent", accepting that the distinction
`422` would draw between malformed and semantically wrong is not available on the wire.

## Context and Problem Statement

A request can fail before any business rule runs: a missing field, a value out of range, a malformed
identifier. The client needs the failure and, ideally, which field caused it, so it can put a message
next to the input rather than at the top of the form.

Two statuses are plausible. `400` is the long-standing answer for a request the server will not accept.
`422` was introduced for a request that is syntactically valid but semantically wrong — which describes
most validation rules here. Either could be defended on its own.
Until commit `bec69a48`, request validation and the constraint and unprocessable-entity refusals
answered `422` — PRD-0026 records the earlier rule for refusals — and the client had no `400` branch; that
commit moved them all to `400`. Its message gives no reason, so the drivers below are read from the code
and PRD-0030 rather than from a recorded rationale.

## Decision Drivers

- One status per category, so the client has one branch and the contract has one entry.
- A machine-readable field map, so the interface can point at the input.

## Considered Options

1. `400` for every validation failure, with an `errors` map.
2. `422` for semantic failures and `400` for structural ones.
3. `400` for structural failures and a bespoke status or body for rules.
4. Accept both, decided per endpoint.

## Decision Outcome

Chosen option: 1. Validation failures answer `400` with a Problem Details body carrying the `errors` member,
which maps each field to its messages, and no `detail`. Refusals from business rules that map to `400`
(`BadRequest` and `Constraint`) are written into the same shape: the message sits in `errors` under the
field it names, or under the empty key when it belongs to the whole request. No `type`, `detail` or
error-code member distinguishes a refusal from a validation failure; PRD-0030 later removed the error code
from the wire (B9).

The switch away from `422` was made deliberately, in a single change that updated the route declarations,
the client's error mapping and the metadata fixture together, so no route answers both.

### Consequences

- Good, because the client has one status to handle and can read `errors` for field placement.
- Good, because validation errors are assembled by the application from its own validators into the
  framework's validation shape, so the client sees one document whichever layer refused.
- Good, because the contract declares one failure for "the request was not acceptable", which is
  simpler to keep accurate per endpoint.
- Bad, because `400` now covers two situations — the request was malformed, and the request was
  understood but refused — so nothing in the body tells the client which; the messages in `errors` are
  prose rather than something to branch on.
- Bad, because the previous `422` declarations had to be found and changed, and a missed one would have
  produced a status no client branch handled.

### Confirmation

- Test: `ShouldBeValidationProblemAsync`
  (`Pot.AspNetCore.Integration.Tests/Host/Extensions/HttpResponseMessageExtensions.cs`), used by the
  expense, income, user and pipeline fixtures, asserts the `400` status, the validation title, the
  expected field keys in `errors`, and no `detail`.
- Test: `EnrichedErrorExtensionsFixture` asserts `BadRequest` and `Constraint` map to `400`, a refusal is
  written as a validation problem, and a whole-request refusal is filed under the general key.
- Test: `EndpointResponseMetadataFixture` asserts the endpoints' metadata — one `400` in the validation
  shape and no `422` declared — while the bodies themselves are asserted by the per-feature fixtures.
- Review: `422` in a new endpoint is a defect; a refusal that needs its own meaning carries it in its
  message in `errors`, not in a new type or code.
- Evidence of the change: commit `bec69a48` removed the `422` route declarations and updated the client's
  error mapping in the same change.

## Pros and Cons of the Options

### `400` for everything unacceptable (chosen)

- Good: one branch in the client, one declared failure per endpoint.
- Bad: less expressive on the wire than two statuses.

### `400` and `422` split

- Good: the status would carry the structural-versus-semantic distinction.
- Bad: every client branch and every endpoint declaration becomes two, for a distinction the field map
  usually makes obvious anyway.

### `400` plus a bespoke body for rules

- Good: keeps the status meanings clean.
- Bad: a second error shape, which is exactly what one contract avoids.

### Per-endpoint choice

- Good: locally convenient.
- Bad: the client cannot write one rule for "invalid request", and the difference is invisible until it
  bites.

## More Information

- The document that carries the failure is a separate decision.
- The client's normalising interceptor is recorded for the client separately.
