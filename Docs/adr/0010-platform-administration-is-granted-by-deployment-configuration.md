---
id: ADR-0010
title: Platform administration is granted by deployment configuration, not by a role
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0010: Platform administration is granted by deployment configuration, not by a role

**Summary:** In the context of a multi-tenant product operated by its author, facing the need for an
authority that can act across tenants and recover a site whose administrators are locked out, we
decided to derive platform administration from a configured list of user identifiers rather than from
a database role, to achieve an authority no tenant can grant or take away, accepting that it changes
only by redeploying.

## Context and Problem Statement

Ordinary authority is site-scoped: an administrator manages their own site, through roles and
permissions that the tenant's own data holds. That model cannot answer two cases:

- A site with no usable administrator, which nobody inside the site can repair.
- An operation that spans tenants, which by definition is not a tenant's business.

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

Chosen option: 1. The configuration holds a comma-separated list of user identifiers. When the
resolved permission set is built, a user whose identifier appears in that list also receives the
platform permission, on top of whatever their roles give them. Everything else about them is an
ordinary user: their site, their roles and their data are unchanged.

Configuration is validated at startup, so a malformed list fails the deployment rather than producing
an administrator who is silently not one.

### Consequences

- Good, because no tenant can create, grant or remove platform authority; it is not in their data.
- Good, because it works whatever state a site's roles are in, which is the recovery case it exists
  for.
- Good, because the list is small, explicit and reviewable in one place, and is visible to whoever
  can read the deployment configuration.
- Bad, because granting or removing platform authority is a deployment change rather than an
  administrative action.
- Bad, because the configuration holds user identifiers, so the operator must know them before the
  first sign-in; the identifier is discovered from the database after the account exists.
- Bad, because the authority is not auditable through the application's own role history.

### Confirmation

- Test: `AuthorizationMatrixFixture` and the platform-scoped endpoints show that the permission is
  required and that it is satisfied only for configured users.
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
  permission-resolution decision recorded separately.
