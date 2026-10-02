---
paths:
  - "Source/Server/*Tests/**/*.cs"
---

# .NET test conventions

Test projects: `Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`, `Pot.Shared.Tests` (in-process unit tests) and `Pot.AspNetCore.Integration.Tests` (hosted API; extra rules in its own `CLAUDE.md`). Shared cross-project helpers live in `Pot.TestUtils` and must stay general-purpose; project-specific helpers stay in the owning test project.

## Scope and structure

- Tests are deterministic and isolated from external systems; assert behavior and contracts, not implementation details.
- Keep unit and integration tests in separate projects. Put a test in the nearest layer-specific project.
  - `Pot.App.Tests` validate `EnrichedResult` success and failure semantics.
  - `Pot.Data.Tests` validate specification and query behavior without leaking API-layer concerns.
- Outer classes are named `<Type>Fixture` (`*Fixture.cs`), with nested classes grouped by method or scenario under test.
- Test methods use a `Should_...` prefix with sentence-style underscores (`Should_Return_No_Validation_Errors`). Keep `[Fact]` / `[Theory]`.
- Keep test order aligned with the implementation's logic order (for example, log/assert-first tests before later-branch tests).
- Keep assertion style consistent within a project (currently Shouldly with NSubstitute).
- XML documentation is not required in test files (the C# XML doc rules do not apply). Add it to a helper only when its logic is non-obvious.
- When setup or assertions repeat, extend a shared helper rather than duplicating logic.
- Use csproj `InternalsVisibleTo` when internals must be visible. Test-only dependencies stay in test projects.
- Aim for full coverage of behavior and meaningful branches in the code under test; add targeted tests for gaps.

## xUnit v3 specifics

- Packages: `xunit.v3.mtp-off` + `xunit.runner.visualstudio` + `Microsoft.NET.Test.Sdk`. The `mtp-off` variant is deliberate: v3 defaults to Microsoft Testing Platform, and on .NET 10+ an MTP-enabled project cannot run through the VSTest target that `dotnet test`, `--collect` and `--settings` rely on.
- v3 test projects must set `<OutputType>Exe</OutputType>` or the build fails.
- `IAsyncLifetime.InitializeAsync` returns `ValueTask`. Disposal comes from `IAsyncDisposable`, implemented explicitly as `async ValueTask IAsyncDisposable.DisposeAsync()`.
- Pass `TestContext.Current.CancellationToken` to awaited calls that accept a token (analyzer xUnit1051). If a fixture declares its own nested `TestContext` type, qualify it as `Xunit.TestContext.Current.CancellationToken`.
- For logging assertions in unit tests, use the NSubstitute capture pattern (`CaptureLogCalls` / `CaptureLogCallsAsync` returning `LoggerCallContext`).

## Snapshot testing (Verify)

- Prefer one `await Verifier.Verify(content)` over several `ShouldContain`/`ShouldBe` checks when asserting generated output (diagram text, serialized JSON, reports, templates). The test method is `async Task`.
- Do not mix snapshot and manual string assertions in the same test.
- Keep snapshots in a `Snapshots` directory per test project, configured in a `[ModuleInitializer]` with `Verifier.UseProjectRelativeDirectory("Snapshots")`. Normalize machine-specific content (absolute paths, timestamps) with `VerifierSettings.AddScrubber(...)` there.
- Approve by renaming `.received.txt` to `.verified.txt` and commit the verified files.
