# POT - Financial Management Made Simple

<img src="AppLogo.png" alt="POT Logo" style="width:200px;"/>

**A comprehensive financial management application to ensure debts are Paid On Time**

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-0.1-green.svg)](https://github.com/mjfreelancing/POT)
[![Docker](https://img.shields.io/badge/docker-supported-blue.svg)](Source/Docker)

## Table of Contents

- [About POT](#about-pot)
  - [Why POT?](#why-pot)
- [Features](#features)
  - [Dashboard](#dashboard)
  - [Financial Projections](#financial-projections)
  - [Accounts Management](#accounts-management)
    - [Accessing Account Features](#accessing-account-features)
    - [Error Handling](#error-handling)
      - [Toast Notifications](#toast-notifications)
      - [Error Sheets](#error-sheets)
      - [Error Types](#error-types)
      - [Error Boundaries](#error-boundaries)
  - [Expenses Management](#expenses-management)
  - [Income Management](#income-management)
  - [Data Management](#data-management)
- [Quick Start Guide](#quick-start-guide)
  - [Clone the repository](#clone-the-repository)
  - [Run the application(s)](#run-the-applications)
    - [Using Docker](#using-docker)
    - [Manually](#manually)
- [Navigation and Usage](#navigation-and-usage)
  - [Application Structure](#application-structure)
  - [Accessibility Features](#accessibility-features)
  - [Environment Configuration](#environment-configuration)
    - [Client Environment Variables](#client-environment-variables)
    - [Server Environment Variables](#server-environment-variables)
- [Scripts](#scripts)
  - [Development](#development)
  - [Production](#production)
  - [Code Quality](#code-quality)
  - [Testing](#testing)
  - [Type Checking](#type-checking)
- [Development Configuration](#development-configuration)
  - [Architecture and Technology Stack](#architecture-and-technology-stack)
  - [UI/UX Design](#uiux-design)
  - [TypeScript Configuration](#typescript-configuration)
  - [Path Aliases](#path-aliases)
  - [ESLint Configuration](#eslint-configuration)
  - [Vite Configuration](#vite-configuration)
  - [Testing](#testing-1)
- [Performance and Monitoring](#performance-and-monitoring)
  - [Performance Optimization](#performance-optimization)
  - [Resource Monitoring](#resource-monitoring)
- [Security and Data Privacy](#security-and-data-privacy)
  - [Data Storage](#data-storage)
  - [Security Considerations](#security-considerations)
  - [Project Structure](#project-structure)
- [License](#license)
- [Acknowledgments](#acknowledgments)
  - [Frontend](#frontend)
  - [Backend](#backend)
  - [DevOps](#devops)
- [Version History](#version-history)
  - [Release Notes](#release-notes)

## About POT

**POT** is a modern financial application designed to help you project your financial status based on current bank account balances and future income (credits) and expenses (debits) to ensure all debts are **Paid On Time**. With interactive visualizations and projections, **POT** gives you a clear picture of your financial future, helping you make informed decisions about your money.

### Why POT?

- 📊 **Visual Financial Projections** - See where your money will be months in advance
- 💸 **Expense Tracking** - Keep tabs on all your recurring and one-time expenses
- 💰 **Income Management** - Track multiple income sources and payment schedules
- 🏦 **Account Management** - Monitor balances across all your financial accounts
- 🔄 **Data Portability** - Export and import your financial data with ease

# Features

POT offers a comprehensive set of financial management features:

## Dashboard

A centralized overview of your financial situation:

- Quick summary of accounts and their balances
- Overview of upcoming expenses
- Quick action buttons for common tasks
- Real-time financial status indicators

## Financial Projections

Visualize your financial future:

- Interactive chart displaying projected account balances
- Configurable projection period (up to 12 months)
- Ability to change the start date for projections
- Visual indicators of potential financial issues

## Accounts Management

Manage your bank accounts with the following capabilities:

- View all accounts in a table format showing:

  - BSB and Account Number
  - Description
  - Current Balance
  - Reserved Amount
  - Accrued Funds
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

POT implements a comprehensive error handling system with multiple notification mechanisms:

#### Toast Notifications

Transient, non-blocking notifications for:

- Success confirmations (e.g., "Account created successfully")
- Warning messages
- Brief error notifications
- Import/Export status updates

#### Error Sheets

Modal error sheets for more serious issues:

- API connection failures
- Data validation errors
- Concurrent modification conflicts
- Server-side errors

#### Error Boundaries

React error boundaries catch and handle unexpected rendering errors to prevent the entire application from crashing. The application uses the `ErrorBoundary` component from `@/components/error/ErrorBoundary` to wrap sections that might throw rendering errors.

## Expenses Management

Manage your recurring and one-time expenses:

- View all expenses in a table format showing:

  - Description
  - Amount
  - Category
  - Frequency (one-time or recurring)
  - Start and end dates for recurring expenses
  - Next due date
  - Associated account

- Create new expenses with:

  - Description
  - Amount
  - Category
  - Frequency options (one-time, daily, weekly, monthly, etc.)
  - Start and end dates (for recurring expenses)
  - Associated account selection

- Edit existing expenses:
  - Update all expense details
  - Modify payment schedules
  - Change associated accounts

## Income Management

Track your income sources:

- View all income entries in a table format showing:

  - Description
  - Amount
  - Category
  - Frequency (one-time or recurring)
  - Start and end dates for recurring income
  - Next expected date
  - Associated account

- Create new income entries with:

  - Description
  - Amount
  - Category
  - Frequency options (one-time, daily, weekly, monthly, etc.)
  - Start and end dates (for recurring income)
  - Associated account selection

- Edit existing income entries:
  - Update all income details
  - Modify income schedules
  - Change associated accounts

## Data Management

POT provides comprehensive data management capabilities for backing up and restoring your financial data:

### Exporting Data

- From the Maintenance page, click "Export Data"
- Choose a location to save the export file
- The exported file will be named: `pot-YYYY-MM-DD_HHMMSS.export`
- The export file contains:
  - All account information and balances
  - Expense definitions and schedules
  - Income definitions and schedules
  - All data is encrypted using RSA encryption

### Importing Data

1. **Preparing for Import**

   - Ensure you have a valid export file (`.export` extension)
   - The server must have the matching RSA private key
   - You can import data while the system has existing data

2. **Import Process**

   - Navigate to the Maintenance page
   - Click "Import Data"
   - Select your export file using the Browse button
   - Click "Import Data" to proceed
   - A success message will show the total number of items imported

3. **Import Validation**

   - The system checks for:
     - File integrity and encryption
     - Data format version compatibility
     - Duplicate account numbers
     - Valid expense/income configurations
   - Any validation errors will be displayed

4. **After Import**
   - The system will show a success message
   - New data will be immediately available
   - Projections will update automatically

### Data Security

- Export files are encrypted using RSA public/private key pairs
- Keys are configured in environment variables
- Export files can only be imported by servers with the matching private key
- Imported data is validated for integrity and correctness

# Quick Start Guide

## **Clone the repository**

```bash
git clone https://github.com/mjfreelancing/POT.git
cd POT
```

## **Run the application(s)**

### **Using Docker**

#### **Option 1 - Server Only**

Run the server in docker:

- Press `Ctrl+Shift+P` to open the Command Palette
- Select "Run Task" and choose "docker-start-pot-server-only"
- Start the client manually:

  - Within the terminal, navigate to `Source/Client/pot-react`
  - Start the client application:

    ```bash
    cd Source/Client/pot-react
    npm install
    npm run dev
    ```

- Open your browser at http://localhost:5175

#### **Option 2 - Client and Server**

Run the client and server in docker:

- Press `Ctrl+Shift+P` to open the Command Palette
- Select "Run Task" and choose "docker-start-pot-client-server"
- Open your browser at http://localhost:5175

### **Manually**

- Start the server found in the `Source/Server` folder:

  ```bash
  # Navigate to the server directory
  cd Source/Server

  # Restore dependencies
  dotnet restore pot.sln

  # Set up the development database (first time only)
  dotnet ef database update --project Pot.Data.Migrations

  # Run the server
  dotnet run --project Pot.AspNetCore
  ```

  The server will start on http://localhost:5242

- Start the client

  - Within the terminal, navigate to `Source/Client/pot-react`
  - Start the client application:

    ```bash
    cd Source/Client/pot-react
    npm install
    npm run dev
    ```

  - Open your browser at http://localhost:5175

# Navigation and Usage

## Application Structure

The POT application is organized into the following main sections:

- **Dashboard** - The landing page showing your overall financial status
- **Projections** - Financial projections based on your accounts and recurring transactions
- **Expenses** - Manage your recurring and one-time expenses
- **Income** - Track your income sources and recurring payments
- **Accounts** - Manage your bank accounts and track balances
- **Maintenance** - Export and import your financial data

## Accessibility Features

POT is built with accessibility in mind:

- Keyboard navigation support throughout the application
- ARIA attributes for screen readers
- Sufficient color contrast ratios for all UI elements
- Responsive design that works on various device sizes
- Focus management for modal dialogs and forms
- Error messages that are clear and descriptive

## Environment Configuration

POT uses environment files for configuration:

### Client Environment Variables

Located in `/Source/Client/pot-react/`:

- `.env` - Base environment variables for all environments
- `.env.development` - Development-specific settings
- `.env.production` - Production build settings

The base `.env` file contains the following environment variables:

```
# Timeout for API requests
VITE_API_TIMEOUT_MS=10000
```

The `.env.development` file contains options for connecting to the server when running locally as well as when running in docker, making it possible to easily switch between the two when applying updates to the server.

When connecting to the server running locally:

```
VITE_API_BASE_URL=http://localhost:5242/api
```

When connecting to the server running in docker:

```
VITE_API_BASE_URL=http://localhost:5241/api
```

The application uses a public/private key pair to encrypt exported data. The `.env.development` file includes the public key:

```
VITE_EXPORT_PUBLIC_KEY=<value here>
```

### Server Environment Variables

The server configuration is managed through environment files and appsettings.json files:

#### **Docker Configuration**

Located in Docker environment files:

- `/Source/Docker/.env` - Base settings
- `/Source/Docker/.env.development` - Development configuration
- `/Source/Docker/.env.production` - Production settings

```bash
# PostgreSQL configuration
POSTGRES_USER=<value>                 # Database user
POSTGRES_PASSWORD=<value>             # Database password

# RSA key for data encryption/decryption
RSA_PRIVATE_KEY=<value>              # Private key for decrypting exported data

# JWT Authentication (if not using default values)
JWT_ISSUER=<value>                   # JWT token issuer
JWT_AUDIENCE=<value>                 # JWT token audience
JWT_SECRET_KEY=<value>               # JWT signing key
```

#### **Local Configuration**

The common settings are located in `Source/Server/Pot.AspNetCore/appsettings.json`

```json
{
  "AllowedHosts": "*",
  "Database": {
    "Host": "localhost", // PostgreSQL server host
    "Username": "postgres", // Database user
    "Password": "password" // Database password
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

The development settings are located in `Source/Server/Pot.AspNetCore/appsettings.Development.json`

```json
{
  "Rsa": {
    "PrivateKey": "<value>" // Private key for decrypting exported data
  },
  "Jwt": {
    "Issuer": "<value>", // JWT token issuer
    "Audience": "<value>", // JWT token audience
    "SecretKey": "<value>" // JWT signing key
  }
}
```

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
  eslint src/**/*.{ts,tsx}
  ```
- `npm run lint:fix` - Automatically fix linting issues
  ```bash
  eslint --fix src/**/*.{ts,tsx}
  ```
- `npm run lint:sort` - Fix and sort imports
  ```bash
  eslint --fix --fix-type layout,suggestion src/**/*.{ts,tsx}
  ```
- `npm run prettier` - Format code using Prettier
  ```bash
  prettier . --write
  ```

## Testing

- `npm run test` - Run unit tests
  ```bash
  vitest
  ```
- `npm run test:ui` - Run tests with UI, API server, and coverage reporting
  ```bash
  vitest --ui --api 9527 --coverage.enabled --coverage.provider=istanbul --coverage.all
  ```

## Type Checking

- `npm run type:check` - Verify TypeScript types
  ```bash
  tsc --noEmit
  ```

# Development Configuration

## Architecture and Technology Stack

POT is built with a modern web development stack:

- **Frontend**: React 18 with TypeScript, built using Vite
- **Backend**: ASP.NET Core API
- **Database**: PostgreSQL
- **Containerization**: Docker for both development and production
- **UI Framework**: Custom components based on shadcn/ui and TailwindCSS
- **State Management**: React Query for server state, React Context and Zustand for local state
- **Routing**: React Router v6

## UI/UX Design

The application features a modern, responsive user interface with:

- Dark and light theme support with system preference detection
- Responsive design that works across different device sizes
- Collapsible sidebar navigation for better space utilization
- Consistent design language across all components
- Toast notifications for success/error feedback
- Loading indicators for all asynchronous operations

## TypeScript Configuration

The project uses a multi-tsconfig setup:

- `tsconfig.json` - Base configuration and path aliases
- `tsconfig.app.json` - Application-specific settings
- `tsconfig.node.json` - Node.js build settings (for Vite config)

The codebase strictly enforces TypeScript best practices:

- Uses types instead of interfaces
- No use of 'any' type allowed
- Strict type checking enabled

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
- Requires braces for all if statements

## Vite Configuration

The development server runs on port 5175 with support for:

- React Fast Refresh
- TailwindCSS
- Path aliases (@/\*)
- Environment variable management

## Testing

Vitest is configured for comprehensive testing:

- Run on port 9527 for UI mode
- Use Istanbul for coverage reporting
- Use JSDOM for DOM simulation
- Component testing with React Testing Library

# Security and Data Privacy

## Authentication and Authorization

POT implements a comprehensive security system combining JWT-based authentication with role-based authorization.

### Implementation Location

#### Client-Side Implementation

The client-side authentication uses React Context for global state management with the following key implementation details:

##### Component Architecture

- AuthProvider wraps the application root for global auth state
- AuthContext exposes tokens, authentication state, login/logout methods
- ProtectedRoute component enforces authentication on sensitive routes
- All components use the useAuth hook to access auth state

##### Token Management Implementation

- AuthTokens type defines access token, refresh token, and expiry
- TokenProvider interface manages token operations (get, refresh, clear)
- Type-safe localStorage wrapper for secure token storage
- Token refresh mechanism with automatic retry of failed requests

##### API Integration

- Axios interceptors automatically add Authorization headers
- 401 responses trigger token refresh flow
- Failed requests are queued and retried after token refresh
- Correlation IDs added to all auth-related requests

##### Centralized Logout

- Global logout manager handles application-wide logout
- Cleans up tokens, state, and redirects to login
- Registered callbacks for component cleanup

Key files and their responsibilities:

- `/Source/Client/pot-react/src/features/auth/` - Core authentication components and hooks
- `/Source/Client/pot-react/src/features/auth/AuthContext.tsx` - Global auth state management
- `/Source/Client/pot-react/src/features/auth/LoginPage.tsx` - Login implementation
- `/Source/Client/pot-react/src/api/hooks/useLogin.ts` - API integration for authentication

Error Handling:

- `/Source/Client/pot-react/src/api/errors/apiErrors.ts` - Centralized error types
- `/Source/Client/pot-react/src/components/feedback/ErrorSheet.tsx` - Error display component
- `/Source/Client/pot-react/src/lib/errors/` - Error utilities and type definitions

Storage and State:

- `/Source/Client/pot-react/src/hooks/useLocalStorage.ts` - Type-safe localStorage wrapper with error handling and logging
- Includes error handling and logging for storage operations
- Type safety for stored values
- Integration with application-wide error handling

API Integration:

- `/Source/Client/pot-react/src/api/hooks/useApi.ts` - Base API hook with error handling
- `/Source/Client/pot-react/src/api/client.ts` - Axios client configuration
- `/Source/Client/pot-react/src/api/interceptors/` - Request/response interceptors

#### Server-Side Implementation

- Authentication:

  - `/Source/Server/Pot.AspNetCore/Features/Auth/` - Authentication endpoints and handlers
  - `/Source/Server/Pot.App/Features/Auth/` - Core authentication business logic
  - `/Source/Server/Pot.AspNetCore/appsettings.json` - JWT configuration

- Authorization:
  - `/Source/Server/Pot.AspNetCore/Security/` - JWT token generation, validation, and permission handlers
  - `/Source/Server/Pot.App/Security/` - Password hashing and crypto utilities
  - `/Source/Server/Pot.Data/Security/` - Security-related database models and permission storage

### Authentication System

#### Token Management

- Short-lived access tokens (60 minute expiry) for API authorization
- Refresh tokens with 30 day expiry
- Automatic token refresh via interceptors with queued request retry
- Secure token storage using type-safe localStorage wrapper
- Global authentication state management through React Context
- Token validation with strict issuer, audience, and signing key checks

#### Authentication Flow

1. Login Process

   - Credential validation and token generation
   - Secure token storage
   - State update and redirection

2. Protected Routes

   - Auth-protected route guards
   - Automatic redirect for unauthenticated users
   - Fresh authentication for sensitive operations

3. Token Lifecycle
   - Automatic refresh of expired tokens
   - Failed request retry with new tokens
   - Secure token cleanup on logout

### Authorization System

The authorization system uses a combination of JWT claims and database-backed permissions for secure access control.

#### Database Structure

The permission system uses four main entities:

1. `UserEntity`

   - Core user information
   - Links to roles via many-to-many relationship

2. `RoleEntity`

   - Named role definitions
   - Links to permissions via many-to-many relationship
   - Many-to-many relationship with users

3. `PermissionEntity`

   - Individual permission definitions
   - Stored as resource:action strings
   - Many-to-many relationship with roles

4. `SiteEntity`
   - Represents a tenant in the system
   - Users and accounts belong to sites

#### Authentication Flow

1. Request arrives with JWT token in Authorization header
2. `JwtSecurityTokenHandler.ValidateToken()` (see `JwtService`) validates the token using parameters configured by `JwtBearerOptionsSetup`:
   - Token lifetime validation
   - Issuer validation against configured value
   - Audience validation against configured value
   - Signature validation using configured signing key
3. JWT claims are extracted with MapInboundClaims=false to preserve original claim types
4. User identity is established from the subject claim (JwtRegisteredClaimNames.Sub)
5. Request proceeds to authorization check

#### Authorization Flow

When an endpoint requires authorization:

1. Request arrives with validated JWT token
2. Framework extracts user identity and claims
3. Authorization middleware checks for required permissions
4. PermissionAuthorizationPolicyProvider creates policy from permission string
5. PermissionAuthorizationHandler evaluates the permission
6. PermissionService loads actual permissions from database
7. Access is granted or denied based on permission match

#### Permission Model

1. Resource-based Permissions

   - Format: `resource:action` (e.g., `account:view`)
   - Granular access control per feature
   - Dynamic policy creation from permission strings

2. Role Structure

   - Admin Role: Full system access to all features
   - Viewer Role: Read-only access across all features (all :view permissions)
   - Custom roles: Can be defined with specific permission sets
   - Role inheritance: Users can have multiple roles
   - Permission aggregation: User's effective permissions are the union of all their roles' permissions

3. Permission Categories

   - Site Management (`site:manage`, `site:view`)
   - User Management (`user:manage`, `user:view`)
   - Account Management (`account:manage`, `account:view`)
   - Expense Management (`expense:manage`, `expense:view`)
   - Income Management (`income:manage`, `income:view`)

4. Endpoint Protection

   To require permissions on an endpoint:

   ```csharp
   routeGroupBuilder
       .MapGet(AccountsEndpoints.GetAll, GetAll.Handler.Invoke)
       .RequireAuthorization("account:view")
   ```

   The permission string automatically becomes a policy requirement through `PermissionAuthorizationPolicyProvider`.

5. Permission and Role Management

   Adding New Permissions:

   - Add permission to database through migration
   - Update role assignments as needed
   - Follow the resource:action naming pattern
   - Use lowercase consistently
   - Consider permission grouping with roles
   - Document the new permission

### Core Security Components

- JWT authentication via `JwtService` and `JwtBearerOptionsSetup`
- Permission-based authorization via `PermissionAuthorizationHandler`
- Role-based access control using database relationships
- Token management through `AuthContext` and `TokenProvider`
- Client-side route protection using `ProtectedRoute` component

### Implementation Guidelines

1. **Permission Implementation**

   - Use lowercase resource:action format for permission strings (e.g., `account:view`)
   - Add new permissions through database migrations
   - Update the permission documentation when adding new ones

2. **Role Implementation**
   - Use the many-to-many relationships in the database for role-permission assignments
   - Implement new roles through database migrations
   - Use `PermissionService` to load user permissions through role relationships

## Data Storage

POT stores all financial data in a PostgreSQL database:

- Account information is stored locally in the database
- No data is sent to external servers
- Import/export files use a proprietary, encrypted, format with data validation

## Security Considerations

- JWT-based authentication for API access
- Docker containers isolate application components
- Database access is restricted to the application
- No sensitive financial data in client-side storage
- All auth operations logged with correlation IDs
- Type-safe implementations throughout

## Project Structure

The POT application follows a clear structure:

```
Source/
├── Client/                  # Frontend React application
│   └── pot-react/
│       ├── src/
│       │   ├── api/         # API clients and hooks
│       │   ├── components/  # Reusable UI components
│       │   ├── features/    # Feature-specific components and logic
│       │   ├── hooks/       # Custom React hooks
│       │   ├── lib/         # Utility functions and helpers
│       │   └── routes/      # Application routing
│       └── ...
├── Docker/                  # Docker configuration
│   ├── Client/              # Client container setup
│   ├── Postgres/            # Database container setup
│   └── Server/              # API server container setup
└── Server/                  # Backend .NET Core application
    ├── Pot.App/             # Core application logic
    ├── Pot.AspNetCore/      # API endpoints and controllers
    ├── Pot.Data/            # Data access and models
    ├── Pot.Data.Migrations/ # Database migrations
    └── Pot.Shared/          # Shared utilities and DTOs
```

# License

POT is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

# Acknowledgments

POT makes use of the following open-source libraries and tools:

## Frontend

- [React](https://reactjs.org/) - UI library
- [TypeScript](https://www.typescriptlang.org/) - Type-safe JavaScript
- [Vite](https://vitejs.dev/) - Frontend build tool
- [TailwindCSS](https://tailwindcss.com/) - Utility-first CSS framework
- [shadcn/ui](https://ui.shadcn.com/) - UI component collection
- [Recharts](https://recharts.org/) - Charting library
- [React Query](https://tanstack.com/query/latest) - Data fetching library
- [React Router](https://reactrouter.com/) - Routing library
- [Vitest](https://vitest.dev/) - Testing framework
- [Zustand](https://github.com/pmndrs/zustand) - State Management library

## Backend

- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Web framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM
- [PostgreSQL](https://www.postgresql.org/) - Database

## DevOps

- [Docker](https://www.docker.com/) - Containerization
- [Docker Compose](https://docs.docker.com/compose/) - Multi-container applications

# Version History

## Release Notes

### v0.1.0 (Current)

- Initial public release
- Core features implemented:
  - Account management
  - Expense management
  - Income management
  - Financial projections
  - Data import/export
- Docker support for development and production
- Modern React frontend with TypeScript
