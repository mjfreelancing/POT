# Authentication Implementation Guide

This guide explains the authentication implementation in the POT application, from the high-level React components down to the API calls and token management.

## Table of Contents

1. [Overview](#overview)
2. [Component Architecture](#component-architecture)
3. [Authentication Flow](#authentication-flow)
4. [Token Management](#token-management)
5. [Secure Local Storage](#secure-local-storage)
6. [API Integration](#api-integration)
7. [Interceptors](#interceptors)
8. [Logout Handling](#logout-handling)

## Overview

The authentication system is built on JWT (JSON Web Tokens) with the following key features:

- Access token for API authorization
- Refresh token for obtaining new access tokens
- Automatic token refresh via interceptors
- Secure token storage in localStorage
- Global auth state management via React Context
- Centralized logout handling

## Component Architecture

### App.tsx - Root Setup

The authentication context wraps the entire application:

```tsx
function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
```

### AuthContext Provider

Manages global authentication state:

- Tracks authentication status
- Provides login/logout methods
- Stores tokens securely
- Exposes auth state to components

```tsx
type AuthContextType = {
  tokens: AuthTokens | undefined;
  isAuthenticated: boolean;
  login: (tokens: AuthTokens) => void;
  logout: () => void;
  error?: DisplayError;
};
```

## Authentication Flow

### 1. Login Process

When a user logs in:

1. User submits credentials via LoginForm
2. Credentials sent to server via useLogin hook
3. Server returns access and refresh tokens
4. Tokens stored in localStorage
5. Auth context updated to reflect logged-in state
6. User redirected to protected route

Example login flow:

```tsx
function LoginPage() {
  const loginMutation = useLogin();
  const { login } = useAuth();

  const handleLogin = async (credentials: LoginCredentials) => {
    const result = await loginMutation.mutateAsync({
      data: credentials,
      signal: controller.signal,
    });

    if (result.success) {
      login(result.value); // Store tokens & update context
      navigate('/');
    }
  };
}
```

### 2. Protected Routes

Routes requiring authentication are wrapped with an auth check:

```tsx
function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" />;
}
```

## Token Management

### Token Types

```typescript
type AuthTokens = {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiryUtc: string;
};
```

### Token Provider

The TokenProvider interface manages token operations:

```typescript
type TokenProvider = {
  getAccessToken: () => string | undefined;
  refreshTokens: () => Promise<string>;
  clearTokens: () => void;
};
```

Implementation:

```typescript
const createAuthTokenProvider = (): TokenProvider => {
  const getAccessToken = () => {
    const authData = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!authData) return undefined;

    const tokens = JSON.parse(authData) as AuthTokens;
    return tokens.accessToken;
  };

  const refreshUserTokens = async (): Promise<string> => {
    // ... token refresh logic
  };

  const clearTokens = () => {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    logoutManager.logout();
  };

  return {
    getAccessToken,
    refreshTokens: refreshUserTokens,
    clearTokens,
  };
};
```

## Secure Local Storage

Tokens are stored in localStorage using a custom hook that provides:

- Type safety
- Error handling
- Consistent key management

```typescript
const { getItem, setItem, removeItem } = useLocalStorage<AuthTokens>({
  key: AUTH_STORAGE_KEY,
  onError: setError,
});
```

## API Integration

### Auth Client

A dedicated axios instance handles auth operations:

```typescript
const authClient = axios.create();
authClient.defaults.baseURL = import.meta.env.VITE_API_BASE_URL;

// Add request/response interceptors
authClient.interceptors.request.use(addCorrelationId);
authClient.interceptors.response.use(
  responseSuccessHandler,
  responseErrorHandler,
);
```

### Token Refresh

```typescript
async function refreshTokens(
  request: RefreshTokenRequest,
  expiredToken: string,
): Promise<AuthTokens> {
  try {
    const response = await authClient.post('/auth/refresh', request, {
      headers: {
        Authorization: `Bearer ${expiredToken}`,
      },
    });

    return response.data;
  } catch (error) {
    // ... error handling
  }
}
```

## Interceptors

### Auth Interceptor

Handles token refresh and authorization:

1. Adds Authorization header to requests
2. Catches 401 responses
3. Attempts token refresh
4. Retries failed requests with new token

```typescript
const setupAuthInterceptors = (tokenProvider: TokenProvider) => {
  // Request interceptor adds auth header
  const requestInterceptor = axios.interceptors.request.use(async config => {
    const token = tokenProvider.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  // Response interceptor handles token refresh
  const responseInterceptor = axios.interceptors.response.use(
    response => response,
    async error => {
      // ... token refresh and retry logic
    },
  );

  return {
    requestInterceptorId,
    responseInterceptorId,
  };
};
```

### General Interceptors

Handle common concerns:

1. Correlation IDs for request tracking
2. Response logging
3. Error transformation
4. Network error handling

## Logout Handling

### Logout Manager

Centralizes logout operations:

```typescript
const logoutManager: LogoutManager = (() => {
  let logoutCallback: (() => void) | undefined;

  return {
    setLogoutCallback: (callback: () => void) => {
      logoutCallback = callback;
    },
    logout: () => {
      logoutCallback?.();
    },
  };
})();
```

### Logout Process

1. AuthContext registers its logout handler
2. Other components can trigger logout via logoutManager
3. Tokens are cleared from storage
4. Auth state is reset
5. User is redirected to login

## Error Handling

### Authentication Errors

Custom error types for different auth scenarios:

```typescript
class AuthenticationError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'AuthenticationError';
  }
}
```

### Error Display

Errors are displayed to users via:

- Toast notifications
- Error sheets
- Inline form errors

## Security Considerations

1. Tokens are stored in localStorage (consider more secure alternatives for high-security applications)
2. Access tokens are short-lived
3. Refresh tokens have configurable expiry
4. All auth operations are logged with correlation IDs
5. Failed auth attempts are rate-limited (server-side)
6. Sensitive operations require fresh authentication

## Best Practices

1. Keep auth logic centralized
2. Use TypeScript for type safety
3. Handle edge cases (network errors, token expiry)
4. Log auth operations for debugging
5. Provide clear error messages
6. Use consistent error handling
7. Implement proper logout cleanup
8. Add correlation IDs for traceability
