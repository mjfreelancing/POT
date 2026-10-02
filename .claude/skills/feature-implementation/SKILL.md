---
name: feature-implementation
description: Implement a feature that spans client, API, application, data or infrastructure layers, using a cross-layer checklist and focused validation.
---

# Feature Implementation

Implement the feature with minimal, targeted edits and root-cause focus.

1. Clarify scope and the affected layers (client, API, app, data, infrastructure).
2. Update contracts or types first, then business logic, then UI or transport wiring.
3. Follow the nested `CLAUDE.md` and `.claude/rules/` files for each affected area.
4. Keep schema changes in `Pot.Data.Migrations`; do not generate a migration, tell the developer to run and review `add-migration`.
5. Add or update tests in the nearest appropriate test project or folder.
6. Validate the smallest relevant test or build scope first, then broaden only if needed.
7. Summarize changed files, behavior impact and any deferred follow-up.

## Guardrails

- Do not refactor unrelated code.
- Do not change public API shape unless requested or approved.
- Keep UI consistent with the existing design system and shared components.
