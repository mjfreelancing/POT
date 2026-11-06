---
id: ADR-0023
title: Passwords are hashed with the ASP.NET Core hasher
status: accepted
date: 2025-09-12
area: backend
---

# ADR-0023 (backfilled): Passwords are hashed with the ASP.NET Core hasher

**Summary:** In the context of local credentials for a product with no external identity provider,
facing the need to store passwords safely, we decided to hash them with the framework's password hasher
without adopting the rest of ASP.NET Core Identity, to achieve a proven, self-describing hash with no
second user model, accepting that the hasher's defaults are taken as they come and nothing rehashes.

## Context and Problem Statement

Accounts are local to the product: a username, an email address and a password. Sign-in must verify a
password without the password ever being stored, and the temporary passwords that sign-up, invitation
and password reset issue are credentials too. A stored value must be useless on its own, including if
the database is read wholesale.

## Decision Drivers

- A stored hash must be useless on its own, including if the database is read wholesale.
- A security primitive should not be written by hand.
- The product's own user table and endpoints stay the only ones.

## Considered Options

1. The framework's password hasher, used without the rest of ASP.NET Core Identity.
2. A hand-written hashing scheme using a primitive such as PBKDF2 or bcrypt directly.
3. ASP.NET Core Identity in full: its user store, schema and endpoints.

## Decision Outcome

Chosen option: 1. Passwords are hashed with `PasswordHasher<UserEntity>` from ASP.NET Core Identity — it
ships in the ASP.NET Core shared framework, so no package is referenced — used without adopting
Identity's user store, schema or endpoints: the hasher is a self-contained algorithm that embeds its
format marker, work factor and salt in the stored string. Temporary passwords are hashed the same way.

### Consequences

- Good, because the stored value carries its own algorithm, work factor and salt, so the format is
  self-describing.
- Good, because taking the hasher without the rest of Identity avoids a second user schema, a second
  set of endpoints and Identity's own conventions.
- Bad, because only a full success is accepted: a hash that verifies but reports that it needs rehashing
  is treated as a failure, and nothing rehashes after a successful verification, so raising the work
  factor — including a future framework default, since the hasher runs with default options — would lock
  users out rather than upgrade them.
- Bad, because a temporary password is stored only as a hash and can never be shown again — which is
  correct, but makes support recovery impossible.

### Confirmation

- Test: the integration host seeds every user with the real `UserPasswordHasher` and signs in through
  the API (`IntegrationAuthFixtureBase`), and `ScenarioProvisioner` signs in with the sign-up and
  invitation temporary passwords read from captured mail, so a hash that stops verifying fails the suite.
- Test: `AuthServiceFixture` asserts that a failed password check refuses sign-in without creating a
  session; the hasher is substituted there.
- Review: a password, temporary or chosen, is stored only through the password hasher.

## Pros and Cons of the Options

### Framework hasher alone (chosen)

- Good: proven implementation, self-describing hash, and no identity framework to absorb.
- Bad: its defaults are the product's defaults, and an upgrade path has to be built.

### Hand-written hashing

- Good: full control over parameters.
- Bad: a security primitive written by hand, with no benefit over a proven one.

### Identity in full

- Good: rehashing, lockout and reset flows come with it.
- Bad: a second user schema and set of endpoints, and Identity's conventions over the product's own.

## More Information

- How an email address is verified and a forgotten password is reset is
  [ADR-0024](ADR-0024-verification-and-password-reset-use-single-use-expiring-codes.md).
- Which sessions are ended by a password change is
  [ADR-0036](ADR-0036-access-tokens-are-rejected-once-their-session-or-user-version-changes.md).
