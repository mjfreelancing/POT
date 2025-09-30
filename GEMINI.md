# POT Developer Guide

This guide provides essential knowledge for developers working on the POT project. It covers the architecture, conventions, and workflows specific to this codebase.

## Big Picture Architecture

POT is a full-stack application with a C#/.NET backend and a React/TypeScript frontend.

- **Backend:** A .NET 8 API built with ASP.NET Core. It follows a **CQRS-like pattern** where features are separated into commands (e.g., `Create`, `Update`, `Delete`) and queries (e.g., `Get`, `GetAll`). It uses **Entity Framework Core** with the repository pattern for data access to a **PostgreSQL** database.
- **Frontend:** A React application built with Vite and TypeScript. It uses **React Query** for server state management, with a set of custom hooks that standardize API interactions. The UI is built with **Tailwind CSS** and **shadcn/ui**.
- **Containerization:** The entire application (backend, frontend, database) can be run using **Docker Compose**, which is the recommended approach for local development.

### Key Files & Directories

- `Source/Server/Pot.App`: The core backend logic, organized by features.
- `Source/Server/Pot.AspNetCore`: The API layer, responsible for exposing endpoints.
- `Source/Client/pot-react`: The React frontend application.
- `Source/Docker`: Dockerfiles and `docker-compose` files for running the application.

## Backend Conventions

### CQRS-like Feature Organization

The backend code in `Pot.App` is organized by features (e.g., `Expenses`, `Incomes`, `Accounts`). Inside each feature, the code is further divided into vertical slices representing commands and queries.

For example, the `Source/Server/Pot.App/Features/Expenses` directory contains:

- `Create/`: Logic for creating a new expense.
- `Delete/`: Logic for deleting an expense.
- `GetAll/`: Logic for retrieving all expenses.
- ...and so on.

This pattern keeps related code together and makes it easy to find and modify the logic for a specific use case.

### API Response Pattern: Direct Data or ProblemDetails

A key convention to understand is how the backend communicates success and failure:

- **On Success:** The API returns the requested data **directly** as a JSON payload. There is no special wrapper object.
- **On Failure:** The API returns a standard `ProblemDetails` JSON object (RFC 7807). This object contains details about the error, such as the status code, a title, and a list of validation errors.

This approach relies on HTTP status codes to differentiate between success (`2xx`) and failure (`4xx`, `5xx`).

## Frontend Conventions

### Client-Side Result Pattern

To standardize the handling of API responses, the frontend uses a **client-side Result pattern**. All API calls made through the custom `useApi` hooks return a `Result` object, which is either a `SuccessResult<T>` or a `FailResult<T>`.

This is defined in `Source/Client/pot-react/src/lib/result.ts`:

```typescript
// Simplified for clarity
class SuccessResult<TSuccess> {
  public readonly success = true;
  constructor(public value: TSuccess) {}
}

class FailResult<TFail extends FailResultBase> {
  public readonly success = false;
  constructor(public error: TFail) {}
}

type Result<TSuccess, TFail extends FailResultBase> =
  | SuccessResult<TSuccess>
  | FailResult<TFail>;
```

This forces developers to explicitly handle both success and error states in their components, leading to more robust code.

### API Interaction: `useApi` Hooks

All communication with the backend is handled by a set of custom hooks defined in `Source/Client/pot-react/src/api/hooks/useApi.ts`. These hooks wrap `react-query`'s `useQuery` and `useMutation` and incorporate the client-side **Result pattern**.

- `useGet`: For fetching data (`GET` requests).
- `usePost`, `usePut`, `useDelete`: For mutating data (`POST`, `PUT`, `DELETE` requests).

The core of this system is the `performOperation` function, which wraps an `axios` call:

```typescript
const performOperation = async <TResponse>(
  operation: () => Promise<AxiosResponse<TResponse>>
): Promise<Result<TResponse, FailResultBase>> => {
  try {
    const response = await operation();
    return new SuccessResult(response.data); // On success, wrap data in SuccessResult
  } catch (error) {
    return error as FailResult<FailResultBase>; // On error, cast to FailResult
  }
};
```

When a component uses one of these hooks, it receives a `Result` object and can render accordingly:

```tsx
const { data: result } = useGet<Account[]>("/api/accounts", ["accounts"]);

if (result && result.success) {
  // Render the accounts
  return <AccountsList accounts={result.value} />;
} else {
  // Render an error message
  return <ErrorDisplay error={result?.error} />;
}
```

### Logging Important Events

The project uses a simple logging utility in `Source/Client/pot-react/src/lib/logging.ts` to standardize console output. It provides `info`, `warn`, and `error` functions.

