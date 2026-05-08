# Getting Started with POT

This guide will help you get POT up and running on your local machine for development or testing. It's designed to be thorough and doesn't assume prior knowledge, so experienced developers may be able to skip familiar sections.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

- **Node.js** (v20 or later) - [Download](https://nodejs.org/)
  - Required for frontend development and Docker builds
- **npm** (comes with Node.js)
- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
  - **Only required if** building/running the backend locally outside Docker
  - **Not needed** if using Docker for development (Docker handles the .NET runtime)
- **Git** - [Download](https://git-scm.com/)
  - **Only required if** your IDE doesn't have built-in Git support (VS Code, Visual Studio, etc. include Git)
  - Used to clone the repository and manage source control

### Highly Recommended

- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop/)
  - Simplifies setup and ensures consistent environment
  - **Includes PostgreSQL database** - no separate database installation needed
  - Can run the entire stack (database, backend, frontend) in containers
  - Docker is the recommended approach for development

### Optional (only if not using Docker)

- **PostgreSQL** (v13 or later) - [Download](https://www.postgresql.org/download/)
  - Required for manual setup without Docker

### Verify Installation

After installing the prerequisites, verify they're working:

```bash
# Check Node.js version (should be v20 or later)
node --version

# Check npm version
npm --version

# Check .NET version (should be 9.x)
dotnet --version

# Check Docker is running
docker --version
docker ps

# Check Git
git --version
```

## Clone the Repository

Clone the POT repository to your local machine:

```bash
git clone https://github.com/mjfreelancing/POT.git
cd POT
```

You should now have the POT project structure:

```
POT/
├── Source/
│   ├── Client/pot-react/    # React frontend application
│   ├── Server/              # ASP.NET Core backend
│   └── Docker/              # Docker configuration files
├── Docs/                    # Documentation
└── README.md
```

---

## Configuration

Before running POT, you need to configure environment settings including database credentials, SMTP for email verification, JWT secrets, and CORS settings.

### Step 1: Locate and Create Environment Files

Navigate to the Docker directory:

```bash
cd Source/Docker
```

You'll find these Docker environment files in version control:

- `.env` - Base configuration (database name, Docker Compose project name)
- `.env.development.template` - Starter development overrides you can copy for local use

For local secrets, create your working copy:

- `.env.development` - Development-specific settings actually used by Docker Compose - **create this from the template**

> **Important:** `.env.development` is excluded from version control (`.gitignore`) because it contains sensitive information. Keep `.env.development.template` as the tracked starter file, then copy or rename it to `.env.development` for your machine.

### Step 2: Create `.env.development` File

Copy `Source/Docker/.env.development.template` to `Source/Docker/.env.development`, then edit the copied file as needed.

You can use the template as-is to get the Docker stack running locally. It contains startup-safe defaults for database, CORS, and placeholder SMTP values.

`Source/Docker/.env.development.template` currently contains:

```bash
# Development environment overrides for the Docker full-stack workflow.
# This file is intentionally local-only because it contains secrets.

# Database Configuration
POSTGRES_USER=postgres
POSTGRES_PASSWORD=password

# JWT Configuration
JWT_ISSUER=http://localhost:5241
JWT_AUDIENCE=http://localhost:5241
JWT_SECRET_KEY=REPLACE_THIS_WITH_AN_EXACTLY_128_CHARACTER_RANDOM_SECRET_KEY_FOR_LOCAL_DEVELOPMENT_ONLY_0123456789ABCDEFGHIJKLMNOPQRSTUVW

# SMTP Configuration
# These placeholder values satisfy startup validation only.
# Replace them with real provider credentials before testing email flows.
SMTP_HOST=localhost
SMTP_PORT=2525
SMTP_REQUIRE_TLS=false
SMTP_AUTH_USERNAME=test
SMTP_AUTH_PASSWORD=test
SMTP_FROM_NAME=POT - Do Not Reply
SMTP_FROM_ADDRESS=pot@example.local

# CORS Configuration
CORS_ALLOWED_ORIGINS=http://localhost:5175

# Platform Admin Configuration
# Leave empty for initial setup. Add your user GUID after first signup.
PLATFORM_ADMIN_USERIDS=
```

> **Startup-only option:** If you just need the Docker stack to boot before wiring up a real mail provider, you can temporarily use `SMTP_HOST=localhost`, `SMTP_PORT=2525`, `SMTP_REQUIRE_TLS=false`, `SMTP_AUTH_USERNAME=test`, `SMTP_AUTH_PASSWORD=test`, and `SMTP_FROM_ADDRESS=pot@example.local`. The server only requires these values to be present at startup. Email-dependent flows will still fail until you replace them with real SMTP credentials.

> **Full usage:** Before using signup, password reset, or any other email flow, replace the placeholder SMTP settings with real provider credentials. Replace the placeholder `JWT_SECRET_KEY` with a real random 128-character secret before relying on the environment beyond basic local startup.

### Step 3: Configure Each Setting

Now update each setting in your `.env.development` file:

#### Database Settings

These are already in the template with defaults suitable for Docker:

```bash
POSTGRES_USER=postgres
POSTGRES_PASSWORD=password
```

> **Important:** The `POSTGRES_PASSWORD` must match the password configured in `Source/Docker/Postgres/Dockerfile`. The default value is `password`. If you change the password in the Dockerfile, you must update it here to match, otherwise the application will not be able to connect to the database.

> **Note:** These defaults work for local development. For production, use stronger credentials.

#### JWT Settings

Update these values in your `.env.development`:

```bash
JWT_ISSUER=http://localhost:5241
JWT_AUDIENCE=http://localhost:5241
JWT_SECRET_KEY=<generate-128-character-key>
```

> **Note:** The `JWT_SECRET_KEY` must be **exactly 128 characters** (uses HMACSHA512 algorithm). Generate a random 128-character string using a password generator or random string tool. Both issuer and audience should point to the API server (the service that issues and validates tokens). For production, generate a new 128-character secret key and update both values to your API domain (e.g., both set to `https://api.yourdomain.com`).

#### SMTP Configuration

Update the SMTP settings with your email provider details:

```bash
# SMTP Configuration
SMTP_HOST=your-smtp-server.com          # Your SMTP server hostname
SMTP_PORT=587                            # SMTP port (587 for TLS, 465 for SSL)
SMTP_REQUIRE_TLS=true                    # Use true for secure connection
SMTP_AUTH_USERNAME=your-email@domain.com # Your email address
SMTP_AUTH_PASSWORD=your-password         # Your email password or app password
SMTP_FROM_NAME=POT - Do Not Reply        # Display name for outgoing emails
SMTP_FROM_ADDRESS=your-email@domain.com  # From email address
```

#### CORS Settings

Already configured for local development:

```bash
CORS_ALLOWED_ORIGINS=http://localhost:5175
```

> **Note:** This allows the React frontend (running on port 5175) to make API requests to the backend.

#### Platform Admin

Leave empty for initial setup:

```bash
PLATFORM_ADMIN_USERIDS=
```

> **Note:** After creating your first user account, you'll add your user GUID here to grant platform admin permissions. See [First-Time Configuration](#first-time-configuration) for details.

### Step 4: Create Required Directories

Create the Docker bind-mount directories for persistent storage and backups:

```bash
# From the project root
mkdir -p Source/Docker/postgres-data
```

> **Note:** The PostgreSQL Docker container uses `Source/Docker/postgres-data` as a mounted volume to persist database data between container recreations. Creating this directory before first run avoids bind mount issues.

---

## Next Steps

Once configuration is complete, proceed to:

1. **[Docker Setup](DOCKER-SETUP.md)** (recommended) - Run POT using Docker containers
2. **[Local Setup](LOCAL-SETUP.md)** - Run API and React locally (with or without Docker PostgreSQL)
3. **[First-Time Configuration](FIRST-TIME-SETUP.md)** - Create your first user and configure platform admin access

---
