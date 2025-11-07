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

### Configuration Structure

The application uses a hierarchical configuration structure in the appsettings files. When using Docker Compose, environment variables are automatically mapped by ASP.NET Core to this hierarchical structure:

- Database configuration:
  - `POSTGRES_USER` `Database:Username`
  - `POSTGRES_PASSWORD` `Database:Password`
  - `POSTGRES_DB` `Database:DatabaseName`

### Frontend (Vite/React) Environment Configuration

The client application uses Vite environment variables for configuration. Understanding how these work across different deployment scenarios is critical.

#### How Vite Environment Variables Work

**Important**: Vite environment variables are **build-time values**, NOT runtime values. They are:

- Embedded into the JavaScript bundle during `npm run build`
- Baked into the compiled code and cannot be changed after build
- Read from `.env` files during the build process
- Only variables prefixed with `VITE_` are exposed to the application code

#### Environment File Hierarchy

Vite loads environment files in this order (later files override earlier ones):

1. `.env` - Base configuration (all environments)
2. `.env.production` - Production builds (`npm run build`)
3. `.env.development` - Development builds (`npm run dev`)
4. `.env.local` - Local overrides (never committed to git)

**Current Configuration**:

**`.env`** (Base - used by all builds):

```bash
VITE_API_TIMEOUT_MS=30000
# Note: VITE_API_BASE_URL is NOT defined here to allow environment-specific files to set it
```

**`.env.production`** (Used by `npm run build` - Docker builds use this):

```bash
# For local Docker Compose: /api (nginx proxy forwards /api/* to server container)
VITE_API_BASE_URL=/api

# For Azure Container Apps: Use --build-arg to override with full URL
# docker build --build-arg VITE_API_BASE_URL=https://pot-api-prod.../api
```

**`.env.development`** (Used by `npm run dev` - local development outside Docker):

```bash
# For local development with API in Docker container
VITE_API_BASE_URL=http://localhost:5241/api

# Or for local development with API running locally (port 5242)
# VITE_API_BASE_URL=http://localhost:5242/api
```

#### Three Deployment Scenarios Explained

**Scenario 1: Local Docker Compose (Full Stack)**

- **Command**: VS Code task `docker-start-pot-client-server` or `docker-compose build`
- **Build mode**: `npm run build` (production mode)
- **Environment file used**: `.env.production`
- **VITE_API_BASE_URL value**: `/api` (relative path)
- **Why `/api`?**:
  - React app runs in nginx container at `http://localhost:5175`
  - User makes request to `/api/auth/login`
  - Browser sends to `http://localhost:5175/api/auth/login`
  - nginx proxy rule forwards `/api/*` → `http://server:5241/api/*` (server container)
  - Containers share Docker network, can communicate via service names
- **Dockerfile behavior**: No `--build-arg` provided, uses `.env.production` file
- **nginx configuration**: `nginx.conf` with API proxy enabled

**Scenario 2: Azure Container Apps (Production Cloud)**

- **Command**: `docker build --build-arg VITE_API_BASE_URL=https://pot-api-prod.../api`
- **Build mode**: `npm run build` (production mode)
- **Environment file used**: `.env.production` (but overridden by build args)
- **VITE_API_BASE_URL value**: Full API URL with `/api` path (e.g., `https://pot-api-prod.whiteground-afd18a05.australiaeast.azurecontainerapps.io/api`)
- **Why full URL?**:
  - React app runs in `pot-client-prod` container
  - API runs in separate `pot-api-prod` container
  - Containers are isolated (no shared network like Docker Compose)
  - Browser must call API directly using public internet URL
  - Request goes from browser → Azure CDN → API container
- **Dockerfile behavior**: `--build-arg` overrides `.env.production` value
- **nginx configuration**: `nginx.azure.conf` with NO API proxy (static files only)

**Scenario 3: Local Development (npm run dev)**

- **Command**: `npm run dev` from `Source/Client/pot-react/`
- **Build mode**: Development mode (no build, hot reload)
- **Environment file used**: `.env.development`
- **VITE_API_BASE_URL value**: `http://localhost:5241/api` (or 5242 for local API)
- **Why full URL?**:
  - React dev server runs on `http://localhost:5175` (Vite)
  - No nginx proxy involved
  - Must call API directly at Docker container port (5241) or local API port (5242)
- **nginx**: Not used in development mode

#### Critical Files for Each Scenario

**`.dockerignore`** (Controls what files are copied into Docker build context):

```
# Excludes local development files from Docker builds
Client/pot-react/.env
Client/pot-react/.env.local
Client/pot-react/.env.development

# .env.production is NOT excluded - needed for local Docker builds!
# Azure builds override it with --build-arg
```

**`Dockerfile`** (Client build process):

