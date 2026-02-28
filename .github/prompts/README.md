# Prompts

This folder contains reusable workflow prompts for Copilot tasks.

## Purpose

Prompts describe task flow (how to execute work), not coding standards.

## Contents

- `repo_tests.prompt.md` - run repository test suites (server/client) and summarize outcomes.
- `server_unit_test.prompt.md` - create or update server unit tests in the nearest test project.
- `client_tests.prompt.md` - create or update client tests (Vitest + Testing Library).
- `server_integration_test.prompt.md` - create/update server API integration tests.
- `feature_implementation.prompt.md` - implement cross-layer features with scoped validation.
- `docker_workflow.prompt.md` - run Docker lifecycle workflows (start/stop/build/health) with task-first defaults.
- `document_csharp.prompt.md` - add/update XML docs for C# class public APIs.
- `document_typescript.prompt.md` - add/update TSDoc/JSDoc for TypeScript modules/components/hooks.

## Source of truth boundary

- Keep coding/architecture standards in `.github/instructions/*`.
- Keep cross-cutting summaries in `.github/README.md` and `.github/copilot-workflow-guide.md`.
- Keep this file lightweight and index-oriented to avoid duplication.

## Notes

- Keep workflow detail in prompt files, not this index.
