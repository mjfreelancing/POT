---
name: feature_implementation
description: Implement a POT feature with a concise cross-layer checklist and focused validation.
---

Implement a feature with minimal, targeted edits and root-cause focus.

## Scope

- Implement requested features across affected POT layers with minimal unrelated change.

## Workflow

- Start with contracts and boundaries before implementation details.
- Prefer incremental validation and smallest-scope checks first.

## Checklist

1. Clarify scope and affected layers (client, API, app, data, docker if needed).
2. Update contracts/types first, then business logic, then UI wiring.
3. Follow scoped instruction files for implementation standards in each affected layer.
4. Keep migrations isolated to `Pot.Data.Migrations` when schema changes are required. Follow the scoped migration instructions for generating and applying migrations.
5. Add or update tests in the nearest appropriate test project/folder.
6. Validate smallest relevant test/build scope first, then broaden only if needed.
7. Summarize changed files, behavior impact, and any deferred follow-up items.

## Guardrails

- Do not refactor unrelated code.
- Do not change public API shape unless requested, or approved.
- Keep UI consistent with existing design system and shared components.

## Expansion Notes

- Add recurring delivery steps to `Checklist` only when they are feature-implementation specific.
- Keep coding standards in instruction files and reference them from this prompt.
