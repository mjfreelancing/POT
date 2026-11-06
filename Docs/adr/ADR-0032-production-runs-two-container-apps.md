---
id: ADR-0032
title: Production runs two container apps: nginx serves the SPA and the API is separate
status: accepted
date: 2025-11-07
area: infrastructure
---

# ADR-0032 (backfilled): Production runs two container apps: nginx serves the SPA and the API is separate

**Summary:** In the context of a static single-page application and a stateless API, facing the choice
between one origin serving both and two deployments with their own hostnames, we decided on two
container apps in one environment — nginx serving the built client and the API image on its own
hostname — to achieve independent deployment and a normal browser cache for static assets, accepting a
cross-origin boundary and two things to deploy.

## Context and Problem Statement

The client is a set of static files once built; the API is a process. They can share a host — nginx can
serve the files and proxy the API — or be separate deployments.

Behind one host, the browser sees one origin, so no cross-origin configuration is needed and cookies are
same-site. Behind two, each can be deployed and rolled back alone, and the static files are served by
whatever does that best.

## Decision Drivers

- The client and the API have different lifecycles: a UI change should not redeploy the API.
- Static assets should be cached by the browser, which wants stable URLs and long lifetimes.
- The API should be reachable by anything that can present a token, not only by the bundled UI.

## Considered Options

1. Two container apps: nginx for the SPA, the API separately, each with its own hostname and certificate.
2. One container app: nginx serves the SPA and proxies `/api` to the API.
3. One container app running both processes.
4. Static hosting for the client with the API elsewhere.

## Decision Outcome

Chosen option: 1. The client image is a multi-stage build whose final stage is nginx serving the built
files, with a health endpoint and cache headers for hashed assets. The API image runs the migration
runner and then Kestrel. Both are published to the same registry and run in one Container Apps
environment, each with its own ingress and managed certificate, on the custom domains.

The client is built with the API's absolute base URL, because there is no proxy to make a relative path
work, and the browser calls the API's hostname directly.

### Consequences

- Good, because the client and the API are deployed independently, and a UI-only change does not touch
  the API.
- Good, because static assets are served by nginx with long cache lifetimes, and hashed names make that
  safe.
- Good, because the API is a normal HTTPS endpoint that any client can call, including the end-to-end
  tests and anything else written later.
- Bad, because the API base URL is baked into the client at build time, so the client image is
  environment-specific and the URL cannot be changed without rebuilding.
- Bad, because every call is cross-origin, so the CORS policy and the cookie's `SameSite` behaviour are
  load-bearing rather than incidental: the refresh cookie is `SameSite=Lax` (`RefreshTokenHelper.cs`),
  which works only because the client and API hostnames share one registrable domain.
- Bad, because two deployments and two ingress configurations must be kept in step.
- Bad, because the two images are built and pushed by separate tasks, so for a while after a deployment
  the new API serves the old client (and a cached PWA shell keeps calling it); no compatibility policy
  covers that window, which is the open question DD-14 in `DECISIONS-TO-DISCUSS.md`.

### Confirmation

- Review: the client image is built with the API base URL for its environment, so a client built for one
  environment must not be deployed to another.
- Gap: no test exercises the deployed arrangement. The production-like end-to-end configuration
  (`playwright.prod.config.ts`) serves the built client through `vite preview`, whose proxy forwards a
  relative `/api`, so it is same-origin and would not catch a wrong absolute base URL or a missing origin
  in the deployed CORS list. `CorsFixture` covers the API's side of the cross-origin boundary.
- Known limitation: the deployment is configured by hand, so the two apps' settings are not recorded as
  code and can only be verified against the running deployment.

## Pros and Cons of the Options

### Two apps (chosen)

- Good: independent lifecycle, and static serving done by a static server.
- Bad: a cross-origin boundary and a build-time base URL.

### One app with a proxy

- Good: one origin, one deployment, and a relative API path.
- Bad: a UI change redeploys the API, and the proxy configuration becomes part of every client change.

### One app, two processes

- Good: one deployment.
- Bad: two processes in one container, which is a supervision problem the platform does not solve.

### Static hosting plus the API

- Good: the cheapest possible client hosting.
- Bad: a second hosting platform and its own configuration, for the same cross-origin boundary this
  decision already accepts.

## More Information

- Where TLS terminates, and what the container receives, is
  [ADR-0033](ADR-0033-public-tls-terminates-at-the-container-apps-ingress.md).
- The CORS policy that makes the cross-origin boundary work is
  [ADR-0020](ADR-0020-cross-origin-access-is-limited-to-configured-browser-origins.md).
- How the images are built and published is
  [ADR-0031](ADR-0031-builds-and-deployments-are-run-by-hand.md).
- The base URL and the nginx configuration are both set in `.vscode/tasks.json`
  (`azure-client-build-and-deploy`): it builds with `NGINX_CONFIG=nginx.azure.conf`, which has no `/api`
  proxy, and with the API's own hostname as the base URL. The browser therefore calls
  `api.payontime.com.au` directly, and nothing routes `/api` on the client's hostname to the API:
  `nginx.azure.conf` answers any unknown path, `/api/...` included, with the SPA's `index.html`.
- An earlier diagram and `Source/Docker/DEVELOPER.md` described ingress routing `/api/*` to the API
  container; both were corrected on 2026-11-06 to follow the build. The record does not show whether the
  single-origin options were weighed when the deployment was set up.
- The API container app is configured by hand with a minimum of 1 and a maximum of 10 replicas (owner,
  2026-11-06); nothing in the repository sets or records this. At current traffic it normally runs one
  replica. The owner accepts the duplicated scheduled work that scale-out brings
  ([ADR-0026](ADR-0026-background-work-runs-inside-the-api-process.md)) until the outbox
  ([PRD-0019](../Future/PRD-0019-email-outbox-durable-dispatch.md)) removes the duplicate for email,
  rather than pinning the maximum to 1.

### Known scale-out risks

Each of these is accepted for now and owned by the PRD named, which records it as a risk to resolve.
They apply whenever more than one API replica runs, including a revision overlap during a deployment.
The risks to background work are in ADR-0026, "Known scale-out risks".

- **Rate limits multiply.** The HTTP rate limiter keeps its counters in each process's memory, so a
  caller whose requests are spread across N replicas gets up to N times the configured limit. This
  matters most for the one-time code endpoints, where the rate limit is the main brake on guessing.
  Owner: [PRD-0008](../Future/PRD-0008-authentication-abuse-protection-and-lockout-policy.md).
- **Concurrent migration runs.** Every replica runs the migration runner when it starts
  ([ADR-0009](ADR-0009-migrations-run-in-a-separate-process-before-the-api-starts.md)), so replicas that
  start together each try to apply a pending migration. Owner:
  [PRD-0034](../Future/PRD-0034-ci-cd-pipeline-and-delivery-policies.md).
