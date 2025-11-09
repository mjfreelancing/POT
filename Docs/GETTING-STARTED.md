# Getting Started with POT

This guide will help you get POT up and running on your local machine for development or testing.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

- **Node.js** (v20 or later) - [Download](https://nodejs.org/)
- **npm** (comes with Node.js)
- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Git** - [Download](https://git-scm.com/)

### Highly Recommended

- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop/)
  - Simplifies setup and ensures consistent environment
  - Handles PostgreSQL database automatically
  - Can run everything without Docker, but Docker is the easiest path

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

**Next:** Choose your setup method - [Docker Setup](#docker-setup-recommended) (recommended) or [Manual Setup](#manual-setup)

## Docker Setup (Recommended)

Docker provides the easiest way to run POT. It automatically sets up PostgreSQL, the backend API, and the frontend with minimal configuration.

### What Docker Provides

The Docker setup runs the complete stack:

- **PostgreSQL database** (port 5444) - Uses non-default port to avoid conflicts with local PostgreSQL installations
- **ASP.NET Core API** (port 5241) - ASP.NET Core typically uses ports 5000-5300; if this conflicts with your local setup, see configuration details <!-- TODO-DOC: Link to port configuration section when created -->
- **React frontend** (port 5175) - Uses port 5175 instead of Vite's default 5173

### Choose Your Method

> **Important:** Before starting Docker, ensure the `Source/Docker/postgres-data/` directory exists. If it doesn't exist, the PostgreSQL container will fail to start. Create it manually if needed:
>
> ```bash
> # From the project root
> mkdir -p Source/Docker/postgres-data
> ```

**Using VS Code?** → [Quick Start with VS Code Tasks](#option-a-using-vs-code-tasks-recommended) (easiest)

**Prefer command line?** → [Manual Setup with Docker Compose](#option-b-manual-docker-compose-commands)

---

### Option A: Using VS Code Tasks (Recommended)

If you're using VS Code, you can start everything with built-in tasks.

> **Note:** The VS Code task creates timestamped Docker images (e.g., `pot-server:20251109-143052`) for versioning. These images will accumulate over time and consume disk space. You'll need to periodically clean them up using `docker image prune` or manually delete old images with `docker rmi <image-name>`.

#### Step 1: Start Services

1. Press `Ctrl+Shift+P` (Windows/Linux) or `Cmd+Shift+P` (Mac)
2. Type "Run Task" and press Enter
3. Select `docker-start-pot-client-server`

The task will build and start all services. This may take several minutes on the first run.

#### Step 2: Verify Services are Running

Check that all containers are running:

```bash
docker ps
```

You should see three containers:

- `pot-postgres` - PostgreSQL database
- `pot-aspnet` - ASP.NET Core API
- `pot-react` - React frontend (nginx)

#### Step 3: Verify and Access

##### 3.1 Verify the API

Check that the API is responding:

```bash
curl http://localhost:5241/_health
```

Expected response should look something like this:

```
StatusCode        : 200
StatusDescription : OK
Content           : Healthy
RawContent        : HTTP/1.1 200 OK
                    Pragma: no-cache
                    Transfer-Encoding: chunked
                    Cache-Control: no-store, no-cache
                    Content-Type: text/plain
                    Date: Sun, 09 Nov 2025 11:44:40 GMT
                    Expires: Thu, 01 Jan 1970 00:00:00 GMT...
Forms             : {}
Headers           : {[Pragma, no-cache], [Transfer-Encoding, chunked], [Cache-Control, no-store, no-cache],
                    [Content-Type, text/plain]...}
Images            : {}
InputFields       : {}
Links             : {}
ParsedHtml        : mshtml.HTMLDocumentClass
RawContentLength  : 7
```

##### 3.2 Verify the Frontend

Check that the frontend is responding:

```bash
curl http://localhost:5175/health
```

Expected response should look something like this:

```
StatusCode        : 200
StatusDescription : OK
Content           : {104, 101, 97, 108...}
RawContent        : HTTP/1.1 200 OK
                    Connection: keep-alive
                    Content-Length: 7
                    Content-Type: application/octet-stream,text/plain
                    Date: Sun, 09 Nov 2025 11:49:09 GMT
                    Server: nginx/1.29.3

                    healthy
Headers           : {[Connection, keep-alive], [Content-Length, 7], [Content-Type,
                    application/octet-stream,text/plain], [Date, Sun, 09 Nov 2025 11:49:09 GMT]...}
RawContentLength  : 7
```

##### 3.3 Open the Application in Your Browser

Navigate to:

- **POT Application**: http://localhost:5175

You should see the POT login/signup page.

#### Stopping Services

1. Press `Ctrl+Shift+P` (Windows/Linux) or `Cmd+Shift+P` (Mac)
2. Type "Run Task" and press Enter
3. Select `docker-stop-pot-client-server`

---

### Option B: Manual Docker Compose Commands

#### Step 1: Start Docker Services

Navigate to the Docker directory:

```bash
cd Source/Docker
```

Start all services using Docker Compose:

```bash
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-client-server.yml up -d
```

This command will:

- Download required Docker images (first time only)
- Build the server and client containers
- Start PostgreSQL database on port 5444
- Start the ASP.NET API server on port 5241
- Start the React frontend on port 5175
- Run in detached mode (background)

> **First-time build:** The initial build can take several minutes as it installs dependencies and compiles both frontend and backend.

#### Step 2: Verify Services are Running

Check that all containers are running:

```bash
docker ps
```

You should see three containers:

- `pot-postgres` - PostgreSQL database
- `pot-aspnet` - ASP.NET Core API
- `pot-react` - React frontend (nginx)

#### Step 3: Access the Application

##### 3.1 Verify the API

Check that the API is responding:

```bash
curl http://localhost:5241/_health
```

Expected response should look something like this:

```
StatusCode        : 200
StatusDescription : OK
Content           : Healthy
RawContent        : HTTP/1.1 200 OK
                    Pragma: no-cache
                    Transfer-Encoding: chunked
                    Cache-Control: no-store, no-cache
                    Content-Type: text/plain
                    Date: Sun, 09 Nov 2025 11:44:40 GMT
                    Expires: Thu, 01 Jan 1970 00:00:00 GMT...
Forms             : {}
Headers           : {[Pragma, no-cache], [Transfer-Encoding, chunked], [Cache-Control, no-store, no-cache],
                    [Content-Type, text/plain]...}
Images            : {}
InputFields       : {}
Links             : {}
ParsedHtml        : mshtml.HTMLDocumentClass
RawContentLength  : 7
```

##### 3.2 Verify the Frontend

Check that the frontend is responding:

```bash
curl http://localhost:5175/health
```

Expected response should look something like this:

```
StatusCode        : 200
StatusDescription : OK
Content           : {104, 101, 97, 108...}
RawContent        : HTTP/1.1 200 OK
                    Connection: keep-alive
                    Content-Length: 7
                    Content-Type: application/octet-stream,text/plain
                    Date: Sun, 09 Nov 2025 11:49:09 GMT
                    Server: nginx/1.29.3

                    healthy
Headers           : {[Connection, keep-alive], [Content-Length, 7], [Content-Type,
                    application/octet-stream,text/plain], [Date, Sun, 09 Nov 2025 11:49:09 GMT]...}
RawContentLength  : 7
```

##### 3.3 Open the Application in Your Browser

Navigate to:

- **POT Application**: http://localhost:5175

You should see the POT login/signup page.

#### Stopping the Services

When you're done:

```bash
cd Source/Docker
docker-compose --env-file .env --env-file .env.development -p pot -f docker-compose-client-server.yml down
```

---

### Useful Docker Commands

View logs from a specific container:

```bash
docker logs pot-aspnet      # Backend API logs
docker logs pot-postgres    # Database logs
docker logs pot-react       # Frontend logs
```

Follow logs in real-time:

```bash
docker logs -f pot-aspnet
```

Restart a specific container:

```bash
docker restart pot-aspnet
```

Stop all POT containers:

```bash
docker stop pot-aspnet pot-postgres pot-react
```

Remove containers (keeps images and data):

```bash
docker rm pot-aspnet pot-postgres pot-react
```

List all Docker images (including timestamped ones):

```bash
docker images | grep pot
```

Remove old timestamped images (if using VS Code tasks):

```bash
# Remove a specific image
docker rmi pot-server:20251109-143052
docker rmi pot-client:20251109-143052

# Or remove all unused images
docker image prune -a
```

### Configuration Files

The Docker setup uses these files (in `Source/Docker/`):

- `docker-compose-client-server.yml` - Main configuration for all services
- `.env` - Base environment variables
- `.env.development` - Development-specific settings (database passwords, JWT secrets, etc.)

> **Note:** Database data is persisted in `Source/Docker/postgres-data/` so your data survives container restarts.

> **For more details:** See [Docker Setup Guide](Development/DOCKER.md) for advanced configuration options

---

**Next:** [First-Time Configuration](#first-time-configuration) or skip to [Manual Setup](#manual-setup)
