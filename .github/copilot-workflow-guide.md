# Copilot Workflow Guide

This guide explains how Copilot instructions are configured in this repository and how scope is selected.

## Required setting

- File: `.vscode/settings.json`
- Setting: `github.copilot.chat.codeGeneration.useInstructionFiles: true`

Why this matters: without this setting, Copilot may not reliably load `.github/instructions/*.md` files.

## Instruction precedence

1. Global repository instructions (`.github/copilot-instructions.md`)
2. Scoped instruction files (`.github/instructions/*.md` with `applyTo`)
3. Prompt files (if/when added)
4. Plan files (if/when added)

## Active scoped instruction files

- `.github/instructions/csharp.instructions.md` (`Source/Server/**/*.cs`)
- `.github/instructions/server.instructions.md` (`Source/Server/**`)
- `.github/instructions/server.tests.instructions.md` (`Source/Server/*Tests/**`)
- `.github/instructions/integration-tests.instructions.md` (`Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs`)
- `.github/instructions/client.instructions.md` (`Source/Client/pot-react/**`)
- `.github/instructions/typescript.instructions.md` (`Source/Client/pot-react/src/**/*.{ts,tsx}`)
- `.github/instructions/client.tests.instructions.md` (`Source/Client/pot-react/tests/**`)
- `.github/instructions/docker.instructions.md` (`Source/Docker/**`)

## Practical use

- Keep shared repo context in `.github/copilot-instructions.md`.
- Keep implementation details in path-scoped instruction files.
- Keep test conventions in test-scoped instruction files.
- Prefer minimal, targeted edits and smallest relevant test scope first.
- Integration tests are implemented in `Source/Server/Pot.AspNetCore.Integration.Tests`; use integration-specific instruction/prompt guidance there.

## Available prompts

- `.github/prompts/repo_tests.prompt.md`
- `.github/prompts/server_unit_test.prompt.md`
- `.github/prompts/server_coverage.prompt.md`
- `.github/prompts/client_tests.prompt.md`
- `.github/prompts/document_csharp.prompt.md`
- `.github/prompts/document_typescript.prompt.md`
- `.github/prompts/server_integration_test.prompt.md`
- `.github/prompts/docker_workflow.prompt.md`
- `.github/prompts/feature_implementation.prompt.md`

## Available skills

- `.github/skills/environment-preflight/SKILL.md`

## Automation

- Automation guide: `.github/automation-playbook.md`
- Copilot agent hooks: `.github/hooks/hooks.json` (`sessionEnd` maintenance scripts in `.github/hooks/scripts/`)

## Repo notes

- Preferred full stack startup task: `docker-start-pot-client-server`.
- Main references: `Docs/ARCHITECTURE.md`, `Docs/LOCAL-SETUP.md`, `Docs/DOCKER-SETUP.md`.
