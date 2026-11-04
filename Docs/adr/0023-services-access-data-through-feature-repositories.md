---
id: ADR-0023
title: Services read and write through feature repositories, never through the context
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0023: Services read and write through feature repositories, never through the context

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
that change state, so a service that only reads is given a type that cannot write. Writes opt into
tracking explicitly; reads do not track.

Repositories return entities or projections. They do not contain business rules, and they do not return
queryables that a caller could keep composing.

### Consequences

- Good, because a service test substitutes the interface, so business rules are tested without a
  database or a container.
- Good, because the query lives in a named method, so the same read is not written twice with different
  filters.
- Good, because the split interfaces make a read-only path unable to write, which is the interface
  segregation principle applied where it prevents a real mistake.
- Good, because untracked reads avoid change tracking that is discarded unread.
- Bad, because each new query shape is a new method on an interface and its implementation, which is more
  ceremony than a lambda over the context.
- Bad, because returning entities keeps some persistence shape in the service, and returning projections
  adds a type per read.
- Bad, because a caller cannot refine a query after the fact — the repository must anticipate the shapes
  it needs, and one that guesses wrong is extended.

### Confirmation

- Test: the application-layer fixtures substitute the interfaces, so a repository dependency reaching for
  the database would fail the test rather than pass.
- Review: a service that resolves the context, or a repository method that returns `IQueryable`, is a
  defect.
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

- Tenant confinement by query filters and site-scoped reads builds on this access model and is a separate
  decision.
- Explicit transactions are used where several repository calls must be one unit, which is a separate
  decision.
