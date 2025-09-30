## POT AI Coding Agent Instructions

This guide provides essential, actionable rules for AI coding agents working in the POT codebase. Follow these conventions to ensure code quality, maintainability, and alignment with project architecture.

### 1. Project Architecture

- **Frontend**: React 18 + TypeScript (Vite, Zustand, React Query, TailwindCSS)
  - Located in `Source/Client/pot-react/`
  - Feature-based structure: `src/features/`, `src/api/`, `src/components/`
- **Backend**: ASP.NET Core (C#)
  - Located in `Source/Server/`
  - Modular projects: `Pot.AspNetCore` (API), `Pot.App` (business logic), `Pot.Data` (EF Core), `Pot.Data.Migrations` (migrations), `Pot.Shared` (DTOs)
- **Database**: PostgreSQL (EF Core migrations)
- **DevOps**: Docker Compose for local/prod, see `Source/Docker/`

### 2. TypeScript & Coding Conventions

- Use **types** instead of interfaces
- **Never** use the `any` type
- Use function declarations (not arrows) where appropriate
- Always use `{}` with `if` statements, even for single lines
- Add a blank line before and after `if` blocks
- Place all exports at the bottom of the file
- Do **not** remove existing comments or code snippets
- Do **not** edit `components/ui` (shadcn components)
- Do **not** include unused imports
- Enforce single responsibility and modularity
- Features may depend on API code, but not vice versa

### 3. State & Data Management

- **Global state**: Use Zustand for app-wide state (see `src/store/`)
- **Server state**: Use React Query for API data
- **Local state**: Use React Context for feature-specific state
- **Local storage**: Use `useLocalStorage` hook (`src/hooks/useLocalStorage.ts`)—type-safe, with error handling

### 4. API, React Query & Error Handling

- **API integration** via `src/api/` (Axios, hooks, interceptors)
- **React Query usage:**
  - All API hooks (see `src/api/hooks/useApi.ts`) return a custom `Result<TSuccess, TFail>` type, not raw data or exceptions.
  - This pattern wraps successful responses in `SuccessResult<T>`, and errors in `FailResult<TError>` (see `src/lib/result.ts`).
  - **Contrast to typical react-query:** Instead of throwing or using `error`/`isError`, consumers always receive a `Result` object and must check `.success` to branch logic.
  - This enables strict, type-safe error handling and prevents unhandled promise rejections.
  - All errors are normalized and wrapped by Axios interceptors (see `src/api/interceptors/axiosInterceptors.ts`) into domain-specific error types (`ApiError`, `ValidationError`, etc.).
  - **Do not** rely on react-query's `error`/`isError` fields; always use the `Result` pattern for all API data and error handling.
- **Centralized error types:** `src/api/errors/apiErrors.ts` defines all domain error types.
- **UI error display:** Use `src/components/feedback/ErrorSheet.tsx` to present errors in a consistent, prominent sheet at the top of the UI.

  - Always display critical or blocking errors (e.g., API failures, auth issues, validation errors) using ErrorSheet at the top of the page or feature container.
  - Pass normalized error objects (see `DisplayError` type) to ErrorSheet for clear messaging.
  - Dismissal should clear the error state in the parent component.

  - Do not use ErrorSheet for transient or minor feedback—prefer toasts for those cases.

- **Correlation IDs:** All auth requests include a correlation ID for traceability.

### ErrorSheet for Critical Errors

- Use `ErrorSheet` (`src/components/feedback/ErrorSheet.tsx`) to present critical or blocking errors (e.g., API failures, authentication issues, validation errors) in a prominent sheet at the top of the UI.
- Pass normalized error objects (see `DisplayError` type) to ErrorSheet for clear messaging.
- Dismissal should clear the error state in the parent component.
- Example usage:
  ```tsx
  {
    error && (
      <ErrorSheet
        title={error.title}
        description={error.description}
        onDismiss={() => setError(null)}
      />
    );
  }
  ```
- Ensure only one ErrorSheet is visible at a time to avoid overwhelming the user.
- Do not use ErrorSheet for transient or minor feedback—use toasts for those cases.

### Toasts for User Feedback

- Use toasts for transient, non-blocking feedback such as success confirmations, minor errors, or user actions.
- Use the toast components in `src/components/feedback/toast/` (e.g., `SuccessToast`, `ErrorToast`).
- Do not use toasts for critical or blocking errors—use ErrorSheet for those.
- Example: Success toast after saving data
  ```tsx
  showToast(
    <SuccessToast
      icon={CheckCircleIcon}
      title="Saved!"
      description="Your changes have been saved."
    />
  );
  ```
- Example: Error toast for a minor failure
  ```tsx
  showToast(
    <ErrorToast
      icon={AlertTriangleIcon}
      title="Failed to Save"
      description="Please try again later."
    />
  );
  ```
- Keep toast messages concise and actionable.
- Avoid stacking multiple toasts for the same event.

### 5. Security & Permissions

- JWT-based authentication (see `/features/auth/` and backend `Features/Auth/`)
- Role-based permissions: use `resource:action` (e.g., `account:view`)
- Add new permissions via DB migrations and update docs
- No sensitive data in client storage

### 6. Build, Test, and Lint Workflows

- **Frontend** (run in `Source/Client/pot-react/`):
  - `npm run dev` — Start Vite dev server
  - `npm run build` — Production build
  - `npm run lint` — Lint code
  - `npm run prettier` — Format code
  - `npm run test` — Run unit tests (Vitest)
  - `npm run type:check` — TypeScript check
- **Backend**:
  - Use `.editorconfig` for C# style
  - Migrations: `dotnet ef migrations add <Name> --project Pot.Data.Migrations`
  - Update DB: `dotnet ef database update --project Pot.Data.Migrations`
  - Test: `dotnet test --collect:"XPlat Code Coverage"`
- **Docker**:
  - Dev: `docker-compose -f docker-compose-server-only.yml up --build -d`
  - Prod: `docker-compose -f docker-compose-client-server.yml up --build -d`

### 7. Environment & Configuration

- Use Vite env files (`.env`, `.env.development`, `.env.production`, `.env.local`)
- Path alias `@/*` maps to `src/`
- See `Environment.md` for details

### 8. Logging of Page Mount/Unmount & Events

- Use the `logger` utility (`src/lib/logging.ts`) to log important lifecycle events and operations.
- **Page/component mount/unmount:** Log when pages and major components mount and unmount, using `logger.info('ComponentName', 'Mounted')` and `logger.info('ComponentName', 'Unmounted')` in `useEffect` hooks.
  - Example:
    ```tsx
    useEffect(() => {
      logger.info("ProjectionsPage", "Mounted");
      return () => {
        logger.info("ProjectionsPage", "Unmounted");
      };
    }, []);
    ```
- **Other important events:** Log key user actions, API operations, and error conditions with clear, structured messages. Include relevant context (e.g., IDs, parameters, error details).
- **Consistency:** Use the same logger for info, warn, and error levels. Always include the component or feature name as the first argument.
- **Purpose:** Logging is used for debugging, analytics, and audit trails. Do not log sensitive data.

### 9. Documentation & Review

- The main `README.md` is the source of truth for architecture and workflows
- Use `Copilot prompt.md` for review/analysis tools and process
- Cross-reference code and docs when updating features or patterns

### 10. Helper Methods and Performance Considerations

- **Memoization:**

  - When adding helper methods, consider whether they should be wrapped in `useCallback` or `useMemo` to prevent unnecessary re-renders or recalculations.
  - Use `useCallback` for functions that are passed as dependencies to `useEffect` or as props to child components.
    - Example:
      ```tsx
      const handleClick = useCallback(() => {
        console.log("Button clicked");
      }, []);
      ```
  - Use `useMemo` for expensive calculations or derived values that depend on state or props.
    - Example:
      ```tsx
      const computedValue = useMemo(() => {
        return expensiveCalculation(input);
      }, [input]);
      ```

- **Dependencies:**

  - Always include all dependencies in the dependency array of `useCallback` or `useMemo` to ensure correctness.
  - Use ESLint rules (e.g., `react-hooks/exhaustive-deps`) to catch missing dependencies.

- **Avoid Overuse:**

  - Do not wrap every function or calculation in `useCallback` or `useMemo`. Use them only when necessary to optimize performance.
  - Premature optimization can lead to unnecessary complexity.

- **Testing:**

  - Test the behavior of memoized functions and values to ensure they update correctly when dependencies change.

- **Documentation:**
  - Document the rationale for using `useCallback` or `useMemo` in comments to help future developers understand the decision.

---

**Example: Adding a new permission**

1. Add to DB via migration (`Pot.Data.Migrations`)
2. Update permission docs in `README.md`
3. Use `resource:action` string in frontend checks

---

**For more details, see:**

- `README.md` (root and `pot-react/`)
- `GEMINI.md` (architecture, security, conventions)
- `Copilot prompt.md` (AI review process)
- `Source/Docker/Readme.md` (Docker workflows)

### Error Handling Conventions

- Use the centralized `ErrorContext` for managing errors in the client application.
- Ensure all errors are normalized and displayed using the `ErrorSheet` component.
