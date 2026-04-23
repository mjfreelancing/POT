---
applyTo: "Source/Client/pot-react/tests/**"
---

# Client Test Instructions

## Core Rules

### Test Design

- Use deterministic tests with mocked external boundaries.
- Prefer behavior-focused assertions over implementation details.
- Keep each test single-concern: one behavior or contract per test case. If a case validates multiple independent behaviors, split it into separate tests.
- For user-visible UI behavior (visual selection, visibility, navigation state, styling state), do not rely only on mocked child-component rendering; include tests that renders the real UI primitive/component and asserts the visible outcome.
- Mirror source structure in test folders when that improves discoverability.
- Do not use guard assertions like `expect(true).toBe(false)` to force failures in async tests; prefer explicit `await expect(promise).rejects...` or `await expect(fn).rejects/toThrow...` patterns.

### Tooling and Naming

- Use the repository's existing client test framework, environment, and shared setup conventions.
- Keep test case order aligned with the implementation logic order when practical so the suite reads in the same flow as the production code.
- Prefer user-observable assertions and interactions for component and UI tests.
- Use `*.test.ts` and `*.test.tsx` as the baseline naming format.
- Default to `<ComponentName>.test.ts(x)`.
- When splitting by concern, use `<ComponentName>.<Category>.test.ts(x)` (for example `EnrichedCalendar.Callbacks.test.tsx`).
- Keep the component name first, and use concise PascalCase category names for the split segment.
- In test files, add one blank line between adjacent multi-line code blocks (for example back-to-back object literals or multiline assignments) and between adjacent `expect(...)` assertions when either assertion spans multiple lines.

### API and Utility Coverage

- Assert on the repository's success and failure contract shape instead of relying on exception-first flows when explicit result objects are used.
- Keep utility tests lightweight and table-driven when that improves readability.
- Keep global test setup changes isolated to the repository's shared test setup entry point.
- Aim for full coverage of the behavior and branches in the production module(s) under test; when gaps are found, add the missing test cases instead of leaving partial coverage.

## Expansion Notes

- Add repository-specific framework names, setup file paths, and command examples in consuming copies of this file.
- Keep project-wide client test behavior here and avoid duplicating it in prompt files.
  - POT: Client tests live under `Source/Client/pot-react/tests/**`.
  - POT: Test framework is Vitest plus jsdom (`vitest.config.ts`) with shared setup in `tests/setup.ts`.
  - POT: Use Testing Library queries and user interactions for component and UI tests.
  - POT: Keep test naming consistent with component-first names: `<ComponentName>.test.tsx` or `<ComponentName>.<Category>.test.tsx` when split.
  - POT: For API hooks and interceptors, assert on the repo `Result` pattern instead of exception-first flow.
  - POT: Mirror existing utility test patterns under `tests/lib/*` where useful.
  - POT: Keep `tests/setup.ts` as the shared setup entry point and keep global side effects minimal.
  - POT: Shared test helpers live under `tests/shared/*` (for example `factories`, `auth`, `react-query`, `rows`) and should be reused before adding local inline builders.
  - POT: When a pattern is repeated across tests, treat it as a candidate for extraction into `tests/shared/*` to improve discoverability and consistency.
  - POT: Run `npm run test` and `npm run test:ui` from `Source/Client/pot-react`.
