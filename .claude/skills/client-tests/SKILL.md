---
name: client-tests
description: Create or update Vitest tests for the React/TypeScript client with deterministic setup and user-observable assertions.
argument-hint: "[component, hook or module under test]"
---

# Client Tests

Conventions: `Source/Client/pot-react/tests/CLAUDE.md`.

1. Place tests under `Source/Client/pot-react/tests/**`, mirroring the `src` structure.
2. Keep tests deterministic; assert user-observable behavior over implementation details.
3. Run narrow first, then broaden. Client tests must be run from `Source/Client/pot-react`: `npm run test` (or `npm run test:ui` for interactive mode). Running from the repository root does not resolve the client project.
