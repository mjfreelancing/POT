---
applyTo: "Source/Client/pot-react/**"
---

# React Client Instructions

## Core Rules

### Architecture and Boundaries

- Organize by feature modules, cross-cutting concerns, and shared libraries.
- Keep dependency direction one-way: feature code can consume shared/platform layers; shared layers should not depend on features.
- Use explicit success/failure result branching rather than exception-first UI decisions.
- Do not edit third-party UI primitives unless explicitly requested.

### State and Data Flow

- Separate server state from UI state.
- Use Zustand for global app state.
- Use React Query for server state.
- Use React Context for feature-local cross-component state.
- Use `useLocalStorage` for persisted local browser state.
- API usage is mandatory via `src/api/hooks/useApi.ts`.
- API hooks return `Result<TSuccess, FailResultBase>` (`src/lib/result.ts`); always branch on `result.success`.
- Do not rely on thrown React Query errors as primary control flow.
- Keep API error normalization centralized in `src/api/interceptors/axiosInterceptors.ts`; extend typed errors in `src/api/errors/*`.
- Centralize API error normalization and cache invalidation policies.
- Use cache invalidation from `src/concerns/cache/cacheInvalidation.ts` (`invalidateCache` / `useCacheInvalidation`) instead of ad-hoc query invalidation.
- Prefer existing `useApi` hook variants (`useGet`, `usePost`, `usePut`, `useDelete`, `usePutWithId`, `usePutWithIdNoData`, `usePostWithId`, `usePostWithIdNoData`) before introducing new transport wrappers.
- Keep auth request correlation IDs intact for traceability.
- Use explicit success/failure branching for mutations and async operations.

### UX and Feedback

- Use consistent patterns for blocking errors and transient feedback.
- Keep feedback copy concise and avoid duplicate notifications.
- Reuse shared style helpers (for example `src/lib/badgeStyles.ts`) rather than duplicating Tailwind class strings.
- For mutations, handle both success and failure paths with consistent user feedback.
- Use `ErrorSheet` for blocking/critical errors and toasts for transient feedback.
- Ensure only one `ErrorSheet` is visible at a time.
- Do not pass a custom icon to `ErrorToast`.
- Keep UI changes aligned with existing design system constraints.

### Auth, Permissions, and Routing

- For logout flows, prefer `logoutManager.logout()` over direct context logout calls.
- Use `PermissionGuard`/`WithPermission` for permission-based rendering and interaction states.
- Prevent self-management lockout patterns (for example user-admin bulk actions must exclude current user when applicable).
- Unknown authenticated routes should resolve through the existing catch-all behavior, and forced recovery logout should use the established `/logout` route pattern.

### Reliability and Maintainability

- Do not trigger state-changing operations directly during render; keep them in handlers/effects.
- For helper methods in render-heavy components, use `useCallback`/`useMemo` when dependency-driven stability is required.
- For long-lived screens, include structured logger events for lifecycle and important async actions.
- For sheet/dialog form actions, keep the existing separator-above-actions pattern (`Separator` + `opacity-80`) for visual consistency.
- Exceptions to `useApi` are allowed for explicit transport scenarios (for example streaming/file transfer) when wrapped with the same result/error handling contract.

## Commands

Run in `Source/Client/pot-react`:

- `npm run dev`
- `npm run build`
- `npm run lint`
- `npm run lint:fix`
- `npm run lint:sort`
- `npm run prettier`
- `npm run test`
- `npm run test:ui`
- `npm run type:check`

## Integration Notes

- Vite alias `@` maps to `src` (`vite.config.ts`).
- Dev proxy forwards `/api` to `http://localhost:5242` when running backend locally.

## Expansion Notes

- Keep React-specific patterns here.
- Keep TypeScript-only language rules in `typescript.instructions.md`.
- Add new rules to the nearest subsection above instead of appending unrelated bullets to the bottom.
- When adding a path-specific rule, include the concrete file or folder path in the bullet.
