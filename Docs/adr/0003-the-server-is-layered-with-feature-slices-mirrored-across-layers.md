---
id: ADR-0003
title: The server is layered by responsibility with feature slices mirrored across the layers
status: accepted
date: 2024-12-28
area: backend
---

# ADR-0003 (backfilled): The server is layered by responsibility with feature slices mirrored across the layers

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
Separating HTTP, business logic and data follows experience on past projects. Feature folders are
mirrored across the layers to separate cross-cutting concerns from features and to make code predictable
to find. The record does not show whether the other options below were weighed.

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
application project holds orchestration, entity checks and calculations, while request-shape validators sit
beside each endpoint in the host; the data project holds entities,
configuration, repositories and migrations; and support projects hold what both sides of a boundary need —
shared types, the email sender and its Razor components, the migration runner and test utilities.
Dependencies point host → application → data → shared. The host also references the data project
directly, and its authentication services read users, sessions and permissions through repositories
without passing through the application. The application reaches shared only through data and the email
sender, never by a direct reference.

Features are named in the host and the application, and the data project's repositories are grouped by
feature; entities, specifications and configuration are grouped by technical concern. Authentication lives
in the host, beside the tokens, cookies, claims and pipeline it works with; the record does not show why it
was placed there rather than in the application. It is not purely transport: the host also owns the login, session and token-version rules while the application
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
- Bad, because the host is not purely transport: it also owns authentication rules and reads data directly
  for them, an exception to a clean separation.

### Confirmation

- Build: a reference from a lower project to a higher one closes a cycle, which the build refuses.
- Review: a new project reference that skips a layer, or that points a support project at a layer, is a
  defect the build does not catch; the host's direct reference to the data project already exists and is
  used by authentication.
- Instruction: `.github/instructions/server.instructions.md` (Architecture) states the layering direction.
- Review: the unit projects are per layer, so a unit test that needs the host is evidence the rule leaked
  into the wrong layer.

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
