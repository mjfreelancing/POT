---
id: ADR-0007
title: Each request resolves the caller's permissions from the database
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0007: Each request resolves the caller's permissions from the database

**Summary:** In the context of role-based authorization where an administrator can change a user's
role while that user is signed in, facing the choice between a token that carries the permissions and
a lookup that asks the database, we decided to resolve the caller's permissions from the database on
every authorized request, to achieve enforcement that follows the data with no reissue and no stale
window, accepting a database read in the authorization path.

## Context and Problem Statement

Policies are named `resource:action` — `accounts:create`, `users:manage`, and so on — and each
endpoint declares the policy it needs. The policy itself carries no permissions; it carries a
requirement, and the handler resolves the caller's permission set and checks membership.

The access token carries identity and session state (`sub`, the token version and the session
identifier) but no permissions. Role and permission data lives in the database and can be changed by
an administrator at any time.

The resolution happens during authorization, before the user-context middleware has established which
site the caller is acting in, so it is necessarily cross-tenant: it answers "what may this user do
anywhere", not "what may this user do here". Site scoping is enforced later, by the data layer.

## Decision Drivers

- A role change or a revocation must take effect without waiting for a token to expire.
- Nothing permission-shaped should be readable from the token, because a token is readable by whoever
  holds it.
- The permissions the client uses to shape the interface must match the ones the server enforces.

## Considered Options

1. Resolve permissions from the database on each authorized request.
2. Put the permission set in the access token as claims.
3. Put the permission set in the token and shorten the token lifetime to bound staleness.
4. Resolve from the database once and cache the set per user for a short window.

## Decision Outcome

Chosen option: 1. `PermissionService` loads the caller's roles and their permissions, flattens them to
names, and adds the platform-administrator permission when the caller's identifier appears in the
deployment's platform-administrator configuration. The handler runs per requirement and succeeds only
when the resolved set contains the required permission; anything unresolved fails the requirement,
so the endpoint answers `403`.

The same service also supplies the caller's permission list to the profile response the client reads,
so the interface is shaped by the same answer that the server enforces.

### Consequences

- Good, because a role change, a permission edit or a revocation applies to the very next request; no
  token reissue and no expiry wait.
- Good, because the token stays identity-only, so a leaked token reveals no permission map and no role
  names.
- Good, because there is one source of truth: the interface and the enforcement cannot disagree.
- Bad, because every authorized request costs at least one database read of the caller's roles and
  their permissions, in a scope created in the handler.
- Bad, because the resolution cannot be site-scoped: it runs before the site is known, so it is
  deliberately cross-tenant and site confinement stays the data layer's job.
- Bad, because platform administration is decided from deployment configuration rather than from the
  same role data, so a permission answer can change only by redeploying.

### Confirmation

- Test: `AuthorizationMatrixFixture` exercises the endpoint-permission matrix, so a route that lost
  its permission check or gained the wrong one fails.
- Test: `EndpointAuthorizationMetadataFixture` requires every endpoint to be authorized or explicitly
  anonymous, so a route cannot silently escape the resolution by omitting its policy.
- Review: caching must not be added without recording the staleness window it introduces; the reason
  this design exists is that there is none.

## Pros and Cons of the Options

### Database lookup per request (chosen)

- Good: always current, and invisible in the token.
- Bad: a read on every authorized call, and a cross-tenant one at that.

### Permissions as token claims

- Good: no database read in the authorization path.
- Bad: a payload is readable, so the permission map is disclosed; and a role change has no effect until
  the token is reissued, which is exactly the behaviour this design rejects.

### Claims with a short token lifetime

- Good: staleness is bounded and explicit.
- Bad: bounds it by making the user refresh more often, and a revocation still waits for expiry; the
  cost moves to every client rather than to the requests that need the answer.

### Cached per user for a window

- Good: removes most of the cost.
- Bad: reintroduces a staleness window that must be reasoned about per request, for a database read
  that is small at this scale.

## More Information

- Platform administration is itself a separate decision: the permission is derived from deployment
  configuration, not from a tenant role.
- Whether the per-request read is worth caching is open; the trade-off to evaluate is permission
  freshness against request cost.
