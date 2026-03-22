---
name: document_csharp
description: Document a C# class and its public API with XML comments.
---

Document the selected C# class using XML comments.

## Scope

- Document class-level and public API surface comments.

## Rules

- Update class `<summary>` and public member docs (`<summary>`, `<param>`, `<returns>` as needed).
- Prefer interface-first docs: use `<inheritdoc/>` on implementations when interface docs are authoritative.
- Do not add XML comments to private members. Regular code comments can be used for internal implementation notes if necessary.
- Use `<see cref="..."/>` for resolvable symbols, `<c>...</c>` for literals or snippets, and `<see langword="..."/>` for language keywords.
- Keep wording concise, implementation-safe, and consistent with existing repository terminology.
- If intent is ambiguous, ask focused clarification questions before writing speculative docs.

## Expansion Notes

- Add repository-specific documentation conventions here when needed.
- Keep coding and architecture rules in instruction files.
  - POT: Keep wording aligned with projection-first terminology used across POT documentation.
