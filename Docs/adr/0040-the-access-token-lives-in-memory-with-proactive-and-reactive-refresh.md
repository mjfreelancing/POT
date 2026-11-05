---
id: ADR-0040
title: The access token lives in memory, with proactive and reactive refresh
status: accepted
date: 2025-09-11
area: frontend
---

# ADR-0040 (backfilled): The access token lives in memory, with proactive and reactive refresh

**Summary:** In the context of a fifteen-minute access token and a refresh token the browser holds in an
HttpOnly cookie, facing the choice between persisting the access token and holding it only in memory, we
decided that the token lives in memory, that it is refreshed shortly before expiry and again on a `401`,
and that every path out of the session goes through one logout, to achieve a token that no script can
read from storage and a session that survives a page reload through the cookie, accepting that a reload
costs a refresh round trip.

## Context and Problem Statement

The token has to be somewhere. In memory it disappears on reload; in storage it survives, and so does a
copy in anything that can read that storage. The refresh cookie already survives a reload, so the memory
copy can be re-obtained rather than persisted.

A token that expires while a request is in flight produces a `401` that is not the user's fault, so it
must be renewed and the request retried rather than surfaced.

## Decision Drivers

- The access token should not be readable from browser storage.
- A reload must not sign the user out.
- An expiry mid-session must be invisible to the user.
- Signing out must end the session everywhere in the client, not wherever the user happened to click.

## Considered Options

1. Token in memory, proactive refresh before expiry, reactive refresh on `401`, one logout path.
2. Token in `localStorage` or `sessionStorage`.
3. Token in memory with only reactive refresh.
4. Token in memory with only proactive refresh.

## Decision Outcome

Chosen option: 1. The token is held in memory only: a small token provider module is the authoritative copy
that the transport reads and the refresh writes, and the authentication context mirrors it for rendering.
Nothing is written to storage. A timer refreshes it before expiry, and a response interceptor refreshes it
on a `401` and replays the request.

Concurrent requests that fail together wait on one refresh rather than each starting their own, and a
single logout path clears the session, the token and the cached server data.

On a reload the token is gone but the cookie is not, so the application refreshes at start-up to resume
the session.

### Consequences

- Good, because no stored artefact contains a usable token, so an injection bug cannot read one from
  storage.
- Good, because a reload resumes the session from the cookie rather than signing the user out.
- Good, because an expiry mid-session is invisible, and a burst of concurrent failures costs one refresh
  rather than one each.
- Bad, because every page load begins with a refresh round trip, which is latency before the interface can
  be sure who the user is.
- Bad, because there are two refresh triggers to keep consistent, and a third — the start-up case — that
  behaves differently from both.
- Bad, because the token now has two in-memory homes — the provider the transport uses and the context the
  interface renders from — and a refresh that writes only the provider leaves the context holding a stale
  value, which the timer then re-arms from.
- Bad, because the start-up refresh has no fixture of its own, so the path a reload takes is verified only
  by using the application.
- Bad, because the wait-and-replay logic is the most intricate part of the client's transport, and a bug
  there shows as duplicated or lost requests rather than as an error.

### Confirmation

- Test: the interceptor fixtures assert a `401` triggers one refresh, that queued requests are replayed
  after it, and that a failed refresh ends the session through the same logout path.
- Review: a token written to storage, or a logout that does not go through the single path, is a defect.

## Pros and Cons of the Options

### Memory with both triggers (chosen)

- Good: no storage exposure, no user-visible expiry, and a reload is recoverable.
- Bad: a refresh on every load, and two trigger paths.

### Browser storage

- Good: no start-up refresh, and the token survives everything.
- Bad: any script can read it, which converts an injection into a stolen session.

### Reactive only

- Good: one trigger, no timer.
- Bad: every expiry produces a `401` and a retry, and a request that raced the expiry is delayed by a
  failure that was predictable.

### Proactive only

- Good: no `401` handling on the happy path.
- Bad: a clock difference, a suspended tab or a revoked session produces a `401` the client cannot
  recover from, so the user is signed out unexpectedly.

## More Information

- The server side of the session and its revocation are separate decisions.
- Cache invalidation on logout belongs to the server-state decision, and the single logout path is what
  triggers it.
