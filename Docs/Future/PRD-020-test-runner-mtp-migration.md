# PRD-020 — Server Test Runner Migration: VSTest → Microsoft Testing Platform (MTP)

> **Classification:** Discovery PRD Draft (gated/future track — not yet executable).
> **Status:** Proposed.
> **Priority:** Low.
> **Last updated:** 2026-09-23.
> **Scope:** `Source/Server` test projects (`Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`, `Pot.Shared.Tests`, `Pot.AspNetCore.Integration.Tests`), the server coverage workflow, and the test-command references in `.github`.
> **Predecessor/context:** xUnit.net v2 → v3 migration completed 2026-09-23 (see §2). xUnit.net v3 test projects are MTP-enabled by default; the repo deliberately uses the `xunit.v3.mtp-off` package variant so the projects stay on VSTest.

## 1. Purpose

Move the five server test projects from the VSTest runner path to **Microsoft Testing Platform (MTP)** — i.e. plain `xunit.v3` plus a `global.json` MTP opt-in — so the repo is on the runner path that .NET is standardising on, with MTP-native coverage and filtering.

This PRD records the current state, the **external blocker** that gates the work, the planned approach, the re-check procedure an agent must run before starting, risks, and the decisions still open.

## 2. Current State (verified 2026-09-23)

| Item                     | Value                                                                                                                                                                                            |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| xUnit.net                | **v3 `4.0.1`** (published 2026-09-13) — migrated from `xunit` 2.9.3 on 2026-09-23                                                                                                                |
| Test framework package   | `xunit.v3.mtp-off` 4.0.1 (MTP support explicitly disabled)                                                                                                                                       |
| Runner packages          | `Microsoft.NET.Test.Sdk` 18.10.1, `xunit.runner.visualstudio` 4.0.0 (VSTest)                                                                                                                     |
| Coverage                 | `coverlet.collector` 10.0.1 + `Source/Server/coverlet.runsettings`, driven by `Source/Server/code_coverage.ps1`                                                                                  |
| Test project TFM         | `net10.0` (+ `<OutputType>Exe</OutputType>`, required by xUnit v3)                                                                                                                               |
| Installed .NET SDKs      | `10.0.302`, `10.0.401`, `11.0.100-preview.6.26359.118` — **no `global.json` pins one, so the newest (11 preview) is used**; the _SDK_ version is independent of the `net10.0` _target framework_ |
| Latest MTP on NuGet      | `Microsoft.Testing.Platform` **2.4.1** (published 2026-09-17); `xunit.v3` 4.0.1 resolves **2.4.0**                                                                                               |
| Baseline suite           | **635 tests** (Pot.Shared 27, Pot.Data 76, Pot.AspNetCore 71, Pot.App 433, Pot.AspNetCore.Integration 28), 0 failures                                                                            |
| Reviewed command surface | `dotnet test pot.sln`, `dotnet test <project> --filter "FullyQualifiedName~..."`, `--collect:"XPlat Code Coverage" --settings coverlet.runsettings`                                              |

## 3. The Blocker (why this is a future/gated track)

MTP mode of `dotnet test` (enabled with a `global.json` test-runner entry, see §7) **fails on this machine's toolchain**, and the failure is reproducible **outside the repository** with a stock template project:

```
dotnet new xunit3 -f net10.0 -o Probe
cd Probe && dotnet test
  -> ...\Probe.dll (net10.0) Zero tests ran
  -> Exit code: 5
  -> Handshake failures: ...\Probe.dll (net10.0)
```

