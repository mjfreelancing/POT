---
name: document_csharp
description: Document a C# class and its public API with XML comments.
---

Document the selected C# class using XML comments.

## Scope

- Document C# class-level and public API surface comments.

## Rules

- Update class `<summary>` and public member docs (`<summary>`, `<param>`, `<returns>` as needed).
- Prefer interface-first docs: use `<inheritdoc/>` on implementations when interface docs are authoritative.
- Do not add XML comments to private members. Regular code comments (e.g., `//`) can be used for internal implementation notes if necessary.
- Use `<see cref="..."/>` for resolvable symbols, `<c>...</c>` for literals/snippets, and `<see langword="..."/>` for language keywords.
- Keep wording concise, implementation-safe, and consistent with POT terminology.
- If intent is ambiguous, ask focused clarification questions before writing speculative docs.

## Expansion Notes

- Add XML documentation conventions here; keep language/framework coding standards in instruction files.
- Keep any future alias/compat behavior documented in this prompt header or README, not duplicated across prompts.
