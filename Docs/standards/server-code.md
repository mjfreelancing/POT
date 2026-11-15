# Server rules: C# code conventions

Owned by `Source/Server/.editorconfig` where a tool can decide a rule, and by review where it cannot.
This document states each convention once; `.github/instructions/server.instructions.md` carries the
condensed version an agent reads. A rule that comes from a decision cites it: the AllOverIt guard and
validator idiom is
[ADR-0056](../adr/ADR-0056-the-server-builds-on-the-alloverit-library-family.md), registration by marker
interface is
[ADR-0055](../adr/ADR-0055-services-are-registered-by-scanning-each-layer-for-its-marker-interface.md),
and the layering the rules sit inside is [ADR-0003](../adr/ADR-0003-the-server-is-layered-with-feature-slices-mirrored-across-layers.md).

## Quick reference

| #   | Rule                                                                               | Enforcement                                         | Provenance                            |
| --- | ---------------------------------------------------------------------------------- | --------------------------------------------------- | ------------------------------------- |
| 1   | Run the style check over the files you changed                                     | `[review]`                                          | project decision                      |
| 2   | One class per file                                                                 | `[review]`                                          | project decision                      |
| 3   | Default to `sealed`                                                                | `[review]`                                          | project decision                      |
| 4   | No primary-constructor syntax, except the base-call form                           | `[review]`                                          | diverges from the platform convention |
| 5   | No sync-over-async                                                                 | `[review]`                                          | platform convention                   |
| 6   | Extension methods live in an `Extensions` folder, named `<ExtendedType>Extensions` | `[review]`                                          | project decision                      |
| 7   | Request mapping extensions stay beside their handler                               | `[review]`                                          | project decision                      |
| 8   | Guards and state checks use AllOverIt                                              | `[review]`                                          | project decision (ADR-0056)           |
| 9   | Parameters wrap only past 140 characters, after a comma                            | `[review]`                                          | project decision                      |
| 10  | Members are declared in one order                                                  | `[review]`                                          | project decision                      |
| 11  | Constructor shape: logger last, backing fields in parameter order                  | `[review]`                                          | project decision                      |
| 12  | `CancellationToken` is the last argument                                           | `[review]`                                          | platform convention                   |
| 13  | Lock fields are declared as `System.Threading.Lock`                                | `[warning]` (`csharp_prefer_system_threading_lock`) | project decision                      |
| 14  | Naming: `I`-prefixed interfaces, PascalCase types and members, `_camelCase` fields | `[warning]` (naming rules)                          | platform convention                   |
| 15  | No unused usings                                                                   | `[warning]` (`IDE0005`, off in migrations)          | platform convention                   |
| 16  | XML documentation on every public, internal and protected member                   | `[review]`                                          | project decision                      |

## Rules

### 1. Run the style check over the files you changed

- **Enforcement:** `[review]` — the whole solution build does not fail on naming, so a run of the
  command below is the only thing that shows them.
- **Provenance:** project decision, because only nullable warnings fail the build
  (`WarningsAsErrors` in `Source/Server/Directory.Build.props`).

From `Source/Server`:

```powershell
dotnet format style <Project>/<Project>.csproj --verify-no-changes --severity warn --include <changed files>
```

Fix every violation in a file you touched, leave pre-existing violations in files you did not, and do not
edit `Pot.Data/Migrations`.

### 2. One class per file

- **Enforcement:** `[review]`
- **Provenance:** project decision.

Nested types, records and enums that belong to the enclosing type are exempt, as is a marker interface
declared beside the base type it marks — the `IPreCreateCheck` / `PreCreateCheckBase` shape.

### 3. Default to `sealed`

- **Enforcement:** `[review]`
- **Provenance:** project decision.

A type is opened for inheritance when a second implementation exists, not in anticipation of one.

### 4. No primary-constructor syntax, except the base-call form

- **Enforcement:** `[review]`
- **Provenance:** diverges from the platform convention. `.editorconfig` preferred primary constructors
  until 2026-11-12; `csharp_style_prefer_primary_constructors` is now `false` so the IDE stops
  suggesting one.
- **Where a tool decides it:** `csharp_style_prefer_primary_constructors`.

The constructor signature is the injection surface and it is written out. Chaining several constructors
to one main constructor is not primary-constructor syntax. The exception is a type that adds nothing and
only passes base arguments: `internal sealed class SmallStringAttribute() : MaxLengthAttribute(50)`.

