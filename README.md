# POT

A simple money budget utility to ensure debts are Paid On Time.

# Starting the Dockerised server

You can start and stop the Docker containers directly from Visual Studio Code using the predefined tasks. From the root folder:

1. Press `Shift+Ctrl+P` (or `Shift+Cmd+P` on macOS) to open the Command Palette.
2. Type `Run Task` and select it.
3. Choose one of the following tasks:
   - **docker-start-pot**: Starts the Docker containers and builds the images if necessary.
   - **docker-stop-pot**: Stops and removes the Docker containers.

These tasks are defined in the `.vscode/tasks.json` file.

# Starting the client

From the `/Source/Client/pot-react` folder:

- `npm run dev`
- Open the browser at the indicated route, such as `http://localhost:5175/`

# Scripts

## Development

- `npm run dev` - Start the development server on port 5175
  ```bash
  vite --port 5175
  ```

## Production

- `npm run build` - Build the application for production
  ```bash
  tsc -b && vite build
  ```
- `npm run preview` - Preview the production build locally
  ```bash
  vite preview
  ```

## Code Quality

- `npm run lint` - Check code for style issues
  ```bash
  eslint .
  ```
- `npm run lint:fix` - Automatically fix linting issues
  ```bash
  eslint . --fix
  ```
- `npm run lint:sort` - Fix and sort imports
  ```bash
  eslint --fix --fix-type layout,suggestion src/**/*.{ts,tsx}
  ```
- `npm run pretty` - Format code using Prettier
  ```bash
  prettier . --write
  ```

## Testing

- `npm run test` - Run unit tests
  ```bash
  jest
  ```
- `npm run test:watch` - Run tests in watch mode
  ```bash
  jest --watch
  ```
- `npm run test:coverage` - Generate test coverage report
  ```bash
  jest --coverage
  ```

## Type Checking

- `npm run type:check` - Verify TypeScript types
  ```bash
  tsc --noEmit
  ```
