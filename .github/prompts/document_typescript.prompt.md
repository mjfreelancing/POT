---
name: document_typescript
description: Document TypeScript modules, functions, types, and components using TSDoc/JSDoc.
---

Document selected TypeScript code with concise, useful docs.

## Scope

- Document TypeScript modules, functions, types, components, and hooks where API/behavior clarity is needed.

## Rules

- Prefer TSDoc/JSDoc comments for exported APIs and non-obvious behavior.
- Document `@param`, `@returns`, and `@throws` only when meaningful.
- For React components, document props contract and side effects when not obvious.
- For hooks, document inputs, return shape, and key behavioral guarantees.
- Do not add noisy comments that restate obvious type signatures.
- Keep terminology aligned with existing repository wording and feature names.
- If intent is ambiguous, ask focused clarification questions before adding speculative docs.

## Expansion Notes

- Add documentation-style rules here; keep coding/architecture rules in instruction files.
- Add examples only when they clarify non-obvious documentation patterns.
