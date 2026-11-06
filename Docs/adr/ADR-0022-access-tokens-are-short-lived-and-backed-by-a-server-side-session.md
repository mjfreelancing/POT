---
id: ADR-0022
title: Access tokens are short-lived and backed by a server-side session
status: accepted
date: 2025-09-12
area: backend
related-prds: [PRD-0001, PRD-0002]
---

# ADR-0022 (backfilled): Access tokens are short-lived and backed by a server-side session

**Summary:** In the context of a browser client that must stay signed in across refreshes and devices
while a stolen credential can be revoked, facing the choice between a self-contained token that lives
until it expires and a session the server can end, we decided to issue a fifteen-minute access token
that names its session and a rotating refresh token held in an HttpOnly cookie, to achieve exposure
limited to minutes with revocation available at any time, accepting a session row, a database read per
request and a refresh round trip.

## Context and Problem Statement

Signing in mints two credentials. The access token is a JWT signed with HMAC-SHA512, expiring fifteen
minutes after issue — and accepted for up to five minutes more, because bearer validation keeps the
framework's default clock skew. It carries the user identifier (`sub`), a token identifier (`jti`), the
user's token version and the identifier of the session it was issued for; beyond the issuer, audience
and validity times, nothing else.

The refresh token is an opaque value that is never stored in clear: the session row keeps a unique
hash and an expiry, and the browser keeps the value in a cookie that is `HttpOnly`, `SameSite=Lax`,
`Secure` in production and, by default, valid for thirty days. The cookie's domain can be widened for
subdomains and is cleared on logout on the same domain it was set on.

Session state is readable on every request, which is what makes revocation possible at all: a
self-contained token cannot be taken back once issued.
Until [PRD-0001](../Future/PRD-0001-per-session-login-support.md) and
[PRD-0002](../Future/PRD-0002-server-login-logout-session-architecture.md), delivered together, the user
row held a single refresh token, so a sign-in or refresh on one device replaced the token another device
held. A session row per sign-in replaced it.

## Decision Drivers

- A stolen access token should be useful only for minutes, not for days.
- Signing out, or being signed out, must take effect immediately for that device.
- Refresh material must not be reachable from script, because script is what an injection attack
  already controls.
- Multiple devices must be able to hold independent sessions.

## Considered Options

1. Short-lived access token plus a server-side session and a rotating refresh token in an HttpOnly
   cookie.
2. A long-lived access token only, with no session.
3. A server-side session cookie as the only credential, with no bearer token.
4. A short access token plus a refresh token kept in browser storage.
5. A short access token plus one refresh token stored on the user row, so one session per user.

## Decision Outcome

Chosen option: 1. The access token stays self-contained for authorization but is deliberately short,
and the refresh token is the long-lived part and never leaves the HttpOnly cookie.

The alternative — one long-lived token — has no revocation path at all: it is valid until expiry
whatever happens to the user. Keeping the refresh token in script-readable storage gives the same
shape but hands it to any script running on the page.

### Consequences

- Good, because a leaked access token is useless after fifteen minutes, and a session revocation or a
  global token-version change ends it sooner.
- Good, because the browser never exposes the refresh token to script, so an injection bug cannot read
  it — though script running on the page can still make the browser send it to the refresh endpoint and
  receive a fresh access token for as long as the page is open.
- Good, because each device holds its own session, so one device signing out leaves the others signed
  in; a second session survives another session's refresh.
- Bad, because every request carries a session identifier the server must be able to interpret, which
  is the mechanism the revocation rules depend on.
- Bad, because a refresh is an extra round trip the client has to interleave with the requests that
  were waiting on it.
- Bad, because the refresh token is renewed on use, so a lost response from a refresh can leave a
  client holding a token the server has already replaced.
- Bad, because the session's expiry slides: every refresh moves it thirty days on, there is no absolute
  lifetime, and nothing purges revoked or expired session rows
  ([PRD-0010](../Future/PRD-0010-auth-session-retention-and-cleanup-policy.md), a draft, proposes a policy).
- Bad, because the raw refresh token is written to the log at `Information` when a refresh is handled, so
  "never stored in clear" holds for the database only.
- Bad, because logout revokes the session only when the request also carries a valid access token: the
  route is anonymous and identifies the caller from the bearer token, so a sign-out sent after the access
  token has expired clears the cookie but leaves the session row active until it expires.

### Confirmation

- Test: `SessionBoundAccessTokenFixture` issues a token for a session and shows it stops being accepted
  once the session is expired, which is the revocation mechanism itself.
- Test: `RefreshFixture` asserts the stored hash changes on refresh and that the previous token no
  longer works.
- Test: `RefreshTokenHelperFixture` asserts the cookie is `HttpOnly`, `SameSite=Lax` and path-scoped on
  set, and cleared as an expired cookie on the same domain.
- Test: `LoginFixture` asserts the stored value is not the refresh token itself, which is what "never
  stored in clear" means in practice.
- Gap: the `Secure` attribute and the thirty-day maximum age are configuration defaults and no test
  asserts them on a set cookie, because the fixture runs with `SecureOnly` disabled. Nothing asserts the
  fifteen-minute access-token lifetime or the claim set either.
- Review: the access-token lifetime stays short and the refresh token never leaves an HttpOnly
  cookie; a change to either belongs in a new record, not in a tweak.

## Pros and Cons of the Options

### Short token with a server session (chosen)

- Good: minutes of exposure, immediate revocation, refresh material out of script.
- Bad: session rows to keep, and a refresh path in the client to get right.

### Long-lived token, no session

- Good: nothing to store and nothing to revoke.
- Bad: a stolen token is valid for its whole lifetime, and there is no way to end it early.

### Session cookie only

- Good: one credential and immediate revocation everywhere.
- Bad: state-changing requests then rely on cookie-based CSRF defences alone, and every API call
  becomes an implicit authenticated request rather than an explicit bearer one.

### Refresh token in browser storage

- Good: no cookie and no cross-site complications.
- Bad: any script on the page can read and exfiltrate it, which is the attack this design avoids.

### One refresh token per user

- Good: no session table to keep.
- Bad: a sign-in or refresh on one device replaces the token another device holds, so that device is
  signed out at its next refresh — the symptom PRD-0002 records for the design this one replaced.

## More Information

- Revocation, both per session and globally, is a separate decision:
  [ADR-0036](ADR-0036-access-tokens-are-rejected-once-their-session-or-user-version-changes.md).
- The client side of this — access token in memory, proactive and reactive refresh, one logout path —
  is [ADR-0019](ADR-0019-the-access-token-lives-in-memory-with-proactive-and-reactive-refresh.md).
