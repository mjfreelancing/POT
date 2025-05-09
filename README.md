# POT

A simple money budget utility to ensure debts are Paid On Time.

# Features

## Accounts Management

Manage your bank accounts with the following capabilities:

- View all accounts in a table format showing:

  - BSB and Account Number
  - Description
  - Current Balance
  - Reserved Amount
  - Allocated Funds
  - Daily Accrual
  - Available Balance

- Create new accounts with:

  - BSB and Account Number
  - Description
  - Initial Balance
  - Reserved Amount

- Edit existing accounts:
  - Update Description
  - Modify Balance
  - Adjust Reserved Amount
  - BSB and Account Number are read-only after creation

### Accessing Account Features

- Create a new account:

  - Click the "Add Account" button at the top of the accounts page
  - A slide-out sheet appears from the right with the account form

- Edit an existing account:
  - Click the "..." menu button in the account's row
  - Select "Edit" from the dropdown menu
  - A slide-out sheet appears from the right with the account form

### Error Handling

The application displays errors in a dismissible notification banner at the top of the screen, such as:

- Account Creation Errors

  - Duplicate BSB/Account number combination
  - Invalid BSB format
  - Duplicate Account description

- Account Edit Errors

  - Concurrent modification detected (someone else modified the account)
  - Invalid balance or reserved amount
  - Account no longer exists

- Loading Errors

  - Network connectivity issues
  - Server availability problems

# Starting the Dockerised server

You can start and stop the Docker containers directly from Visual Studio Code using the predefined tasks. From the root folder:

1. Press `Shift+Ctrl+P` (or `Shift+Cmd+P` on macOS) to open the Command Palette.
2. Type `Run Task` and select it.
3. Choose one of the following tasks:
   - **docker-start-pot**: Starts the Docker containers and builds the images if necessary.
   - **docker-stop-pot**: Stops and removes the Docker containers.

These tasks are defined in the `.vscode/tasks.json` file.

# Starting the client

From the `/Source/Client/pot-react` folder:

- `npm run dev`
- Open the browser at the indicated route, such as `http://localhost:5175/`

# Scripts

## Development

- `npm run dev` - Start the development server on port 5175
  ```bash
  vite --port 5175
  ```

## Production

- `npm run build` - Build the application for production
  ```bash
  tsc -b && vite build
  ```
- `npm run preview` - Preview the production build locally
  ```bash
  vite preview
  ```

## Code Quality

- `npm run lint` - Check code for style issues
  ```bash
  eslint .
  ```
- `npm run lint:fix` - Automatically fix linting issues
  ```bash
  eslint . --fix
  ```
- `npm run lint:sort` - Fix and sort imports
  ```bash
  eslint --fix --fix-type layout,suggestion src/**/*.{ts,tsx}
  ```
- `npm run pretty` - Format code using Prettier
  ```bash
  prettier . --write
  ```

## Testing

- `npm run test` - Run unit tests and coverage
  ```bash
  vitest
  ```
- `npm run test:ui` - Run tests and coverage with UI
  ```bash
  vitest --ui
  ```

## Type Checking

- `npm run type:check` - Verify TypeScript types
  ```bash
  tsc --noEmit
  ```

# Development Configuration

## TypeScript

The project uses a multi-tsconfig setup:

- `tsconfig.json` - Base configuration and path aliases
- `tsconfig.app.json` - Application-specific settings
- `tsconfig.node.json` - Node.js build settings (for Vite config)

## Path Aliases

The `@/*` path alias is configured for importing from the `src` directory:

```typescript
import { Button } from "@/components/ui/button";
```

## ESLint Configuration

The project uses the new flat ESLint config with:

- TypeScript ESLint recommended rules
- React Hooks plugin
- React Refresh plugin
- Simple Import Sort plugin

Additional rules:

- Enforces type over interface
- Enforces import sorting
- Disables react-refresh warnings for constant exports

## Vite Configuration

The development server runs on port 5175 with support for:

- React Fast Refresh
- TailwindCSS
- Path aliases (@/\*)

## Testing

Vitest is configured to:

- Run on port 9527 for UI mode
- Use Istanbul for coverage reporting
- Use JSDOM for DOM simulation
