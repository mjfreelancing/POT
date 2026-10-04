---
description: "React and TypeScript client architecture, state and style rules"
applyTo: "Source/Client/pot-react/**"
---

# Client (React + TypeScript)

Client commands must be run from `Source/Client/pot-react`: `npm run test`, `npm run lint`, `npm run type:check`, `npm run build`; running them from the repository root does not resolve the client project. Test rules: `.github/instructions/client-tests.instructions.md`. E2E rules: `.github/instructions/e2e.instructions.md`.

## Architecture

- Organize by feature modules (`src/features/*`), shared concerns (`src/concerns/*`) and reusable helpers (`src/lib/*`).
- One-way dependencies: features may use shared layers; shared/cross-cutting layers must never depend on features.
- Do not edit third-party UI primitives in `src/components/ui/*` unless explicitly requested.
- Keep transport, feature orchestration and UI state separate. Reuse shared helpers before adding new variants.
- Use barrel exports only when they improve discoverability and do not create cycles.

## State and data flow

- Global app state: Zustand. Server state: React Query. Feature-local cross-component state: React Context. Persisted browser state: `useLocalStorage`.
- API calls go through `src/api/hooks/useApi.ts`. Hook results are `Result<TSuccess, FailResultBase>` from `src/lib/result.ts`; branch explicitly on `result.success`.
- Normalize API errors only in `src/api/interceptors/axiosInterceptors.ts` and typed errors in `src/api/errors/*`.
- Invalidate caches via `useCacheInvalidation` / `invalidateCache` in `src/concerns/cache/cacheInvalidation.ts` and existing `useApi` variants before adding new transport wrappers.
- Encapsulate mutation flows in feature-level hooks that own cancellation and cache invalidation. State-changing work belongs in handlers/effects, never in render paths.
- Logout goes through `logoutManager.logout()`; do not scatter logout side effects across contexts/components. Keep the `/logout` recovery route and authenticated catch-all behavior.

## UX and permissions

- Blocking errors use `ErrorSheet` (only one visible blocking error surface at a time); transient feedback uses toasts. Keep copy concise and avoid duplicate notifications.
- Shared badge styles: `src/lib/badgeStyles.ts`. Sheet/dialog actions use `Separator` plus `opacity-80`. Stay within the existing design system.
- Gate rendering and interaction with `PermissionGuard` / `WithPermission`. Prevent self-lockout (for example exclude the current user from bulk admin actions).

## TypeScript style

- Prefer explicit types and `type` aliases. No `any` unless unavoidable at a boundary.
- Exported utilities are function declarations, not exported arrow functions. If an exception is required, state the reason in your response first.
- Small local object-factory helpers may use concise implicit-return arrows.
- Always use braces on `if`, even single-line branches.
- Blank line before and after multi-line `if` blocks, after early-return guard clauses, and before a standalone comment that introduces a new logical block.
- Keep modules small and cohesive.
