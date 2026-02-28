---
applyTo: "Source/Client/pot-react/tests/**"
---

# Client Test Instructions

## Core Rules

### Test Design

- Use deterministic tests with mocked external boundaries.
- Prefer behavior-focused assertions over implementation details.
- Mirror source structure in test folders for discoverability.

### Tooling and Naming

- Test framework is Vitest + jsdom (`vitest.config.ts`) with shared setup in `tests/setup.ts`.
- Use Testing Library queries and user interactions for component/UI tests.
- Keep test naming consistent with existing pattern (`*.test.ts` / `*.test.tsx`).

### API and Utility Coverage

- For API hooks and interceptors, assert on the repo `Result` pattern (`success` branch + fail branch), not exception-first flow.
- For utilities, keep tests lightweight and table-driven where useful (match existing `tests/lib/*`).
- If test setup behavior changes, update only `tests/setup.ts` and keep global side effects minimal.

## Commands

Run in `Source/Client/pot-react`:

- `npm run test`
- `npm run test:ui`

## Expansion Notes

- Add testing-library or Vitest conventions under `Tooling and Naming`.
- Add feature-specific test patterns under `API and Utility Coverage` with concrete path examples.
- Keep project-wide test behavior in this file and avoid duplicating it in prompts.
