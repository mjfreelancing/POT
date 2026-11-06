---
id: ADR-0039
title: The PWA precaches the application shell only and enforces updates
status: accepted
date: 2026-03-30
area: frontend
---

# ADR-0039 (backfilled): The PWA precaches the application shell only and enforces updates

**Summary:** In the context of a single-page application served as static files that should work on a
phone, facing the choice between caching whatever the service worker sees and caching only the shell with
an update check, we decided to precache the built shell and its assets, never cache API responses, and to
detect a new version and prompt for a reload, to achieve fast repeat loads without serving stale data,
accepting that a cached shell can outlive the server it was built for until its enforced update applies.

## Context and Problem Statement

A service worker sits between the browser and the network, which makes caching nearly free and staleness
nearly invisible. Two staleness problems follow.

Caching API responses would serve financial data that the server has already changed, and would leave no
signal that it is old. Caching the shell alone means the interface loads from cache while every datum
still comes from the server — but the interface itself can be an old build.
The shell-only precache has been in place since PWA support was added on 2026-03-30. The first update
flow, added on 2026-04-04, let the user postpone an update, which left the client on an old build for as
long as the user kept postponing it; the enforced update replaced it on 2026-09-22.

## Decision Drivers

- Data must never come from a cache the server cannot invalidate.
- Repeat loads should not download the whole bundle again.
- A deployment should reach users rather than leaving them on an old build indefinitely.

## Considered Options

1. Precache the shell and assets, never cache `/api`, and check for a new version.
2. Cache everything the service worker requests, with network-first for API calls.
3. No service worker at all.
4. Cache only on first navigation, with no update check.5. Precache the shell, and prompt for an update that the user may postpone.

## Decision Outcome

Chosen option: 1. The build generates a precache manifest of the built files that `workbox.globPatterns`
matches (scripts, styles, HTML, images and fonts). The service worker has no runtime caching routes
(`runtimeCaching: []`), so it caches nothing it was not built with and every API call goes to the network;
`navigateFallbackDenylist` also keeps a navigation to `/api` from being answered with the cached
`index.html`. A new build is detected through two channels: the service worker's own update check, at
start-up, on focus and every 30 minutes, and a comparison of the build identifier baked into the bundle
with `/version.json`, which nginx serves with `no-cache` and the client fetches with `no-store` at
start-up, on focus and every 5 minutes. Once a new worker is waiting, the user sees a notice that cannot
be dismissed, and the application applies the update and reloads after 45 seconds without input, or after
5 minutes regardless.

### Consequences

- Good, because repeat loads come from cache, and the data on screen is always from the server.
- Good, because an update is detected and applied deliberately, rather than a user staying on an old build
  until the cache happens to be cleared.
- Good, because excluded API traffic means the service worker cannot mask a failure or a rate limit with a
  cached success.
- Bad, because the cached shell is only as new as the last enforced update, so a change that removes an
  endpoint leaves old code calling it until the reload happens.
- Bad, because the reload is not the user's: the notice cannot be dismissed, and the interface reloads on
  its own, which can discard a half-typed form; nothing tracks unsaved changes across the app.
- Bad, because each tab enforces independently on its own timers, so several tabs can reload at different
  moments.
- Bad, because the precache covers only the file types in `globPatterns`, so an asset of a new type is
  fetched from the network each time and is unavailable offline.

### Confirmation

- Test: the enforcement fixtures assert the quiet-period and maximum-deferral behaviour, and the version
  check asserts the manifest is requested without cache.
- Test: `tests/concerns/pwa/pwaUpdatePrompt.test.ts` asserts the notice cannot be dismissed.
- Gap: no fixture asserts that `runtimeCaching` stays empty or that `/api` stays in
  `navigateFallbackDenylist`; both are configuration in `vite.config.ts` only.
- Review: a `runtimeCaching` entry that matches API traffic is a defect; a change to the update flow keeps
  the update enforced — not dismissible, and applied by the maximum deferral.
- Known limitation: no client build identifier reaches the server and no compatibility policy covers a
  breaking API change, so an old shell keeps calling a changed API until its enforced update applies —
  roughly ten minutes after a client deploy for an open tab, plus any gap between the separate server and
  client deploy tasks (`.vscode/tasks.json`). Whether that skew is accepted or closed is not yet decided.

## Pros and Cons of the Options

### Shell precache with update check (chosen)

- Good: fast loads, fresh data, and visible updates.
- Bad: a stale shell window, and a prompt to handle.

### Cache everything, network-first for the API

- Good: works offline for more than the shell.
- Bad: offline financial data is cached data, with no way for the server to correct it.

### No service worker

- Good: nothing can be stale.
- Bad: there is no installable app, and the shell cannot load without the network; hashed assets would
  still come from the HTTP cache, which nginx marks `immutable` for `/assets/`.

### Cache on first navigation, no update check

- Good: simple.
- Bad: the user is pinned to the build they first loaded, and the only signal is a bug report.
### Prompt the user may postpone

- Good: the user chooses when to reload, so no form is lost.
- Bad: a postponed update leaves the client on the old build for as long as the user keeps postponing it.
  This shipped on 2026-04-04, with a `Later` action that snoozed the prompt, and was replaced by
  enforcement on 2026-09-22 (commit `edd85d43`).

## More Information

- Where TLS and the ingress sit in front of the static files is a separate decision.
- A user-facing announcement of significant change is a product item, not part of this decision.
