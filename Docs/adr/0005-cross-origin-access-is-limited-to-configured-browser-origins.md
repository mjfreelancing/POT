---
id: ADR-0005
title: Cross-origin access is limited to configured browser origins
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0005: Cross-origin access is limited to configured browser origins

**Summary:** In the context of a browser client served from one origin and an API on another, facing
the need for the refresh cookie to travel with requests and for the client to read a few response
headers, we decided to allow credentialed cross-origin calls from an explicitly configured set of
origins with every method and header permitted and four response headers exposed, to achieve working
cookie-based sessions without a wildcard, accepting that every new deployment hostname is a
configuration change.

## Context and Problem Statement

This is a backfilled record, written on 2026-11-04 from the policy, the client code and the owner's
account. The owner reviewed and approved the original configuration; it is recorded here as the
project's decision rather than as something that arrived unnoticed.

In the deployed system the client and the API are separate origins, and the access token is refreshed
by an HttpOnly cookie, so the browser must send credentials cross-origin. Browsers refuse a wildcard
origin together with credentials, and they hide non-safelisted response headers from script unless the
server names them.

The client sets `withCredentials` on both its axios instances, reads the download filename out of
`content-disposition`, and identifies requests with its own `X-Correlation-ID`, which is not a
safelisted header.

## Considered Options

1. Configured origins, credentials allowed, any method and header, four headers exposed, and a
   preflight cache.
2. Wildcard origin, which forbids credentials.
3. One origin in production by serving the API under the client's host, so no CORS is involved.
4. Configured origins with explicit method and header lists instead of allowing any.

## Decision Outcome

Chosen option: 1. The default policy allows only origins from configuration, allows credentialed
requests, allows every method and header, exposes `Content-Disposition`, `Location`, `Retry-After` and
`X-Correlation-Id`, and caches preflights for ten minutes. Startup validation rejects an empty list,
`*`, and malformed origins.

Owner's account: the configuration was reviewed and accepted as it stands. The reasoning for each
setting was confirmed by audit rather than recalled: credentials are mandatory because the refresh
cookie is HttpOnly and cross-origin; `Authorization`, `Content-Type` and `X-Correlation-ID` are all
non-safelisted, so a header list would have to name them; and without a preflight cache, nearly every
call pays an extra round trip.

### Consequences

- Good, because the refresh cookie works while only configured origins can read responses.
- Good, because the download filename, the rate-limit hint and the correlation id are readable by
  script; the correlation id is also the only support signal when a gateway failure carries no body.
- Good, because a malformed or wildcard origin fails at startup rather than at request time.
- Bad, because adding a deployment hostname is a configuration change; an origin that is omitted is
  blocked by the browser before the request is even sent.
- Bad, because `Location`, `Retry-After` and `X-Correlation-Id` are exposed although nothing reads
  them from a response today. They are kept deliberately: `Location` is part of the 201 contract, and
  `Retry-After` is what a cooldown feature would read.
- Neutral, because the preflight cache is per browser, so removing an origin takes effect no later
  than that browser's cached preflight expires.

### Confirmation

- Test: `CorsFixture` drives real preflight and validation-error requests, asserting the allowed
  origin is echoed, the disallowed origin receives no `Access-Control-Allow-Origin`, a second
  configured origin works with both `,` and `;` separators, and the cache is 600 seconds.
- Test: `CorsOptionsSetupFixture` and `CorsConfigurationSetupFixture` assert the policy and the
  startup validation, including rejection of `*`.
- Review: a policy change that adds an origin or exposes a header belongs in configuration and in this
  record's consequences, not in an inline policy built in a handler.

## Pros and Cons of the Options

### Configured origins with credentials (chosen)

- Good: the refresh cookie works and the exposed set is deliberate.
- Bad: one configuration entry per hostname, and the exposed headers outlive their current readers.

### Wildcard origin

- Good: no configuration at all.
- Bad: incompatible with credentialed requests, so the refresh cookie cannot be used; the policy also
  fails to build, which is a request-time failure rather than a startup one.

### A single origin in production

- Good: no CORS problem exists, and cookies become same-site.
- Bad: the client and API would have to be served under one host, which changes the deployment shape
  rather than the policy, and would still leave local development cross-origin.

### Explicit method and header lists

- Good: a new header fails closed rather than silently being allowed.
- Bad: every request-shape change becomes a deployment-blocking policy change, for no security gain
  once the origins are explicit and credentialed.

## More Information

- Client side: `Source/Client/pot-react/src/api/axios.ts` and `src/api/authClient.ts` set
  `withCredentials`; `src/features/maintenance/export/utils/fileUtils.ts` reads
  `content-disposition`.
- The transport boundary these origins live behind is
  [ADR-0003](0003-public-tls-terminates-at-the-container-apps-ingress.md).