```dockerfile
# Build arguments for Azure builds only
ARG VITE_API_BASE_URL
ARG VITE_API_TIMEOUT_MS

# Copy application code (includes .env.production from source)
COPY Client/pot-react/ .

# Build with production environment
ENV NODE_ENV=production
# Only set ENV if ARG is provided (Azure), otherwise use .env.production (local Docker)
RUN if [ -n "$VITE_API_BASE_URL" ]; then \
      export VITE_API_BASE_URL=$VITE_API_BASE_URL; \
      export VITE_API_TIMEOUT_MS=$VITE_API_TIMEOUT_MS; \
    fi && \
    npm run build
```

**nginx Configurations**:

- `nginx.conf` (local Docker): Has `/api/` proxy to forward requests to server container
- `nginx.azure.conf` (Azure): NO proxy, serves static files only

#### Common Mistakes to Avoid

1. **Don't set `VITE_API_BASE_URL` in `.env`**

   - This overrides environment-specific files
   - Keep it undefined in `.env` to allow `.env.development` and `.env.production` to work

2. **Don't exclude `.env.production` from `.dockerignore`**

   - Local Docker builds need this file
   - Azure builds override it with `--build-arg`

3. **Don't forget the `/api` path in URLs**

   - All API routes are prefixed with `/api` (server code: `ApiBase = "api"`)
   - Both local Docker (`/api`) and Azure (`https://.../api`) need this

4. **Don't confuse Docker Compose env files with Vite env files**
   - `Source/Docker/.env.development` - PostgreSQL passwords, JWT secrets (server config)
   - `Source/Client/pot-react/.env.development` - API URLs (client config)
   - These are completely separate systems!

#### Verification Commands

**Check what's baked into Docker image**:

```bash
# View the built JavaScript to verify API URL
docker run --rm pot-client:latest sh -c 'cat /usr/share/nginx/html/assets/index-*.js | grep -o "baseURL.*" | head -1'
```

**Check nginx configuration in container**:

```bash
docker exec pot-react cat /etc/nginx/conf.d/default.conf
```

**Test API routing**:

```bash
# From browser console (F12):
axios.defaults.baseURL  # Should show: "/api" for local Docker, "https://..." for Azure
```

## Health Checks

Both configurations include health checks to ensure proper service readiness:

- **PostgreSQL**: Uses `pg_isready` to verify the database is ready
- **ASP.NET Server**: Checks the `_health` endpoint at `http://localhost:5241/_health` (Port 5242 if running locally)
- Services are configured with appropriate dependencies to ensure proper startup order

## Docker Image Versioning System

POT implements **automatic timestamp-based versioning** for application Docker images (server and client) to preserve development history and enable easy rollbacks.

### How Versioning Works

1. **Build Process**:

   - Each build creates a unique timestamp (format: `YYYYMMDD-HHMMSS`)
   - **Server and Client**: Built with timestamp tags (e.g., `pot-server:20251101-143022`, `pot-client:20251101-143022`)
   - **PostgreSQL**: Uses standard Docker build (no versioning - rarely changes)
   - Application images are also tagged as `latest` for consistent development
   - Previous versions are automatically preserved

2. **Development Workflow**:

   - VS Code tasks always run the `latest` tagged images
   - Every build creates new timestamped versions for server/client
   - No manual version management required

3. **Version History**:
   - All previous application builds remain available
   - Easy rollback to any previous server/client version
   - Automatic cleanup can be done with `docker image prune`

### Running Previous Versions

**Method 1: Docker Desktop (Recommended)**

1. Open Docker Desktop
2. Navigate to **Images** tab
3. Find the desired timestamped image (e.g., `pot-server:20251101-140530`)
4. Click **Run** button

**Method 2: Command Line**

```bash
# Stop current containers
docker-compose -p pot -f docker-compose-server-only.yml down

# Set specific version and run
export IMAGE_TAG="20251101-140530"
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-server-only.yml up -d
```

**Method 3: Manual Container Run**

```bash
# Run individual service with specific version
docker run -d -p 5241:5241 pot-server:20251101-140530
```

## Building and Running Docker Services

### Local Development (Server-Only Configuration)

**Using VS Code Tasks (Recommended)**:

- Press `Ctrl+Shift+P` → "Run Task" → "docker-start-pot-server-only"
- Automatically handles versioning and runs latest versions

**Manual Command Line**:

```bash
# Start services (creates timestamped versions + latest)
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-server-only.yml up --build -d

# Stop services
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-server-only.yml down

# View logs
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-server-only.yml logs -f
```

### Production Deployment (Client-Server Configuration)

**Using VS Code Tasks (Recommended)**:

- Press `Ctrl+Shift+P` → "Run Task" → "docker-start-pot-client-server"
- Automatically handles versioning and runs latest versions

**Manual Command Line**:

