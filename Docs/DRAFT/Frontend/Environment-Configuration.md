# Environment Configuration Guide

This document explains how environment configuration works in the POT React application. We use Vite's built-in environment variable handling, which provides a flexible and secure way to manage different environments.

## Environment Files Overview

The application uses multiple environment files for different scenarios:

- `.env` - Base configuration, shared across all environments

  ```properties
  # API request timeout in milliseconds
  VITE_API_TIMEOUT_MS=10000

  # Application title
  VITE_APP_TITLE=POT - Financial Management
  ```

- `.env.development` - Development-specific settings

  ```properties
  # Development API URL (local ASP.NET server)
  VITE_API_BASE_URL=http://localhost:5242/api
  ```

- `.env.production` - Production settings

  ```properties
  # Production API URL (Docker container)
  VITE_API_BASE_URL=http://localhost:5241/api
  ```

- `.env.local` - Local developer overrides (git-ignored)
  ```properties
  # Override any environment variables for local testing
  VITE_API_BASE_URL=http://custom-api.local/api
  ```

## Loading Priority (highest to lowest)

1. `.env.${mode}.local`
2. `.env.local`
3. `.env.${mode}`
4. `.env`

Where `${mode}` is either `development` or `production` based on the command being run.

## Important Rules

1. Only variables prefixed with `VITE_` are exposed to your React code
2. Variables in higher priority files override lower priority ones
3. `.env.*.local` files should be git-ignored as they're for local overrides

## Available Variables

### Required Variables

- `VITE_API_BASE_URL` - The base URL for API requests

### Optional Variables

- `VITE_APP_TITLE` - Application title

## Usage in Code

Access variables in your React code using `import.meta.env`:

```typescript
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
const publicKey = import.meta.env.VITE_EXPORT_PUBLIC_KEY;
```

## Environment Selection

- Running `npm run dev` uses development environment (`.env.development`)
- Running `npm run build` or `npm run preview` uses production environment (`.env.production`)
