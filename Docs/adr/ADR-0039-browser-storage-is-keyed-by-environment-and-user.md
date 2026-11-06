---
id: ADR-0039
title: Browser storage is keyed by environment and user
status: accepted
date: 2026-05-03
area: frontend
related-prds: [PRD-0001]
---

# ADR-0039 (backfilled): Browser storage is keyed by environment and user

**Summary:** In the context of a single browser used against more than one environment and by more than
one user, facing preferences that would otherwise leak between them, we decided that every persisted
client setting is keyed by environment and, when it belongs to a person, by user, to achieve preferences
that belong to the person and the environment they were chosen in, accepting a key scheme that must be
applied consistently and storage that grows with users.

## Context and Problem Statement

A preference is chosen in an environment, by a person. Two users can share a browser profile, and a key
without a user segment means one user's filters, dashboard layout and theme appear for the other. Browser
storage is already separated by origin, so a deployed site and a local server on another origin never see
each other's keys; what can share an origin is a development-mode and a production-mode build of the same
client (`npm run dev` and `npm run preview` both serve on port 5175).

Until [PRD-0001](../Future/PRD-0001-per-session-login-support.md) (decisions D001–D007), every setting used
a flat, unscoped `localStorage` key shared by every user and environment.

## Decision Drivers

- Preferences must not cross users or environments.
- Storage must not become a place where server data is cached.

## Considered Options

1. Keys scoped by environment and user.
2. Unscoped keys, one per setting (the shape before PRD-0001).
3. Keys scoped by user only.
4. Server-side storage of preferences.

## Decision Outcome

Chosen option: 1. Two builders in `src/concerns/storage/storageKeyBuilder.ts` make every scoped key. A
setting that belongs to a person uses `pot:{env}:user:{userId}:{feature}`. State that belongs to the
device rather than to a person uses `pot:{env}:{feature}`. `{env}` is `prod` for a production build and
`dev` for every other mode. While no one is signed in, the feature storage hooks return their defaults and
write nothing.

### Consequences

- Good, because two users of one browser profile do not see each other's filters, layout or theme once
  signed in, and a development build cannot read what a production build wrote on the same origin.
- Good, because, apart from the signed-in identity, storage holds no server data, so there is no second
  copy to invalidate.
- Bad, because the environment segment has only two values: every production build — local Docker and the
  deployed site alike — writes `prod`, so the key alone does not separate those environments; their
  different origins do.
- Bad, because storage holds a copy of the signed-in identity and its permission list, so a stale entry can
  describe a user the server no longer agrees with until the next fetch replaces it.
- Bad, because keys accumulate per user and are not collected when a user stops using the browser, so
  storage grows.
- Bad, because a signed-out state has to decide which key it uses, and that is a decision to make rather
  than a default.

### Confirmation

- Test: `tests/concerns/storage/storageKeyBuilder.test.ts` and `storageEnv.test.ts` assert both key
  formats and the environment segment. Each consumer's fixture asserts the key it builds —
  `tests/features/*/hooks/use*Storage.test.ts`, `tests/app/App.test.tsx` for the theme and
  `tests/stores/useUserStore.test.ts` for the identity — so moving a setting to another key fails its
  fixture.
- Review: a storage key built by hand is a defect.

## Pros and Cons of the Options

### Scoped keys (chosen)

- Good: correct per user and environment.
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

- The device-scoped keys are the theme before sign-in, the one-shot session notice, and the persisted
  signed-in identity (`src/stores/useUserStore.ts`), which cannot be keyed by the user it identifies.
- Each setting also declares, where its storage hook is written, whether it persists or lives for the tab
  session; PRD-0001 (current storage table and D001–D007) records the choice for each setting.
- That lifetime is not enforced by the helper: `resolveStorageBackend` exists, but each hook passes its
  backend directly. Each consumer's fixture asserts the backend it chooses, and no fixture stops a new
  setting from choosing the wrong lifetime.
- Server data's caching and invalidation are a separate decision.
- Filters are read from the URL at render time, which is decided separately (PRD-0011); the session-scoped
  copy exists so the sidebar can rebuild a filtered link (`src/hooks/useFeatureFilterHref.ts`).
