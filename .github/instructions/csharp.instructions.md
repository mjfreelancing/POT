---
applyTo: "Source/Server/**/*.cs"
---

# C# Instructions

## Core Rules

### Language and Code Quality

- Treat `.editorconfig` as the source of truth for formatting and analyzer style rules.
- Use clear naming and cohesive feature-level organization.
- Avoid one-letter variable names unless loop/index intent is obvious.
- Keep methods focused and avoid unnecessary abstraction.
- Keep method and constructor signatures reasonably compact for readability; wrap parameters when needed for clarity.
- Follow member-ordering conventions consistently within each project:
  - Place nested types above constants, fields, and properties.
  - Place constants before other fields; place static readonly fields before instance fields.
  - Place private readonly dependency fields above other instance fields.
  - Place properties above constructors.
  - Keep constructors above methods.
  - Order methods by visibility: public, protected, internal, private.
- Avoid sync-over-async (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`).
- Keep `CancellationToken` propagation intact in async chains.
- Place extension methods in an `Extensions` folder and name extension files/classes after the extended type (`<ExtendedType>Extensions`).
- Preserve public API shape unless change is explicitly requested or approved.
- Implement with testability in mind; prefer constructor injection and avoid static dependencies.
- Default to sealed classes unless extensibility is required.
- Do not use primary constructors; prefer explicit constructor definitions for clarity and testability.

### C# Architecture

- Keep handlers thin: validate, map input, call service, map result.

## Expansion Notes

- Baseline cross-language rules live in `language-agnostic-core.instructions.md`.
- Keep C#-specific conventions in this file.
- Move project- or layer-specific rules to additional scoped instruction files.
  - POT: Layering direction is `Pot.AspNetCore` -> `Pot.App` -> `Pot.Data`.
  - POT: API handlers are minimal and map to typed HTTP results; business failures flow via `EnrichedResult` plus Problem Details mapping.
  - POT: Authorization convention uses `resource:action` permission strings.
  - POT: Keep entity contracts on external `RowId` plus concurrency `Etag` conventions.
  - POT: For guard clauses, prefer AllOverIt assertion extensions such as `_ = arg.WhenNotNull();` over framework guards like `ArgumentNullException.ThrowIfNull(arg)`.
