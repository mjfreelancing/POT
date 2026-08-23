# Instructions

This folder contains instruction source files that are copied into `.github/instructions/` in a consuming repository.

## Copy target

Copy the selected `*.instructions.md` files from this folder into `.github/instructions/` in your target repository.

## Available files

| File                                           | Description                                                            |
| ---------------------------------------------- | ---------------------------------------------------------------------- |
| `language-agnostic-core.instructions.md`       | Baseline coding behavior that applies across stacks.                   |
| `csharp.instructions.md`                       | Core C# and .NET guidance.                                             |
| `typescript.instructions.md`                   | Core TypeScript guidance.                                              |
| `react-client.instructions.md`                 | React client architecture and UI guidance.                             |
| `aspnetcore-api.instructions.md`               | ASP.NET Core API endpoint, contract, and server-layer guidance.        |
| `aspnetcore.integration-tests.instructions.md` | ASP.NET Core hosted integration testing guidance.                      |
| `dotnet.tests.instructions.md`                 | Deterministic .NET unit testing guidance.                              |
| `client.tests.instructions.md`                 | Client test design and tooling guidance.                               |
| `postgres-efcore.instructions.md`              | PostgreSQL and EF Core modeling/query/migration guidance.              |
| `docker.instructions.md`                       | Docker workflow safety and validation guidance.                        |
| `playwright-e2e.instructions.md`               | Playwright E2E locator, click, assertion, and stability guidance.      |
| `coding-patterns.instructions.md`              | C# coding patterns and AllOverIt extension and validation preferences. |

## Notes

- These files are stored flat to mirror the final `.github/instructions/` structure.
- Adjust `applyTo` values after copying if your repository layout differs from the examples.
- Avoid `applyTo: "**/*"` on multiple instruction files: it sends every file with every request and dilutes agent attention. Reserve always-on guidance for the repository's `.github/copilot-instructions.md` and keep `*.instructions.md` scoped to specific file patterns.
- `language-agnostic-core.instructions.md` is the sanctioned exception: it is the single always-on baseline and keeps `applyTo: "**/*"`. Do not add more `**/*` instruction files.
- See `../README.md` for asset combinations and adoption guidance.

## Baseline and Mirror Workflow

Use this workflow for instruction files that combine reusable guidance with repository-specific additions.

1. Keep reusable, cross-project guidance in this repository under `instructions/*.instructions.md`.
2. In consuming repositories, mirror the file structure and keep the same section layout (`Core Rules` + `Expansion Notes`).
3. Treat `Core Rules` as baseline-owned. Do not fork these rules in consuming repositories unless a baseline update is intentionally deferred.
4. Put repository-specific behavior only in `Expansion Notes` (for example route anchors, selector anchors, fixtures, scripts, and conventions).
5. When baseline files change, sync mirrors by:
   - copying updated `Core Rules` from baseline,
   - preserving only repository-specific bullets in `Expansion Notes`,
   - confirming `applyTo` paths are correct for the target repository layout.

Recommended review check:

- `Core Rules` are text-equivalent between baseline and mirror.
- Mirror-only content exists only in `Expansion Notes`.
- Tone and heading structure match other instruction files in the repository.
