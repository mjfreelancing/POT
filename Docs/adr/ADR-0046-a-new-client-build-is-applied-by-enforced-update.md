---
id: ADR-0046
title: A new client build is applied by enforced update
status: accepted
date: 2026-09-22
area: frontend
---

# ADR-0046 (backfilled): A new client build is applied by enforced update

**Summary:** In the context of a PWA that serves its shell from the service worker's precache, facing a
deployment that reaches an open client only when that client applies it, we decided that a new build is
detected through both the service worker's update check and a comparison of build identifiers, and that
once it is waiting the user cannot postpone it indefinitely, to achieve deployments that reach every open
client within a bounded time, accepting a reload that is not the user's choice and can discard unsaved
input.

## Context and Problem Statement

The shell is precached
([ADR-0038](ADR-0038-the-pwa-precaches-the-application-shell-only-and-never-caches-api-responses.md)), so a client keeps running
the build it loaded until a new service worker takes over. A deployment changes nothing for an open tab on
its own: the new worker installs and then waits while the old one controls the page. Every datum still
comes from the server, so the cost of an old shell is old code calling an API that may have changed.

The first update flow, added on 2026-04-04, prompted the user and let them postpone the update, which left
the client on an old build for as long as the user kept postponing it. The enforced update replaced it on
2026-09-22 (commit `edd85d43`).

## Decision Drivers

- A deployment should reach users rather than leaving them on an old build indefinitely.
- A reload should wait for a pause in the user's input where it can, so a half-typed form is not lost.
## Considered Options

1. Detect a new build through both channels, and apply it by an update that cannot be postponed
   indefinitely.
2. Prompt for an update that the user may postpone.
3. Apply the update silently the next time the application is opened or reloaded.
4. Reload as soon as a new build is detected.

## Decision Outcome

Chosen option: 1, because only an enforced update bounds how long an old shell can keep running against
the current API once a new build exists.

A new build is detected through two channels: the service worker's own update check, and a comparison of
the build identifier baked into the bundle with the deployed `/version.json`, which is never served or
fetched from a cache. Once a new worker is waiting, the user sees a notice that cannot be dismissed, and
the application applies the update and reloads after a quiet period without input, or after a maximum
deferral regardless.

### Consequences

- Good, because an update is detected and applied deliberately, rather than a user staying on an old build
  until the cache happens to be cleared.
- Good, because the time an old shell keeps running after its replacement is detected has an upper bound.
- Bad, because the reload is not the user's: the notice cannot be dismissed, and the interface reloads on
  its own, which can discard a half-typed form; nothing tracks unsaved changes across the app.
- Bad, because each tab enforces independently on its own timers, so several tabs can reload at different
  moments.
- Bad, because the bound starts when a check finds the new build, not when it is deployed.

### Confirmation

- Test: `tests/concerns/pwa/pwaUpdateEnforcement.test.ts` asserts the quiet-period and maximum-deferral
  behaviour, and `tests/concerns/pwa/pwaVersionCheck.test.ts` asserts the version file is requested
  without cache.
- Test: `tests/concerns/pwa/pwaUpdatePrompt.test.ts` asserts the notice cannot be dismissed.
- Review: a change to the update flow keeps the update enforced — not dismissible, and applied by the
  maximum deferral.
- Known limitation: no client build identifier reaches the server and no compatibility policy covers a
  breaking API change, so an old shell keeps calling a changed API until its enforced update applies —
  roughly ten minutes after a client deploy for an open tab, plus any gap between the separate server and
  client deploy tasks (`.vscode/tasks.json`). Whether that skew is accepted or closed is not yet decided.

## Pros and Cons of the Options

### Enforced update through two channels (chosen)

- Good: every open client reaches the new build within a bounded time.
- Bad: a reload the user did not ask for.

### Prompt the user may postpone

- Good: the user chooses when to reload, so no form is lost.
- Bad: a postponed update leaves the client on the old build for as long as the user keeps postponing it.
  This shipped on 2026-04-04, with a `Later` action that snoozed the prompt, and was replaced by
  enforcement on 2026-09-22 (commit `edd85d43`).

### Apply silently on the next open

- Good: no interruption at all.
- Bad: a tab or an installed app that stays open is never updated, which is the case the bound exists for.

### Reload as soon as a build is detected

- Good: the shortest possible window on an old build.
- Bad: the reload can land in the middle of any input, with no warning.

## More Information

- The caching decision this depends on:
  [ADR-0038](ADR-0038-the-pwa-precaches-the-application-shell-only-and-never-caches-api-responses.md).
- Current values (`src/concerns/pwa/pwaRuntime.ts`): the service worker's update check runs at start-up,
  on focus and every 30 minutes; the build-identifier check runs at start-up, on focus and every 5 minutes.
  nginx serves `/version.json` with `no-cache` and the client fetches it with `no-store`. The quiet period
  is 45 seconds and the maximum deferral 5 minutes.
- A user-facing announcement of significant change is a product item, not part of this decision.
