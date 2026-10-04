# Client tests (Vitest + jsdom)

Client tests must be run from `Source/Client/pot-react`: `npm run test` (`npm run test:ui` for interactive mode). Running from the repository root does not resolve the client project. Config: `vitest.config.ts`. Shared setup: `tests/setup.ts`; keep global side effects there minimal and make global setup changes only there.

## Design

- Deterministic tests with mocked external boundaries; assert behavior, not implementation details.
- One behavior or contract per test; split a case that validates several independent behaviors.
- Use Testing Library queries and user interactions for component/UI tests. For user-visible behavior (selection, visibility, navigation state, styling state), also include a test that renders the real UI primitive and asserts the visible outcome, not only mocked children.
- For API hooks and interceptors, assert on the repo `Result` shape (`result.success`) instead of exception-first flows.
- Never force failures with `expect(true).toBe(false)` in async tests; use `await expect(promise).rejects...` or `await expect(fn).rejects/toThrow...`.
- Utility tests (see `tests/lib/*`) are lightweight and table-driven where that reads better.
- Aim for full coverage of behavior and branches in the module under test; add missing cases rather than leaving gaps.

## Structure and naming

- Mirror the `src` structure under `tests/` when it helps discoverability. Order test cases to follow the production logic.
- Files: `<ComponentName>.test.ts(x)`; when split by concern, `<ComponentName>.<Category>.test.ts(x)` with a concise PascalCase category (for example `EnrichedCalendar.Callbacks.test.tsx`).
- Reuse shared helpers in `tests/shared/*` (`factories`, `auth`, `react-query`, `rows`) before writing inline builders. A pattern repeated across tests is a candidate for extraction there.
- Formatting: one blank line between adjacent multi-line code blocks (back-to-back object literals, multiline assignments) and between adjacent `expect(...)` calls when either spans multiple lines.
