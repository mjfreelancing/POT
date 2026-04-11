# POT Architecture

This document provides a high-level overview of POT's technical architecture.

## Technology Stack

### Frontend

- **React 19** - Modern UI library with latest features
- **TypeScript 5** - Type-safe development
- **Vite 6** - Fast build tool and dev server
- **TailwindCSS** - Utility-first CSS framework
- **shadcn/ui** - High-quality, customizable UI components
- **React Router v7** - Client-side routing
- **React Query** - Server state management and caching
- **Zustand** - Global state management
- **React Hook Form** - Form handling
- **Zod** - Schema validation
- **Recharts** - Data visualization

### Backend

- **ASP.NET Core** - High-performance web framework
- **.NET 9** - .NET runtime
- **Minimal APIs** - Lightweight API endpoints
- **Entity Framework Core** - Object-relational mapping
- **PostgreSQL** - Relational database
- **FluentValidation** - Input and business validation
- **MailKit** - Email sending
- **JWT** - Token-based authentication

### DevOps

- **Docker & Docker Compose** - Containerization
- **nginx** - Web server for production frontend
- **Git** - Version control

## Project Structure

```
POT/
├── Source/
│   ├── Client/pot-react/                     # React frontend application
│   │   ├── src/
│   │   │   ├── features/                     # Feature modules (auth, accounts, expenses, etc.)
│   │   │   ├── components/                   # Reusable UI components
│   │   │   ├── api/                          # API integration layer
│   │   │   ├── lib/                          # Utilities and helpers
│   │   │   └── store/                        # Global state management
│   │   └── DEVELOPER.md                      # Frontend development guide
│   │
│   ├── Server/                               # ASP.NET Core backend
│   │   ├── Pot.AspNetCore/                   # API endpoints and configuration
│   │   ├── Pot.AspNetCore.Tests/             # Unit tests for API-layer components/services (non-hosted)
│   │   ├── Pot.AspNetCore.Integration.Tests/ # Hosted API integration tests
│   │   ├── Pot.App/                          # Business logic and services
│   │   ├── Pot.App.Tests/                    # Unit tests for app/business layer
│   │   ├── Pot.Data/                         # EF Core data access
│   │   ├── Pot.Data.Tests/                   # Unit tests for data layer
│   │   ├── Pot.Data.Migrations/              # Database migrations
│   │   ├── Pot.Shared/                       # DTOs and shared types
│   │   ├── Pot.EmailSender/                  # Email infrastructure
│   │   ├── Pot.TestUtils/                    # Shared test helpers and utilities
│   │   └── DEVELOPER.md                      # Backend development guide
│   │
│   └── Docker/                               # Docker configuration
│       ├── docker-compose-client-server.yml
│       ├── .env                              # Base environment variables
│       ├── .env.development                  # Development settings
│       └── DEVELOPER.md                      # Docker development guide
│
├── Docs/                                     # Documentation
│   ├── GETTING-STARTED.md                    # Setup and installation
│   ├── FEATURES.md                           # Feature descriptions
│   └── ARCHITECTURE.md                       # This file
│
└── README.md                                 # Project overview
```

## Frontend Architecture

The frontend follows a **feature-based architecture** where related code is grouped by feature rather than by technical type.

**Key Patterns:**

- Feature modules in `src/features/` (auth, accounts, expenses, income, etc.)
- Centralized API layer with React Query
- Type-safe API communication with custom Result pattern
- Global state with Zustand, server state with React Query
- Component composition with shadcn/ui primitives

**For detailed frontend patterns:** See `Source/Client/pot-react/DEVELOPER.md`

## Backend Architecture

The backend uses a **hybrid architecture** combining layered and feature-based approaches:

**Layered structure:**

- **Pot.AspNetCore** - API endpoints, middleware, configuration
- **Pot.App** - Business logic, services, features
- **Pot.Data** - Entity Framework Core, data access
- **Pot.Shared** - DTOs, contracts shared between layers

**Feature-based organization within each layer:**

- Each layer organizes code by business feature (Auth, Expenses, Income, etc.)
- Features span multiple layers but maintain clear separation of concerns

**Key Patterns:**

- Minimal APIs for endpoint definition
- FluentValidation for input validation
- EF Core for database operations
- JWT-based authentication and authorization

**For detailed backend patterns:** See `Source/Server/DEVELOPER.md`

## Cashflow Metrics Model

POT uses a dual-metric model for expense accrual behavior:

- Dynamic accrual (`DailyExpenseAccrual`) for operational projection simulation.
- Stable accrual (`StableExpenseAccrual`) for long-run daily funding guidance.

Both metrics are computed server-side by accrual calculators to keep behavior deterministic across API and UI.

Accrual policy is explicit per expense:

- `Automatic` enables accrual contribution.
- `None` disables accrual contribution while retaining due-date scheduling and balance debits.

## Cashflow Policy Foundations

POT uses an obligation-first cashflow model.

### Core Objective

Prioritize upcoming required payments and derive daily guidance from those obligations.

### Foundational Approach

