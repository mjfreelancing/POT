---
id: ADR-0016
title: Validation failures answer 400 with an errors map, never 422
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0016: Validation failures answer 400 with an errors map, never 422

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

## Decision Drivers

- One status per category, so the client has one branch and the contract has one entry.
- A machine-readable field map, so the interface can point at the input.
- Proxies, gateways and tooling handle the conventional status more consistently.

## Considered Options

1. `400` for every validation failure, with an `errors` map.
2. `422` for semantic failures and `400` for structural ones.
3. `400` for structural failures and a bespoke status or body for rules.
4. Accept both, decided per endpoint.

## Decision Outcome

Chosen option: 1. Validation failures answer `400` with a Problem Details body whose `errors` member
maps each field to its messages. Refusals from business rules answer `400` as well, distinguished by
their `type` and error code rather than by status.

The switch away from `422` was made deliberately, and the endpoints that had declared it were updated
with it, so no route answers both.

### Consequences

- Good, because the client has one status to handle and can read `errors` for field placement.
- Good, because the contract declares one failure for "the request was not acceptable", which is
  simpler to keep accurate per endpoint.
- Bad, because `400` now covers two situations — the request was malformed, and the request was
  understood but refused — so the status alone does not tell the client which.
- Bad, because the `type` and error code carry that distinction, so they must stay stable.
- Bad, because the previous `422` declarations had to be found and changed, and a missed one would have
  produced a status no client branch handled.

### Confirmation

- Test: the validation fixtures assert the `400` shape and the `errors` map for a request that fails
  several rules.
- Test: `EndpointResponseMetadataFixture` asserts the declared failure set matches the behaviour, so a
  route cannot reintroduce `422` in its contract.
- Review: `422` in a new endpoint is a defect; the distinction belongs in the `type`.

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