**Root cause (from the app's own MTP diagnostic log):** the SDK launches the test application in _server mode_ and the MTP runtime that xUnit v3 ships does not implement the switch it passes.

```
Version: 2.4.0+...
Command line arguments: '--nologo --server dotnettestcli --dotnet-test-pipe testingplatform.pipe.<guid>'
```

Directly probing the built test app confirms which switch is unsupported (MTP exit code 5 = _invalid command-line arguments_):

| Invocation                              | Observed result                                                                   | Interpretation                                             |
| --------------------------------------- | --------------------------------------------------------------------------------- | ---------------------------------------------------------- |
| `Probe.exe --server dotnettestcli`      | exit `5`, nothing runs                                                            | `--server` **not implemented**                             |
| `Probe.exe --dotnet-test-pipe x`        | tests actually run (`Passed! total: 1`), exit `5` only because the pipe is absent | `--dotnet-test-pipe` is implemented (hidden from `--help`) |
| `Probe.exe --help` (≈19 KB of switches) | no `server` / `pipe` / `msbuild-node` switch listed                               | server mode is absent from this MTP build                  |

**Ruled out (all still fail identically):**

- Stock template project **outside** the repo → not a POT configuration defect.
- Template's MTP-command-line variant (`dotnet new xunit3 --command-line mtp`, i.e. `UseMicrosoftTestingPlatformRunner=true`).
- SDK pinned to **`10.0.401` (GA)** via `global.json` → not an artefact of the 11-preview SDK.
- `Microsoft.Testing.Platform` **2.4.1** referenced explicitly (newest release).
- `<DisableTestingPlatformServerCapability>true</DisableTestingPlatformServerCapability>` → the SDK still passes `--server`, still exit 5.

**Second, independent constraint (why `mtp-off` is needed even before the above):** an MTP-enabled project cannot be executed through the VSTest target on **.NET 10 SDKs and later** — the SDK target rejects it (`.NET 10 SDK and later` guard in `Microsoft.Testing.Platform.MSBuild.targets`, MTP 2.4.0), which is exactly what happened when the repo first referenced plain `xunit.v3`:

```
error : Testing with VSTest target is no longer supported by Microsoft.Testing.Platform
        on .NET 10 SDK and later. If you use dotnet test, you should opt-in to the new dotnet test experience.
```

**Trigger to begin implementation:** the re-check procedure in §7 must pass — the stock `dotnet new xunit3` probe runs green under MTP mode (and, as a second gate, a repo-representative project runs green under MTP mode).

## 4. Goals

- All five server test projects run under MTP (`dotnet test` in MTP mode) with the **same 635+ passing tests** and no behavioural change.
- Coverage continues to produce Cobertura XML consumable by `reportgenerator` / `code_coverage.ps1`, via whichever coverage approach §9 decides.
- Targeted reruns keep working with equivalent ergonomics (MTP filter switches instead of `--filter "FullyQualifiedName~..."`).
- Every documented command in `.github/**` and `Source/Server/**` that references `dotnet test` is updated so no stale syntax remains.
- No change to the test code's public shape (fixtures, `Pot.TestUtils`, `Shouldly`/`NSubstitute` stack) beyond runner-facing plumbing.

## 5. Non-Goals

- No change to test framework version or assertion stack (already xUnit v3 + Shouldly/NSubstitute).
- No change to the client (`pot-react`) tooling or its Vitest/Playwright suites.
- No change to Docker/Azure images, except where a build/test invocation is affected.
- No switch to a different test framework (MSTest/TUnit).
- No adoption of MTP-only features (attachments, warnings, `--timeout`, device testing) as part of the migration itself.

## 6. Planned Approach (when the trigger fires)

1. **Confirm the trigger** (§7) on the current SDK; record the SDK version and MTP version in this PRD's status line.
2. **Flip one project first** — `Pot.Shared.Tests` (no Testcontainers): plain `xunit.v3`, drop `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio`, add `global.json` with `{"test":{"runner":"Microsoft.Testing.Platform"}}` at the repo root (or `Source/Server`), then `dotnet test --project Pot.Shared.Tests/Pot.Shared.Tests.csproj`. Expect `TestingPlatformDotnetTestSupport` to be unnecessary in MTP mode.
3. **Prove the repo-representative stack**: `Pot.AspNetCore.Integration.Tests` (ASP.NET host + Testcontainers + Postgres) under MTP — this is the suite most likely to surface runner-specific issues.
4. **Coverage**: implement the option chosen in §9 (open decision), including `Microsoft.Testing.Extensions.CodeCoverage` (or the accepted alternative), a settings file replacing `coverlet.runsettings`, and a `code_coverage.ps1` update for `*.cobertura.xml` naming and MTP-mode invocation.
5. **Roll out to the remaining projects**, verifying per project: full suite green, filter reruns work, coverage artifact present.
6. **Update documentation** (§8) and the `xunit.v3.mtp-off` package comment in the five `.csproj` files (they must be removed or replaced when the switch happens).
7. **Record the outcome**: this PRD → Complete, README index row, repo memory (`/memories/repo/server-tests.md`), and the POT bullets in `.github/instructions/dotnet.tests.instructions.md`.

**Watch items:** MTP-mode `dotnet test` changes option syntax and results layout; the `--` separator rules; whether `Microsoft.NET.Test.Sdk`/`xunit.runner.visualstudio` must be removed for the SDK to treat the project as MTP (the SDK errors when a solution mixes MTP and VSTest projects while MTP is opted in); editor/Test Explorer behaviour (VS Code C# Dev Kit / VS 2022 use the MTP _server capability_, which is precisely the path that is currently broken).

## 7. Agent Re-check Procedure (is the MTP migration possible yet?)

Run this **before** starting §6. It is safe: everything happens in a temp folder, no repo files change. PowerShell, from any directory.

### Step 1 — Preconditions

| Check                     | Command                                                                         | Current (2026-09-23)                      |
| ------------------------- | ------------------------------------------------------------------------------- | ----------------------------------------- |
| SDK in use                | `dotnet --version`                                                              | `11.0.100-preview.6.26359.118` (OK, ≥ 10) |
| xUnit templates installed | `dotnet new list xunit3`                                                        | `xunit3` + `xunit3-extension` installed   |
| Newest MTP release        | `dotnet package search Microsoft.Testing.Platform --exact-match` (or nuget.org) | `2.4.1`                                   |

### Step 2 — Primary probe (stock template, MTP mode)

```powershell
$probe = Join-Path $env:TEMP "mtp-probe"
Remove-Item -Recurse -Force $probe -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $probe | Out-Null
Push-Location $probe
dotnet new xunit3 -f net10.0 -o Probe        # writes global.json with the MTP runner opt-in
Pop-Location
Push-Location (Join-Path $probe "Probe")
dotnet test
Pop-Location
```

| Result                                                   | Verdict                                                                                             |
| -------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| `Test run summary: Passed!` with `total: 1`              | ✅ **Unblocked** — proceed to Step 3                                                                |
| `Zero tests ran` / `Exit code: 5` / `Handshake failures` | ❌ **Still blocked** — stop; optionally run Step 4, then re-check after the next MTP or SDK release |
| `Testing with VSTest target is no longer supported...`   | ❌ Still blocked (probe project is MTP-enabled but ran under VSTest — check `global.json`)          |

### Step 3 — Secondary probe (repo-representative stack)

Only if Step 2 passes. The stock template has no ASP.NET host, EF Core, or Testcontainers, so prove the repo's actual stack before planning §6:

1. Copy `Pot.Shared.Tests` and then `Pot.AspNetCore.Integration.Tests` to a temp folder **with** their `ProjectReference`s, or (simpler) temporarily switch one project in-place within a scratch branch/worktree.
2. In the probe copy: replace `xunit.v3.mtp-off` with `xunit.v3`, drop `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio`, ensure `<OutputType>Exe</OutputType>` is set, and add the `global.json` MTP opt-in.
3. Run `dotnet test --project <probe project>`. Docker must be running for the integration fixture.
4. Pass criteria: identical test counts/outcomes (`27` for Pot.Shared, `28` for the integration fixture) and a Cobertura artifact produced by the chosen coverage approach.

### Step 4 — Diagnostics (only when a probe fails)

```powershell
$env:TESTINGPLATFORM_DIAGNOSTIC='1'
$env:TESTINGPLATFORM_DIAGNOSTIC_VERBOSITY='Trace'
dotnet test
# then read <project>\TestResults\*_<tfm>_<arch>_<timestamp>.diag
```

Interpretation:

- The `Command line arguments:` line shows what the SDK passes to the app. If it contains `--server dotnettestcli` **and** the app exits `5`, the MTP runtime still lacks server mode → blocked.
- Per-switch check against the built app (MTP exit `5` = unsupported):
  - `& .\bin\Debug\net10.0\Probe.exe --server dotnettestcli` → `5` means server mode is missing.
  - `& .\bin\Debug\net10.0\Probe.exe --dotnet-test-pipe x` → running tests means the pipe switch exists.
  - `& .\bin\Debug\net10.0\Probe.exe --help` → authoritative list of supported switches (extension options appear here too).
- If the SDK passes different switches than §3 recorded (protocol change), update §3 of this PRD with the new evidence before drawing conclusions.

### Step 5 — Bookkeeping

- If **blocked**: update the "Last updated" date and append the observed SDK/MTP versions to §3 (or a short status log). Do not start §6.
- If **unblocked**: flip this PRD's classification to Delivery-ready, then execute §6.

## 8. Files That Must Change With the Migration

| Area                 | Files                                                                                                                                                                                                                                                    |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Package/runner       | 5 × `Source/Server/*Tests/*.csproj` (drop `xunit.v3.mtp-off`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`; use `xunit.v3`) + new `global.json`                                                                                                 |
| Coverage             | `Source/Server/code_coverage.ps1`, `Source/Server/coverlet.runsettings` (retire/replace), `server-run-test-coverage` VS Code task (path unchanged)                                                                                                       |
| Instructions         | `.github/copilot-instructions.md`, `.github/instructions/dotnet.tests.instructions.md`, `.github/instructions/aspnetcore.integration-tests.instructions.md`, `.github/instructions/aspnetcore-api.instructions.md`                                       |
| Prompts/skills/index | `.github/prompts/dotnet_unit_test.prompt.md`, `.github/prompts/repo_tests.prompt.md`, `.github/README.md`, `.github/skills/code-coverage/SKILL.md`, `.github/skills/dotnet-integration-test/SKILL.md`, `.github/skills/server-integration-test/SKILL.md` |
| Server docs          | `Source/Server/DEVELOPER.md` (test/coverage command tables)                                                                                                                                                                                              |
| Historical docs      | `Docs/Future/PRD-002-server-login-logout-session-architecture.md` (command example) — decide whether to update historical PRDs or leave them as-is                                                                                                       |

Command translation table (VSTest → MTP mode):

| Current                                                                       | MTP mode                                                                                                                                  |
| ----------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `dotnet test pot.sln`                                                         | `dotnet test --solution pot.sln`                                                                                                          |
| `dotnet test <path>.csproj`                                                   | `dotnet test --project <path>.csproj`                                                                                                     |
| `dotnet test --filter "FullyQualifiedName~<Name>"`                            | `dotnet test --filter-class <Name>` / `--filter-method` / `--filter-namespace` / `--filter-trait`                                         |
| `dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings` | `dotnet test --coverage --coverage-output-format cobertura --coverage-settings <settings file>` (requires the coverage extension package) |

## 9. Risks

| #   | Risk                                                                             | Likelihood       | Impact                   | Mitigation                                                                                                            |
| --- | -------------------------------------------------------------------------------- | ---------------- | ------------------------ | --------------------------------------------------------------------------------------------------------------------- |
| R1  | MTP-mode `dotnet test` stays broken (upstream SDK/MTP gap)                       | Certain today    | Low (VSTest still works) | Gate on §7; keep `mtp-off` until proven; re-check on each MTP/SDK release                                             |
| R2  | Coverage tooling change loses coverage fidelity or report shape                  | Med              | Med                      | Compare per-project Cobertura output before/after; `reportgenerator` consumption is format-based and unchanged        |
| R3  | Documented commands drift (docs updated in some places, not others)              | High (13+ files) | Med                      | §8 checklist; treat as part of the same change, not a follow-up                                                       |
| R4  | Test Explorer / editor integration regressions after dropping the VSTest adapter | Med              | Med-Unmed                | Verify VS Code C# Dev Kit + VS 2022 against MTP mode before completing; keep the adapter only if the SDK tolerates it |
| R5  | MTP parallelisation/default differences change integration-test timing/flakiness | Low-Med          | Med                      | Full integration suite (Testcontainers) is a mandatory gate; watch per-fixture container startup                      |
| R6  | Lost regression signal during the switch (KVP)                                   | Low              | Med                      | 635-test baseline recorded in §2; compare total counts and all-green before/after                                     |

## 10. Open Questions / Decisions

- **DQ-1 (coverage):** which coverage approach? Options: (a) `Microsoft.Testing.Extensions.CodeCoverage` — MTP-native, requires a new settings schema and script changes; (b) keep `coverlet` — **incompatible with MTP mode** (coverlet's collector is a VSTest data collector), so choosing this means not migrating; (c) other MTP-compatible coverage (e.g. `dotnet-coverage` tooling) if it matures. Decided: **leave open**, list trade-offs for now.
- **DQ-2 (VSTest packages):** must `Microsoft.NET.Test.Sdk` + `xunit.runner.visualstudio` be removed, or can they stay for editor/Test Explorer while `global.json` opts into MTP? ("It is an error if any of the test projects use VSTest" once MTP is opted in — verify empirically in Step 3 before committing to an answer.)
- **DQ-3 (SDK pinning):** if the blocker turns out to be preview-SDK-specific, do we pin a GA SDK in `global.json` as part of the migration, or wait for the newest SDK to be fixed? (Prefer no pin; the repo currently has none.)
- **DQ-4 (sequencing):** migrate all five projects in one change, or project-by-project (starting `Pot.Shared.Tests`) in separate increments? (Recommendation: per-project increments, matching PRD-015/017 discipline.)
- **DQ-5 (historical docs):** update command examples inside completed PRDs (e.g. PRD-002) or leave historical documents frozen?
- **Audience/ownership:** single-developer repo; the user is sole owner/decision-maker — no team assignments. (TBD if this ever moves to a team.)

## 11. Assumptions & Constraints Register

- `net10.0` requires an SDK ≥ 10, and on SDK ≥ 10 an MTP-enabled project cannot use the VSTest target — so VSTest and MTP are mutually exclusive per project, not complementary.
- VSTest mode is still the default `dotnet test` mode for non-MTP projects; the repo is not on a broken path today, only on a deprecated one.
- MTP mode requires the `global.json` opt-in (SDK ≥ 10) or `DOTNET_TEST_RUNNER=Microsoft.Testing.Platform` (SDK ≥ 11 preview 6).
- `xunit.v3` (MTP on) is the forward package; `xunit.v3.mtp-off` exists precisely for the VSTest-retention case and is the current choice.
- No git operations by agents; the user stages/commits.
- Docker must be running for the integration-suite probes.

## 12. Design Notes (Rationale and Rejected Alternatives)

- **Why `mtp-off` now instead of plain `xunit.v3`:** moving to xUnit v3 (latest framework, 4.0.1) without `mtp-off` broke `dotnet test` on SDK ≥ 10 with the VSTest-target error, and MTP mode is blocked by §3 — so `mtp-off` is the only configuration where the full documented workflow (filters, coverlet collection, runsettings) works today. The reason is recorded in a comment above the package reference in each of the five test projects.
- **Rejected: `TestingPlatformDotnetTestSupport=true` (legacy VSTest-mode invocation of MTP apps)** — Microsoft documents this mode as legacy and it is removed for .NET 10 SDKs; it is the exact path that produced the VSTest-target error.
- **Rejected: running MTP apps only via `dotnet run` / the test executable** — would abandon `dotnet test`, the coverage script, and the documented reruns; more churn for less capability than waiting for MTP mode.
- **Rejected: `xunit.v3` 3.x (MTP v1) to sidestep the v2 server-mode gap** — would deliberately downgrade the framework (3.x < 4.0.1) and still requires the coverage/command-syntax migration; not examined further once MTP v2 was confirmed blocked on both SDK lines.
- **Rejected: pinning SDK `10.0.401`** — tested; MTP mode fails there too, so pinning buys nothing today.
- **Rejected: `DisableTestingPlatformServerCapability=true`** — tested; the SDK still launches with `--server dotnettestcli`.
- **SDK vs TFM:** the `net10.0` target framework does not select the SDK; with three SDKs installed and no `global.json`, the newest (11 preview) drives `dotnet test`/MSBuild. Runner behaviour is SDK-version driven, which is why both SDK lines were probed.

## 13. Discovery Exit Criteria (what closes this draft)

1. §7 Step 2 passes on the machine's default SDK, with the SDK and MTP versions recorded in §3.
2. §7 Step 3 passes for both a unit-test project and the Testcontainers integration project under MTP mode.
3. DQ-1 (coverage) and DQ-2 (VSTest packages) are answered with verified evidence.
4. The §8 command translation is confirmed against real runs (including a filtered rerun and a coverage run producing Cobertura XML).
5. This PRD is re-classified as delivery-ready, with the README index row and repo memory updated.

## 14. References

- `Docs/Future/PRD-016-typescript-7-upgrade.md` — the gated-track format this PRD follows (trigger, probe, exit criteria).
- `Docs/Future/PRD-017-client-dependency-drift-maintenance.md` — per-increment gate discipline referenced by DQ-4.
- xUnit.net v3 docs — migration, MTP support, MTP code coverage: <https://xunit.net/docs/getting-started/v3/microsoft-testing-platform>, <https://xunit.net/docs/getting-started/v3/code-coverage-with-mtp>.
- Microsoft Learn — `dotnet test` MTP mode and troubleshooting/exit codes: <https://learn.microsoft.com/dotnet/core/tools/dotnet-test-mtp>, <https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-troubleshooting>.
- Repo memory `/memories/repo/server-tests.md` — current runner configuration and the recorded blocker evidence.
- `.github/instructions/dotnet.tests.instructions.md` — POT test conventions, including the `mtp-off` rationale.
