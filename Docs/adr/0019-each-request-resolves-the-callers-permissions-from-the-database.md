---
id: ADR-0019
title: Each request resolves the caller's permissions from the database
status: accepted
date: 2025-09-12
area: backend
related-prds: [PRD-0026]
---

# ADR-0019 (backfilled): Each request resolves the caller's permissions from the database

**Summary:** In the context of role-based authorization where an administrator can change a user's
role while that user is signed in, facing the choice between a token that carries the permissions and
a lookup that asks the database, we decided to resolve the caller's permissions from the database on
every authorized request, to achieve enforcement that follows the data with no reissue and no stale
window, accepting a database read in the authorization path.

## Context and Problem Statement

Policies are named after `resource:action` permissions — `account:view`, `account:manage`,
`user:manage`, `platform:manage` — and each endpoint declares the policy it needs. The policy itself
carries no permissions; it carries a requirement, and the handler resolves the caller's permission set and
checks membership.

The access token carries identity and session state — `sub`, `jti`, the token version and the session
identifier — but no permissions. Role and permission data lives in the database and can be changed by an
administrator at any time.

The resolution happens during authorization, before the user-context middleware has established which
site the caller is acting in, so it is necessarily cross-tenant: it answers "what may this user do
anywhere", not "what may this user do here". Site scoping is enforced later, by the data layer.
[PRD-0026](../Future/PRD-0026-cross-tenant-authorization-hardening.md) §4.6 records why permission and
site claims are kept out of the token: a JWT payload is encoded, not encrypted, so a claim is readable by
anyone holding the token; and a claim goes stale while the token stays valid.

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

Chosen option: 1, because a permission carried as a claim would be readable by whoever holds the token
and would stay in force until the token expired. `PermissionService` loads the caller's roles and their
permissions — reference data, not
site data — flattens them to names, and adds the platform-administrator permission when the caller's
identifier appears in the deployment's platform-administrator configuration. The handler runs per
requirement and succeeds only when the resolved set contains the required permission. A caller whose
permissions do not include it is refused with `403`; a caller with no valid session is challenged with
`401` before the permission is even considered.

The same service also supplies the caller's permission list to the profile response the client reads,
so the interface is shaped by the same answer that the server enforces.

### Consequences

- Good, because a role change, a permission edit or a revocation applies to the very next request; no
  token reissue and no expiry wait.
- Good, because the token stays identity-only, so a leaked token reveals no permission map and no role
  names.
- Good, because there is one source of truth: the interface and the enforcement cannot disagree.
- Bad, because the read is repeated per requirement, not per request: the handler resolves the set
  afresh, in a scope it creates, for each permission an endpoint declares, so the accruals route, which
  declares three, pays three reads on top of the session check.
- Bad, because the resolution cannot be site-scoped: it runs before the site is known, so it is
  deliberately cross-tenant and site confinement stays the data layer's job.
- Bad, because platform administration is decided from deployment configuration rather than from the
  same role data, so that grant changes only when the configuration changes and the API restarts: the
  options are bound once into a singleton.
- Bad, because a policy name is not checked against the permissions that exist: the policy provider turns
  any name it does not recognise into a requirement, so a misspelt name builds, is held by no one and
  answers `403` to every caller. Whether to close that is open (`DECISIONS-TO-DISCUSS.md`, DD-09).

### Confirmation

- Test: `AuthorizationMatrixFixture` exercises the endpoint-permission matrix from each endpoint's own
  declaration, so a route that answers a caller without the permission fails. Because the expectation is
  derived from the declaration, a deliberately changed permission changes the expectation with it and still
  passes.
- Test: `EndpointAuthorizationMetadataFixture` requires every endpoint to be authorized or explicitly
  anonymous and pins the expected policy for the routes it names, so a new route cannot silently join the
  set of unprotected ones.
- Gap: no fixture checks the resolution itself. The matrix takes each caller's expected permissions from
  `PermissionService`, so it proves enforcement against the resolved set, not the flattening; its callers
  are a tenant `Admin` and a `Viewer`, so it proves the platform routes refuse them, while the
  configuration-added grant is exercised only by fixtures that start `PlatformAdminApiWebApplicationFactory`
  (for example the approvals `UpdateStatusFixture`).
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

- Platform administration is itself a separate decision, recorded as
  [ADR-0032](0032-platform-administration-is-granted-by-deployment-configuration.md).
- The token and its revocation are [ADR-0020](0020-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md)
  and [ADR-0033](0033-access-tokens-are-rejected-once-their-session-or-user-version-changes.md).
- Whether the per-request read is worth caching is open; the trade-off to evaluate is permission
  freshness against request cost.
