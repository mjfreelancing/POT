# Docker Setup for POT Project

This guide provides instructions for building, running, and managing the POT project's Dockerized services.

## Deployment Configurations

The POT project supports two deployment configurations:

1. **Server-Only Configuration** (`docker-compose-server-only.yml`):

   - Used for local development
   - Runs PostgreSQL database and .NET server only
   - Client application can be run separately in development mode

2. **Full-Stack Configuration** (`docker-compose-client-server.yml`):
   - Self-contained production deployment
   - Runs PostgreSQL database, .NET server, and React client
   - All components are containerized and integrated

## Directory Structure and Key Files

- **Postgres Service**
  - Build context: `Postgres` directory
  - Dockerfile: `Postgres/Dockerfile`
- **Server Service**
  - Build context: project root (one level up from `Docker`)
  - Dockerfile: `Docker/Server/Dockerfile`
  - Entrypoint script: `Docker/Server/entrypoint.sh`
- **Client Service** (client-server configuration only)
  - Build context: project root (one level up from `Docker`)
  - Dockerfile: `Docker/Client/Dockerfile`
- **Solution File**
  - Located at the project root: `pot.sln`

## Ports

- **PostgreSQL**: Port **5444** (host) mapped to **5432** (container)
- **ASP.NET Server**: Port **5241**
- **React Client** (client-server configuration only): Port **5175** mapped to container port 80

## Environment Variables

### Docker Compose Environment Files

Docker Compose supports environment-specific configuration files:

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

### Configuration Structure

The application uses a hierarchical configuration structure in the appsettings files. When using Docker Compose, environment variables are automatically mapped by ASP.NET Core to this hierarchical structure:

- Database configuration:
  - `POSTGRES_USER` `Database:Username`
  - `POSTGRES_PASSWORD` `Database:Password`
  - `POSTGRES_DB` `Database:DatabaseName`
- RSA configuration:
  - `RSA_PRIVATE_KEY` `Rsa:PrivateKey`

Note: The public key is not configured in the server as it is provided by the client with each request.

### Frontend (Vite/React) `.env` File

Frontend environment variables must be defined in a `.env` file at the root of the React project (`Source/Client/pot-react/`). Only variables prefixed with `VITE_` are available to the frontend code.

While developing the React app use the following (in `.env.development`) to communicate with the API server in the docker container (production data, Postgres listening on port 5444):

```
VITE_API_BASE_URL=http://localhost:5241/api
```

And use this to communicate with the API server running locally (requires another docker container running Postgres that listens on the default port 5432):

```
VITE_API_BASE_URL=http://localhost:5242/api
```

## RSA Encryption for Data Export/Import

The application includes RSA encryption for securing exported data:

- **Public Key**: Provided by the client application for each request
- **Private Key**: Stored separately for security:
  - Development: In `appsettings.Development.json` (excluded from version control)
  - Production: Via environment variable `RSA_PRIVATE_KEY` in Docker

### RSA Key Management

- The React client application requires the public key in its environment configuration
- The server only requires the private key in its configuration
- Each export/import request includes the public key in its headers

## Health Checks

Both configurations include health checks to ensure proper service readiness:

- **PostgreSQL**: Uses `pg_isready` to verify the database is ready
- **ASP.NET Server**: Checks the `_health` endpoint at `http://localhost:5241/_health` (Port 5242 if running locally)
- Services are configured with appropriate dependencies to ensure proper startup order

## Building and Running Docker Services

### Local Development (Server-Only Configuration)

```bash
# Start services
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-server-only.yml up --build -d

# Stop services
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-server-only.yml down
```

### Production Deployment (Client-Server Configuration)

```bash
# Start services
docker-compose --env-file .env --env-file .env.production -p pot -f docker-compose-client-server.yml up --build -d

# Stop services
docker-compose --env-file .env --env-file .env.production -p pot -f docker-compose-client-server.yml down
```

## Managing Docker Containers via Visual Studio Code

Predefined VS Code tasks are available for managing containers:

1. Open the Command Palette (`Shift+Ctrl+P` or `Shift+Cmd+P` on macOS)
2. Select `Run Task`
3. Choose one of the following:

### Available Tasks

- `docker-start-pot-server-only`: Builds and starts the database and server containers (development)
- `docker-stop-pot-server-only`: Stops and removes the database and server containers
- `docker-start-pot-client-server`: Builds and starts all containers (production deployment)
- `docker-stop-pot-client-server`: Stops and removes all containers

These tasks are defined in `.vscode/tasks.json` and automatically use the appropriate environment files and Docker Compose configurations.
