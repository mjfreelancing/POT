# Local Setup (Without Docker)

This guide covers running the POT backend API and React frontend locally on your machine for development and debugging. This approach allows you to use IDEs like Visual Studio for the backend and VS Code for the frontend.

## Overview

When running locally:

- **Database**: PostgreSQL (local installation OR Docker container)
- **Backend API**: Run via Visual Studio or `dotnet run` on port 5242
- **Frontend**: Run via `npm run dev` on port 5175

---

## Step 1: PostgreSQL Setup

You have two options for running PostgreSQL:

### Option A: Local PostgreSQL Installation

1. **Download and install PostgreSQL** (v13 or later): [Download PostgreSQL](https://www.postgresql.org/download/)

2. **Configure PostgreSQL:**
   - **Default port**: 5432 (recommended)
   - **Username**: `postgres`
   - **Password**: Set during installation (you'll need this for configuration)
   - **Alternative**: If you need a different port, you'll need to update the port in both `Pot.Data.Migrations/appsettings.json` and `Pot.AspNetCore/appsettings.Development.json`

### Option B: PostgreSQL Docker Container

If you prefer to use Docker just for PostgreSQL:

```bash
# Navigate to the Docker directory
cd Source/Docker/Postgres

# Build and run the PostgreSQL container
docker build -t pot-postgres .
docker run -d --name pot-postgres -p 5432:5432 pot-postgres
```

This creates a PostgreSQL container with:

- Username: `postgres`
- Password: `password`
- Port: 5432 (mapped to host port 5432)

> **Note:** The PostgreSQL Dockerfile exposes port 5432 (the service port inside the container). The `-p 5432:5432` maps the container's port 5432 to your host machine's port 5432 (format: `-p <host-port>:<container-port>`).

---

## Step 2: Run Database Migrations

The migration application will connect to PostgreSQL, create the database if it doesn't exist, and apply all migrations.

1. **Navigate to the migrations project:**

   ```bash
   cd Source/Server
   ```

2. **Run the migrations application:**

   ```bash
   dotnet run --project Pot.Data.Migrations
   ```

   The application will:

   - Check if PostgreSQL is available
   - Create the `Pot` database if it doesn't exist
   - Apply all migrations to create tables and seed initial data

> **Note:** If you're using a local PostgreSQL installation with different credentials, update `Source/Server/Pot.Data.Migrations/appsettings.json` before running migrations (you'll also need to update `appsettings.Development.json` in Step 3). If you're using the PostgreSQL Docker container with default credentials, no changes are needed.

---

## Step 3: Configure and Run the Backend API

### 3.1: Create Configuration File

Create a configuration file for local development:

1. **Navigate to the API project:**

   ```bash
   cd Source/Server/Pot.AspNetCore
   ```

2. **Create `appsettings.Development.json`** (if it doesn't exist) or update it with your settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:5175"
  },
  "AllowedHosts": "*",
  "Database": {
    "Name": "Pot",
    "Host": "localhost",
    "Username": "postgres",
    "Password": "password"
  },
  "Smtp": {
    "Host": "your-smtp-server.com",
    "Port": 587,
    "RequireTLS": "true",
    "Authentication": {
      "Username": "your-email@domain.com",
      "Password": "your-email-password"
    },
    "From": {
      "Name": "POT - Do Not Reply",
      "Address": "your-email@domain.com"
    }
  },
  "Jwt": {
    "Issuer": "http://localhost:5242",
    "Audience": "http://localhost:5242",
    "SecretKey": "YOUR-128-CHARACTER-SECRET-KEY-HERE-REPLACE-THIS-ENTIRE-STRING-WITH-RANDOM-CHARACTERS-MUST-BE-EXACTLY-128-CHARS-LONG-XXXXXXXX"
  },
  "PlatformAdmin": {
    "UserIds": ""
  }
}
```

> **Note:** This file is excluded from version control. Each developer must create their own copy with their credentials.

3. **Update the configuration values:**

   - **Database**: Update `Password` if you changed it during PostgreSQL installation
   - **SMTP**: Configure with your email provider's settings (required for user email verification)
   - **JWT SecretKey**: Generate a random 128-character string
   - **PlatformAdmin UserIds**: Leave empty for initial setup (you'll add your user GUID after first signup)

> **Note:** The `Database.Port` and `Database.SSLMode` default to 5432 and "disable" respectively in the base `appsettings.json`. Only add them to `appsettings.Development.json` if you're using different values.

### 3.2: Run the Backend API

**Using Visual Studio:**

1. Open `Source/Server/pot.sln` in Visual Studio
2. Set `Pot.AspNetCore` as the startup project
3. Press F5 to run with debugging

**Using Command Line:**

```bash
cd Source/Server/Pot.AspNetCore
dotnet run
```

The API will start on `http://localhost:5242` and automatically open the Scalar OpenAPI documentation page in your browser.

---

## Step 4: Configure and Run the React Frontend

### 4.1: Install Dependencies

```bash
cd Source/Client/pot-react
npm install
```

### 4.2: Create Environment Configuration

Create `.env.development` in the `Source/Client/pot-react/` directory:

```bash
# Development environment variables (npm run dev)

# API running locally (Visual Studio or dotnet run)
VITE_API_BASE_URL=http://localhost:5242/api
```

> **Note:** This file is excluded from version control. Each developer must create their own copy.

### 4.3: Run the React Development Server

```bash
npm run dev
```

The React application will start on `http://localhost:5175`

**Access the application:**

- Open your browser to http://localhost:5175
- You should see the POT login/signup page

---

## Next Steps

Once you have POT running locally, proceed to:

**[First-Time Configuration](FIRST-TIME-SETUP.md)** - Create your first user and configure platform admin access

---

## Additional Commands

### Stop PostgreSQL Docker Container

```bash
docker stop pot-postgres
docker rm pot-postgres
```

### Restart PostgreSQL Docker Container

```bash
docker start pot-postgres
```

### View PostgreSQL Logs

```bash
docker logs pot-postgres
```

### Re-run Migrations (after pulling new code)

```bash
cd Source/Server
dotnet run --project Pot.Data.Migrations
```

---

**Return to:** [Getting Started](GETTING-STARTED.md)
