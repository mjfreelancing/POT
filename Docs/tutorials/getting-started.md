# Getting Started with POT

This guide will help you get POT up and running on your local machine for development or testing. It's designed to be thorough and doesn't assume prior knowledge, so experienced developers may be able to skip familiar sections.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

- **Node.js** (>= 22.22.0 required; Node 24.x LTS recommended) - [Download](https://nodejs.org/)
  - Required for frontend development and Docker builds
- **npm** (comes with Node.js)
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
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
# Check Node.js version (should be >= 22.22.0; Node 24.x LTS recommended)
node --version

# Check npm version
npm --version

# Check .NET version (should be 10.x)
dotnet --version

# Check Docker is running
docker --version
docker ps

# Check Git
git --version
```

### Tooling Version Reference

POT is built against the following tooling versions. **Required** is the minimum the code enforces; **Recommended / verified** is what the maintainers build and test with. Planning and decision records under `Docs/Future/` may quote older versions as of their writing date — this table reflects the current codebase.

| Tool                     | Required             | Recommended / verified                                                                                       |
| ------------------------ | -------------------- | ------------------------------------------------------------------------------------------------------------ |
| Node.js                  | `>= 22.22.0`         | Node **24.x LTS** (24.15.0)                                                                                  |
| npm                      | bundled with Node.js | bundled with Node 24 (npm 11.x)                                                                              |
| .NET SDK                 | 10.0                 | 10.0                                                                                                         |
| Docker client base image | —                    | `node:24-alpine`                                                                                             |
| Docker server base image | —                    | `mcr.microsoft.com/dotnet/sdk:10.0-alpine` (build) · `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` (runtime) |

Where these are declared:

- **Node.js `engines`** — `Source/Client/pot-react/package.json`
- **.NET target framework** — `Source/Server/Directory.Build.props` (`net10.0`)
- **Docker base images** — `Source/Docker/Client/Dockerfile` and `Source/Docker/Server/Dockerfile` (`ARG DOTNET_VERSION=10.0`)

> **Why Node 24.15.0 and not just 22.22.0?** jsdom 30 — the client unit-test DOM — declares engines `^22.22.2 || ^24.15.0 || >=26.0.0`. On earlier Node 24.x (e.g. 24.7.0) `npm ci` prints an `EBADENGINE` warning. It is cosmetic, but using 24.15.0+ keeps installs warning-free. Docker client builds use `node:24-alpine` and are unaffected.

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

Three environment files matter in `Source/Docker`, and two of them are tracked:

| File                        | Tracked | Purpose                                                  |
| --------------------------- | ------- | -------------------------------------------------------- |
| `.env`                      | yes     | Compose project name and database name                   |
| `.env.development.template` | yes     | The starter overrides, and the only copy of the key list |
| `.env.development`          | no      | Your working copy, created from the template             |

### Step 2: Create `.env.development`

```powershell
cd Source/Docker
Copy-Item .env.development.template .env.development
```

The template's defaults boot the stack as they are: database credentials, CORS and placeholder SMTP
values that satisfy startup validation. Its comments explain each key. The copy is git-ignored because it
holds your local settings; never put a real credential in the tracked template.

### Step 3: Change what you need

| Setting                      | What to change for a local Docker run                                                                                    |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `JWT_SECRET_KEY`             | Replace the placeholder with exactly 128 random characters before relying on the environment for anything beyond a boot  |
| `SMTP_*`                     | Replace the placeholders with real provider credentials before testing sign-up, password reset, invitations or reminders |
| `POSTGRES_PASSWORD`          | Only if you changed the password in `Source/Docker/Postgres/Dockerfile`; the two must match                              |
| `CORS_ALLOWED_ORIGINS`       | Already correct for `http://localhost:5175`                                                                              |
| `PLATFORM_ADMIN_USERIDS`     | Leave empty for now; add your `RowId` after sign-up — see [First-Time Configuration](first-time-setup.md)                |
| `JWT_ISSUER`, `JWT_AUDIENCE` | Already correct for `http://localhost:5241`; they must equal the URL the API is reached at                               |

What each value does and which values must agree: [Configuration](../reference/configuration.md).

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

1. **[Docker Setup](../how-to/run-the-docker-stack.md)** (recommended) - Run POT using Docker containers
2. **[Local Setup](../how-to/run-locally-without-docker.md)** - Run API and React locally (with or without Docker PostgreSQL)
3. **[First-Time Configuration](first-time-setup.md)** - Create your first user and configure platform admin access

---
