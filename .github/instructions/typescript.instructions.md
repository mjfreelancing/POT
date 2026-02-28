---
applyTo: "Source/Client/pot-react/src/**/*.{ts,tsx}"
---

# TypeScript Instructions

## Core Rules

### Language and Style

- Prefer explicit types and `type` aliases.
- Do not use `any` unless explicitly requested for an unavoidable boundary.
- Prefer function declarations over arrow functions for exported utilities where practical.
- Always use braces for `if` statements, including single-line branches.
- Keep a blank line before and after multi-line `if` blocks for readability consistency.

### Modules and Boundaries

- Keep module boundaries explicit; use barrel exports only when they improve discoverability and avoid cycles.
- Reuse shared helpers/utilities before introducing new variants.
- Keep API transport, feature orchestration, and UI state responsibilities separated.
- Keep feature logic in `src/features/*`, shared concerns in `src/concerns/*`, and reusable helpers in `src/lib/*`.
- Maintain dependency direction: features may depend on `api` and `concerns`; cross-cutting layers must not depend on features.
- Do not edit `src/components/ui/*` primitives unless explicitly requested.

### API and State Patterns

- Use API hooks from `src/api/hooks/useApi.ts` rather than ad-hoc client calls.
- Handle API outcomes via `Result<TSuccess, FailResultBase>` and branch on `result.success`.
- Keep error normalization centralized in `src/api/interceptors/*` and typed errors in `src/api/errors/*`.
- Prefer cache invalidation via `useCacheInvalidation` / `invalidateCache` in `src/concerns/cache/cacheInvalidation.ts`.
- For feature mutations, use feature-level wrapper hooks that own cancellation (`AbortController`) and success-only invalidation.

### UI and Side Effects

- Use `PermissionGuard` (hide) vs `WithPermission` (disable) intentionally based on UX discoverability needs.
- Use `ErrorSheet` for blocking failures and toast components for transient outcomes.
- Do not trigger state-changing operations during render; keep them in handlers/effects.

## Expansion Notes

- Place new language-level rules under `Language and Style`; place framework usage rules under the nearest domain subsection.
- For new rules tied to concrete source locations, include the path (for example `src/features/...`) directly in the bullet.
- If a rule applies beyond `.ts/.tsx` files, add it to `client.instructions.md` instead of duplicating it here.
