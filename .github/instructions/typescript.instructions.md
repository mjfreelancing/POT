---
applyTo: "Source/Client/pot-react/src/**/*.{ts,tsx}"
---

# TypeScript Instructions

## Core Rules

### Language and Style

- Prefer explicit types and `type` aliases where practical.
- Do not use `any` unless unavoidable at a boundary.
- Prefer function declarations over arrow functions for exported utilities where practical.
- Always use braces for `if` statements, including single-line branches.
- Keep a blank line before and after multi-line `if` blocks for readability consistency.
- Keep a blank line after early-return guard clauses before continuing with the main execution path.
- Keep a blank line before standalone explanatory comments that introduce a new logical block.
- Keep modules small and cohesive.

### Modules and Boundaries

- Keep module boundaries explicit; use barrel exports only when they improve discoverability and avoid cycles.
- Reuse shared helpers/utilities before introducing new variants.
- Keep transport, feature orchestration, and UI state responsibilities separated.
- Enforce one-way dependency direction: features may depend on API/shared concerns; shared layers must not depend on features.

### API and State Patterns

- Encapsulate mutation flows in feature-level hooks that own cancellation and cache invalidation behavior.

### Runtime Behavior

- Keep state-changing operations in handlers/effects, not in render paths.

## Expansion Notes

- Baseline cross-language rules live in `language-agnostic-core.instructions.md`.
- Keep framework-agnostic TypeScript-only rules here.
- Put React- or API-specific conventions in separate instruction packs.
  - POT: Concrete TypeScript client scope is `Source/Client/pot-react/src/**/*.{ts,tsx}`.
  - POT: Keep feature logic in `src/features/*`, shared concerns in `src/concerns/*`, and reusable helpers in `src/lib/*`.
  - POT: Do not edit `src/components/ui/*` primitives unless explicitly requested.
  - POT: Standard API usage goes through `src/api/hooks/useApi.ts`.
  - POT: Handle API outcomes via `Result<TSuccess, FailResultBase>` and branch on `result.success`.
  - POT: Keep error normalization centralized in `src/api/interceptors/*` and typed errors in `src/api/errors/*`.
  - POT: Prefer cache invalidation via `useCacheInvalidation` and `invalidateCache` in `src/concerns/cache/cacheInvalidation.ts`.
