---
id: ADR-0011
title: Services read and write through feature repositories, never through the context
status: accepted
date: 2025-01-01
area: backend
---

# ADR-0011 (backfilled): Services read and write through feature repositories, never through the context

**Summary:** In the context of application services that need data but must be testable without a
database, facing the choice between injecting the persistence context and injecting a named repository,
we decided that every read and write goes through a feature-specific repository interface split into
read-only and persistable capabilities, with reads untracked by default, to achieve named data access the
service code reads as intent and a persistence layer that can be substituted in tests, accepting a
repository method per query shape and some ceremony for the tracking opt-in.

## Context and Problem Statement

A service that holds the context can express any query it likes, against any table, at any time. That is
convenient, and it means the database is in every service, every test of a service needs a database, and
the same query is written three slightly different ways.

A repository reverses that: the service names what it wants, and the repository decides how to get it.
The cost is that a new query shape is a new method.

Named repository methods make service code easier to read and let a service test mock the database
calls. The split between read-only and persistable interfaces follows the Interface Segregation
Principle, and reads do not track, for performance. The alternatives below record what each would have
cost; the record does not show whether they were weighed before the choice.

## Decision Drivers

- Application tests must run without a database, which means data access must be substitutable.
- A service should read as business steps, not as query composition.
- Reads dominate, and tracking every read is wasted work.
- A consumer that should not write must not be given the ability to.

## Considered Options

1. Feature-specific repository interfaces, split into read-only and persistable, with untracked reads by
   default.
2. Inject the context into services and query directly.
3. One generic repository over all entities.
4. No repository, and a query object per operation.

## Decision Outcome

Chosen option: 1. Each feature has repository interfaces — plural where the capabilities differ — naming
the queries the feature needs. Read interfaces expose only reads, persistable ones add the operations
that change state. Some entity repositories expose a single named queryable that services compose for
filters the repository does not anticipate; the tenant-scoped repositories for users and sites expose
none, because composing them would bypass the confinement they exist to enforce. Writes opt into
tracking explicitly; reads do not track.

### Consequences

- Good, because a service test substitutes the interface, so business rules are tested without a
  database or a container.
- Good, because the query lives in a named method, so the same read is not written twice with different
  filters.
- Good, because the split interfaces make every writing capability explicit, so a reader can see which
  paths may change data.
- Bad, because the split is not applied uniformly: several read-only services take a persistable
  interface, so the type does not prove they only read.
- Good, because untracked reads avoid change tracking that is discarded unread.
- Bad, because each new query shape is a new method on an interface and its implementation, which is more
  ceremony than a lambda over the context.
- Bad, because returning entities keeps some persistence shape in the service, and returning projections
  adds a type per read.
- Bad, because a caller of the user or site repository cannot refine a query after the fact. The
  repository must anticipate the shapes it needs, and one that guesses wrong is extended. The account,
  expense, income and role repositories avoid this by exposing a queryable that callers compose.

### Confirmation

- Test: most application-layer fixtures build services with substituted repository interfaces, so
  business rules run without a database. Four use the EF in-memory provider instead: the account
  description checks, the expense delete fixture and the projections fixture. Nothing fails if a service
  injects `PotDbContext`, which is public, so that part of the rule rests on review.
- Test: `DbContextExtensionsFixture` asserts that the context defaults to
  `NoTrackingWithIdentityResolution` and that nested tracking scopes restore it.
- Instruction: `.github/instructions/server.instructions.md` (Persistence: default reads to no-tracking;
  opt into tracking only on write paths).
- Review: a service that resolves the context, or a repository method on a tenant-scoped interface that
  returns `IQueryable`, is a defect.
- Review: a read that tracks is justified in the change, because tracking exists for a write that
  follows.

## Pros and Cons of the Options

### Split feature repositories (chosen)

- Good: substitutable, named, and constrained by interface.
- Bad: a method per shape, and more files per feature.

### Context in services

- Good: the least code, and any query is one lambda away.
- Bad: business tests need a database, and query shapes proliferate by convention rather than by design.

### One generic repository

- Good: one interface for everything.
- Bad: `GetAll` over an aggregate is a query the caller composes, which is the context again with extra
  steps, and tenant rules become the caller's responsibility.

### Query objects per operation

- Good: a query is an explicit, testable object.
- Bad: a second abstraction beside the services, for the same set of shapes.

## More Information

- Tenant confinement by query filters and site-scoped reads builds on this access model:
  [ADR-0030](0030-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md).
- Explicit transactions are used where several repository calls must be one unit:
  [ADR-0029](0029-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md).
