# Skills

On-demand multi-step workflows. Copilot discovers each skill by its `name` and `description` and loads it when relevant; each also appears as a `/<name>` slash command in chat.

| Skill                     | Purpose                                                                                   |
| ------------------------- | ----------------------------------------------------------------------------------------- |
| `client-tests`            | Create or update Vitest tests for the client.                                             |
| `code-coverage`           | Run the server coverage workflow and close gaps in changed code.                          |
| `docker-workflow`         | Build, start, stop and health-check the Docker stack (manual invocation only).            |
| `document-csharp`         | Add or update XML documentation on a C# class and its public API.                         |
| `document-typescript`     | Add or update TSDoc/JSDoc on TypeScript modules, components and hooks.                    |
| `dotnet-unit-test`        | Create or update server unit tests.                                                       |
| `feature-implementation`  | Implement a feature across layers using a checklist.                                      |
| `mmd2png`                 | Convert Mermaid `.mmd` files to PNG.                                                      |
| `prd`                     | Generate Product Requirements Documents.                                                  |
| `prd-delivery`            | Deliver an approved PRD as a staged plan of testable increments (manual invocation only). |
| `run-tests`               | Run the server and/or client test suites and summarize failures.                          |
| `server-integration-test` | Create or update hosted API integration tests.                                            |

## Adding a skill

- Create `.github/skills/<name>/SKILL.md`. The folder name must match the `name` field (lowercase letters, numbers, hyphens).
- Make the `description` specific about what the skill does and when to use it; it drives discovery.
- Set `disable-model-invocation: true` for skills with side effects so they only run when invoked explicitly.
- Keep the body concise and link bundled files with relative Markdown links.
