---
id: ADR-0038
title: The PWA precaches the application shell only and never caches API responses
status: accepted
date: 2026-03-30
area: frontend
---

# ADR-0038 (backfilled): The PWA precaches the application shell only and never caches API responses

**Summary:** In the context of a single-page application served as static files that should work on a
phone, facing the choice between caching whatever the service worker sees and caching only the built
shell, we decided to precache the built shell and its assets and never cache API responses, to achieve
fast repeat loads without serving stale data, accepting that the interface itself can be an old build
until a new one is applied.

## Context and Problem Statement

A service worker sits between the browser and the network, which makes caching nearly free and staleness
nearly invisible. Two staleness problems follow.

Caching API responses would serve financial data that the server has already changed, and would leave no
signal that it is old. Caching the shell alone means the interface loads from cache while every datum
still comes from the server — but the interface itself can be an old build.
The shell-only precache has been in place since PWA support was added on 2026-03-30. How a new build
reaches a client that holds an old shell is a separate decision (see More Information).

## Decision Drivers

- Data must never come from a cache the server cannot invalidate.
- Repeat loads should not download the whole bundle again.

## Considered Options

1. Precache the shell and assets, and never cache `/api`.
2. Cache everything the service worker requests, with network-first for API calls.
3. No service worker at all.
4. Cache what the first navigation loads, at run time, rather than a manifest built with the client.

## Decision Outcome

Chosen option: 1. The build generates a precache manifest of the built files that `workbox.globPatterns`
matches (scripts, styles, HTML, images and fonts). The service worker has no runtime caching routes
(`runtimeCaching: []`), so it caches nothing it was not built with and every API call goes to the network;
`navigateFallbackDenylist` also keeps a navigation to `/api` from being answered with the cached
`index.html`.

### Consequences

- Good, because repeat loads come from cache, and the data on screen is always from the server.
- Good, because excluded API traffic means the service worker cannot mask a failure or a rate limit with a
  cached success.
- Bad, because the cached shell is only as new as the last applied update, so a change that removes an
  endpoint leaves old code calling it until a new build is applied.
- Bad, because the precache covers only the file types in `globPatterns`, so an asset of a new type is
  fetched from the network each time and is unavailable offline.

### Confirmation

- Test: `e2e/tests/pwa/pwaContract.test.ts` asserts that the built client serves the manifest and registers
  the service worker, and that the development server does neither.
- Gap: no fixture asserts that `runtimeCaching` stays empty or that `/api` stays in
  `navigateFallbackDenylist`; both are configuration in `vite.config.ts` only.
- Review: a `runtimeCaching` entry that matches API traffic is a defect.

## Pros and Cons of the Options

### Shell precache, no API caching (chosen)

- Good: fast loads and fresh data.
- Bad: a window in which the shell is older than the server.

### Cache everything, network-first for the API

- Good: works offline for more than the shell.
- Bad: offline financial data is cached data, with no way for the server to correct it.

### No service worker

- Good: nothing can be stale.
- Bad: there is no installable app, and the shell cannot load without the network; hashed assets would
  still come from the HTTP cache, which nginx marks `immutable` for `/assets/`.

### Cache on first navigation

- Good: simple.
- Bad: there is no built manifest to compare with a newer one, so the user is pinned to the build they
  first loaded, and the only signal is a bug report.

## More Information

- How a new build is detected and applied, and the skew between an old shell and a changed API, are
  decided in [ADR-0046](ADR-0046-a-new-client-build-is-applied-by-enforced-update.md).
- Where TLS and the ingress sit in front of the static files is a separate decision.