```bash
# Start services (creates timestamped versions + latest)
docker-compose --env-file .env --env-file .env.production -p pot -f docker-compose-client-server.yml up --build -d

# Stop services
docker-compose --env-file .env --env-file .env.production -p pot -f docker-compose-client-server.yml down

# View logs
docker-compose --env-file .env --env-file .env.production -p pot -f docker-compose-client-server.yml logs -f
```

### Image Management Commands

```bash
# View all versioned POT images with timestamps
docker images pot-server pot-client

# Clean up unused images (keeps currently running versions)
docker image prune

# Force cleanup of all unused images
docker image prune -a

# Remove specific version
docker rmi pot-server:20251101-140530

# View image history and sizes
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
```

## Troubleshooting Common Issues

### Database Connection Issues

If the ASP.NET server can't connect to PostgreSQL:

1. Check if PostgreSQL container is running:

   ```bash
   docker ps | grep pot-postgres
   ```

2. View PostgreSQL logs:

   ```bash
   docker logs pot-postgres
   ```

3. Common fixes:
   - Ensure PostgreSQL is accepting connections:
     ```bash
     docker exec pot-postgres pg_isready
     ```
   - Check environment variables are correctly set
   - Verify PostgreSQL port (5444) is not in use
   - Clear PostgreSQL data volume if needed:
     ```bash
     docker-compose -f docker-compose-server-only.yml down -v
     ```

### ASP.NET Server Issues

1. Check server logs:

   ```bash
   docker logs pot-server
   ```

2. Access health endpoint:

   ```bash
   curl http://localhost:5241/_health
   ```

3. Common fixes:
   - Rebuild server container:
     ```bash
     docker-compose -f docker-compose-server-only.yml build pot-server
     ```
   - Check database connection string
   - Verify environment variables are set correctly

### React Client Issues (Production)

1. Check nginx logs:

   ```bash
   docker logs pot-client
   ```

2. Verify nginx configuration:

   ```bash
   docker exec pot-client nginx -t
   ```

3. Common fixes:
   - Clear browser cache
   - Check API base URL configuration
   - Rebuild client container:
     ```bash
     docker-compose -f docker-compose-client-server.yml build pot-client
     ```

### General Docker Issues

1. View all container statuses:

   ```bash
   docker-compose -f <compose-file> ps
   ```

2. View resource usage:

   ```bash
   docker stats
   ```

3. Common fixes:
   - Remove all containers and volumes:
     ```bash
     docker-compose -f <compose-file> down -v
     ```
   - Prune unused Docker resources:
     ```bash
     docker system prune -a
     ```
   - Restart Docker daemon
   - Check disk space

## Managing Docker Containers via Visual Studio Code

Predefined VS Code tasks are available for managing containers with automatic versioning:

1. Open the Command Palette (`Shift+Ctrl+P` or `Shift+Cmd+P` on macOS)
2. Select `Run Task`
3. Choose one of the following:

### Available Tasks

- **`docker-start-pot-server-only`**:

  - Builds timestamped server image (`pot-server:YYYYMMDD-HHMMSS`)
  - Builds PostgreSQL with standard Docker build (no versioning)
  - Tags server image as `latest`
  - Starts database and server containers using `latest` versions
  - Preserves all previous server versions for rollback

- **`docker-stop-pot-server-only`**:

  - Stops and removes the database and server containers
  - Preserves all built images

- **`docker-start-pot-client-server`**:

  - Builds timestamped application images (`pot-server:YYYYMMDD-HHMMSS`, `pot-client:YYYYMMDD-HHMMSS`)
  - Builds PostgreSQL with standard Docker build (no versioning)
  - Tags application images as `latest`
  - Starts all containers using `latest` versions
  - Preserves all previous application versions for rollback

- **`docker-stop-pot-client-server`**:
  - Stops and removes all containers
  - Preserves all built images

### Versioning Benefits

- **Automatic**: No manual version management required
- **Consistent**: Development always uses `latest` for predictable behavior
- **Safe**: All previous versions preserved for rollback
- **Shareable**: Timestamp format makes versions easy to identify and communicate

### Task Implementation

These tasks are defined in `.vscode/tasks.json` and automatically:

- Generate unique timestamps for each build
- Create both versioned and `latest` tagged images
- Use appropriate environment files and Docker Compose configurations
- Preserve version history without manual intervention

**Example task execution**:

```bash
# What happens when you run docker-start-pot-server-only:
$env:IMAGE_TAG = Get-Date -Format 'yyyyMMdd-HHmmss'  # Creates: 20251101-143022
docker-compose build                                  # Builds: pot-server:20251101-143022, postgres (no version)
docker tag pot-server:20251101-143022 pot-server:latest  # Tags server as: latest
$env:IMAGE_TAG = 'latest'                            # Switch to latest
docker-compose up -d                                 # Runs: latest versions
```
