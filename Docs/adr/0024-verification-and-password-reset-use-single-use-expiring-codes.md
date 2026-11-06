---
id: ADR-0024
title: Verification and password reset use single-use, expiring codes
status: accepted
date: 2025-09-12
area: backend
---

# ADR-0024 (backfilled): Verification and password reset use single-use, expiring codes

**Summary:** In the context of local accounts with no external identity provider, facing the need to
prove an email address is reachable and to reset a forgotten password without revealing whether an
account exists, we decided to issue single-use, expiring codes for verification and reset, to email a
temporary password once a reset is verified, and to answer a reset request the same way whether or not
the account exists, accepting the codes' extra state and the cost of a temporary password.

## Context and Problem Statement

Two operations need proof that the person asking controls an email address:

- Sign-up, which proves the address belongs to the person asking for the account.
- Password reset, which proves control of the address again.

A reset request names a username, so it is also the most obvious way to ask whether an account exists,
and its answer must not depend on the truth.

## Decision Drivers

- A code must stop working once used, and expire on its own.
- A verification or a reset must not confirm that an account exists to someone who does not control
  the address.

## Considered Options

1. Single-use expiring codes stored against the request, with a reset ending in an emailed temporary
   password.
2. Password reset by emailed one-time link containing a token, with no temporary password.
3. Security questions or administrator-set passwords.

## Decision Outcome

Chosen option: 1. Verification and reset codes are rows keyed by username, reason and reference code,
carrying an expiry, an attempt count and a status, with indexes matching the queries that find expired
codes, count failures and invalidate the previous active code for a username. A code is six digits,
expires fifteen minutes after issue and fails after three wrong attempts, after which a new request is
needed; once three requests for a username have failed within five minutes, further wrong codes are
refused as too many attempts. A new request invalidates the earlier active code for that username.

A reset request is answered identically whether or not the account exists, and the temporary password
it produces travels only by email.

### Consequences

- Good, because codes expire, are counted and are invalidated by a later request, so a leaked earlier
  code is not a permanent key.
- Bad, because a code is not unconditionally consumed: a matched code whose username became taken
  returns a refusal with the row still active and no attempt counted, so it can be retried until it
  expires.
- Bad, because a temporary password is a credential the user did not choose and must change.
- Bad, because the temporary password keeps working until the user changes it; there is no
  forced-change flag, so "works once" is a statement about the email, not about the credential.
- Bad, because code rows are never purged: a background worker only marks lapsed codes `Expired` every
  five minutes, and rows are deleted only with the user they belong to.
- Bad, because the reset's enumeration safety is local: a sign-up request answers `UsernameTaken` for a
  username that exists, so usernames can be probed there instead.
- Bad, because the verification code is stored as issued and compared as plain text, and the sign-up and
  reset verification handlers and services log it at `Information` with the username and reference code.
  The temporary password is stored only as a hash, so a code alone signs nobody in. Removing the code
  from the logs is decided and carried by
  [PRD-0035](../Future/PRD-0035-telemetry-tracing-and-redaction.md); hashing stored codes is left to that
  PRD's discovery.

### Confirmation

- Test: `ScenarioProvisioner` completes every sign-up through the API with the code it reads from the
  store, so a code that cannot be verified fails the hosted suite.
- Gap: nothing exercises a password reset end to end, and no fixture asserts that a used, expired or
  failed code is refused, or that a reset for an unknown username answers as a known one does over HTTP;
  `RequestPasswordResetServiceFixture` asserts only that no email is sent. None asserts that a temporary
  password stops working, because nothing implements that.
- Review: no endpoint may return a different answer for a reset request depending on whether the
  account exists.

## Pros and Cons of the Options

### Single-use codes and a temporary password (chosen)

- Good: one code mechanism for both flows, and nothing secret in a URL.
- Bad: code lifecycle to maintain, and a temporary password the user must replace.

### Reset by token in a link

- Good: no temporary password to change afterwards.
- Bad: still needs the same single-use, expiring state, and adds a route whose token is a credential in
  a URL.

### Security questions or administrator-set passwords

- Good: nothing to email.
- Bad: either weaker shared secrets or a person who knows the password, which is worse than the
  password alone.

## More Information

- How passwords, including the temporary one, are stored is
  [ADR-0023](0023-passwords-are-hashed-with-the-framework-hasher.md).
- Which sessions are ended by a password change is
  [ADR-0037](0037-access-tokens-are-rejected-once-their-session-or-user-version-changes.md). Completing a
  reset ends none: it replaces the password hash only.
- Email delivery is queued in process: [ADR-0027](0027-email-is-queued-in-process.md).
