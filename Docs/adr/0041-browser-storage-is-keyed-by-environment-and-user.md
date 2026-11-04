---
id: ADR-0041
title: Browser storage is keyed by environment and user, with a policy per setting
status: accepted
date: 2026-11-04
area: frontend
---

# ADR-0041: Browser storage is keyed by environment and user, with a policy per setting

**Summary:** In the context of a single browser used against more than one environment and by more than
one user, facing preferences that would otherwise leak between them, we decided that every persisted
client setting is keyed by environment and user and that each setting declares whether it persists or
lives for the session, to achieve preferences that belong to the person and the environment they were
chosen in, accepting a key scheme that must be applied consistently and storage that grows with users.

## Context and Problem Statement

A preference is chosen in an environment, by a person. A local development server and a deployed
environment share a browser, and two users can share a machine. A key without either dimension means one
user's column widths and filters appear for another, in the wrong environment, where they may not even be
valid.

Some preferences are also not preferences at all: a filter or a transient view state should not survive
the session, while a theme or a column layout should.

## Decision Drivers

- Preferences must not cross users or environments.
- A setting's lifetime should match what it means.
- Storage must not become a place where server data is cached.

## Considered Options

1. Keys scoped by environment and user, with a declared lifetime per setting.
2. Unscoped keys, one per setting.
3. Keys scoped by user only.
4. Server-side storage of preferences.

## Decision Outcome

Chosen option: 1. A storage helper builds every key from an environment marker and a feature or setting
name, so the same setting holds different values per environment. Keys are environment-scoped rather than
user-scoped, which is what makes them usable before anyone is signed in; the one value that follows the
user is the signed-in identity the interface keeps so it can render before the first request completes.

Most settings are persisted and some are session-scoped, decided per setting when it is added; the helper
does not enforce the choice, so a setting that should be session-scoped is one declaration away from
being persistent.

### Consequences

- Good, because two users on one machine do not see each other's preferences, and a local environment
  cannot corrupt a deployed one.
- Good, because a setting's lifetime is declared with it, so a filter does not survive a session it was
  meant to describe.
- Good, because storage is not a shadow cache of server data, so there is no second copy to invalidate.
- Bad, because the environment marker is the only isolation: two users of the same browser against the
  same environment share every persisted preference, and the identity kept for rendering is the only
  per-user value.
- Bad, because storage holds a copy of the signed-in identity and its permission list, so a stale entry can
  describe a user the server no longer agrees with until the next fetch replaces it.
- Bad, because keys accumulate per user and are not collected when a user stops using the browser, so
  storage grows.
- Bad, because a signed-out state has to decide which key it uses, and that is a decision to make rather
  than a default.

### Confirmation

- Test: the storage helper's fixtures assert the key contains the environment segment and that the chosen
  backend is the one written to; the lifetime rule itself has no fixture, because the helper does not
  enforce it.
- Review: a storage key built by hand is a defect; a new setting states whether it persists when it is added.

## Pros and Cons of the Options

### Scoped keys with declared lifetime (chosen)

- Good: correct per user and environment, and lifetime matches meaning.
- Bad: a helper that must be used, and storage that grows.

### Unscoped keys

- Good: simplest possible.
- Bad: preferences leak between users and environments, and a value can be invalid where it is found.

### User only

- Good: separates users.
- Bad: the same user against two environments still shares values, and local experiments reach the
  deployed copy.

### Server-side preferences

- Good: portable across devices, and central.
- Bad: every preference change becomes an API call, and the interface waits on the network for things it
  currently applies immediately.

## More Information

- Server data's caching and invalidation are a separate decision.
- Filters that belong in the URL are decided separately and are not stored at all.
