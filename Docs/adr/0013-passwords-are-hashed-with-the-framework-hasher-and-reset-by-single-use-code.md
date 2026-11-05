---
id: ADR-0013
title: Passwords are hashed with the ASP.NET Core hasher and reset by single-use code
status: accepted
date: 2025-09-12
area: backend
---

# ADR-0013 (backfilled): Passwords are hashed with the ASP.NET Core hasher and reset by single-use code

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

A reset request is also the most obvious way to ask whether an address is registered, so its answer
must not depend on the truth.

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

Chosen option: 1. Passwords are hashed with `PasswordHasher<UserEntity>` from the ASP.NET Core Identity
package, used without adopting Identity's user store, schema or endpoints: the hasher is a
self-contained algorithm that embeds its format marker, work factor and salt in the stored string.

Verification and reset codes are rows keyed by username, reason and reference code, carrying an
expiry, an attempt count and a status, with indexes matching the queries that find expired codes,
count failures and invalidate the previous active code for a username. A code is six digits, expires
fifteen minutes after issue, allows three attempts and then locks verification for five minutes. A new
request invalidates the earlier active code for that username.

A reset request is answered identically whether or not the account exists, and the temporary password
it produces travels only by email.

### Consequences

- Good, because the stored value carries its own algorithm, work factor and salt, so the format is
  self-describing.
- Bad, because only a full success is accepted: a hash that verifies but reports that it needs rehashing
  is treated as a failure, and nothing rehashes after a successful verification, so raising the work
  factor would lock users out rather than upgrade them.
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
- Bad, because code state is another table to clean up, which is why a background sweep exists.
- Bad, because a temporary password is a credential the user did not choose and must change.

### Confirmation

- Test: the authentication fixtures assert that a sign-in with the wrong password fails and that a
  temporary password is accepted.
- Gap: no fixture asserts that a used or expired code is refused, and none asserts that a temporary
  password stops working, because nothing implements that.
- Review: no endpoint may return a different answer for a reset request depending on whether the
  account exists.
- Known gap: codes are stored and logged in a form that a reader of the data can use; a change here
  belongs to a new record.

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

- Which sessions are ended by a password change is covered by the revocation decision.
- Email delivery is queued in process, which is a separate decision.
