---
id: ADR-0057
title: Server tests run on xUnit v3 with the Microsoft Testing Platform turned off
status: accepted
date: 2026-09-23
area: backend
---

# ADR-0057: Server tests run on xUnit v3 with the Microsoft Testing Platform turned off

**Summary:** In the context of five .NET test projects that are run through `dotnet test` with its filter,
collect and settings options, facing xUnit v3's default enablement of the Microsoft Testing Platform,
which a .NET 10 SDK cannot run through the VSTest target those options rely on, we decided to stay on the
VSTest target using the `xunit.v3.mtp-off` package variant, to achieve v3 without losing the test workflow
the repository depends on, accepting a package variant that must be revisited rather than a plain v3
reference.

## Context and Problem Statement

Every server test run in this repository goes through `dotnet test`: a targeted run filters with
`--filter "FullyQualifiedName~..."`, and the coverage script runs `dotnet test pot.sln --collect:"XPlat
Code Coverage"`. Both options are VSTest features.

xUnit v3 test projects are Microsoft Testing Platform (MTP) enabled by default, and on the .NET 10 SDK an
MTP-enabled project cannot run through the VSTest target. A plain `xunit.v3` reference therefore breaks
`--filter` and `--collect` before a single test runs.

The v3 move landed on 2026-09-23 (`f80f3fea`). The reason for the variant is recorded once per test
project, in an XML comment beside the package reference — all five carry it — rather than in a document:

> xUnit.net v3 test projects are Microsoft Testing Platform (MTP) enabled by default, and on .NET 10 SDKs
> and later an MTP-enabled project cannot run through the VSTest target that `dotnet test` (with its
> filter, collect, and settings options) and the coverage script rely on. This package variant ships the
> same v3 framework with MTP support turned off, keeping the project on VSTest. Revisit (plain xunit.v3
> with a global.json MTP opt-in plus Microsoft.Testing.Extensions.CodeCoverage) once MTP-mode
> `dotnet test` is verified to work for this repository.

`Source/Server/Directory.Packages.props` pins `xunit.v3.mtp-off` at `[4.0.2,5.0.0)`,
`xunit.runner.visualstudio` at `[4.0.1,5.0.0)` and `Microsoft.NET.Test.Sdk` at `[18.11.0,19.0.0)`. No
project references plain `xunit.v3`, and there is no `global.json` opting into MTP.

## Decision Drivers

- Targeted runs and coverage collection are the normal workflow, and both need the VSTest target.
- The framework should be current, because the suite is where most changes are verified.
- One test stack across five projects, so a developer learns it once.

## Considered Options

1. xUnit v3 with `xunit.v3.mtp-off` and `xunit.runner.visualstudio` (status quo).
2. xUnit v2 with `xunit.runner.visualstudio`.
3. xUnit v3 with MTP enabled, opting in through `global.json`.
4. MTP with `Microsoft.Testing.Extensions.CodeCoverage` and MTP-mode `dotnet test`.

## Decision Outcome

Chosen option: 1, because it is the only option that is both current and compatible with the commands the
repository already runs. Option 2 would be compatible but would give up v3. Option 3 is what the plain v3
package does, so it fails the requirement the workflow imposes. Option 4 is the documented revisit, not
something verified for this repository.

The rules it imposes:

- Every server test project references `xunit.v3.mtp-off`, never plain `xunit.v3` and never v2's `xunit`.
- `xunit.runner.visualstudio` stays beside it with `PrivateAssets="all"`; it is what makes the project
  visible to the VSTest target.
- Each test project declares `<OutputType>Exe</OutputType>`, which v3 requires.
- A package upgrade that "modernises" the reference to plain `xunit.v3` is a change to this decision, not
  a routine bump. Make the MTP opt-in work first, then record the change.
- Revisit when MTP-mode `dotnet test` is verified to work here; the condition is named in the project
  comment.

### Consequences

- Good, because `dotnet test --filter`, `--collect` and the coverage script keep working unchanged.
- Good, because the suite is on v3, so `IAsyncLifetime.InitializeAsync` returning `ValueTask`, the
  `IAsyncDisposable` disposal shape and the xUnit1051 cancellation-analyzer rule are available.
- Good, because one package pair is used by all five projects, so the stack is learned once.
- Bad, because it is a variant package rather than the framework's own package, and an upgrade path has to
  understand why it was chosen — which is why the reason sits beside every reference.
- Bad, because MTP's own tooling is unavailable while this decision stands: single-executable discovery,
  its filtering syntax and its coverage extension cannot be used.
- Bad, because the package variant must be watched for lag behind plain `xunit.v3` releases.

### Confirmation

- Package: `Source/Server/Directory.Packages.props` pins `xunit.v3.mtp-off`; all five test projects
  reference it and carry the reason in a comment beside it.
- Test: the suite runs through `dotnet test`, including a filtered run and the `--collect` coverage run.
- Instruction: `.github/instructions/dotnet-tests.instructions.md` (xUnit v3 specifics).
- Review: a `xunit.v3` or `xunit` package reference, or a new test project without `xunit.v3.mtp-off`, is a
  change to this decision.

## More Information

- Shouldly (assertions) and NSubstitute (substitution) are the libraries in use across the test projects.
  They were adopted as the house style rather than weighed against alternatives, so they are not part of
  this decision.
- Verify (snapshot testing) is not adopted. The instruction describes it as guidance for when that form of
  testing is needed, not as an available package.
- The structure this tooling serves — a unit project per layer plus one hosted integration project — is
  ADR-0037.
