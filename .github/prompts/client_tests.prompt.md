---
name: client_tests
description: Create or update POT client tests (Vitest + Testing Library).
---

Create tests for `Source/Client/pot-react`.

## Scope

- Target client tests under `Source/Client/pot-react/tests/**`.
- Follow scoped instruction files for coding and assertion standards.

## Workflow

1. Place tests in the mirrored `tests/**` location.
2. Follow scoped instruction files for coding and assertion conventions.
3. Keep tests deterministic and behavior-focused.
4. Run narrow scope first, then broaden only if required.

## Execution

- Run `npm run test` in `Source/Client/pot-react`.
- Broaden only when necessary (for example `npm run test:ui`).

## Notes

- This prompt currently covers client test work broadly (unit/component/integration-style tests under the existing Vitest setup).
- If the client test architecture later splits into explicit unit vs integration harnesses, add separate prompts at that time.

## Expansion Notes

- Add framework-specific client test conventions to instruction files, not this prompt.
- Add any new execution command variants under `Execution`.
