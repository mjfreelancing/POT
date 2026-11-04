---
id: ADR-0003
title: Public TLS terminates at the Container Apps ingress
status: accepted
date: 2026-11-04
area: infrastructure
---

# ADR-0003: Public TLS terminates at the Container Apps ingress

**Summary:** In the context of a single deployed API that is reached only through Azure Container Apps
ingress, facing the need for HTTPS on the public domains without managing certificates or public
hostnames inside the application, we decided that public TLS terminates at the ingress and the
container is served plain HTTP, to achieve managed certificates and one application configuration for
local and deployed runs, accepting one unencrypted hop inside the Azure environment whose safety
depends on the container never being reachable except through the ingress.

## Context and Problem Statement

This is a backfilled record, written on 2026-11-04 from the deployment record, the application
pipeline and the owner's account. The deployment is configured by hand, so the date the decision was
taken is not recorded.

The apex, www and api domains are served over HTTPS with Azure-managed (Let's Encrypt) certificates.
The API container app accepts traffic from anywhere through its ingress, with ingress type `HTTP`.
The PostgreSQL server requires SSL on its connections.

The application itself contains no HTTPS configuration. Kestrel listens on `http://+:5241`,
`UseHttpsRedirection()` is deliberately absent from the pipeline, and the local compose stack serves
plain HTTP on 5175 and 5241.

## Decision Drivers

- Certificate provisioning and renewal are platform concerns for a single-operator deployment.
- Local and deployed runs should differ as little as possible in application configuration.
- The application must not be able to learn its public hostname from a caller, so the host is not
  honoured from a forwarding header.

## Considered Options

1. Terminate public TLS at the ingress and serve the container over HTTP.
2. Encrypt the ingress-to-container hop as well, so no plain HTTP exists inside the environment.
3. Terminate TLS in Kestrel with certificate material mounted into the container.

## Decision Outcome

Chosen option: 1. Container Apps terminates TLS at the ingress with managed certificates and the
container receives plain HTTP. The application therefore neither redirects to HTTPS nor knows the
public scheme except through the header the ingress writes.

Owner's account: this was the platform default, accepted as it came; options 2 and 3 were not weighed
at the time. The consequences below are what follow from that choice, not a comparison that was made.

### Consequences

- Good, because certificate provisioning and renewal need no application code or secret handling.
- Good, because one HTTP-only application configuration serves both local and deployed runs.
- Bad, because one hop inside the Azure environment is unencrypted. That is accepted because the hop
  never leaves the environment's own network.
- Bad, because correctness depends on the ingress always being the only route to the container. The
  forwarded-header trust boundary exists for exactly that reason and is recorded separately.
- Bad, because a caller's scheme is whatever the ingress reports, so a misconfigured ingress silently
  changes it rather than failing loudly.

### Confirmation

- Review: an API container published anywhere other than through its ingress contradicts this record
  and is a security defect, not a configuration preference.
- Code check: `UseHttpsRedirection()` must stay out of the pipeline. Re-adding it makes the container
  redirect on its own scheme, which turns an HTTPS request into a plain-HTTP redirect.
- Source of the deployed facts: the private Azure deployment record, which is not in the repository.
  The tracked summary of this boundary is in `Source/Docker/DEVELOPER.md`, "How Azure differs from
  local".

## Pros and Cons of the Options

### Terminate at the ingress (chosen)

- Good: no certificate material, renewal job or public hostname anywhere in the application.
- Bad: relies on network isolation rather than encryption for the final hop.

### Encrypt the ingress-to-container hop

- Good: no plain HTTP exists inside the environment at all.
- Bad: more configuration to keep working, for a hop that is already confined to a private network.

### Terminate TLS in Kestrel

- Good: encryption from the ingress to the process.
- Bad: certificate material and its rotation move into the container, and the public hostname becomes
  application configuration, which is the thing the forwarded-host decision deliberately avoids.

## More Information

- The forwarded-header trust boundary this decision depends on — which headers are honoured, and why
  any sender is trusted — is a separate decision with its own record.
