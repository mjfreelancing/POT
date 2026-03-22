---
applyTo: "Source/Client/pot-react/**"
---

# React Client Instructions

## Core Rules

### Architecture

- Organize by feature modules and shared concerns.
- Keep dependency direction one-way: features may use shared layers.
- Prevent shared/cross-cutting layers from depending on feature modules.
- Do not edit third-party UI primitives unless explicitly requested.

### State and Data Flow

- Separate server state from UI state.
- Keep API error normalization centralized.
- Use explicit success/failure branching for mutations and async operations.
- Use centralized auth/logout orchestration instead of scattering logout side effects across unrelated contexts/components.

### UX and Feedback

- Use consistent patterns for blocking errors and transient feedback.
- Keep feedback copy concise and avoid duplicate notifications.
- Enforce one visible blocking error surface at a time to reduce user confusion.
- Keep UI changes aligned with existing design system constraints.

### Auth, Permissions, and Routing

- Use explicit permission guards for conditional rendering and interaction states.
- Prevent self-management lockout patterns (for example excluding current user from bulk admin actions when applicable).

## Expansion Notes

- Keep React-specific patterns here.
- Keep TypeScript-only language rules in `typescript.instructions.md`.
  - POT: Concrete client scope is `Source/Client/pot-react/**`.
  - POT: Global app state uses Zustand; server state uses React Query; feature-local cross-component state uses React Context; persisted browser state uses `useLocalStorage`.
  - POT: Standard API usage goes through `src/api/hooks/useApi.ts`; hook results use `Result<TSuccess, FailResultBase>` from `src/lib/result.ts`.
  - POT: Normalize API errors in `src/api/interceptors/axiosInterceptors.ts` and typed errors in `src/api/errors/*`.
  - POT: Prefer cache invalidation from `src/concerns/cache/cacheInvalidation.ts` and existing `useApi` hook variants before introducing new transport wrappers.
  - POT: Use `ErrorSheet` for blocking errors, toasts for transient feedback, `src/lib/badgeStyles.ts` for shared badge styles, and `Separator` plus `opacity-80` for sheet or dialog actions.
  - POT: Prefer `logoutManager.logout()`, `PermissionGuard`, and `WithPermission`; keep the established `/logout` recovery route and authenticated catch-all behavior.
