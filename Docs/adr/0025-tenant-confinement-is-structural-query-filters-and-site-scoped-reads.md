---
id: ADR-0025
title: Tenant confinement is structural: query filters and site-scoped reads
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0025: Tenant confinement is structural: query filters and site-scoped reads

**Summary:** In the context of a shared database where every row belongs to a site, facing the risk that
a forgotten filter exposes another tenant's data, we decided to confine reads in two mechanisms — query
filters for financial data, and site-scoped repository methods for users and sites — with a foreign
identifier answered exactly as a missing one, to achieve isolation that a feature cannot forget rather
than one it must remember, accepting that the two mechanisms differ and must be understood separately.

## Context and Problem Statement

Every tenant's data lives in the same tables, distinguished by a site. Isolation therefore depends on
every query being confined, and the failure is silent: a query missing its filter returns another
tenant's rows and nothing complains.

One mechanism cannot cover everything. A global filter needs the current site to be known, which it is
not during authentication, and a filter on users or sites would have to exclude the very rows used to
resolve which site the caller is in.

## Decision Drivers

- Isolation must not depend on each new query remembering a filter.
- Some lookups happen before a site is known, so those must be constrained differently.
- A caller probing for another tenant's row must not learn that it exists.

## Considered Options

1. Query filters for financial data, plus site-scoped repository methods for users and sites.
2. A query filter on every entity, driven by the current site.
3. An explicit site filter written in every query.
4. Separate schemas or databases per tenant.

## Decision Outcome

Chosen option: 1. Accounts, expenses, incomes and settings are filtered globally by the current site, so
a query that does not mention tenancy is still confined. Users and sites are reached only through
repository methods that apply the current site themselves — the caller cannot pass one — and the raw
queryables for those two are not exposed. A small set of deliberately cross-tenant user reads exists and
is named as such, which is how authentication and recovery work before a site is known.

A lookup that targets another tenant's row answers exactly as a missing one: `404`, with a body identical
to the response for an id that does not exist, so the answer does not confirm the row's existence.

### Consequences

- Good, because a new query over financial data is confined by default; forgetting tenancy does not leak.
- Good, because the two exception types are handled where they can be handled: users and sites are read
  before a site is known, so their confinement belongs to the lookup method rather than to a filter.
- Good, because a cross-tenant probe learns nothing: both "not yours" and "does not exist" are the same
  answer.
- Bad, because there are two mechanisms, and a reader must know which entities use which.
- Bad, because a filter applies to every query, including administrative ones, so a cross-tenant read
  has to be written deliberately and is visible as such in the code.
- Bad, because the site a scoped read confines itself to comes from ambient state, so a read outside a
  site's scope fails rather than returning everything.
- Bad, because an operation acting on many rows must still reason about the site, since the filter
  decides what it can see rather than what it may change.

### Confirmation

- Test: `CrossTenantAuthorizationFixture` addresses another tenant's records across the endpoint set and
  asserts `404` with a body byte-identical to the missing-id response, so the answer leaks neither the
  row's existence nor its absence.
- Test: `EndpointAuthorizationMetadataFixture` keeps every endpoint in the authorization inventory, so a
  new route is covered by the same sweeps.
- Review: exposing the user or site queryable, or writing a cross-tenant read without a named method, is
  a defect.

## Pros and Cons of the Options

### Filters plus scoped methods (chosen)

- Good: default confinement where it can be applied, explicit confinement where it cannot.
- Bad: two mechanisms to learn.

### A filter on everything

- Good: one rule to read.
- Bad: cannot work before the site is known, and would filter out the rows used to resolve it.

### Explicit filter in every query

- Good: visible at every call site.
- Bad: silence when forgotten, which is the failure this decision exists to remove.

### Separate schema or database per tenant

- Good: the strongest isolation, at the storage level.
- Bad: migration and connection management per tenant for an application whose tenants are small and
  few, and cross-tenant administration becomes a different kind of operation.

## More Information

- Which authority a signed-in caller has anywhere is a separate decision; this one is about which rows a
  query can see.
- Queries over financial data were the ones that could be filtered safely; the two identity entities
  were not, and the reason is recorded in the decision above.
