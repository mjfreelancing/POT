---
id: ADR-0042
title: The PWA precaches the application shell only and enforces updates
status: accepted
date: 2026-11-04
area: frontend
---

# ADR-0042: The PWA precaches the application shell only and enforces updates

**Summary:** In the context of a single-page application served as static files that should work on a
phone, facing the choice between caching whatever the service worker sees and caching only the shell with
an update check, we decided to precache the built shell and its assets, never cache API responses, and to
detect a new version and prompt for a reload, to achieve fast repeat loads without serving stale data,
accepting that a cached shell can outlive the server it was built for until the user reloads.

## Context and Problem Statement

A service worker sits between the browser and the network, which makes caching nearly free and staleness
nearly invisible. Two staleness problems follow.

Caching API responses would serve financial data that the server has already changed, and would leave no
signal that it is old. Caching the shell alone means the interface loads from cache while every datum
still comes from the server — but the interface itself can be an old build.

## Decision Drivers

- Data must never come from a cache the server cannot invalidate.
- Repeat loads should not download the whole bundle again.
- A deployment should reach users rather than leaving them on an old build indefinitely.

## Considered Options

1. Precache the shell and assets, never cache `/api`, and check for a new version.
2. Cache everything the service worker requests, with network-first for API calls.
3. No service worker at all.
4. Cache only on first navigation, with no update check.

## Decision Outcome

Chosen option: 1. The build generates a precache manifest of the shell and hashed assets. Requests to the
API are excluded from the service worker, so data is always the server's answer. A version manifest is
served with no-cache so the client can detect that a newer build exists, and when it does the user is told
and the application reloads to it.

### Consequences

- Good, because repeat loads come from cache, and the data on screen is always from the server.
- Good, because an update is detected and applied deliberately, rather than a user staying on an old build
  until the cache happens to be cleared.
- Good, because excluded API traffic means the service worker cannot mask a failure or a rate limit with a
  cached success.
- Bad, because the cached shell is only as new as the last update, so a change that removes an endpoint
  leaves old code calling it until the reload.
- Bad, because the update prompt is an interruption the user must accept, and its behaviour across tabs and
  visibility changes needs its own checks.
- Bad, because the precache manifest is another artefact whose entries drift from the build.

### Confirmation

- Test: the service-worker fixtures assert API routes are not handled by the worker and that the version
  manifest is requested without cache.
- Review: a route added to the service worker for API traffic is a defect; a change to the update prompt
  keeps the reload explicit rather than automatic behind a form.
- Known limitation: an old shell can outlive a breaking API change until the user reloads, which is a
  compatibility window the client does not otherwise provide.

## Pros and Cons of the Options

### Shell precache with update check (chosen)

- Good: fast loads, fresh data, and visible updates.
- Bad: a stale shell window, and a prompt to handle.

### Cache everything, network-first for the API

- Good: works offline for more than the shell.
- Bad: offline financial data is cached data, with no way for the server to correct it.

### No service worker

- Good: nothing can be stale.
- Bad: every load downloads the bundle, and there is no installable app.

### Cache on first navigation, no update check

- Good: simple.
- Bad: the user is pinned to the build they first loaded, and the only signal is a bug report.

## More Information

- Where TLS and the ingress sit in front of the static files is a separate decision.
- A user-facing announcement of significant change is a product item, not part of this decision.
