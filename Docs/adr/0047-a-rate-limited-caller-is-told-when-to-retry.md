---
id: ADR-0047
title: A rate-limited caller is told when to retry, and nothing retries automatically
status: proposed
date: 2025-11-08
area: frontend
supersedes: [ADR-0002]
---

# ADR-0047 (backfilled): A rate-limited caller is told when to retry, and nothing retries automatically

**Summary:** In the context of a server that answers a throttled request with `429` and a `Retry-After`
value, facing the choice between retrying automatically and telling the user when they may try again, we
decided that the client surfaces the server's guidance as a countdown and never retries a `429` by itself,
to achieve behaviour that honours the server's authority without hiding it, accepting that the user has to
wait and act.

## Context and Problem Statement

A `429` is not a transient fault: the server has decided that this caller may not proceed yet, and it says
for how long. Retrying on a timer contradicts that decision and, if several clients did it, would replace a
rate limit with a synchronised retry storm.

The client cannot compute the wait itself either. The server owns the limit, the window and the partition,
so only the server knows when the caller may try again.

## Decision Drivers

- The server is the only authority on when the caller may proceed.
- A user who is told "too many requests" needs to know how long, or they will retry immediately.
- An automatic retry would defeat the control that produced the response.

## Considered Options

1. Read `Retry-After`, show a countdown, and never retry a `429` automatically.
2. Retry automatically after the indicated delay.
3. Retry automatically with exponential backoff, ignoring the header.
4. Show a generic "try again later" message with no timing.

## Decision Outcome

Chosen option: 1. A `429` becomes a rate-limited failure, and the interface shows how long remains and
holds the action until it elapses; nothing in the transport retries the request.

The same rule applies to the authentication flows that carry their own guidance, such as a one-time code
policy that reports a wait in minutes.

This record states the intended behaviour. The transport does not yet read the server's guidance: a `429`
is mapped to a rate-limited failure with fixed copy, and no countdown exists, so the first two
implementations below are what would make the decision true.

### Consequences

- Good, because the client never retries a `429`, so the control keeps working and a burst cannot amplify
  itself.
- Bad, because until the guidance is read, a throttled user is told to wait without being told how long,
  which is the behaviour that invites immediate retries.
- Bad, because the number is available in two places — the exposed header and the problem detail — so a
  future implementation has to choose one rather than assume it is only in the header.
- Bad, because the user must act again after the wait; there is no silent recovery.
- Bad, because the displayed countdown is client-side arithmetic on a server value, so a long wait crossing
  a clock change can be shown inaccurately.
- Bad, because the header must be readable by script, which is why it is in the exposed set, and an omitted
  exposure would turn every `429` into an untimed message.

### Confirmation

- Test: the interceptor fixtures assert a `429` produces the rate-limited failure and that no retry is
  attempted. Nothing yet asserts a wait, because none is read.
- Review: a retry of a `429` anywhere in the client is a defect; when the countdown is implemented, a `429`
  must show it rather than a generic message.

## Pros and Cons of the Options

### Show the wait, never retry (chosen)

- Good: honours the server and informs the user.
- Bad: recovery needs a second action.

### Retry after the indicated delay

- Good: transparent when the wait is short.
- Bad: a user watching the screen sees nothing happen, and the retry is indistinguishable from the failure
  being ignored.

### Backoff ignoring the header

- Good: no parsing.
- Bad: contradicts the server's timing, and multiplies load exactly when the server is protecting itself.

### Generic message

- Good: nothing to implement.
- Bad: invites immediate retries, which is the behaviour the limit exists to stop.

## More Information

- The server's chained policy that produces `429` is a separate decision.
- Retry-After being readable by script is a consequence of the cross-origin policy, which is separate.
