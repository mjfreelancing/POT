---
id: ADR-0025
title: Tenant confinement is structural: query filters and site-scoped reads
status: accepted
date: 2025-10-26
area: backend
related-prds: [PRD-026]
---

# ADR-0025 (backfilled): Tenant confinement is structural: query filters and site-scoped reads

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

The site-scoped repository methods for users and sites, and the refusal contract, come from
[PRD-026](../Future/PRD-026-cross-tenant-authorization-hardening.md) (§§4.1, 4.2). That PRD also
considered a filter on `User` and `Site` and rejected it (§4.7). The four entity filters predate the PRD;
beyond what §4.7 says, the record does not show why they were chosen, or whether options 3 and 4 below
were weighed.

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
queryables for those two are not exposed. A small set of deliberately cross-tenant user reads exists, and
this is how authentication and recovery work before a site is known. Each read is marked in the
repository with a `Cross-tenant` comment and listed in PRD-026 §4.5. Some names say so, such as
`GetEnabledUsersForAllSitesAsync`, but `GetByUsernameAsync` and `GetSecurityStateAsync` do not.

A lookup that targets another tenant's row answers exactly as a missing one: `404`, with a body identical
to the response for an id that does not exist, so the answer does not confirm the row's existence.

### Consequences

- Good, because a new query over financial data is confined by default; forgetting tenancy does not leak.
- Good, because the two exception types are handled where they can be handled: users and sites are read
  before a site is known, so their confinement belongs to the lookup method rather than to a filter.
- Good, because a cross-tenant probe learns nothing: both "not yours" and "does not exist" are the same
  answer.
- Bad, because there are two mechanisms, and a reader must know which entities use which.- Bad, because the user and site confinement holds only for code that goes through the repositories.
  `PotDbContext` is public and exposes `Users`, so code that injects the context can still write an
  unconfined lookup (PRD-026 §4.1).
- Bad, because a filter applies to every query, including administrative ones, so a cross-tenant read
  has to be written deliberately and is visible as such in the code.
- Bad, because the site a scoped read confines itself to comes from ambient state, so a read outside a
  site's scope fails rather than returning everything.
- Bad, because an operation acting on many rows must still reason about the site, since the filter
  decides what it can see rather than what it may change.

### Confirmation

- Test: `AuthorizationMatrixFixture` generates a case naming another tenant's row for every documented
  operation whose path carries an id. It expects `404`, or, for an operation with a body, no success and
  no `401` or `403`. A path naming an unclassified resource fails the coverage check, so a new route is
  swept too. `Should_Answer_A_Foreign_Tenants_Row_As_Not_Found` covers the filtered account, expense and
  income reads.
- Test: `CrossTenantAuthorizationFixture` covers the user and site routes.
  `Should_Report_A_Foreign_Target_Indistinguishably_From_A_Missing_One` asserts that a foreign user and a
  missing user get the same status, content type and body once per-request identifiers are normalised.
- Test: `IsolationFixture` fingerprints a canary tenant before and after each destructive action, and
  fails if any of its rows changed.
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

- Which authority a signed-in caller has anywhere is a separate decision,
  [ADR-0007](0007-each-request-resolves-the-callers-permissions-from-the-database.md). This one is about
  which rows a query can see.
- Queries over financial data were the ones that could be filtered safely; the two identity entities
  were not, and the reason is recorded in the decision above.
