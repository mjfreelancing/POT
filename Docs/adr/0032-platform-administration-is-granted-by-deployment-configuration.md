---
id: ADR-0032
title: Platform administration is granted by deployment configuration, not by a role
status: accepted
date: 2025-11-09
area: backend
---

# ADR-0032 (backfilled): Platform administration is granted by deployment configuration, not by a role

**Summary:** In the context of a multi-tenant product operated by its author, facing the need for an
authority above every tenant (today it approves sign-ups from every site), we
decided to derive platform administration from a configured list of user identifiers rather than from
a database role, to achieve an authority no tenant can grant or take away, accepting that it changes
only by redeploying.

## Context and Problem Statement

Ordinary authority is site-scoped: an administrator manages their own site, through roles and
permissions that the tenant's own data holds. That model cannot answer two cases:

- A site with no usable administrator, which nobody inside the site can repair.
- An operation that spans tenants, which by definition is not a tenant's business.
Today `platform:manage` guards only the approvals routes: listing sign-ups awaiting approval in every
site and deciding them. Repairing a site with no usable administrator, and the other cross-tenant
operations, are planned in PRD-0027 and not delivered.

If platform authority were a role, whoever can administer roles in a site could grant it, and the
tenant would be able to create an authority above itself.

## Decision Drivers

- No tenant must be able to grant or revoke platform authority.
- The authority must survive any state a tenant's data can get into, including a site with no
  administrator.
- The set of platform administrators is small, stable and known to the operator.

## Considered Options

1. Derive the platform authority from a list of user identifiers in deployment configuration.
2. Model it as a role that exists only outside any site.
3. Model it as a role in every site, held by the operator's user.
4. Infer it from the identity provider, such as an external group membership.

## Decision Outcome

Chosen option: 1. The configuration holds a comma-separated list of user identifiers in
`PlatformAdmin:UserIds`, each one a user's `RowId`. When the resolved permission set is built, a user
whose identifier appears in that list also receives the platform permission, on top of whatever their
roles give them. Everything else about them is an ordinary user: their site, their roles and their data
are unchanged, apart from guards that keep at least one enabled platform administrator: the last one
cannot be disabled or deleted by another user, cannot delete their own account, and the site that holds
them cannot be deleted. A platform administrator who is not the last enabled one has no protection.

The authority is kept out of tenant data because platform administration belongs to the product's
operator, and a tenant is a consumer of the product.

The list is validated on start, so a malformed entry fails the deployment rather than producing an
administrator who silently is not one. An empty list is accepted, which means a deployment can run with
no platform administrator at all; a well-formed identifier that names no user also passes validation.

### Consequences

- Good, because no tenant can create, grant or remove platform authority; it is not in their data.
- Good, because it holds whatever state a site's roles are in, so the cross-tenant recovery PRD-0027 plans
  has an authority to rest on; no recovery operation exists yet.
- Good, because the list is small, explicit and reviewable in one place, and is visible to whoever
  can read the deployment configuration.
- Bad, because granting or removing platform authority is a deployment change rather than an
  administrative action.
- Bad, because a syntactically valid identifier that names nobody, or an empty list, passes validation:
  the deployment starts and has no platform administrator.
- Bad, because the configuration holds user identifiers that exist only once the account does: the first
  platform administrator signs up, reads their `RowId` from the database, adds it to configuration and
  restarts the API, and, because sign-up waits for a platform administrator's approval, enables their
  own account directly in the database (`Docs/FIRST-TIME-SETUP.md`, step 4).
- Bad, because the authority is not auditable through the application's own role history.

### Confirmation

- Test: `OptionsStartupValidationFixture` asserts a malformed list fails the start, and
  `AuthorizationMatrixFixture` asserts that an Admin or Viewer without the permission is refused. The grant
  side is exercised through `PlatformAdminApiWebApplicationFactory`, which configures the list for
  `Approvals/UpdateStatusFixture`, `DeleteSiteFixture`, `DeleteUserFixture`, `DeleteAccountFixture` and
  `CrossTenantAuthorizationFixture`.
- Gap: no unit fixture covers `PermissionService` adding the permission on top of role permissions, or
  `PlatformAdminOptions` parsing the list.
- Review: a platform authority granted anywhere other than this list is a defect, and a tenant-scoped
  endpoint must never depend on it for its ordinary behaviour.

## Pros and Cons of the Options

### Configuration list (chosen)

- Good: outside tenant reach, and independent of site state.
- Bad: a deployment to change, and identifiers rather than names.

### A role outside any site

- Good: administrable through the product.
- Bad: needs its own storage and administration surface, and would be editable by whatever authority
  manages roles unless that is carefully separated.

### A role in every site

- Good: reuses existing machinery.
- Bad: makes every site's administrator able to grant platform authority, which is the failure mode
  this decision exists to prevent.

### Identity-provider group

- Good: centralised and auditable outside the application.
- Bad: introduces an external dependency and a second identity source for an application that has no
  other need for one.

## More Information

- Platform authority is added to the caller's ordinary permissions on each request, which is the
  permission-resolution decision recorded in
  [ADR-0019](0019-each-request-resolves-the-callers-permissions-from-the-database.md).
- The cross-tenant operations this authority is meant to carry (deleting a user or site in another
  tenant, and administrator recovery) are specified in
  [PRD-0027](../Future/PRD-0027-platform-administration.md), which is not delivered.