**Convention:** There is no automatic logging of component lifecycle events (e.g., mount/unmount). Instead, developers should use the `logger` to record significant events, user actions, or errors that are helpful for debugging.

**Example:**

```typescript
import { logger } from "@/lib/logging";

const MyComponent = () => {
  const handleAction = () => {
    logger.info("MyComponent", "User performed an important action.");
    // ...
  };
  // ...
};
```

### Error Handling and User Feedback

The application uses a combination of a persistent **Error Sheet** for critical errors and temporary **Toasts** for less critical feedback.

#### The Error Sheet

The `ErrorSheet` is a persistent, dismissible banner displayed at the top of the screen. It is designed to inform the user about critical errors that require their attention, such as a failed API request or an unexpected application state.

There are two primary ways the `ErrorSheet` is used:

**1. Feature-Level for API Errors (Expected Errors)**

The most common pattern is to use the `ErrorSheet` within a feature's main component to display errors related to that feature's API calls.

- A local state variable (e.g., `error`) is used to hold the error details.
- A `useEffect` hook watches the result of an `useApi` hook. If the result is a `FailResult`, the `error` state is updated.
- The `ErrorSheet` is rendered conditionally based on the `error` state.

**Example from `AccountsPage.tsx`:**

```tsx
function AccountsPage() {
  const { error, setError } = useErrorContext();
  const { data: accountsResult, isLoading } = useApiGetAllAccounts();

  // ...

  // Handle errors from the API call
  useEffect(() => {
    if (accountsResult) {
      setError(
        accountsResult.success
          ? null
          : {
              title: accountsResult.error.code,
              description: accountsResult.error.description,
            }
      );
    }
  }, [accountsResult]);

  return (
    <div>
      {/* ... */}
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      {/* ... */}
    </div>
  );
}
```

**2. Global for Application Errors (Unexpected Errors)**

A single, application-wide `ErrorSheet` is rendered in `App.tsx` to handle unexpected errors caught by a global `ErrorBoundary`.

- The `ErrorBoundary` wraps the entire application content.
- If any component in the tree throws a rendering error, the `onError` callback of the `ErrorBoundary` is triggered.
- This callback updates a state variable in `App.tsx`, which in turn renders the `ErrorSheet`.

This provides a safety net for the entire application, ensuring that even unexpected errors are presented to the user in a controlled way.

**Example from `App.tsx`:**

```tsx
const App = () => {
  const [error, setError] = useState<DisplayError | undefined>();

  const handleError = (error: Error) => {
    setError({
      title: "Application Error",
      description: error.message,
    });
  };

  return (
    <AuthProvider>
      <ThemeProvider>
        <ErrorBoundary onError={handleError}>
          <AppContent />
        </ErrorBoundary>
        {error && (
          <ErrorSheet
            title={error.title}
            description={error.description}
            onDismiss={() => setError(undefined)}
          />
        )}
      </ThemeProvider>
    </AuthProvider>
  );
};
```

#### Toasts

**Toasts** are small, temporary notifications used for less critical feedback, such as:

- Confirming a successful action (e.g., "Account created successfully").
- Displaying validation errors on a form.
- Notifying the user of a background process.

They are managed by the `sonner` library and can be triggered from anywhere in the application.

### Error Handling

The client application uses a centralized `ErrorContext` for managing errors. This context provides a consistent way to handle and display errors across components. All components should use the `useErrorContext` hook to access the `error` and `setError` methods.

## Critical Developer Workflows

### Running the Application (Docker - Recommended)

The easiest way to get started is to use the provided Docker Compose tasks in VS Code.

1.  **Server-Only Mode:**

    - Press `Ctrl+Shift+P` and select `Tasks: Run Task`.
    - Choose `docker-start-pot-server-only`.
    - This starts the backend API and the database in Docker.
    - Navigate to `Source/Client/pot-react` and run `npm run dev` to start the frontend.

2.  **Client and Server Mode:**
    - Press `Ctrl+Shift+P` and select `Tasks: Run Task`.
    - Choose `docker-start-pot-client-server`.
    - This starts the entire stack in Docker.
    - The application will be available at `http://localhost:5175`.

### Frontend Scripts

The following scripts are available in `Source/Client/pot-react/package.json`:

- `npm run dev`: Starts the Vite development server.
- `npm run build`: Builds the application for production.
- `npm run lint`: Lints the code using ESLint.
- `npm run test`: Runs the tests using Vitest.
- `npm run type:check`: Performs a TypeScript type check.