### 5. No sync-over-async

- **Enforcement:** `[review]`
- **Provenance:** platform convention.

No `.Result`, `.Wait()` or `.GetAwaiter().GetResult()`, and `CancellationToken` propagation stays intact
through the call chain.

### 6. Extension methods live in an `Extensions` folder

- **Enforcement:** `[review]`
- **Provenance:** project decision.

Name the file and class `<ExtendedType>Extensions`.

### 7. Request mapping extensions stay beside their handler

- **Enforcement:** `[review]`
- **Provenance:** project decision.

A feature's request mapping extension methods live in a `RequestMapping` file and class in the feature's
own namespace. The `<ExtendedType>Extensions` convention is for reusable helpers, not for these.

### 8. Guards and state checks use AllOverIt

- **Enforcement:** `[review]`
- **Provenance:** project decision, because the library's conditionals have no primitive equivalent and
  mixing the two idioms is what the decision avoids — [ADR-0056](../adr/ADR-0056-the-server-builds-on-the-alloverit-library-family.md).

`_ = argument.WhenNotNull();` rather than `ArgumentNullException.ThrowIfNull(argument)`, and
`Throw<TException>` for state that should already hold. Where the library's member is not
nullable-annotated and the compiler must narrow the value, the framework's member is used instead.

### 9. Parameters wrap only past 140 characters, after a comma

- **Enforcement:** `[review]`
- **Provenance:** project decision.

Parameters start on the same line as the method name. Wrap after a comma, several parameters per
continuation line — never one per line and never at the opening parenthesis.

```csharp
private async Task DoSomethingAsync(string firstParam, int secondParam, bool thirdParam,
    CancellationToken cancellationToken)
```

### 10. Members are declared in one order

- **Enforcement:** `[review]`
- **Provenance:** project decision.

Nested types, constants, static readonly fields, private readonly dependency fields, other fields,
properties, constructors, then methods ordered public → protected → internal → private. Constants come
before other fields and static readonly fields before instance fields.

### 11. Constructor shape

- **Enforcement:** `[review]`
- **Provenance:** project decision.

The logger (`ILogger<T>`) is the last injected parameter. Backing fields follow the order of the
constructor parameters. Chain several constructors to one main constructor rather than duplicating the
assignments.

### 12. `CancellationToken` is the last argument

- **Enforcement:** `[review]`
- **Provenance:** platform convention.

Unless a language constraint requires otherwise, for example `[CallerMemberName]` after it.

### 13. Lock fields are declared as `System.Threading.Lock`

- **Enforcement:** `[warning]` — `csharp_prefer_system_threading_lock` in `.editorconfig`.
- **Provenance:** project decision, because every project targets `net10.0`, where `object` is only the
  choice for a framework that predates `System.Threading.Lock`.

`private readonly Lock _gate = new();`. `lock (_gate)` is written as before and compiles to
`Lock.EnterScope()`.

### 14. Naming

- **Enforcement:** `[warning]` — the naming rules in `.editorconfig` (interfaces begin with `I`, types
  and non-field members are PascalCase, private and internal fields are `_camelCase`, private and
  internal const and static fields are PascalCase).
- **Provenance:** platform convention.

The one allowed abbreviation in a variable name is `kvp`, for a dictionary key-value pair in LINQ.

### 15. No unused usings

- **Enforcement:** `[warning]` — `IDE0005`, set to `none` for `Pot.Data/Migrations`.
- **Provenance:** platform convention.

### 16. XML documentation on every public, internal and protected member

- **Enforcement:** `[review]`
- **Provenance:** project decision.

- `private` members are documented only when they are not obvious from their name and signature. Each
  enum member gets a `<summary>`.
- `<inheritdoc />` on an explicit interface implementation, and on a member that fully implements or
  overrides a documented member. Add `<remarks>` only when the override meaningfully diverges.
- `<summary>` is one or two sentences saying what, not how, and does not restate the member name. Every
  parameter gets a `<param>`; `<returns>` appears only when the return type does not already say what the
  value means.
- `<see cref="..." />` for a type, `<see langword="..." />` for a keyword, `<remarks>` for threading,
  side effects or rationale, and `<exception cref="..." />` only on a public or internal method that
  deliberately throws.
- Do not add documentation to silence an analyzer on trivial boilerplate, generated code or a test
  helper; suppress the warning instead.
