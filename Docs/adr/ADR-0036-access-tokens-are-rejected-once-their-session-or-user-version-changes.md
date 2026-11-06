---
id: ADR-0036
title: Access tokens are rejected once their session or user version changes
status: accepted
date: 2025-11-22
area: backend
related-prds: [PRD-0002]
---

# ADR-0036 (backfilled): Access tokens are rejected once their session or user version changes

**Summary:** In the context of self-contained access tokens that stay valid until they expire, facing
the need to end one device's access without waiting, or to invalidate every token after a credential
change, we decided to check the token's session and user version against the database on every
authenticated request, to achieve immediate revocation while keeping the token self-contained,
accepting an indexed database read in the authentication path.

## Context and Problem Statement

A JWT is valid because it is signed, not because the server still agrees with it. Revoking a session
row therefore does nothing on its own to a token already issued. Two revocations are wanted:

- **One device.** Logging out ends that device's session and nothing else, because a user may be signed
  in on several.
- **Everything.** A password change must end every existing session at once, because a credential the
  account no longer uses should not keep working.

Neither can be expressed inside the token, because a token cannot be edited after it is issued. They
can only be detected by comparing the token against current state.
Global revocation by a per-user token version came first, with the password-change goal recorded in
[PRD-0002](../Future/PRD-0002-server-login-logout-session-architecture.md). Per-session rejection was added
on 2026-10-17, when each access token began to carry its session's identifier; before that, logging out
stopped the session's refresh but left its access token usable until it expired.

## Decision Drivers

- Revocation must take effect on the next request, not at the next token expiry.
- Ending one device must not end the others.
- The check must be cheap enough to run on every authenticated request.

## Considered Options

1. Validate the token's session and user version against the database on every request.
2. Rely on token expiry alone, with no revocation.
3. Keep a server-side denylist of revoked tokens until they expire.
4. Cache the validation result per user for a short window.
5. Validate the user's token version only, so logging out ends a session's refresh but not its access
   token.

## Decision Outcome

Chosen option: 1. On token validation the request reads the user's current security state for the
token's session: the stored token version must equal the token's, the session must exist, be
unrevoked and be unexpired, and the account must still be enabled. Any mismatch fails authentication,
so a protected route answers `401`; an anonymous route carries on with no signed-in caller.

A password change increments the user's version and revokes every unrevoked session row, so the tokens
issued before it fail on both counts and no refresh token can mint a replacement. Logging out revokes
one session row, which invalidates the tokens that name it. An account being disabled does
not bump the version: it is caught by the account-status part of the same check, so its tokens stop being
accepted for as long as the account is disabled and work again if it is re-enabled.

### Consequences

- Good, because revocation is immediate and per device, which is what the product needs and what a
  self-contained token cannot give on its own.
- Good, because the same check also enforces account state, so a disabled account cannot keep using a
  token it already holds.
- Good, because there is no denylist to grow, expire or lose on restart.
- Bad, because every authenticated request pays an indexed database read before any handler runs, so
  availability of the database is part of authentication rather than only of the work.
- Bad, because a token is only as revocable as that read is reliable; a stale cache or a replicated
  read in the future would reintroduce the window this design removes.
- Bad, because the failure is a plain `401` with no distinction between "never valid" and "revoked".
- Bad, because only the signed-in password change revokes anything: completing a password reset
  replaces the password hash and touches neither the version nor the sessions, so sessions opened with
  the old password keep working after a reset.
- Bad, because the version is incremented only on the path that also revokes every session, so today it
  never rejects a token the session check would accept; it is held for a future sign-out-everywhere.

### Confirmation

- Test: `SessionBoundAccessTokenFixture` expires a session and asserts the token it issued is no longer
  accepted.
- Test: `RefreshFixture` asserts a refresh rotates the stored token, so the superseded one cannot be
  replayed.
- Test: `LogoutFixture` asserts that a signed-out device's access token is refused while another
  device's still works, that a password change ends every session's refresh, and that a password change
  increments the token version while logging out does not.
- Gap: no fixture presents a token whose version is stale and expects it to be refused; because a
  password change also revokes every session, removing the version comparison would leave the suite green.
- Gap: no fixture asserts that a disabled account's access token is rejected; that half of the check is
  unverified by the suite.
- Review: no caching may be added to this path without recording the revocation delay it introduces.

## Pros and Cons of the Options

### Database check per request (chosen)

- Good: immediate, per-session, and it also covers account state.
- Bad: a read on every authenticated request.

### Expiry only

- Good: no cost at all.
- Bad: signing out or changing a password does not stop anything already issued, which makes "sign out
  everywhere" a promise the system cannot keep.

### Token denylist

- Good: no change to the token contents.
- Bad: needs storage that must survive restarts and be purged as tokens expire, and it is a second
  source of truth about who may act.

### Cached validation

- Good: removes most of the cost.
- Bad: reintroduces exactly the delay the design exists to remove, for a read that is small at this
  scale.

### Token version only

- Good: the token names no session, and the check reads the user row alone.
- Bad: a signed-out device's access token keeps working until it expires — the gap closed on 2026-10-17.

## More Information

- The session and token model this extends is [ADR-0022](ADR-0022-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md).
- Which sessions exist, and which one a request is acting in, is separate from the decision that a
  session can be ended.
