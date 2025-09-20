# Gemini Code Assistant Context

This document provides context for the Gemini code assistant to understand the "POT" project.

## Project Overview

POT (Paid On Time) is a full-stack financial management application designed to help users project their financial status based on bank account balances, income, and expenses.

- **Backend:** ASP.NET Core (C#) API that provides data to the client. It uses Entity Framework Core for data access. The main projects are `Pot.AspNetCore` (the web host), `Pot.App` (business logic), and `Pot.Data` (data layer).
- **Frontend:** A single-page application (SPA) built with React and TypeScript, using Vite as the build tool. It features a modern UI using TailwindCSS and shadcn/ui components.
- **Database:** PostgreSQL is used for data storage.
- **Containerization:** The entire application (client, server, database) is designed to be run using Docker and Docker Compose, which is the recommended method for development and production.

## Building and Running

The primary method for running the application is via Docker Compose.

### Docker (Recommended)

- **Services:** The `docker-compose-client-server.yml` defines three services:
  - `pot-postgres`: The PostgreSQL database.
  - `pot-aspnet`: The .NET backend server.
  - `pot-react`: The React frontend client.
- **Run all services:** From the project root, execute:
  ```bash
  docker-compose -f "Source\Docker\docker-compose-client-server.yml" up --build
  ```
- **Access:**
  - **Client:** `http://localhost:5175`
  - **Server API:** `http://localhost:5241`

### Manual (Local Development)

#### Backend (.NET)

1.  **Navigate to the server directory:**
    ```bash
    cd Source/Server
    ```
2.  **Restore dependencies:**
    ```bash
    dotnet restore pot.sln
    ```
3.  **Run the web application:**
    ```bash
    dotnet run --project Pot.AspNetCore
    ```
    The API will be available at `http://localhost:5242`.

#### Frontend (React)

1.  **Navigate to the client directory:**
    ```bash
    cd Source/Client/pot-react
    ```
2.  **Install dependencies:**
    ```bash
    npm install
    ```
3.  **Run the development server:**
    ```bash
    npm run dev
    ```
    The client will be available at `http://localhost:5175`.

## Key Scripts (Frontend)

All frontend scripts are run from the `Source/Client/pot-react` directory.

- **`npm run dev`**: Starts the Vite development server.
- **`npm run build`**: Builds the application for production.
- **`npm run lint`**: Lints the TypeScript and TSX files.
- **`npm run test`**: Runs unit tests using Vitest.
- **`npm run type:check`**: Performs a TypeScript type check without emitting files.

## Development Conventions

### Backend

- **Code Organization:**

  - Follows standard C# and ASP.NET Core conventions
  - Uses an `.editorconfig` file for code styling
  - Modular solution structure:
    - `Pot.AspNetCore`: Web host and API endpoints
    - `Pot.App`: Core business logic and services
    - `Pot.Data`: Entity Framework context and models
    - `Pot.Data.Migrations`: Database migrations
    - `Pot.Shared`: DTOs and shared utilities

- **Database Management:**

  - Entity Framework Core for data access
  - Code-first migrations in `Pot.Data.Migrations`
  - Migration commands:

    ```bash
    # Add a new migration
    dotnet ef migrations add MigrationName --project Pot.Data.Migrations

    # Update database to latest
    dotnet ef database update --project Pot.Data.Migrations

    # Generate SQL script
    dotnet ef migrations script --project Pot.Data.Migrations
    ```

  - Each migration includes:
    - Up() method for applying changes
    - Down() method for reverting changes
    - Generated SQL for review

- **Testing Practices:**
  - Unit tests for business logic
  - Integration tests for API endpoints
  - Test data builders for complex objects
  - Mock repositories for data access
  - Fixtures for common test scenarios
  - Recommended test command:
    ```bash
    dotnet test --collect:"XPlat Code Coverage"
    ```

### Frontend

- **Code Quality:**

  - Uses `prettier` for code formatting
  - ESLint for linting with custom rules
  - Component-based architecture
  - Path alias `@/*` points to `src` directory

- **Feature Organization:**

  - `src/features/`: Feature-specific components and logic
    - `auth/`: Authentication and authorization
    - `accounts/`: Account management
    - `expenses/`: Expense tracking
    - `income/`: Income management
    - `projections/`: Financial projections

- **State Management:**
  - React Query for server state
  - Zustand for global UI state
  - React Context for localized state
  - Strict TypeScript usage

### Security Model

- **Authentication:**

  - JWT-based authentication
  - Secure token storage
  - Automatic token refresh
  - Session management

- **Authorization:**

  - Role-based access control
  - Feature-level permissions
  - Secure route guards

- **Data Protection:**

  - RSA encryption for exports
  - HTTPS-only communication
  - SQL injection prevention
  - XSS protection
  - CSRF protection

- **Audit Trail:**
  - Activity logging
  - Change tracking
  - Error logging
  - Security event monitoring

### General

- The `README.md` is the primary source of truth
- Docker is the preferred environment
- Consistent coding standards across all projects
- Regular security updates and dependency scanning
