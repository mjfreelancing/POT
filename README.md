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

#### Error Types

The application handles various error scenarios:

- Account Management Errors

  - Duplicate BSB/Account number combination
  - Invalid BSB format
  - Duplicate Account description
  - Concurrent modification detected
  - Invalid balance or reserved amount
  - Account no longer exists

- Expense & Income Management Errors

  - Invalid amount values
  - Missing required fields
  - Date range errors (end date before start date)
  - Overlapping recurring expenses
  - Frequency configuration errors

- Data Import/Export Errors

  - Invalid file format
  - Corrupted data
  - Incompatible version
  - File system permission issues

- API and Network Errors
  - Network connectivity issues
  - Server availability problems
  - Authentication failures
  - Rate limiting errors

#### Error Boundaries

React error boundaries catch and handle unexpected rendering errors to prevent the entire application from crashing.

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

Secure and flexible data handling:

- Export all financial data to a portable file format

  - Creates a secure backup of your financial information
  - Includes all accounts, expenses, and income data

- Import previously exported data
  - Restore your financial setup from backups
  - Transfer data between different installations

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

- Start the server found in the `Source/Server` folder.

  - TODO: Update instructions here

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

#### **Docker Configuration**

Located in Docker environment files:

- `/Source/Docker/.env` - Base settings
- `/Source/Docker/.env.development` - Development configuration
- `/Source/Docker/.env.production` - Production settings

```
POSTGRES_USER=<value here>
POSTGRES_PASSWORD=<value here>
RSA_PRIVATE_KEY=<value here>

```

#### **Local Configuration**

The common settings are located in `Source/Server/Pot.AspNetCore/appsettings.json`

```
{
  "AllowedHosts": "*",
  "Database": {
    "Host": "localhost",
    "Username": "postgres",
    "Password": "password"
  }
}

```

The development settings are located in `Source/Server/Pot.AspNetCore/appsettings.Development.json`

```
{
  "Rsa": {
    "PrivateKey": "<value here>"
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

# Performance and Monitoring

## Performance Optimization

POT is optimized for efficient operation:

- **Code Splitting**: Lazy-loaded components reduce initial bundle size
- **Memoization**: React memo and useMemo optimize rendering
- **Virtualization**: Efficient rendering of large data sets
- **API Caching**: React Query reduces unnecessary API calls
- **Database Indexes**: Key fields are indexed for faster queries

## Resource Monitoring

Monitor POT's resource usage:

- **Docker Stats**: Monitor container resource usage with:
  ```bash
  docker stats pot-client pot-server pot-postgres
  ```
- **Client Performance**: Use browser DevTools performance panel
- **Server Performance**: Check ASP.NET Core logs for request durations

# Security and Data Privacy

## Data Storage

POT stores all financial data in a PostgreSQL database:

- Account information is stored locally in the database
- No data is sent to external servers
- Import/export files use a proprietary, encrypted, format with data validation

## Security Considerations

- Docker containers isolate application components
- Database access is restricted to the application
- No sensitive financial data is stored in client-side storage

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
