---
id: ADR-0060
title: The client and server deploy separately, and API version skew is accepted for minutes
status: accepted
date: 2026-09-24
area: cross-cutting
---

# ADR-0060: The client and server deploy separately, and API version skew is accepted for minutes

**Summary:** In the context of a client deployed by its own manual task and a PWA that can keep an old
shell alive, facing the question of whether a breaking API change needs a compatibility window, we decided
to accept the skew and let the enforced client update end it, to achieve independent deploys with no
version machinery, accepting that a stale client can fail for a few minutes after a breaking deploy.

## Context and Problem Statement

The PRDs that built the current API took the same position: no compatibility period is needed, because the
client and server are released together. Stated first in the PRD that removed the account BSB and number
fields, and repeated through later ones, that is the policy on record.

It is not literally true, and the repository contains the counter-evidence. The server and the client are
built and pushed by two separate manual tasks, so between them the new server serves the old client. The
client is also a PWA: an installed or cached shell keeps calling the API until it updates.

That window is bounded, by the client rather than the server. An open client compares its build with the
deployed build at startup, on focus and every 5 minutes while visible; the service-worker check runs every
30 minutes; a detected update is applied after 45 seconds of quiet, and never deferred more than 5 minutes
(`src/concerns/pwa/pwaRuntime.ts`). Nothing sends a client build id to the server: there is no version
header in `src/api/axios.ts` and nothing in `Pot.AspNetCore` reads one.

What the exposure looks like: for around ten minutes after a deploy, plus the gap between the two tasks, a
stale client can call a changed endpoint. A write is refused with a 400 when a field was renamed; a read
shows a blank where a field was removed. It is not silent damage, because the server validates every input.

The client guide had claimed the opposite — that a breaking change "relies on the backward-compatibility
policy" — which never existed. It now points here instead.

## Decision Drivers

- One client and one operator: there is nobody to coordinate a compatibility window with.
- The enforced update ends the skew without any server-side change.
- The server validates input, so a refusal is the worst case rather than corruption.

## Considered Options

1. Accept the skew with no compatibility window, and rely on the enforced client update (chosen).
2. Expand, then contract: the server accepts both shapes for one deploy and drops the old one later.
3. A version handshake: the client sends its build id, the server refuses one it is too old for.

## Decision Outcome

Chosen option: 1, because the exposure is already bounded by machinery that exists, and options 2 and 3
buy protection against failures that have not happened.

The rules it imposes:

- A breaking API change ships with no compatibility window. The skew is accepted and lasts minutes.
- The window is bounded by the enforced client update. If that mechanism is weakened or disabled, this
  decision no longer holds and has to be revisited before the next breaking change.
- The server and client remain two independent deploys. Nothing may describe the release as atomic.
- The server keeps validating every input, so a stale client's failure stays a rejection rather than a
  corrupt write.

### Consequences

- Good, because there is no version header, no second response shape and no per-change process cost.
- Good, because the skew ends without an operator noticing: the open client updates itself.
- Good, because the failure mode is loud — a 400 or a missing value — rather than silent.
- Bad, because for several minutes after a breaking deploy a user can hit an error or a blank field.
- Bad, because a backgrounded tab checks only when it regains focus, so a user can meet the stale client
  on their first action after returning.
- Bad, because the bound depends on the update path continuing to work, which nothing in this repository
  can enforce.

### Confirmation

- Test: `e2e/tests/pwa/pwaContract.test.ts` asserts the PWA contract in the dev and prodlike profiles,
  which is the mechanism the bound relies on.
- Guide: `Source/Client/pot-react/DEVELOPER.md` (deployed-build detection) states the accepted skew and
  points here.
- Review: a breaking change that introduces a compatibility window, or a change to the update cadence in
  `pwaRuntime.ts`, is a change to this decision rather than a local tweak.

## More Information

- The enforced update itself is ADR-0046; the shell precache it applies to is ADR-0038.
- Option 3 is the revisit path: a new request header, a dedicated Problem Details type for an
  out-of-date client, and a client that forces the update on seeing it.
- Option 2 spreads the cost across every breaking change instead of absorbing it once.