1. Use cash-basis forecasting as the primary decision lens.
2. Model income and expenses as dated obligations.
3. Use running balance projection as the primary truth source.
4. Protect mandatory obligations before discretionary spending.
5. Present a stable daily funding target for user planning and a dynamic operational accrual metric for simulation behavior.

### Decision-Ready Outputs

At product level, the model is expected to support these outputs:

1. Stable daily funding requirement.
2. Available (safe-to-spend) balance.
3. Upcoming risk dates from projected balance dips.
4. Forecast runway under current assumptions.

### Why This Model

1. Handles mixed frequencies, including one-time obligations.
2. Prioritizes due-date reliability over category-only budgeting.
3. Produces actionable daily guidance while preserving operational simulation detail.

### Core Guardrails

1. Do not mix planned obligations with already-paid obligations in the same state model.
2. Keep mandatory and optional spending concerns separate.
3. Recompute daily metrics whenever balance, amount, due date, schedule, or policy changes.
4. Use rolling forecast windows instead of static month snapshots.
5. Make calculation assumptions explicit in user and developer documentation.

### Sinking Fund Semantics (High Level)

1. Each obligation accrues toward a due-date target over an accrual cycle.
2. Reserved and accrued amounts restrict spendable funds; they are not double-counting.
3. Payment events reduce both ledger balance and obligation reserve, then start the next cycle when applicable.

## Testing Architecture

Server tests are split by intent to keep boundaries clear and test execution predictable.

### Unit Test Projects

- `Pot.App.Tests` - business logic and app-layer behavior in isolation
- `Pot.Data.Tests` - data-layer behavior/specifications in isolation
- `Pot.AspNetCore.Tests` - API-layer units (services/components) without booting the full HTTP pipeline

### Integration Test Project

- `Pot.AspNetCore.Integration.Tests` - hosted API tests using `WebApplicationFactory<Program>` + real `HttpClient`, including endpoint handler behavior
- Focus areas include middleware, CORS, rate limiting, security headers, endpoint method contracts (`405`), and ProblemDetails contracts

### Unit vs Integration Decision Rule

- **Unit test:** execute class/method directly with in-process test doubles; no hosted API required
- **Integration test:** boot `Program` and verify behavior through HTTP boundary (status, headers, response body contract)

Shared test helpers live in `Pot.TestUtils` and are referenced by test projects.

## Database

**PostgreSQL** is used for data persistence with Entity Framework Core managing:

- Schema migrations
- Entity relationships
- Query generation
- Transaction management

Database migrations are managed in the `Pot.Data.Migrations` project.

## Authentication & Authorization

- **JWT tokens** with refresh token support
- **Email verification** via OTP (One-Time Password)
- **Platform admin approval** for new user accounts
- **Role-based permissions** using `resource:action` format
- **Site-based isolation** for multi-tenant support

## Docker Setup

POT runs in Docker containers for consistent development and deployment:

- **PostgreSQL container** - Database
- **ASP.NET Core container** - Backend API
- **nginx + React container** - Frontend

**For Docker details:** See `Source/Docker/DEVELOPER.md`

## Development Workflow

1. **Clone the repository**
2. **Configure environment** - Create `Source/Docker/.env.development` with:
   - Database credentials
   - SMTP settings (for email verification)
   - JWT configuration
   - CORS settings
3. **Create required directories** - `Source/Docker/postgres-data/`
4. **Start services** - Docker containers (recommended) or run manually
5. **Access application** - Navigate to http://localhost:5175
6. **First-time setup** - Create user account and configure platform admin

**For complete setup:** See [Getting Started](GETTING-STARTED.md)

## API Communication

The frontend communicates with the backend via RESTful APIs:

- Base URL: `/api` (proxied to backend in development)
  - Docker: `http://localhost:5241` (via docker-compose)
  - Local: `http://localhost:5242` (via Visual Studio/dotnet run)
- Authentication: JWT Bearer tokens
- Content-Type: `application/json`
- Error handling: RFC 7807 Problem Details

## State Management Strategy

- **Server state** - React Query (API data, caching, synchronization)
- **Global state** - Zustand (authentication, user preferences)
- **Local state** - React Context (feature-specific state)
- **Form state** - React Hook Form (form inputs, validation)

## Build & Deployment

**Development:**

- Frontend: Vite dev server with hot module replacement (HMR)
- Backend: Docker container with ASP.NET Core runtime OR `dotnet run` locally
- Database: Docker PostgreSQL container OR local PostgreSQL installation

**Production:**

- Frontend: Vite production build → Docker container with nginx serving static files
- Backend: Docker container with ASP.NET Core runtime (framework-dependent deployment)
- Database: Docker PostgreSQL with persistent volume mounts
- Orchestration: Docker Compose

> **Note:** Development supports both Docker (recommended) and manual setup. Production uses Docker containers for all services.

---

**For detailed technical information:**

- [Frontend Development Guide](../Source/Client/pot-react/DEVELOPER.md)
- [Backend Development Guide](../Source/Server/DEVELOPER.md)
- [Docker Development Guide](../Source/Docker/DEVELOPER.md)
