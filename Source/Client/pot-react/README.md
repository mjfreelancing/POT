# POT React Client

This is the React client application for the POT (Paid On Time) budget utility.

## Setup Options

You can run the application in two different modes depending on whether you want to test against the production Docker environment or debug the server locally.

### Production Use (Docker)

This mode runs the entire application stack using Docker containers.

1. **Configure the React app for production**:

   - Ensure the `.env` file has the production API URL uncommented:

   ```properties
   # Production
   VITE_API_BASE_URL=http://localhost:5241/api

   # Development
   # VITE_API_BASE_URL=http://localhost:5242/api
   ```

2. **Start the Docker containers**:

   ```bash
   # From the root workspace, run the Docker task
   # This starts PostgreSQL (port 5444) and the ASP.NET server (port 5241)
   ```

   Or manually:

   ```bash
   cd Source/Docker
   docker-compose -p pot -f docker-compose.yml up --build -d
   ```

3. **Install React dependencies and start the client**:

   ```bash
   npm install
   npm run dev
   ```

4. **Access the application**:
   - React app: `http://localhost:5175`
   - API server: `http://localhost:5241`
   - Database: `localhost:5444` (PostgreSQL)

### Development Testing (Local Server)

This mode allows you to debug the ASP.NET server locally while still running the React development client.

1. **Configure the React app for development**:

   - Update the `.env` file to use the development API URL:

   ```properties
   # Production
   # VITE_API_BASE_URL=http://localhost:5241/api

   # Development
   VITE_API_BASE_URL=http://localhost:5242/api
   ```

2. **Set up the PostgreSQL database for development**:

   **Important**: Development mode uses a separate PostgreSQL database on the default port (5432) to avoid conflicts with the production Docker database.

   - **First, ensure the production Docker containers are stopped** to avoid accidental data conflicts:

   ```bash
   cd Source/Docker
   docker-compose -p pot -f docker-compose.yml down
   ```

   - **Set up a local PostgreSQL instance** running on the default port (5432):
     - Install PostgreSQL locally, or
     - Use a separate Docker container on the default port, or
     - Use an existing PostgreSQL installation on your development machine
   - **Ensure your development database is configured with**:
     - Host: `localhost` (port 5432 - the default)
     - Username: `postgres`
     - Password: `password`
     - Database name: `Pot` (or as configured in your connection string)

3. **Run the ASP.NET server locally**:

   - Open the server solution in Visual Studio or VS Code
   - Set `Pot.AspNetCore` as the startup project
   - Run with debugger (F5) - this will start the server on `http://localhost:5242`

4. **Install React dependencies and start the client**:

   ```bash
   npm install
   npm run dev
   ```

5. **Access the application**:
   - React app: `http://localhost:5175`
   - API server: `http://localhost:5242` (local with debugging)
   - Database: `localhost:5432` (local PostgreSQL - default port)

## Development Setup

Choose one of the setup options above based on your testing needs.

## Available Scripts

- `npm run dev` - Start development server on port 5175 (use `npx kill-port 5175` if you need to brute-force kill it)
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Check code style
- `npm run lint:fix` - Fix linting issues
- `npm run lint:sort` - Fix and sort imports
- `npm run pretty` - Format code with Prettier
- `npm run test` - Run tests
- `npm run test:ui` - Run tests with UI and coverage on port 9527 (the default 51204 fails with an access error)
- `npm run type:check` - Verify TypeScript types

## Technology Stack

- React 19
- TypeScript
- Vite
- TailwindCSS
- Radix UI Components
- React Query
- React Hook Form
- React Router
- Vitest for testing
