---
id: ADR-0020
title: The server is layered by responsibility with feature slices mirrored across the layers
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0020: The server is layered by responsibility with feature slices mirrored across the layers

**Summary:** In the context of a server that must keep HTTP concerns, business rules and persistence
separable while remaining navigable, facing the choice between technical grouping and feature grouping,
we decided on a host, an application and a data project with support projects beside them, and feature
folders inside the layers that carry features, to achieve one direction of dependency and a predictable
place to look, accepting that features appear in some layers and not others.

## Context and Problem Statement

Three responsibilities have different reasons to change: a route's shape, a business rule, and the way
data is stored. Mixing them in one project means a rule cannot be tested without a host, and a query
cannot change without touching HTTP code.

Within each layer, grouping by technical type — all services, all repositories — makes a change to one
feature a search across the codebase. Grouping by feature instead means the same name identifies the
feature everywhere.

## Decision Drivers

- Business rules must be testable without a web host or a database.
- Dependencies must point one way, so a lower layer cannot reach a higher one.
- A new feature must have an obvious place in every layer.

## Considered Options

1. Four projects by responsibility, with feature folders mirrored in each.
2. A single project with folders by responsibility.
3. Vertical slices: one project per feature, each holding its routes, rules and persistence.
4. Layers grouped by technical type within the project.

## Decision Outcome

Chosen option: 1. The host project holds the HTTP pipeline, endpoints, middleware and authentication; the
application project holds orchestration, validation and entity checks; the data project holds entities,
configuration, repositories and migrations; and support projects hold what both sides of a boundary need —
shared types, the email sender and its Razor components, the migration runner and test utilities.
Dependencies point host → application → data, and shared is reached through data rather than referenced
directly by the application.

Features are named in the host and the application, and the data project's repositories are grouped by
feature; entities, specifications and configuration are grouped by technical concern. Authentication lives
in the host because everything it touches — tokens, cookies, claims, the pipeline — is transport, but it is
not purely transport: the host also owns the login, session and token-version rules while the application
layer holds the account workflows around them.

### Consequences

- Good, because application code can be tested with substituted repositories without a web host, and data
  code can be tested against a database — an in-memory provider for the repository rules, and the real
  engine in the hosted suite.
- Good, because a feature's name identifies it in the layers that carry features, so navigation is
  predictable.
- Good, because the dependency direction is enforced by project references rather than by convention.
- Bad, because one feature's change touches several projects, and a review sees it in pieces.
- Bad, because the boundary invites a type to be copied per layer — a request model, an input model, an
  output model, an entity — where a single type would have been shorter.
- Bad, because the host is not purely transport: it also owns authentication rules, which is a deliberate
  exception rather than a clean separation.

### Confirmation

- Review: a project reference that points backwards is a defect, because the compiler alone will not
  prevent it appearing.
- Test: the unit projects are per layer, so a test that needs the host is evidence the rule leaked into
  the wrong layer.

## Pros and Cons of the Options

### Layered with mirrored features (chosen)

- Good: testable layers, and predictable placement.
- Bad: each change spans projects.

### One project, folders by responsibility

- Good: nothing to wire, and no cross-project churn.
- Bad: no compiler-enforced boundary, and layers drift into each other.

### Vertical slices per feature

- Good: one folder per feature, end to end.
- Bad: shared persistence concerns — context, configuration, migrations, tenancy — end up either
  duplicated or in a slice that everything references.

### Technical grouping inside a project

- Good: familiar to anyone coming from a layered tutorial.
- Bad: every feature change is a scatter across sibling folders.

## More Information

- The operation-per-slice endpoint shape inside the host is a separate decision.
- Repository and transaction conventions live in the data layer and are recorded separately.
