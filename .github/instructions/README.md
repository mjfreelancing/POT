# Instructions

This folder contains path-scoped Copilot instruction files (`applyTo`) used during code generation.

## Purpose

Instructions are the canonical source for coding standards and conventions.

## Contents

- `language-agnostic-core.instructions.md`
- `aspnetcore-api.instructions.md`
- `csharp.instructions.md`
- `dotnet.tests.instructions.md`
- `aspnetcore.integration-tests.instructions.md`
- `react-client.instructions.md`
- `postgres-efcore.instructions.md`
- `server.instructions.md`
- `typescript.instructions.md`
- `client.tests.instructions.md`
- `docker.instructions.md`

## Source of truth boundary

- Put stable standards here.
- Keep prompts in `.github/prompts/*` focused on execution workflow.
- Keep this file as an index to reduce duplicated rule text.

## Notes

- Keep rule detail in instruction files, not this index.
