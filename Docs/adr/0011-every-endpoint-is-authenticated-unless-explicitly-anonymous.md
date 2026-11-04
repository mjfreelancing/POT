---
id: ADR-0011
title: Every endpoint is authenticated unless it is explicitly anonymous
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0011: Every endpoint is authenticated unless it is explicitly anonymous

**Summary:** In the context of an API where a missing authorization annotation leaves a route open, and
where an omission is invisible in review, we decided that the fallback policy requires an
authenticated caller so anonymity is an explicit opt-out, to achieve a default that fails closed,
accepting that a forgotten annotation now breaks a public route instead of exposing a protected one,
and that a test must keep the two in step.

## Context and Problem Statement

Authorization in ASP.NET Core is applied per endpoint: a route with no policy and no fallback is
reachable by anyone. That is a safe default for a public site and an unsafe one for an API whose every
route but sign-in, sign-up, refresh and the health probes expects a caller.

The failure is silent in both directions. A new protected endpoint that forgets its policy is
immediately public, and nothing in a normal review catches it.

## Decision Drivers

- The consequence of forgetting should be a broken feature, not an exposed one.
- Routes that must stay public before a session exists are a known, small, enumerable set.
- The rule must survive a new endpoint being added by someone who has not read this record.

## Considered Options

1. A fallback policy that requires authentication, with `AllowAnonymous()` as the explicit opt-out.
2. Policies only on endpoints that declare them, and nothing else.
3. A route-group convention that applies authentication to every group at registration.
4. A middleware that rejects any request without an authenticated user except for a configured path
   list.

## Decision Outcome

Chosen option: 1. The authorization fallback requires an authenticated user, so a route is protected
unless it explicitly opts out. The endpoints reachable without a session — sign-in, sign-up, password
reset, refresh, and the health probes — carry `AllowAnonymous()` explicitly.

A test enumerates the application's endpoints and requires each one to be either authorized or
explicitly anonymous, so a new route cannot arrive without that decision having been made.

### Consequences

- Good, because the default is the safe one, and anonymity is visible in the code at the route that
  needs it.
- Good, because the endpoint inventory test turns the rule into something that fails a build, rather
  than something a reviewer must remember.
- Bad, because the public routes now depend on that annotation, so removing one accidentally locks
  users out rather than exposing data.
- Bad, because the fallback is authentication only: it says the caller is signed in, not that they may
  do this, so every protected route still needs its permission policy.
- Bad, because the endpoint inventory must be maintained as the list of anonymous routes changes.

### Confirmation

- Test: `EndpointAuthorizationMetadataFixture` requires every endpoint to be authorized or explicitly
  anonymous, and fails when a new route is neither.
- Review: `AllowAnonymous()` on a route that reads or writes tenant data is a defect, whatever the
  fallback says.

## Pros and Cons of the Options

### Fail-closed fallback (chosen)

- Good: forgetting a policy denies access instead of granting it.
- Bad: forgetting `AllowAnonymous()` breaks a public route, which is noticed immediately by users
  rather than silently by an attacker.

### Per-endpoint policies only

- Good: nothing global to reason about.
- Bad: exactly the failure this record exists to prevent — silence is indistinguishable from a
  deliberate public route.

### Group convention at registration

- Good: applies consistently to whatever is registered through the helper.
- Bad: only covers endpoints registered that way, so a route added outside the convention is invisible
  to it.

### Path-list middleware

- Good: one place to read the public routes.
- Bad: duplicates what endpoint metadata already knows, and drifts from the routes as they are added.

## More Information

- This decides authentication, not authority: which permissions a signed-in caller needs is recorded
  separately.
- The health probes are anonymous for the same reason sign-in is: infrastructure holds no credentials.
