---
name: client_tests
description: Create or update client tests with deterministic setup and behavior-focused assertions.
---

Create tests for client code.

## Scope

- Target client tests and supporting test utilities for the active web client.
- Follow scoped instruction files for framework-specific testing conventions.

## Workflow

1. Place tests in the nearest client test location or mirrored test folder.
2. Keep tests deterministic and behavior-focused.
3. Prefer user-observable outcomes over implementation details.
4. Run narrow scope first, then broaden only if required.

## Execution

Adjust the example paths and commands below to match your repository structure.

- Default client test suite: `npm run test` (from `.\Source\Client\my-app`)
- Broaden only when necessary, such as UI or watch modes documented by the repository.

## Expansion Notes

- Keep framework-specific client test rules in instruction files.
- Add repository-specific commands and paths under `Execution`.
  - POT: Client tests live under `Source/Client/pot-react/tests/**`.
  - POT: Mirror tests into the repository's `tests/**` location.
  - POT: Run `npm run test` from `Source/Client/pot-react`.
  - POT: Use `npm run test:ui` when interactive UI mode is needed.
