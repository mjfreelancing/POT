---
name: document-typescript
description: Add or update TSDoc/JSDoc on TypeScript modules, functions, types, React components and hooks.
argument-hint: "[file or symbol]"
---

# Document TypeScript

Document the target with concise, useful docs.

- Use TSDoc/JSDoc on exported APIs and non-obvious behavior.
- Add `@param`, `@returns` and `@throws` only when meaningful.
- Components: document the props contract and side effects when not obvious. Hooks: document inputs, return shape and key behavioral guarantees.
- Do not add comments that restate obvious type signatures.
- Keep terminology aligned with existing repository wording and feature names.
- If intent is ambiguous, ask focused questions before adding speculative docs.
