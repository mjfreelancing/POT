# Skills

This folder contains skill source folders that are copied into `.github/skills/<name>/` in a consuming repository.

Skills are on-demand agent capabilities. Unlike instructions, which are always applied, skills are discovered by their `name` and `description` and loaded only when relevant. They can bundle scripts, templates, and examples alongside the `SKILL.md` instructions.

## Copy target

Copy each selected skill folder from this folder into `.github/skills/` in your target repository.

- `skills/<name>/` -> `.github/skills/<name>/`

## Available skills

| Skill                     | Description                                                                  |
| ------------------------- | ---------------------------------------------------------------------------- |
| `prd`                     | Generate high-quality Product Requirements Documents (PRDs).                 |
| `docker-workflow`         | Run Docker lifecycle workflows with task-first defaults and validation.      |
| `code-coverage`           | Run repository coverage workflow and summarize results.                      |
| `dotnet-integration-test` | Create or update .NET integration tests.                                     |
| `server-integration-test` | Create or update hosted server API integration tests that boot the app host. |
| `feature-implementation`  | Implement a feature with a cross-layer checklist and focused validation.     |

## Notes

- Each skill lives in a folder named after its `name` field. The `name` must match the folder name and use lowercase letters, numbers, and hyphens only.
- The `description` drives discovery. Keep it specific about what the skill does and when to use it.
- Keep `SKILL.md` bodies concise and reference bundled files with relative Markdown links.
- The five workflow prompts in `prompts/` were migrated here because they are multi-step procedures rather than single tasks.
- See `../README.md` for asset combinations and adoption guidance.

## Prompt vs Skill

- Use a prompt for a single focused task with parameterized inputs.
- Use a skill for a multi-step workflow that may bundle scripts, templates, or examples.
- A skill appears as a slash command in chat and is also loaded automatically when the agent decides it is relevant.
