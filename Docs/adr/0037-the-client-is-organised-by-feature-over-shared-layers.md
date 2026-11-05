---
id: ADR-0037
title: The client is organised by feature over shared API, concern and utility layers
status: accepted
date: 2025-03-15
area: frontend
---

# ADR-0037 (backfilled): The client is organised by feature over shared API, concern and utility layers

**Summary:** In the context of a single-page application whose screens are independent but share
transport, state and presentation helpers, facing the choice between grouping by feature or by technical
type, we decided on feature folders served by shared layers — `api`, `concerns` and `lib` — with
dependencies pointing from features into those layers, to achieve screens that can be found and changed
one at a time, accepting that shared behaviour must be deliberately promoted rather than reached for.

## Context and Problem Statement

A screen is the unit of change: accounts, expenses, incomes, projections, users, settings. Grouping by
type instead — all components, all hooks, all services — makes every screen change a scatter and makes
it hard to know which pieces belong together.

Sharing is the counter-pressure. A second feature that needs the same thing either copies it or promotes
it, and promotion has to have a place to happen.

## Decision Drivers

- A screen and everything only it uses should sit together.
- Shared transport, state and presentation should exist once.
- A dependency between features should be visible when it happens, not implicit.

## Considered Options

1. Feature folders, with shared layers for transport, cross-cutting concerns and utilities.
2. Technical folders: all components, all hooks, all services.
3. Features that may import each other freely.
4. A package per feature, built independently.

## Decision Outcome

Chosen option: 1. Each feature holds its pages, its components, its forms, its contexts and its API hooks.
Below the features sit the shared folders — `api` for the HTTP client, interceptors and shared types;
`concerns` for cross-cutting behaviour such as caching, logging, token providers and session handling;
`lib` for utilities; and the shared `components`, `data`, `hooks`, `routes` and `stores` folders. A feature
depends on those layers, and the layers are meant not to depend on a feature.

In practice they do: shared hooks, the logout manager and the sidebar import from features, because a
piece promoted to a shared folder kept its old imports. Feature-to-feature imports also exist — the
dashboard composes other features' data — and one feature folder, `features/shared`, holds sheets that
several features use. Both are visible in the import graph rather than prevented by the structure, which
is the one place this record's rule is softer than the folder layout suggests.

### Consequences

- Good, because a screen's code is in one folder, and its removal is one deletion.
- Good, because shared transport and state exist once, which is what makes the interceptors and the query
  client possible at all.
- Good, because a new screen has an obvious home that does not need a decision.
- Bad, because shared layers can become a dumping ground: something used by two features can be promoted
  before it is genuinely general.
- Bad, because the boundary is not holding: shared folders import features, so "a layer depends on no
  feature" is an intention rather than a property anyone can check.
- Bad, because a feature that is imported by others cannot be changed freely, which is the coupling the
  layout was meant to reduce.
- Bad, because `features/shared` is a feature folder that behaves as a shared layer, so the convention has
  an exception that newcomers have to learn.

### Confirmation

- Review: an import from a shared layer into a feature is a defect; the existing ones are debt to be
  removed, not a pattern to copy. A new feature-to-feature import is challenged, and recorded if it stays.
- Review: a new file in a shared layer states what more than one feature uses it.

## Pros and Cons of the Options

### Features over shared layers (chosen)

- Good: screens are independent, and shared code has one home.
- Bad: the boundary between shared and feature-local is a judgement call.

### Technical folders

- Good: obvious where a component or hook lives by kind.
- Bad: every screen change crosses several folders, and nothing groups what belongs together.

### Free feature imports

- Good: nothing to decide.
- Bad: the graph becomes a mesh, and a change's effect is unbounded.

### A package per feature

- Good: the boundary is enforced by the build.
- Bad: build tooling and versioning per feature for an application deployed as one bundle.

## More Information

- Server state and caching are a separate decision.
- The API hooks' return shape and the single error interceptor are separate decisions.
