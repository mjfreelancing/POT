---
id: ADR-0027
title: Email is queued in process
status: accepted
date: 2025-10-18
area: backend
---

# ADR-0027 (backfilled): Email is queued in process

**Summary:** In the context of transactional email — verification codes, invitations, password resets,
reminders — facing the choice between a durable delivery mechanism and a channel inside the process, we
decided to queue messages in process and send them from a hosted worker, to achieve working email with no
new infrastructure, accepting that a message is lost if the process stops before it is sent.

## Context and Problem Statement

Sending mail synchronously in a request would make a sign-up wait on somebody else's SMTP server and fail
with it. So the send is moved off the request, which leaves the question of how durable the handover is.

The in-process queue is a deliberately simple first implementation that gets email working, and it is
expected to become more durable under PRD-0019.

## Decision Drivers

- A request must not wait for, or fail with, the mail server.
- No new infrastructure for a volume measured in messages per day.

## Considered Options

1. An in-process channel drained by a hosted worker.
2. Send synchronously inside the request.
3. A database outbox drained by a worker, with retry and backoff.
4. An external transactional email service called directly from the request.

## Decision Outcome

Chosen option: 1. A queued message carries the email type and the message's values; nothing is rendered
until the worker dequeues it. The channel is unbounded and lives as long as the process.

### Consequences

- Good, because the request completes without waiting for the mail server, and a mail failure cannot fail
  the request that caused it.
- Good, because no additional component exists to deploy, monitor or pay for.
- Bad, because a message queued and not yet sent is lost if the process stops, with no record that it
  existed and nothing to retry.
- Bad, because there is no backoff: a transient mail failure loses that message rather than delaying it.
- Bad, because the channel is unbounded, so a burst is absorbed by memory rather than by backpressure, and
  it declares single-writer semantics while request threads write to it concurrently.
- Bad, because delivery is not observable after the fact, so "did the invitation go out" is answered from
  logs rather than from state.

### Confirmation

- Test: the application fixtures for services that send mail (for example
  `RequestPasswordResetServiceFixture` and `InviteUserServiceFixture`) assert the message is submitted to
  the channel writer; the hosted suite replaces that writer with `CapturingEmailWriter`, proved by
  `MessageCaptureFixture`, so no request path reaches a mail server in tests.
- Gap: `SendEmailChannel`, `EmailSender` and `SendEmailWorker` have no fixtures, so dequeueing, the
  swallowed send failure and messages still queued when the host stops are not asserted anywhere.
- Review: a new message is queued rather than sent inline; a request must never call the mail sender
  directly.

## Pros and Cons of the Options

### In-process queue (chosen)

- Good: no infrastructure.
- Bad: at-most-once delivery, with no record of what was lost.

### Send inside the request

- Good: nothing to queue.
- Bad: the user's request now depends on the mail server's availability and latency.

### Database outbox

- Good: durable, retryable and observable, and it survives a restart.
- Bad: a table, a drainer with claim semantics, retry and backoff policies, and a decision about ordering —
  real work for a small volume.

### External email service from the request

- Good: delivery is somebody else's problem.
- Bad: the request still waits on it, and the credential and failure handling move into the request path.

## More Information

- This record is expected to be superseded by a durable outbox. The intended direction — a database
  outbox drained with claim and lease, with retry and same-day catch-up — is written up in
  `Docs/prd/PRD-0019-email-outbox-durable-dispatch.md` (Proposed); adopting it would supersede this
  record rather than edit it. PRD-0019 leaves open whether transactional messages move to the outbox or
  stay on the in-process channel, so the supersession may be partial.
- How a message's subject and bodies are produced is
  [ADR-0028](ADR-0028-email-bodies-are-rendered-from-razor-components.md), which a change of delivery
  mechanism leaves standing.
- Running the worker inside the API process is
  [ADR-0026](ADR-0026-background-work-runs-inside-the-api-process.md), with its own intended supersession.
