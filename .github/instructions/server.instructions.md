---
description: "C#/.NET, ASP.NET Core, EF Core, DI and XML documentation rules for the server"
applyTo: "Source/Server/**"
---

# Server (.NET / ASP.NET Core / EF Core)

Run server commands from `Source/Server`. `.editorconfig` is the source of truth for formatting and analyzer style. Test conventions are in `.github/instructions/dotnet-tests.instructions.md`; AllOverIt helper preferences are in `.github/instructions/allsoverit-patterns.instructions.md`.

## Architecture

- Layering direction: `Pot.AspNetCore` -> `Pot.App` -> `Pot.Data`. Migrations live in `Pot.Data.Migrations`.
- Prefer vertical-slice organization per feature or operation.
- Handlers are thin: validate, map input, call service, map result to a typed HTTP result.
- Register endpoints through the extension chain in `Pot.AspNetCore/Program.cs` (`Add*Endpoints()`).
- Business failures flow via `EnrichedResult` plus Problem Details mapping. Validate requests with FluentValidation; keep Problem Details mapping consistent and do not leak sensitive request/header data in errors.
- Public contracts use the external `RowId` (Guid) plus concurrency `Etag`; never expose internal persistence ids.
- Authorization uses `resource:action` permission strings, enforced at endpoint/service boundaries, respecting the existing site-isolation patterns.
- Keep `/_health` available and stable when changing startup or infrastructure.

## Persistence (Postgres + EF Core)

- Schema changes are migration-backed and isolated to `Pot.Data.Migrations`. Keep migrations small and focused. Migration generation is a developer-reviewed step: prompt the developer to run `add-migration`; do not generate one unless asked.
- Default reads to no-tracking; opt into tracking only on write paths.
- Avoid N+1 queries and over-fetching. Keep transaction boundaries explicit for multi-step writes.
- Preserve concurrency and integrity constraints. No destructive schema/data operations unless explicitly requested.

## C# conventions

- After creating or editing C# files, run `dotnet format style <Project>/<Project>.csproj --verify-no-changes --severity warn --include <changed files>` from `Source/Server` and fix every reported violation in the files you touched (naming rules are enforced at `warning` in `.editorconfig`; the normal build does not report them). Ignore pre-existing violations in files you did not change, and do not edit `Pot.Data/Migrations`.

- One class per file (nested types/records/enums that belong to the enclosing type are exempt). Default to `sealed`.
- No primary constructors; use explicit constructors. Prefer constructor injection; avoid static dependencies.
- No sync-over-async (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`); keep `CancellationToken` propagation intact.
- Extension methods go in an `Extensions` folder; name the file/class `<ExtendedType>Extensions`.
- Guard clauses use AllOverIt (`_ = arg.WhenNotNull();`), not `ArgumentNullException.ThrowIfNull(arg)`.
- Parameter formatting: parameters start on the same line as the method name and wrap only when the signature exceeds 140 characters. Wrap after a comma, several parameters per continuation line, never one per line and never at the opening parenthesis.

  ```csharp
  // CORRECT (> 140 chars)
  private async Task DoSomethingAsync(string firstParam, int secondParam, bool thirdParam,
      CancellationToken cancellationToken)
  ```

- Member order: nested types, constants (before other fields), static readonly fields (before instance fields), private readonly dependency fields, other fields, properties, constructors, methods ordered public -> protected -> internal -> private.
- Constructors: the logger (`ILogger<T>`) is the last injected parameter. Backing fields follow the same order as the constructor parameters. Chain multiple constructors to one primary constructor instead of duplicating assignments.
- Methods: `CancellationToken` is the last argument, unless a language constraint requires otherwise (for example `[CallerMemberName]` after it).
- Allowed abbreviation in variable names: `kvp` (dictionary key-value pairs in LINQ). Add new entries here as needed.

## Dependency injection

- Register concrete classes for internal, single-implementation plumbing. Add an interface only when there are multiple implementations now, it is an intentional public/extensible boundary, a real outside consumer needs it, or tests need a seam and no simpler one exists.
- Keep implementation classes non-public; do not make them `public` for DI convenience. "Internal first": promote to interface/public only when a concrete requirement appears. Re-evaluate single-implementation interfaces with no extension point.
- Keep registration in a small set of composition-root extension methods. Default lifetime is `Scoped`; use `Singleton` or `Transient` only with a proven reason.

## XML documentation

- Document all `public` and `internal` types and members, and all `protected` members. Do not document `private` members unless non-obvious. Enum members each get a `<summary>`.
- Use `<inheritdoc />` for explicit interface implementations and for members that fully implement/override a documented member; add `<remarks>` only when the override meaningfully diverges.
- `<summary>`: one or two sentences saying what, not how; do not restate the member name. Write a `<param>` for every parameter. Write `<returns>` only when the return type does not already say what the value means.
- Use `<see cref="..." />` rather than plain type names, `<see langword="..." />` for keywords, `<remarks>` for threading/side-effect/rationale notes, and `<exception cref="..." />` only on public/internal methods that deliberately throw.
- Do not add docs just to silence an analyzer on trivial boilerplate, generated code or test helpers; suppress the warning instead.
