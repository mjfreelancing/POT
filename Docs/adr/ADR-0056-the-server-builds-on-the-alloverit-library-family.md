---
id: ADR-0056
title: The server builds on the AllOverIt library family, not the framework's primitives
status: accepted
date: 2024-08-13
area: backend
---

# ADR-0056: The server builds on the AllOverIt library family, not the framework's primitives

**Summary:** In the context of a layered server that guards arguments, asserts state, validates requests
and registers services in every layer, facing the choice between the framework's own primitives and one
third-party family, we decided to build on AllOverIt for those concerns, to achieve a single idiom across
the server and its tests, accepting a dependency that spans every layer and a handful of members that the
framework still covers better.

## Context and Problem Statement

Every layer needs the same four things: a guard on an argument, an assertion about state that should
already hold, a validator for a request, and a way to register a service. Each is available in the
framework — `ArgumentNullException.ThrowIfNull`, `AbstractValidator<T>`, plain `IServiceCollection`
registration — so the choice is not about capability but about which idiom the code is written in.

The reason on record for the family is consistency, with a concrete edge: its conditional helpers have
overloads the primitives do not, so using both would mean two ways to express the same guard. The family
has been in the tree since the first check-in (`b06dd624`, 2024-08-13) and is pinned as twelve packages in
`Source/Server/Directory.Packages.props` at `[9.4.0,10.0.0)`. The server makes 432 `WhenNotNull()` calls
and no `ArgumentNullException.ThrowIfNull` calls at all. The same family supplies `ServiceRegistrarBase`
and the `AutoRegister*` extensions behind [ADR-0055](ADR-0055-services-are-registered-by-scanning-each-layer-for-its-marker-interface.md),
`ValidatorBase<T>` with `DisablePropertyNameSplitting()`, and `AllOverIt.EntityFrameworkCore.Diagrams` for
the ERD.

## Decision Drivers

- One guard idiom, because a per-file choice produces two spellings of the same check.
- The conditional helpers (`Throw<T>.When`, `.WhenNot`, `.WhenNullOrEmpty`, up to four deferred exception
  arguments) express checks the framework's throw helpers cannot.
- The tests build on the same family, so a fixture or helper exists once.

## Considered Options

1. The framework's primitives with FluentValidation used directly.
2. AllOverIt for guards, state assertions, validation and registration (status quo).
3. Mixed, choosing per concern or per file.

## Decision Outcome

Chosen option: 2, because it is the only option that leaves one idiom for the checks that appear in every
layer, and its conditional helpers cover cases the primitives do not. Consistency is the stated reason;
the alternatives were not weighed in a recorded comparison.

The rules it imposes:

- An argument or state guard uses AllOverIt: `_ = argument.WhenNotNull();`, and `Throw<TException>` for
  state that should have been established before the method runs. Constructor parameters resolved by the
  container need no guard, because the container throws when a dependency cannot be resolved.
- A validator inherits `ValidatorBase<T>`, calls `DisablePropertyNameSplitting()` in its static
  constructor, and uses `IsNotEmpty()` for a required string.
- Service registration goes through the family's scanning — see
  [ADR-0055](ADR-0055-services-are-registered-by-scanning-each-layer-for-its-marker-interface.md).
- Where the family's member would lose nullability information, the framework's member is used instead,
  because nullable warnings are errors here. `string.IsNullOrEmpty` is used for exactly that reason in
  `Pot.Data.Migrations/App.cs:98-100`, with the reason in a comment beside it.

### Consequences

- Good, because a guard reads the same way in every layer, and a reader learns one idiom.
- Good, because the conditional helpers state a precondition that the framework's throw helpers cannot
  express without a hand-written `if` and message.
- Good, because the validator base removes property-name splitting and gives the project one validation
  shape.
- Bad, because twelve packages must be kept in step, and the pinned range is a standing upgrade decision
  rather than a one-off choice.
- Bad, because the members are not uniformly nullable-annotated, so the framework's equivalent is still
  used where the compiler must narrow a value — two idioms exactly where nullability matters.
- Bad, because `AllOverIt.Logging.Testing` is deliberately not adopted, so a log assertion reads
  `ReceivedCalls()` off the `ILogger<T>` substitute rather than a helper.
- Bad, because a reader who does not know the library must learn it before a guard or a validator looks
  ordinary.

### Confirmation

- Instruction: `.github/instructions/alloverit-patterns.instructions.md` states the guard, validator,
  state-check and logging rules.
- Rule document: `Docs/standards/server-code.md` (rule 8) states the guard rule and its exception.
- Package: `Source/Server/Directory.Packages.props` pins the family; leaving the pinned range is a
  change to this decision.
- Review: `ArgumentNullException.ThrowIfNull` or an `AbstractValidator<T>` subclass in new code is a
  defect, and so is a validator that omits `DisablePropertyNameSplitting()`.

## More Information

- The family in use spans `AllOverIt`, `.Validation`, `.DependencyInjection`, `.EntityFrameworkCore`,
  `.EntityFrameworkCore.Diagrams`, `.GenericHost`, `.Logging`, `.Cryptography`, `.Csv`,
  `.Serialization.Binary`, `.Serialization.Json.SystemText` and `.Fixture` for the tests.
- `AllOverIt.Logging.Testing` is not referenced by any project; logging assertions are an open decision
  recorded in `.github/instructions/dotnet-tests.instructions.md`.
