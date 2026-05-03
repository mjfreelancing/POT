# Docker Setup

Docker provides the easiest way to run POT. It automatically sets up PostgreSQL, the backend API, and the frontend with minimal configuration.

## What Docker Provides

The Docker setup runs the complete stack:

- **PostgreSQL database** (port 5444) - Uses non-default port to avoid conflicts with local PostgreSQL installations
- **ASP.NET Core API** (port 5241) - ASP.NET Core typically uses ports 5000-5300; if this conflicts with your local setup, see configuration details <!-- TODO-DOC: Link to port configuration section when created -->
- **React frontend** (port 5175) - Uses port 5175 instead of Vite's default 5173

## Choose Your Method

**Using VS Code?** → [Quick Start with VS Code Tasks](#option-a-using-vs-code-tasks-recommended) (easiest)

**Prefer command line?** → [Docker Compose CLI Commands](#option-b-docker-compose-cli-commands)

---

## Option A: Using VS Code Tasks (Recommended)

If you're using VS Code, you can start everything with built-in tasks.

> **Important:** VS Code must be opened at the workspace root (the `POT` folder) for tasks to work correctly. The tasks are defined in `.vscode/tasks.json` and rely on the workspace folder structure.

> **Note:** The VS Code task creates timestamped Docker images (e.g., `pot-server:20251109-143052`) for versioning. These images will accumulate over time and consume disk space. You'll need to periodically clean them up using `docker image prune` or manually delete old images with `docker rmi <image-name>`.

### Task Policy and Workflow Matrix

POT Docker build tasks always use `--no-cache` to guarantee fresh image builds.

| Workflow                                | Start Task                   | Stop Task                   | Notes                                         |
| --------------------------------------- | ---------------------------- | --------------------------- | --------------------------------------------- |
| Full stack (client + server + postgres) | `docker-start-client-server` | `docker-stop-client-server` | Standard local workflow                       |
| Server only (server + postgres)         | `docker-start-server`        | `docker-stop-server`        | Verify API at `http://localhost:5241/_health` |
| Client only (client only)               | `docker-start-client`        | `docker-stop-client`        | Backend (`server`) must already be running    |

### Step 1: Start Services

1. Press `Ctrl+Shift+P` (Windows/Linux) or `Cmd+Shift+P` (Mac)
2. Type "Run Task" and press Enter
3. Select `docker-start-client-server`

The task will build and start all services. This may take several minutes on the first run.

### Step 2: Verify Services are Running

Check that all containers are running:

```bash
docker ps
```

You should see three running containers (container names → image names):

- `pot-postgres` → `pot-postgres` - PostgreSQL database
- `pot-aspnet` → `pot-server:latest` - ASP.NET Core API
- `pot-react` → `pot-client:latest` - React frontend (nginx)

### Step 3: Verify and Access

#### 3.1 Verify the API

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

#### 3.2 Verify the Frontend

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

#### 3.3 Open the Application in Your Browser

Navigate to:

- **POT Application**: http://localhost:5175

You should see the POT login/signup page.

### Stopping Services

1. Press `Ctrl+Shift+P` (Windows/Linux) or `Cmd+Shift+P` (Mac)
2. Type "Run Task" and press Enter
3. Select `docker-stop-client-server`

---

## Option B: Docker Compose CLI Commands

### Step 1: Start Docker Services

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

### Step 2: Verify Services are Running

Check that all containers are running:

```bash
docker ps
```

You should see three running containers (container names → image names):

- `pot-postgres` → `pot-postgres` - PostgreSQL database
- `pot-aspnet` → `pot-server:latest` - ASP.NET Core API
- `pot-react` → `pot-client:latest` - React frontend (nginx)

### Step 3: Access the Application

#### 3.1 Verify the API

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

#### 3.2 Verify the Frontend

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

#### 3.3 Open the Application in Your Browser

Navigate to:

- **POT Application**: http://localhost:5175

You should see the POT login/signup page.

### Stopping the Services

When you're done:

```bash
cd Source/Docker
docker-compose -p pot -f docker-compose-client-server.yml down
```

---

## Useful Docker Commands

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

---

**Next:** [Local Setup](LOCAL-SETUP.md) (alternative approach) or [First-Time Configuration](FIRST-TIME-SETUP.md)

**Return to:** [Getting Started](GETTING-STARTED.md)
