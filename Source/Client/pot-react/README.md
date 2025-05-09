# POT React Client

This is the React client application for the POT (Paid On Time) budget utility.

## Development Setup

1. Install dependencies:

```bash
npm install
```

2. Start the development server:

```bash
npm run dev
```

The application will be available at `http://localhost:5175`

## Available Scripts

- `npm run dev` - Start development server on port 5175
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
