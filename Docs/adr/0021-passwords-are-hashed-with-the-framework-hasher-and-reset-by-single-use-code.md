---
id: ADR-0021
title: Passwords are hashed with the ASP.NET Core hasher and reset by single-use code
status: accepted
date: 2025-09-12
area: backend
---

# ADR-0021 (backfilled): Passwords are hashed with the ASP.NET Core hasher and reset by single-use code

**Summary:** In the context of local credentials for a product with no external identity provider,
facing the need to store passwords safely, to prove an email address is reachable, and to reset a
forgotten password without revealing whether an account exists, we decided to hash passwords with the
framework's password hasher, to issue single-use expiring codes for verification and reset, and to
answer a reset request the same way whether or not the account exists, accepting the reset flow's
extra state and the cost of a temporary password.

## Context and Problem Statement

Accounts are local to the product: a username, an email address and a password. Three operations need
credential material that is safe at rest:

- Sign-up, which proves the address belongs to the person asking for the account.
- Sign-in, which verifies a password without ever storing it.
- Password reset, which proves control of the address again.

A reset request names a username, so it is also the most obvious way to ask whether an account exists,
and its answer must not depend on the truth.

## Decision Drivers

- A stored hash must be useless on its own, including if the database is read wholesale.
- A code must stop working once used, and expire on its own.
- A verification or a reset must not confirm that an account exists to someone who does not control
  the address.

## Considered Options

1. The framework's password hasher, with single-use expiring codes stored against the request.
2. A hand-written hashing scheme using a primitive such as PBKDF2 or bcrypt directly.
3. Password reset by emailed one-time link containing a token, with no temporary password.
4. Security questions or administrator-set passwords.

## Decision Outcome

Chosen option: 1. Passwords are hashed with `PasswordHasher<UserEntity>` from ASP.NET Core Identity — it
ships in the ASP.NET Core shared framework, so no package is referenced — used without adopting
Identity's user store, schema or endpoints: the hasher is a
self-contained algorithm that embeds its format marker, work factor and salt in the stored string.

Verification and reset codes are rows keyed by username, reason and reference code, carrying an
expiry, an attempt count and a status, with indexes matching the queries that find expired codes,
count failures and invalidate the previous active code for a username. A code is six digits, expires
fifteen minutes after issue and fails after three wrong attempts, after which a new request is needed;
once three requests for a username have failed within five minutes, further wrong codes are refused as
too many attempts. A new request invalidates the earlier active code for that username.

A reset request is answered identically whether or not the account exists, and the temporary password
it produces travels only by email.

### Consequences

- Good, because the stored value carries its own algorithm, work factor and salt, so the format is
  self-describing.
- Bad, because only a full success is accepted: a hash that verifies but reports that it needs rehashing
  is treated as a failure, and nothing rehashes after a successful verification, so raising the work
  factor — including a future framework default, since the hasher runs with default options — would lock
  users out rather than upgrade them.
- Good, because taking the hasher without the rest of Identity avoids a second user schema, a second
  set of endpoints and Identity's own conventions.
- Good, because codes expire, are counted and are invalidated by a later request, so a leaked earlier
  code is not a permanent key.
- Bad, because a code is not unconditionally consumed: a matched code whose username became taken
  returns a refusal with the row still active and no attempt counted, so it can be retried until it
  expires.
- Bad, because the same hasher is used for the temporary password that a reset issues, so that value
  is stored as a hash it can never be shown again from — which is correct, but makes support recovery
  impossible.
- Bad, because the temporary password keeps working until the user changes it; there is no
  forced-change flag, so "works once" is a statement about the email, not about the credential.
- Bad, because code rows are never purged: a background worker only marks lapsed codes `Expired` every
  five minutes, and rows are deleted only with the user they belong to.
- Bad, because a temporary password is a credential the user did not choose and must change.- Bad, because the reset's enumeration safety is local: a sign-up request answers `UsernameTaken` for a
  username that exists, so usernames can be probed there instead.
- Bad, because the verification code is stored as issued and compared as plain text, and the sign-up and
  reset verification handlers and services log it at `Information` with the username and reference code.
  The temporary password is stored only as a hash, so a code alone signs nobody in; whether to stop
  logging codes, or to hash them, is open (`DECISIONS-TO-DISCUSS.md`, DD-08).

### Confirmation

- Test: the integration host seeds every user with the real `UserPasswordHasher` and signs in through
  the API (`IntegrationAuthFixtureBase`), and `ScenarioProvisioner` signs in with the sign-up and
  invitation temporary passwords read from captured mail, so a hash that stops verifying fails the suite.
- Test: `AuthServiceFixture` asserts that a failed password check refuses sign-in without creating a
  session; the hasher is substituted there.
- Gap: nothing exercises a password reset end to end, and no fixture asserts that a used, expired or
  failed code is refused, or that a reset for an unknown username answers as a known one does over HTTP;
  `RequestPasswordResetServiceFixture` asserts only that no email is sent. None asserts that a temporary
  password stops working, because nothing implements that.
- Review: no endpoint may return a different answer for a reset request depending on whether the
  account exists.


## Pros and Cons of the Options

### Framework hasher with single-use codes (chosen)

- Good: proven implementation, self-describing hash, and no identity framework to absorb.
- Bad: two credential shapes in the same table, and code lifecycle to maintain.

### Hand-written hashing

- Good: full control over parameters.
- Bad: a security primitive written by hand, with no benefit over a proven one.

### Reset by token in a link

- Good: no temporary password to change afterwards.
- Bad: still needs the same single-use, expiring state, and adds a route whose token is a credential in
  a URL.

### Security questions or administrator-set passwords

- Good: nothing to email.
- Bad: either weaker shared secrets or a person who knows the password, which is worse than the
  password alone.

## More Information

- Which sessions are ended by a password change is covered by the revocation decision. Completing a
  reset ends none: it replaces the password hash only.
- Email delivery is queued in process, which is a separate decision.
