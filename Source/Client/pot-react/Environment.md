# Environment Configuration

This React application uses Vite's built-in environment variable handling. Here's how it works:

## Environment Files

- `.env` - Default variables, shared across all environments
- `.env.development` - Development-specific variables (used with `npm run dev`)
- `.env.production` - Production-specific variables (used with `npm run build` or `npm run preview`)
- `.env.local` - Local overrides (optional, git-ignored)

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
- `VITE_EXPORT_PUBLIC_KEY` - RSA public key for data export/import. Sent to the server as a header called 'export-public-key'

### RSA Encryption Details

The RSA encryption flow is as follows:

1. The client stores only the public key in its environment configuration
2. When making export/import requests, the client sends the public key in the request headers
3. The server uses its private key (from its own configuration) along with the provided public key to perform encryption/decryption

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
