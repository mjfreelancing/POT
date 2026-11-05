---
id: ADR-0012
title: One chained rate-limit policy partitions by caller and never locks an account
status: accepted
date: 2025-11-08
area: backend
---

# ADR-0012 (backfilled): One chained rate-limit policy partitions by caller and never locks an account

**Summary:** In the context of an internet-facing API whose sign-in route is the obvious target for
credential guessing, facing the need to slow abuse without penalising a legitimate user who mistyped a
password, we decided on a single chained policy that partitions by caller address when anonymous and
by user when signed in, using a ten-second fixed window for anonymous traffic and a thirty-second
sliding window for authenticated traffic, to achieve cheap brute-force resistance, accepting that the
protection is deliberately temporary and never becomes a lockout.

## Context and Problem Statement

Two populations share the pipeline. Anonymous callers include sign-in, sign-up, password reset and
refresh — the routes worth attacking. Signed-in callers are already known and are trusted more.

An account lockout is the traditional answer to guessing, and it is the wrong one here: it converts a
guessing attempt into a denial of service against the account's real owner, and it needs a recovery
path that is itself an attack surface.

## Decision Drivers

- Guessing must be slowed at the caller, not at the account.
- The window must be short enough that a throttled request fails fast rather than making the user wait.
- Limits should be configurable, and the code's defaults should be a sound starting point.

## Considered Options

1. One chained policy: fixed window for anonymous callers, sliding window for authenticated users.
2. Per-endpoint limits, tuned per route.
3. Account lockout after repeated failures.
4. A single global limit regardless of who is calling.

## Decision Outcome

Chosen option: 1. Every endpoint group requires the same named policy, which chooses a partition:
anonymous callers get fifteen permits in a ten-second fixed window per address, and authenticated
callers fifty permits in a thirty-second sliding window per user, split into ten segments, with no
queueing in either. Rejection is `429` with a Problem Details body that names the wait. `Retry-After` is
written where the limiter exposes the retry metadata, which the fixed window does and the sliding window
does not, so an authenticated rejection carries the detail without the header.

There is no lockout of an account. The temporary rejection is the whole of the response to guessing.

### Consequences

- Good, because an attacker's address is throttled while the account owner signing in normally is not.
- Good, because signed-in users get a smoother sliding window, which is fair to normal interactive use.
- Good, because the short anonymous window means a rejected anonymous request returns quickly, and can
  be reported as a cooldown rather than a hang.
- Bad, because the authenticated window is as long as the client's request timeout, so a rejected
  authenticated request can be reported as a timeout rather than as a refusal.
- Bad, because a caller sharing an address — an office, a mobile network — is throttled as one, which
  is a real limit of address-based partitioning.
- Bad, because anonymous partitions depend on the caller address being the real one, so the forwarded
  header trust is part of this control working at all.
- Bad, because nothing prevents repeated guessing spread across many addresses; the control is on rate,
  not on the number of attempts per account.

### Confirmation

- Test: `HeadersAndRateLimitFixture` exercises the chained policy, asserts the anonymous rejection
  carries `Retry-After`, and asserts the authenticated one does not.
- Review: the permit limits and windows are constants, not settings; the configuration section exists so
  a test run can raise them, and must not appear in a deployed environment's settings.
- Review: a new endpoint group must opt into the same policy; a route that omits it is unthrottled, and
  that is a defect rather than a tuning choice.

## Pros and Cons of the Options

### One chained policy (chosen)

- Good: one place to reason about, and it adapts to whether the caller is known.
- Bad: cannot express a route-specific tighter limit without leaving the policy.

### Per-endpoint limits

- Good: the sign-in route could be far stricter than the rest.
- Bad: every route becomes a tuning decision, and the differences are invisible in review.

### Account lockout

- Good: bounds attempts per account regardless of address.
- Bad: hands an attacker a denial-of-service against any known user, and needs a recovery flow that is
  itself abusable.

### One global limit

- Good: trivial to reason about.
- Bad: treats a signed-in user's normal use the same as an anonymous flood.

## More Information

- The client's handling of `429` and `Retry-After` is a separate decision.
- An account lockout policy could be added later without changing this one; it is deferred rather than
  rejected.
