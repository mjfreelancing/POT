# Docker Setup for POT Project

## Key Paths and Contexts

1. **Docker Compose Build Contexts**:

   - **Postgres Service**:
     - `context: Postgres`
     - `dockerfile: Dockerfile`
     - The `Postgres` folder contains the `Dockerfile` for the PostgreSQL service.
   - **Server Service**:
     - `context: ..` (one level up from the `Docker` folder)
     - `dockerfile: Docker/Server/Dockerfile`
     - The `Server` folder (at the root level) contains the server source code, and the `Docker/Server` folder contains the `Dockerfile` and `entrypoint.sh`.

2. **Server Dockerfile**:

   - **Build Context**:
     - The `context: ..` ensures that the `Server` folder (containing the source code) and the `Docker/Server` folder (containing the `Dockerfile` and `entrypoint.sh`) are included in the build context.
   - **Paths in Dockerfile**:
     - `COPY Server ./Server`: Copies the `Server` folder (source code) into the Docker image.
     - `COPY Docker/Server/entrypoint.sh .`: Copies the `entrypoint.sh` file from the `Docker/Server` folder into the Docker image.

3. **Entrypoint Script**:

   - Located in `Docker/Server/entrypoint.sh`.
   - Referenced in the `Server/Dockerfile` as `COPY Docker/Server/entrypoint.sh .`.

4. **Solution File**:

   - Located in the root directory: `pot.sln`.
   - The `Server` folder contains all the projects referenced in the solution file.

5. **Environment Variables**:
   - Defined in `.env`:
     - `COMPOSE_PROJECT_NAME=pot`
     - `POSTGRES_USER=postgres`
     - `POSTGRES_PASSWORD=password`
     - `POSTGRES_DB=postgres`

## Updating and Rebuilding Docker Images

To update and rebuild the Docker images after making changes to the source code:

1. Stop the containers if they are running:

   ```bash
   docker-compose down
   ```

2. Rebuild the images:

   ```bash
   docker-compose build
   ```

3. Start the containers with the new images:
   ```bash
   docker-compose up -d
   ```

Alternatively, you can combine steps 2 and 3 with:

```bash
docker-compose up -d --build
```

Note: These commands should be run from the Docker directory containing the docker-compose.yml file.

## Managing Docker Containers via VS Code

You can start and stop the Docker containers directly from Visual Studio Code using the predefined tasks. From the root folder:

1. Press `Shift+Ctrl+P` (or `Shift+Cmd+P` on macOS) to open the Command Palette.
2. Type `Run Task` and select it.
3. Choose one of the following tasks:
   - **docker-start-pot**: Starts the Docker containers and builds the images if necessary.
   - **docker-stop-pot**: Stops and removes the Docker containers.

These tasks are defined in the `.vscode/tasks.json` file.
