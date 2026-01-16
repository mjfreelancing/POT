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
│   ├── Client/pot-react/           # React frontend application
│   │   ├── src/
│   │   │   ├── features/           # Feature modules (auth, accounts, expenses, etc.)
│   │   │   ├── components/         # Reusable UI components
│   │   │   ├── api/                # API integration layer
│   │   │   ├── lib/                # Utilities and helpers
│   │   │   └── store/              # Global state management
│   │   └── DEVELOPER.md            # Frontend development guide
│   │
│   ├── Server/                     # ASP.NET Core backend
│   │   ├── Pot.AspNetCore/         # API endpoints and configuration
│   │   ├── Pot.App/                # Business logic and services
│   │   ├── Pot.Data/               # EF Core data access
│   │   ├── Pot.Data.Migrations/    # Database migrations
│   │   ├── Pot.Shared/             # DTOs and shared types
│   │   ├── Pot.EmailSender/        # Email infrastructure
│   │   └── DEVELOPER.md            # Backend development guide
│   │
│   └── Docker/                     # Docker configuration
│       ├── docker-compose-client-server.yml
│       ├── .env                    # Base environment variables
│       ├── .env.development        # Development settings
│       └── DEVELOPER.md            # Docker development guide
│
├── Docs/                           # Documentation
│   ├── GETTING-STARTED.md          # Setup and installation
│   ├── FEATURES.md                 # Feature descriptions
│   └── ARCHITECTURE.md             # This file
│
└── README.md                       # Project overview
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
