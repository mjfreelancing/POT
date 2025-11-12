# Environment Configuration Guide

This document explains how environment configuration works in the POT React application. We use Vite's built-in environment variable handling, which provides a flexible and secure way to manage different environments.

## Environment Files Overview

The application uses multiple environment files for different scenarios:

- `.env` - Base configuration, shared across all environments

  ```properties
  # API request timeout in milliseconds (30 seconds for Azure cold starts)
  VITE_API_TIMEOUT_MS=30000
  ```

- `.env.development` - Development-specific settings

  ```properties
  # Development API URL (local ASP.NET server)
  VITE_API_BASE_URL=http://localhost:5242/api

  # Or for Docker-hosted API
  # VITE_API_BASE_URL=http://localhost:5241/api
  ```

- `.env.production` - Production settings (not currently used, built into Docker images)

  ```properties
  # Production API URL is configured at Docker build time
  # See Docker/Client/Dockerfile for --build-arg VITE_API_BASE_URL
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

- `VITE_API_BASE_URL` - The base URL for API requests (e.g., `http://localhost:5242/api`)

### Optional Variables

- `VITE_API_TIMEOUT_MS` - Request timeout in milliseconds (default: 30000 for Azure cold starts)

## Usage in Code

Access variables in your React code using `import.meta.env`:

```typescript
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
const apiTimeout = import.meta.env.VITE_API_TIMEOUT_MS;
```

## Docker Build Configuration

For production Docker builds, environment variables are passed as build arguments:

```bash
docker build \
  --build-arg VITE_API_BASE_URL=https://api.yourdomain.com/api \
  --build-arg VITE_API_TIMEOUT_MS=30000 \
  -t pot-client:latest \
  -f Docker/Client/Dockerfile .
```

These build arguments are embedded into the static JavaScript bundle at build time and cannot be changed at runtime.

## Environment Selection

- Running `npm run dev` uses development environment (`.env.development`)
- Running `npm run build` or `npm run preview` uses production environment (`.env.production`)
