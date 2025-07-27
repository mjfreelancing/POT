# Docker Setup for POT Project

This guide provides instructions for building, running, and managing the POT project's Dockerized services.

## Directory Structure and Key Files

- **Postgres Service**
  - Build context: `Postgres` directory
  - Dockerfile: `Postgres/Dockerfile`
- **Server Service**
  - Build context: project root (one level up from `Docker`)
  - Dockerfile: `Docker/Server/Dockerfile`
  - Entrypoint script: `Docker/Server/entrypoint.sh`
- **Solution File**
  - Located at the project root: `pot.sln`

## Ports

- The Dockerized server is exposed on port **5241**. Access the API at `http://localhost:5241/` when running via Docker Compose.
- The Dockerized Postgres database is exposed on port **5444** (host) mapped to **5432** (container). Connect to `localhost:5444` from database tools on the host.
- The local development version of the server (when running directly with .NET tooling) uses port **5242**, as defined in the server's `launchSettings.json`.

## Environment Variables

### Docker Compose Environment Files

Docker Compose supports environment-specific configuration files, similar to ASP.NET Core's appsettings pattern:

- **`.env`** - Default/shared environment variables (committed to version control)
- **`.env.development`** - Development-specific overrides (excluded from version control)
- **`.env.production`** - Production-specific overrides (excluded from version control)
- **`.env.local`** - Local developer overrides (excluded from version control)

#### Default `.env` File

Contains non-sensitive variables that are safe to commit:

- `COMPOSE_PROJECT_NAME`: Prefix for Docker resources (containers, networks, volumes)
- `POSTGRES_DB`: Default database name

#### Environment-Specific Files

Contain sensitive or environment-specific values:

- `POSTGRES_USER`: Database user account
- `POSTGRES_PASSWORD`: Database password
- `RSA_PRIVATE_KEY`: The RSA private key used for decrypting export data

### Using Environment-Specific Files

#### Development (Default)

```bash
# Uses .env + .env.development
docker-compose --env-file .env --env-file .env.development up -d --build
```

#### Production

```bash
# Uses .env + .env.production
docker-compose --env-file .env --env-file .env.production up -d --build
```

**Important Security Notes**:

- Environment-specific files (`.env.development`, `.env.production`, `.env.local`) are excluded from version control
- In production deployments, sensitive values should be managed through secure secret management systems
- The `.env.production` file is a template - replace placeholder values with actual production secrets

### Configuration Structure

The application uses a hierarchical configuration structure in the appsettings files. When using Docker Compose, environment variables are automatically mapped by ASP.NET Core to this hierarchical structure:

- Database configuration:
  - `POSTGRES_USER` → `Database:Username`
  - `POSTGRES_PASSWORD` → `Database:Password`
  - `POSTGRES_DB` → `Database:DatabaseName`
- RSA configuration:
  - `RSA_PRIVATE_KEY` → `Rsa:PrivateKey`

Note: The public key is not configured in the server as it is provided by the client with each request.

### Frontend (Vite/React) `.env` File

Frontend environment variables must be defined in a `.env` file at the root of the React project (`Source/Client/pot-react/`). Only variables prefixed with `VITE_` are available to the frontend code.

Example:

```
VITE_API_BASE_URL=http://localhost:5241/api
```

## RSA Encryption for Data Export/Import

The application includes RSA encryption for securing exported data:

- **Public Key**: Provided by the client application for each request, not stored in server configuration
- **Private Key**: Stored separately for security:
  - Development: In `appsettings.Development.json` (excluded from version control)
  - Production: Via environment variable `RSA_PRIVATE_KEY` in Docker

### RSA Key Management

- The React client application requires the public key in its environment configuration
- The server only requires the private key in its configuration
- Each export/import request includes the public key in its headers

## Building and Running Docker Services

- **Development Environment**:

  ```bash
  # Start services
  docker-compose -p pot -f docker-compose.yml --env-file .env --env-file .env.development up --build -d

  # Stop services
  docker-compose -p pot -f docker-compose.yml down
  ```

- **Production Environment**:

  ```bash
  # Start services
  docker-compose -p pot -f docker-compose.yml --env-file .env --env-file .env.production up --build -d

  # Stop services
  docker-compose -p pot -f docker-compose.yml down
  ```

These commands are encapsulated in VS Code tasks for convenience. See the "Managing Docker Containers via Visual Studio Code" section below for details.

## Managing Docker Containers via Visual Studio Code

Predefined VS Code tasks are available for managing containers:

1. Open the Command Palette (`Shift+Ctrl+P` or `Shift+Cmd+P` on macOS)
2. Select `Run Task`
3. Choose one of the following:

### Development Tasks (Default)

- `docker-start-pot`: Builds and starts containers using development environment
- `docker-stop-pot`: Stops and removes containers using development environment

### Production Tasks

- `docker-start-pot-production`: Builds and starts containers using production environment
- `docker-stop-pot-production`: Stops and removes containers using production environment

These tasks are defined in `.vscode/tasks.json` and automatically use the appropriate environment files.
