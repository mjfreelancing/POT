# Local Setup (Without Docker)

> **Note:** This section is under development. Content will be added in a future update.

This guide covers running the POT backend API and React frontend locally on your machine, without Docker containers. You can run PostgreSQL either locally or in a Docker container (a PostgreSQL Docker container is available in `Source/Docker/Postgres/`).

## Overview

When running locally without Docker:
- **Backend API**: Run the .NET application directly using `dotnet run`
- **Frontend**: Run the React development server using `npm run dev`
- **Database**: Your choice - run PostgreSQL locally OR use the Docker container

## Coming Soon

Detailed instructions will cover:
- PostgreSQL setup (local installation OR Docker container)
- Backend API local setup and configuration
- Frontend local setup and configuration
- Environment configuration for local development
- Running and debugging locally

For now, please refer to:
- `Source/Server/README.md` for backend setup
- `Source/Client/pot-react/README.md` for frontend setup
- `Source/Docker/Postgres/Dockerfile` for PostgreSQL Docker container option

---

**Return to:** [Getting Started](GETTING-STARTED.md)
