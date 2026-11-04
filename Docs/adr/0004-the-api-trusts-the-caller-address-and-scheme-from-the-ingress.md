---
id: ADR-0004
title: The API trusts the caller address and scheme from the ingress
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0004: The API trusts the caller address and scheme from the ingress

**Summary:** In the context of an API whose only route is an ingress that terminates TLS and rewrites
the caller's scheme and address, facing the need for a correct `201 Location`, correct rate-limit
partitions and a meaningful caller address, we decided to honour `X-Forwarded-For` and
`X-Forwarded-Proto` from any sender with a hop limit of one and to ignore `X-Forwarded-Host`, to
achieve correct values without enumerating an address the platform does not fix, accepting that the
container must never be reachable except through the ingress.

## Context and Problem Statement

This is a backfilled record, written on 2026-11-04 from the pipeline, the tests and the owner's
account. The values have been in place since the deployment and were reviewed rather than newly
chosen.

The ingress terminates TLS and forwards plain HTTP to the container (ADR-0003). Without processing
the headers it writes, two things are wrong and wrong quietly:

- `Request.Scheme` is `http`, so the `Location` header on every `201 Created` is an `http://` URL that
  the client then has to correct.
- `Connection.RemoteIpAddress` is the ingress, so every anonymous caller shares one rate-limit
  partition, and every sign-in records the ingress as the session's address.

The framework defaults trust loopback only. The ingress is not at a fixed, enumerable address, so
those defaults read the headers and then discard them, which looks exactly like not configuring
anything.

## Decision Drivers

- The public scheme and caller address are only knowable from the proxy, never from the connection.
- The API must not be able to be told what host it is serving.
- A caller must not be able to choose its own address by writing the header.

## Considered Options

1. Honour `X-Forwarded-For` and `X-Forwarded-Proto` from any sender, with a hop limit of one.
2. Set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` and let the hosting layer configure the middleware.
3. Keep the framework default and trust loopback proxies only.
4. Honour `X-Forwarded-Host` as well, so the proxy can supply the host.

## Decision Outcome

Chosen option: 1. `ForwardedHeadersOptions` is configured in code with
`XForwardedFor | XForwardedProto`, `KnownProxies` and `KnownIPNetworks` cleared, and `ForwardLimit`
left at its default of one. `UseForwardedHeaders()` runs first in the pipeline.

Owner's account: trusting any sender was deliberate. The ingress address cannot be enumerated, the
container is unreachable except through the ingress, and the limit of one means only the entry the
ingress wrote is used.

### Consequences

- Good, because `201 Created` returns a URL the caller can use, and rate limiting partitions by real
  caller rather than by one proxy address.
- Good, because the trust decision is in one file, testable and commented, rather than spread across
  deployment configuration.
- Bad, because the safety argument is environmental rather than structural: exposing the container
  directly would let a caller choose its own address and scheme.
- Bad, because another proxy in front of the ingress would become the address the API sees, so this
  must be revisited if one is ever added.
- Neutral, because the local compose proxy (`nginx.conf`) sets neither header, so local calls through
  it present the client container's address and always report `http`. That is correct for a local
  stack and needs no change.

### Confirmation

- Test: `ForwardedHeadersOptionsSetupFixture` asserts both headers are enabled, `X-Forwarded-Host` is
  not, `KnownProxies` is empty and `ForwardLimit` is 1.
- Test: `ForwardedHeadersFixture` drives a real request through the pipeline and asserts the effect of
  each header, so the unit test cannot pass while the middleware is unused.
- Review: a second proxy, a CDN, or any published port on the API container invalidates this record.

## Pros and Cons of the Options

### Forwarded headers from any sender, limit one (chosen)

- Good: correct scheme and address with nothing to maintain as the platform reassigns addresses.
- Bad: trusts the network boundary instead of an address list.

### Enable the framework flag

- Good: no code, and it sets the same header set with the same cleared proxy lists.
- Bad: it does not express which headers are wanted or why the host is excluded, and it registers a
  second middleware instance at the start of the pipeline, ahead of the ordering the application
  relies on. It is also invisible in code review, which is where a trust decision should be visible.

### Loopback-only defaults

- Good: no trust granted at all.
- Bad: the ingress is not loopback, so the headers are silently discarded and every consequence above
  returns.

### Also honour `X-Forwarded-Host`

- Good: the API would know its public hostname without configuring it.
- Bad: the ingress passes through the `Host` header it received, so honouring the forwarded header
  would let a caller choose the host the API believes it serves, for no benefit this application uses.

## More Information

- The transport boundary this depends on is [ADR-0003](0003-public-tls-terminates-at-the-container-apps-ingress.md).
- Tracked description of the deployed behaviour: `Source/Docker/DEVELOPER.md`, "How Azure differs
  from local"; `Source/Server/DEVELOPER.md`, "Forwarded Headers".
