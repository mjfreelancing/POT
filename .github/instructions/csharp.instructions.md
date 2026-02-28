---
applyTo: "Source/Server/**/*.cs"
---

# C# Instructions

## Core Rules

### Language and Code Quality

- Use clear naming and cohesive feature-level organization (handlers/services/repositories grouped by capability).
- Prefer `var` when the assigned type is obvious from the right-hand side.
- Avoid one-letter variable names unless loop/index intent is obvious.
- Keep methods focused and avoid unnecessary abstraction.
- Prefer compact method/constructor signatures: keep parameter lists on one line when reasonably readable; avoid one-parameter-per-line formatting unless length/clarity requires wrapping.
- Follow these member-ordering conventions for classes:
  - Place nested types (including private nested classes) at the top of the containing type, above constants, fields, and properties.
  - Place constants before other fields; place static readonly fields before instance fields.
  - Place private readonly dependency fields above other instance fields.
  - Place properties above constructors.
  - Keep constructors above methods.
  - Order methods by visibility: public, protected, internal, private.
- Avoid sync-over-async (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`).
- Keep `CancellationToken` propagation intact in async chains.
- Place extension methods in an `Extensions` folder and name extension files/classes after the extended type (`<ExtendedType>Extensions`).
- Preserve public API shape unless change is explicitly requested / approved.
- Always implement code with testability in mind; prefer constructor injection and avoid static dependencies.
- Default to using sealed classes unless extensibility is required.
- Use collection initializers and expression-bodied members where they enhance readability.
- Do not use primary constructors; prefer explicit constructor definitions for clarity and testability.

### Architecture and Contracts

- Layering direction: `Pot.AspNetCore` -> `Pot.App` -> `Pot.Data`.
- API handlers are minimal and map to typed HTTP results; business failures flow via `EnrichedResult` + Problem Details mapping.
- Authorization convention uses `resource:action` permission strings.
- Keep entity contracts on external `RowId` + concurrency `Etag` conventions.

## Test Alignment

- Current repository uses xUnit with Shouldly and NSubstitute.
- Keep assertion style consistent per test project; avoid mixing assertion frameworks within new tests.
- Refer to Pot.TestUtils for Shouldly test helpers.

## Expansion Notes

- Keep general C# language conventions here; move server-specific workflow rules to `server.instructions.md`.
- Add project or namespace examples when introducing new architecture constraints.
- For test-style changes, update `server.tests.instructions.md` in the same change.
