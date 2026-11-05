---
id: ADR-0036
title: Production runs two container apps: nginx serves the SPA and the API is separate
status: accepted
date: 2025-11-07
area: infrastructure
---

# ADR-0036 (backfilled): Production runs two container apps: nginx serves the SPA and the API is separate

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
work.

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
  load-bearing rather than incidental.
- Bad, because two deployments and two ingress configurations must be kept in step.

### Confirmation

- Review: the client image is built with the API base URL for its environment, so a client built for one
  environment must not be deployed to another.
- Test: the production-like end-to-end configuration runs the built client against the API, which is the
  arrangement that catches a base-URL mistake.
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

- Where TLS terminates, and what the container receives, is a separate decision.
- The CORS policy that makes the cross-origin boundary work is a separate decision.
