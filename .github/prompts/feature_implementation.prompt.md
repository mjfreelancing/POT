---
name: feature_implementation
description: Implement a feature with a concise cross-layer checklist and focused validation.
---

Implement a feature with minimal, targeted edits and root-cause focus.

## Scope

- Implement requested features across affected repository layers with minimal unrelated change.

## Workflow

- Start with contracts and boundaries before implementation details.
- Prefer incremental validation and smallest-scope checks first.

## Checklist

1. Clarify scope and affected layers, such as client, API, app, data, or infrastructure.
2. Update contracts or types first, then business logic, then UI or transport wiring.
3. Follow scoped instruction files for implementation standards in each affected layer.
4. Keep schema and migration changes isolated to the repository's migration workflow when they are required.
5. Add or update tests in the nearest appropriate test project or folder.
6. Validate the smallest relevant test or build scope first, then broaden only if needed.
7. Summarize changed files, behavior impact, and any deferred follow-up items.

## Guardrails

- Do not refactor unrelated code.
- Do not change public API shape unless requested or approved.
- Keep UI consistent with the repository's existing design system and shared components.

## Expansion Notes

- Add recurring delivery steps to `Checklist` only when they are feature-implementation specific.
- Keep coding standards in instruction files and reference them from this prompt.
  - POT: Keep schema changes in `Pot.Data.Migrations` when server-side data changes are involved.
