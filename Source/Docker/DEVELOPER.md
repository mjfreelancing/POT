# Docker Developer Guide

Technical guide for Docker configuration and container orchestration in the POT project.

## Table of Contents

- [Overview](#overview)
- [Container Architecture](#container-architecture)
- [Docker Compose Configurations](#docker-compose-configurations)
- [Environment Variables](#environment-variables)
- [Networking](#networking)
- [Volume Management](#volume-management)
- [Health Checks](#health-checks)
- [Build Configuration](#build-configuration)
- [Troubleshooting](#troubleshooting)

---

## Overview

POT uses Docker for local development and production deployment. The application consists of three containers:

1. **pot-postgres** - PostgreSQL database
2. **pot-server** - ASP.NET Core API
3. **pot-client** - React frontend (Vite + nginx)

**Docker Compose Files:**

- `docker-compose-client-server.yml` - Full stack (client + server + database)
- Additional configurations for different environments

---

## Container Architecture

### Container Dependencies

```
pot-client (port 5175)
    ↓ depends on
pot-server (port 5241)
    ↓ depends on
pot-postgres (port 5432)
```

**Startup Order:**

1. PostgreSQL starts and becomes healthy
2. Server starts after PostgreSQL is healthy, runs migrations
3. Client starts after server is healthy

### Port Mapping

**Development:**

- Client: `http://localhost:5175` → container port 80
- Server: `http://localhost:5241` → container port 8080
- PostgreSQL: `localhost:5432` → container port 5432

**Production (Azure):**

- Client: `https://yourdomain.com` → port 80
- Server: `https://api.yourdomain.com` → port 8080
- PostgreSQL: Internal network (not exposed)

---

## Docker Compose Configurations

### Full Stack: docker-compose-client-server.yml

**Location:** `Source/Docker/docker-compose-client-server.yml`

**Services:**

- `postgres` - PostgreSQL database
- `server` - ASP.NET Core API
- `client` - React frontend with nginx

**Usage:**

```bash
# Start all services
cd Source/Docker
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-client-server.yml up --build -d

# Stop all services
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-client-server.yml down

# View logs
docker-compose -p pot logs -f

# View specific service logs
docker-compose -p pot logs -f server
docker-compose -p pot logs -f client
docker-compose -p pot logs -f postgres
```

### Key Configuration Sections

#### PostgreSQL Service

```yaml
postgres:
  image: postgres:17
  container_name: pot-postgres
  restart: unless-stopped
  ports:
    - "5432:5432"
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: password123
    POSTGRES_DB: pot
  volumes:
    - postgres-data:/var/lib/postgresql/data
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s
    timeout: 5s
    retries: 5
```

#### Server Service

```yaml
server:
  build:
    context: ..
    dockerfile: Docker/Server/Dockerfile
  container_name: pot-server
  restart: unless-stopped
  depends_on:
    postgres:
      condition: service_healthy
  ports:
    - "5241:8080"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Host=postgres;Database=pot;Username=postgres;Password=password123
  networks:
    - pot-network
```

#### Client Service

```yaml
client:
  build:
    context: ..
    dockerfile: Docker/Client/Dockerfile
    args:
      NGINX_CONFIG: nginx.conf
      VITE_API_BASE_URL: http://localhost:5241/api
      VITE_API_TIMEOUT_MS: 30000
  container_name: pot-client
  restart: unless-stopped
  depends_on:
    server:
      condition: service_started
  ports:
    - "5175:80"
  networks:
    - pot-network
```

---

## Environment Variables

### Docker Compose Environment Files

**`.env`** - Base configuration (shared across environments):

```properties
COMPOSE_PROJECT_NAME=pot
POSTGRES_DB=Pot
```

**`.env.development.template`** - Tracked starter file for local development.

Copy or rename this file to `.env.development` before running Docker Compose. The template is enough for basic local startup, but placeholder SMTP and JWT values should be replaced for full usage.

**`.env.development`** - Local development overrides:

```properties
# Database Configuration
POSTGRES_USER=postgres
POSTGRES_PASSWORD=password

# JWT Configuration
JWT_ISSUER=http://localhost:5241
JWT_AUDIENCE=http://localhost:5241
JWT_SECRET_KEY=<exactly-128-random-characters>

# SMTP Configuration
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
PLATFORM_ADMIN_USERIDS=
```

**Loading order:**

```bash
docker-compose --env-file .env --env-file .env.development -f docker-compose.yml up
```

### Server Container Variables

For the current `docker-compose-client-server.yml`, `.env.development` is used for value substitution into the compose file.

**Required by the current compose file:**

- `POSTGRES_USER` - PostgreSQL superuser name
- `POSTGRES_PASSWORD` - PostgreSQL superuser password
- `JWT_ISSUER` - JWT issuer
- `JWT_AUDIENCE` - JWT audience
- `JWT_SECRET_KEY` - JWT signing key
- `SMTP_HOST` - SMTP server host
- `SMTP_PORT` - SMTP server port
- `SMTP_REQUIRE_TLS` - SMTP TLS toggle
- `SMTP_AUTH_USERNAME` - SMTP username
- `SMTP_AUTH_PASSWORD` - SMTP password
- `SMTP_FROM_NAME` - Email display name
- `SMTP_FROM_ADDRESS` - Email from address
- `CORS_ALLOWED_ORIGINS` - Allowed browser origins
- `PLATFORM_ADMIN_USERIDS` - Comma-separated platform admin GUIDs

**Not currently consumed by the full-stack compose file:**

- `POSTGRES_PORT`
- `SERVER_PORT`
- `CLIENT_PORT`
- `VITE_API_BASE_URL`

**Notes:**

- The committed `.env` currently supplies `COMPOSE_PROJECT_NAME` and `POSTGRES_DB`.
- The server validates SMTP settings at startup, so SMTP values must be present and non-empty.
- Placeholder SMTP values allow the containers to start, but email-dependent flows will fail until you replace them with real credentials.

**Example runtime environment inside the server container:**

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - DATABASE__HOST=pot-postgres
  - DATABASE__USERNAME=${POSTGRES_USER}
  - DATABASE__PASSWORD=${POSTGRES_PASSWORD}
  - DATABASE__NAME=${POSTGRES_DB}
  - JWT__ISSUER=${JWT_ISSUER}
  - JWT__AUDIENCE=${JWT_AUDIENCE}
  - JWT__SECRETKEY=${JWT_SECRET_KEY}
```

### Client Container Build Arguments

Client environment variables are **embedded at build time**, not runtime.

**Build args in Dockerfile:**

```dockerfile
ARG NGINX_CONFIG=nginx.conf
ARG VITE_API_BASE_URL
ARG VITE_API_TIMEOUT_MS=30000
```

**Passing build args in docker-compose:**

```yaml
client:
  build:
    args:
      NGINX_CONFIG: nginx.conf
      VITE_API_BASE_URL: http://localhost:5242/api
      VITE_API_TIMEOUT_MS: 30000
```

**Important:** Changing these requires rebuild:

```bash
docker-compose -p pot up --build client
```

---

## Networking

### Docker Network

**Network name:** `pot-network`

**Bridge network** - allows container-to-container communication by service name.

**Configuration:**

```yaml
networks:
  pot-network:
    driver: bridge
```

**Service DNS:**

- Server can reach PostgreSQL at `postgres:5432`
- Client can reach server at `server:8080`

**Example connection string:**

```properties
# Server → PostgreSQL (uses service name)
ConnectionStrings__DefaultConnection=Host=postgres;Database=pot;Username=postgres;Password=password123

# Host → PostgreSQL (uses localhost)
ConnectionStrings__DefaultConnection=Host=localhost;Database=pot;Username=postgres;Password=password123
```

---

## Volume Management

### Persistent Volumes

**PostgreSQL data:**

```yaml
volumes:
  - postgres-data:/var/lib/postgresql/data
```

### Volume Commands

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect pot_postgres-data

# Remove volume (WARNING: deletes data)
docker volume rm pot_postgres-data

# Remove all unused volumes
docker volume prune
```

---

## Health Checks

### PostgreSQL Health Check

```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres"]
  interval: 10s
  timeout: 5s
  retries: 5
```

**What it does:**

- Runs `pg_isready` command every 10 seconds
- Waits 5 seconds for response
- Retries 5 times before marking unhealthy
- Server waits for `service_healthy` before starting

### Server Health Check (optional)

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

**Requires:**

- Health check endpoint in ASP.NET Core
- curl installed in container

### Checking Health Status

```bash
# Check all container health
docker ps

# Inspect specific container health
docker inspect --format='{{.State.Health.Status}}' pot-postgres
docker inspect --format='{{.State.Health.Status}}' pot-server

# View health check logs
docker inspect pot-postgres | grep -A 10 Health
```

---

## Build Configuration

### Server Dockerfile

**Location:** `Source/Docker/Server/Dockerfile`

**Multi-stage build:**

1. **Build stage** - Restore packages, build C# projects
2. **Publish stage** - Publish Release build
3. **Runtime stage** - Copy published files, run app

**Key sections:**

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Server/Pot.AspNetCore/Pot.AspNetCore.csproj", "Server/Pot.AspNetCore/"]
RUN dotnet restore "Server/Pot.AspNetCore/Pot.AspNetCore.csproj"
COPY . .
RUN dotnet build "Server/Pot.AspNetCore/Pot.AspNetCore.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Server/Pot.AspNetCore/Pot.AspNetCore.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pot.AspNetCore.dll"]
```

### Client Dockerfile

**Location:** `Source/Docker/Client/Dockerfile`

**Multi-stage build:**

1. **Build stage** - npm install, build React app with Vite
2. **Runtime stage** - nginx serves static files

**Key sections:**

```dockerfile
# Build args (embedded at build time)
ARG NGINX_CONFIG=nginx.conf
ARG VITE_API_BASE_URL
ARG VITE_API_TIMEOUT_MS=30000

# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY Client/pot-react/package*.json ./
RUN npm ci
COPY Client/pot-react/ ./
ARG VITE_API_BASE_URL
ARG VITE_API_TIMEOUT_MS
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
ENV VITE_API_TIMEOUT_MS=${VITE_API_TIMEOUT_MS}
RUN npm run build

# Runtime stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
ARG NGINX_CONFIG
COPY Docker/Client/${NGINX_CONFIG} /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**Important:** VITE variables are embedded at build time and cannot be changed at runtime.

### Nginx Configuration

**Development:** `Source/Docker/Client/nginx.conf`

```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

**Production (Azure):** `Source/Docker/Client/nginx.azure.conf`

```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://server:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Troubleshooting

### Container Won't Start

**Check logs:**

```bash
docker logs pot-server
docker logs pot-client
docker logs pot-postgres
```

**Common issues:**

- Port already in use: `netstat -an | findstr 5432`
- Missing environment variables
- Health check failing (PostgreSQL not ready)
- Build failed (check Dockerfile syntax)

### Database Connection Issues

**From server container:**

```bash
docker exec -it pot-server bash
apt-get update && apt-get install -y postgresql-client
psql -h postgres -U postgres -d pot
```

**From host:**

```bash
psql -h localhost -U postgres -d pot
```

**Check connection string format:**

```properties
# ✅ GOOD - from container
Host=postgres;Database=pot;Username=postgres;Password=password123

# ✅ GOOD - from host
Host=localhost;Database=pot;Username=postgres;Password=password123

# ❌ BAD - wrong separator
Host:postgres,Database:pot,Username:postgres,Password:password123
```

### Client Can't Reach Server

**Check network connectivity:**

```bash
docker exec -it pot-client sh
apk add curl
curl http://server:8080/api/health
```

**Verify VITE_API_BASE_URL:**

- Development: `http://localhost:5242/api`
- Production with nginx proxy: `/api`

**Rebuild if API URL changed:**

```bash
docker-compose -p pot up --build client
```

### Volume Permissions Issues

**PostgreSQL won't start:**

```bash
# Check volume permissions
docker volume inspect pot_postgres-data

# Remove and recreate volume
docker-compose -p pot down -v
docker-compose -p pot up -d
```

### Rebuild Containers

**Rebuild specific service:**

```bash
docker-compose -p pot up --build server
docker-compose -p pot up --build client
```

**Rebuild all services:**

```bash
docker-compose -p pot up --build
```

**Clean rebuild (removes cache):**

```bash
docker-compose -p pot build --no-cache
docker-compose -p pot up -d
```

### Reset Everything

**Warning: This deletes all data!**

```bash
# Stop and remove containers, networks, volumes
docker-compose -p pot down -v

# Remove images
docker rmi pot-server pot-client

# Rebuild from scratch
docker-compose -p pot up --build -d
```

---

## Development Workflow

### Starting Development Environment

The `postgres-data` directory must exist before the first start. The PowerShell script and the VS Code tasks both create it automatically.

**Using the PowerShell script (standalone):**

```powershell
cd Source/Docker
.\Start-ProdlikePot.ps1
```

The script creates `postgres-data` if needed, builds all images, and starts all services.

**Using the terminal directly:**

```bash
cd Source/Docker

# Create postgres-data if this is a first-time run
New-Item -ItemType Directory -Force -Path postgres-data

# Start all services
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up --build -d

# Watch logs
docker-compose logs -f
```

### Making Changes

**Frontend changes:**

1. Edit React code in `Source/Client/pot-react/`
2. Rebuild client: `docker-compose -p pot up --build client`

**Backend changes:**

1. Edit C# code in `Source/Server/`
2. Rebuild server: `docker-compose -p pot up --build server`

**Database changes:**

1. Create migration: `dotnet ef migrations add Name --project Pot.Data.Migrations`
2. Rebuild server (migrations run on startup)

### VS Code Tasks

**Available tasks** (run from VS Code Task menu):

- `docker-start-client-server` - Build and start all services (equivalent to `Start-ProdlikePot.ps1`)
- `docker-stop-client-server` - Stop all services
- `azure-server-build-and-deploy` - Build and push server to registry (`--no-cache`)
- `azure-client-build-and-deploy` - Build and push client to registry (`--no-cache`)

---

## Production Deployment

### Building for Azure

**Server:**

```bash
cd Source
docker build -t ghcr.io/mjfreelancing/pot-server:latest -f Docker/Server/Dockerfile .
docker push ghcr.io/mjfreelancing/pot-server:latest
```

**Client:**

```bash
cd Source
docker build \
  --build-arg NGINX_CONFIG=nginx.azure.conf \
  --build-arg VITE_API_BASE_URL=https://api.yourdomain.com/api \
  --build-arg VITE_API_TIMEOUT_MS=30000 \
  -t ghcr.io/mjfreelancing/pot-client:latest \
  -f Docker/Client/Dockerfile .
docker push ghcr.io/mjfreelancing/pot-client:latest
```

### Image Tagging Strategy

**Development:**

```bash
docker tag pot-server:latest pot-server:dev
docker tag pot-client:latest pot-client:dev
```

**Production:**

```bash
# Date-based tags
docker tag pot-server:latest pot-server:20250112
docker tag pot-client:latest pot-client:20250112

# Version tags
docker tag pot-server:latest pot-server:v1.0.0
docker tag pot-client:latest pot-client:v1.0.0
```

---

## Best Practices

### Container Design

1. **Use multi-stage builds** to reduce image size
2. **Copy only necessary files** to container
3. **Use .dockerignore** to exclude node_modules, bin, obj
4. **Run containers as non-root user** (where possible)
5. **Use specific base image tags** (not `latest`)

### Environment Variables

1. **Never commit secrets** to .env files
2. **Use .env.local** for sensitive local overrides (git-ignored)
3. **Use Docker secrets** for production secrets
4. **Validate required variables** in application startup

### Networking

1. **Use service names** for inter-container communication
2. **Expose only necessary ports** to host
3. **Use internal networks** for services that don't need external access

### Volumes

1. **Use named volumes** for persistent data
2. **Mount bind volumes** for backups and logs
3. **Regular backups** of PostgreSQL data
4. **Don't mount volumes** unnecessarily (slows performance)

### Health Checks

1. **Implement health endpoints** in applications
2. **Configure proper timeouts** and retries
3. **Use depends_on with conditions** for startup order

---

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Nginx Docker Hub](https://hub.docker.com/_/nginx)

---

**For frontend patterns, see:** `Source/Client/pot-react/DEVELOPER.md`

**For backend patterns, see:** `Source/Server/DEVELOPER.md`

**For setup instructions, see:** `Docs/DOCKER-SETUP.md` and `Docs/LOCAL-SETUP.md`
