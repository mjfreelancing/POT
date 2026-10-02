---
name: document-csharp
description: Add or update XML documentation comments on a C# class and its public API.
argument-hint: "[file or class]"
---

# Document C#

Document the target class and its public API surface, following the XML documentation rules in `Source/Server/CLAUDE.md`.

- Update the class `<summary>` and public member docs (`<summary>`, `<param>`, `<returns>` where needed).
- Prefer interface-first docs: use `<inheritdoc/>` on implementations when the interface docs are authoritative.
- No XML comments on private members; use regular code comments for internal notes.
- Use `<see cref="..."/>` for resolvable symbols, `<c>...</c>` for literals or snippets, `<see langword="..."/>` for keywords.
- Keep wording concise, implementation-safe and aligned with the projection-first terminology used across POT documentation.
- If intent is ambiguous, ask focused questions before writing speculative docs.
